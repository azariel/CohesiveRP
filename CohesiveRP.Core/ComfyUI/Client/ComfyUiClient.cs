using System.Net.WebSockets;
using System.Text.Json;
using System.Text.Json.Nodes;
using CohesiveRP.Common.Configuration;
using CohesiveRP.Common.Diagnostics;
using CohesiveRP.Common.HttpClient;
using CohesiveRP.Common.Websocket;

namespace CohesiveRP.Core.ComfyUI.Client
{
    public class ComfyUiClient : IComfyUiClient, IDisposable
    {
        private readonly HttpRestClient _http;
        private WebSocketClient _ws;
        private readonly string _baseUrl;
        private readonly string _clientId = Guid.NewGuid().ToString("N");
        private const string MsgExecutionComplete = "execution_complete";
        private const string MsgExecutionError = "execution_error";

        public ComfyUiClient(ComfyUiEndpointConfig config)
        {
            _baseUrl = config.BaseUrl.TrimEnd('/');
            _http = new HttpRestClient();
            _ws = new WebSocketClient();
        }

        // ------------------------------------------------------------------ //
        //  Connection
        // ------------------------------------------------------------------ //

        public async Task ConnectAsync(CancellationToken cancellationToken)
        {
            // A ClientWebSocket is single-use: once it leaves the None/Open state
            // it cannot be reconnected. Recreate it whenever it's stale.
            if (_ws.State != WebSocketState.Open)
            {
                if (_ws.State != WebSocketState.None)
                {
                    _ws.Dispose();
                    _ws = new WebSocketClient();
                }

                var wsUri = new Uri(_baseUrl
                    .Replace("https://", "wss://", StringComparison.OrdinalIgnoreCase)
                    .Replace("http://", "ws://", StringComparison.OrdinalIgnoreCase)
                    + $"/ws?clientId={_clientId}");

                await _ws.ConnectAsync(wsUri, cancellationToken);
            }
        }

        // ------------------------------------------------------------------ //
        //  Submit
        // ------------------------------------------------------------------ //

        /// <summary>
        /// Submits an API-format workflow JSON. Returns the prompt_id assigned by ComfyUI.
        /// </summary>
        public async Task<string> SubmitAsync(string workflowApiJson, CancellationToken cancellationToken)
        {
            string payload = $"{{\"prompt\":{workflowApiJson},\"client_id\":\"{_clientId}\"}}";
            string response = await _http.PostAsync($"{_baseUrl}/prompt", payload, cancellationToken);

            using var doc = JsonDocument.Parse(response);
            return doc.RootElement.GetProperty("prompt_id").GetString()
                   ?? throw new Exception($"[{nameof(ComfyUiClient)}] /prompt response missing prompt_id. Body: {response}");
        }

        // ------------------------------------------------------------------ //
        //  Wait
        // ------------------------------------------------------------------ //

        /// <summary>
        /// Blocks until ComfyUI finishes the given prompt.
        /// Returns the output filename from the SaveImage node.
        /// </summary>
        public async Task<ComfyUiOutputFile> WaitAsync(string promptId, string nodeSaveImage, CancellationToken cancellationToken)
        {
            while (true)
            {
                string raw = await _ws.ReceiveAsync(cancellationToken);

                if (!raw.TrimStart().StartsWith('{'))
                    continue;

                JsonDocument doc;
                try { doc = JsonDocument.Parse(raw); } catch { continue; }

                using (doc)
                {
                    if (!doc.RootElement.TryGetProperty("type", out var typeProp)) continue;
                    string type = typeProp.GetString() ?? string.Empty;

                    if (!doc.RootElement.TryGetProperty("data", out var data)) continue;

                    bool isForThisPrompt = data.TryGetProperty("prompt_id", out var pid)
                                           && pid.GetString() == promptId;

                    if (type == MsgExecutionError && isForThisPrompt)
                    {
                        string msg = data.TryGetProperty("exception_message", out var err)
                            ? err.GetString() ?? "Unknown error"
                            : "Unknown error";
                        throw new Exception($"[{nameof(ComfyUiClient)}] ComfyUI error for [{promptId}]: {msg}");
                    }

                    if (type == MsgExecutionComplete && isForThisPrompt)
                        return await GetOutputFileAsync(promptId, nodeSaveImage, cancellationToken);

                    if (type == "executing" && isForThisPrompt
                        && data.TryGetProperty("node", out var node)
                        && node.ValueKind == JsonValueKind.Null)
                        return await GetOutputFileAsync(promptId, nodeSaveImage, cancellationToken);
                }
            }
        }

        public async Task<string> UploadImageAsync(byte[] imageBytes, string fileName, CancellationToken cancellationToken)
        {
            using var content = new MultipartFormDataContent();
            using var imageContent = new ByteArrayContent(imageBytes);
            imageContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/png");
            content.Add(imageContent, "image", fileName);
            content.Add(new StringContent("input"), "type");
            content.Add(new StringContent("true"), "overwrite");

            string responseBody = await _http.PostMultipartAsync($"{_baseUrl}/upload/image", content, cancellationToken);
            var json = JsonNode.Parse(responseBody);
            return json!["name"]!.GetValue<string>();
        }

        // ------------------------------------------------------------------ //
        //  Download
        // ------------------------------------------------------------------ //

        /// <summary>
        /// Downloads the final image bytes for a completed output file.
        /// </summary>
        public async Task<byte[]> DownloadAsync(ComfyUiOutputFile file, CancellationToken cancellationToken)
        {
            string url = $"{_baseUrl}/view" +
                         $"?filename={Uri.EscapeDataString(file.Filename)}" +
                         $"&subfolder={Uri.EscapeDataString(file.Subfolder)}" +
                         $"&type={file.Type}";

            return await _http.GetBytesAsync(url, cancellationToken);
        }

        // ------------------------------------------------------------------ //
        //  Lifecycle
        // ------------------------------------------------------------------ //

        public void Dispose()
        {
            _ws.Dispose();
            _http.Dispose();
        }

        // ------------------------------------------------------------------ //
        //  Private
        // ------------------------------------------------------------------ //

        private async Task<ComfyUiOutputFile> GetOutputFileAsync(string promptId, string nodeSaveImage, CancellationToken cancellationToken)
        {
            string response = await _http.GetAsync($"{_baseUrl}/history/{promptId}", cancellationToken);

            try
            {
                using var doc = JsonDocument.Parse(response);

                // Guard: prompt entry may not exist yet if history is polled too early
                if (!doc.RootElement.TryGetProperty(promptId, out var promptEntry))
                    throw new Exception($"[{nameof(ComfyUiClient)}] History response has no entry for prompt [{promptId}]. Body: {response}");

                if (!promptEntry.TryGetProperty("outputs", out var outputs))
                    throw new Exception($"[{nameof(ComfyUiClient)}] History entry for [{promptId}] has no 'outputs'. Body: {response}");

                if (!outputs.TryGetProperty(nodeSaveImage, out var saveNode))
                    throw new Exception($"[{nameof(ComfyUiClient)}] Outputs for [{promptId}] has no node [{nodeSaveImage}]. Available nodes: [{string.Join(", ", outputs.EnumerateObject().Select(p => p.Name))}]. Body: {response}");

                if (!saveNode.TryGetProperty("images", out var images) || images.GetArrayLength() == 0)
                    throw new Exception($"[{nameof(ComfyUiClient)}] Node [{nodeSaveImage}] for [{promptId}] has no images. Body: {response}");

                var image = images[0];

                return new ComfyUiOutputFile(
                    Filename: image.GetProperty("filename").GetString()!,
                    Subfolder: image.GetProperty("subfolder").GetString()!,
                    Type: image.GetProperty("type").GetString()!);
            } catch (Exception ex) when (ex is not InvalidOperationException)
            {
                LoggingManager.LogToFile("3f7a9c12-8b4e-4d5a-a1f6-2e9c0d7b5f83",
                    $"[{nameof(ComfyUiClient)}] Failed to parse history for prompt [{promptId}]. Raw response: {response}", ex);
                throw;
            }
        }

        // ------------------------------------------------------------------ //
        //  Health
        // ------------------------------------------------------------------ //

        /// <summary>
        /// Returns true when the ComfyUI REST API is reachable and responsive.
        /// Uses /system_stats — a lightweight endpoint that requires no auth.
        /// </summary>
        public async Task<bool> IsAvailableAsync(CancellationToken cancellationToken)
        {
            try
            {
                string response = await _http.GetAsync($"{_baseUrl}/system_stats", cancellationToken);
                // Any non-throwing response with a JSON body counts as healthy
                using var doc = JsonDocument.Parse(response);
                return true;
            } catch
            {
                return false;
            }
        }
    }

    public record ComfyUiOutputFile(string Filename, string Subfolder, string Type);
}
