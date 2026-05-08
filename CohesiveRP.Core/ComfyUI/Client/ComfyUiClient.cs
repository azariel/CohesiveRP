using System.Net.WebSockets;
using System.Text.Json;
using CohesiveRP.Common.Configuration;
using CohesiveRP.Common.Diagnostics;
using CohesiveRP.Common.HttpClient;
using CohesiveRP.Common.Websocket;

namespace CohesiveRP.Core.ComfyUI.Client
{
    public class ComfyUiClient : IComfyUiClient, IDisposable
    {
        private readonly HttpRestClient _http;
        private readonly WebSocketClient _ws;
        private readonly string _baseUrl;
        private readonly string _clientId = Guid.NewGuid().ToString("N");

        private const string NodeSaveImage = "54";
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
            if (_ws.State == WebSocketState.Open)
                return;

            var wsUri = new Uri(_baseUrl
                .Replace("https://", "wss://", StringComparison.OrdinalIgnoreCase)
                .Replace("http://", "ws://", StringComparison.OrdinalIgnoreCase)
                + $"/ws?clientId={_clientId}");

            await _ws.ConnectAsync(wsUri, cancellationToken);
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
        public async Task<ComfyUiOutputFile> WaitAsync(string promptId, CancellationToken cancellationToken)
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
                        return await GetOutputFileAsync(promptId, cancellationToken);

                    if (type == "executing" && isForThisPrompt
                        && data.TryGetProperty("node", out var node)
                        && node.ValueKind == JsonValueKind.Null)
                        return await GetOutputFileAsync(promptId, cancellationToken);
                }
            }
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

        private async Task<ComfyUiOutputFile> GetOutputFileAsync(string promptId, CancellationToken cancellationToken)
        {
            string response = await _http.GetAsync($"{_baseUrl}/history/{promptId}", cancellationToken);

            using var doc = JsonDocument.Parse(response);
            var image = doc.RootElement
                .GetProperty(promptId)
                .GetProperty("outputs")
                .GetProperty(NodeSaveImage)
                .GetProperty("images")[0];

            return new ComfyUiOutputFile(
                Filename: image.GetProperty("filename").GetString()!,
                Subfolder: image.GetProperty("subfolder").GetString()!,
                Type: image.GetProperty("type").GetString()!);
        }
    }

    public record ComfyUiOutputFile(string Filename, string Subfolder, string Type);
}
