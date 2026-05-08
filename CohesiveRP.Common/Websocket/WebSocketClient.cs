using System.Net.WebSockets;
using System.Runtime.CompilerServices;
using System.Text;
using CohesiveRP.Common.Diagnostics;
using CohesiveRP.Common.Exceptions;

namespace CohesiveRP.Common.Websocket
{
    /// <summary>
    /// Abstracts a managed WebSocket connection, mirroring the HttpRestClient contract.
    /// Supports fire-and-forget sends, request/response patterns, and async streaming.
    /// </summary>
    public class WebSocketClient : IDisposable
    {
        // ------------------------------------------------------------------ //
        //  Configuration
        // ------------------------------------------------------------------ //

        private const int RECEIVE_BUFFER_SIZE_BYTES = 4096;
        private const string DONE_SENTINEL = "[DONE]";

        // ------------------------------------------------------------------ //
        //  State
        // ------------------------------------------------------------------ //

        private readonly ClientWebSocket _socket;
        private readonly SemaphoreSlim _sendLock = new(1, 1);   // WebSocket sends are NOT thread-safe
        private bool _disposed;

        // ------------------------------------------------------------------ //
        //  Construction / connection
        // ------------------------------------------------------------------ //

        public WebSocketClient()
        {
            _socket = new ClientWebSocket();

            // Mirror the HttpRestClient "accept application/json" default header
            _socket.Options.SetRequestHeader("Accept", "application/json");
        }

        /// <summary>
        /// Opens the WebSocket connection to <paramref name="uri"/>.
        /// Call this once before any send/receive operation.
        /// </summary>
        public async Task ConnectAsync(Uri uri, CancellationToken cancellationToken)
        {
            ThrowIfDisposed();

            try
            {
                await _socket.ConnectAsync(uri, cancellationToken);
            } catch (OperationCanceledException)
            {
                LoggingManager.LogToFile(
                    "150cc3a3-1112-443f-afd2-402846240166",
                    $"[{nameof(WebSocketClient)}.{nameof(ConnectAsync)}] cancelled while connecting to [{uri}].");
                throw;
            } catch (WebSocketException ex)
            {
                LoggingManager.LogToFile(
                    "dd470db0-27f0-442e-93c3-703a72b174ea",
                    $"[{nameof(WebSocketClient)}.{nameof(ConnectAsync)}] WebSocket error connecting to [{uri}].", ex);
                throw;
            } catch (Exception ex)
            {
                LoggingManager.LogToFile(
                    "4a0ad618-29cf-44c8-b073-a52aa13733ae",
                    $"[{nameof(WebSocketClient)}.{nameof(ConnectAsync)}] Unhandled exception connecting to [{uri}].", ex);
                throw;
            }
        }

        // ------------------------------------------------------------------ //
        //  Public — Send
        // ------------------------------------------------------------------ //

        /// <summary>
        /// Serialises <paramref name="payload"/> as JSON and sends it over the socket.
        /// Thread-safe: concurrent callers are serialised through an internal lock.
        /// </summary>
        public async Task SendAsync(string jsonPayload, CancellationToken cancellationToken)
        {
            ThrowIfDisposed();
            EnsureConnected(nameof(SendAsync));

            await _sendLock.WaitAsync(cancellationToken);
            try
            {
                byte[] bytes = Encoding.UTF8.GetBytes(jsonPayload);
                var segment = new ArraySegment<byte>(bytes);

                await _socket.SendAsync(segment, WebSocketMessageType.Text, endOfMessage: true, cancellationToken);
            } catch (OperationCanceledException)
            {
                LoggingManager.LogToFile(
                    "1fd9fdfd-b0a5-449f-98d2-2fadc4930181",
                    $"[{nameof(WebSocketClient)}.{nameof(SendAsync)}] cancelled.");
                throw;
            } catch (WebSocketException ex)
            {
                LoggingManager.LogToFile(
                    "20959ef5-01ed-4c76-a275-8d011fd94320",
                    $"[{nameof(WebSocketClient)}.{nameof(SendAsync)}] WebSocket error during send.", ex);
                throw;
            } catch (Exception ex)
            {
                LoggingManager.LogToFile(
                    "fb8dac2e-5c53-41dd-9108-186a40b016de",
                    $"[{nameof(WebSocketClient)}.{nameof(SendAsync)}] Unhandled exception during send.", ex);
                throw;
            } finally
            {
                _sendLock.Release();
            }
        }

        // ------------------------------------------------------------------ //
        //  Public — Receive (single message)
        // ------------------------------------------------------------------ //

        /// <summary>
        /// Reads a single complete message from the socket and returns it as a string.
        /// Accumulates frames until <see cref="WebSocketReceiveResult.EndOfMessage"/> is true.
        /// </summary>
        public async Task<string> ReceiveAsync(CancellationToken cancellationToken)
        {
            ThrowIfDisposed();
            EnsureConnected(nameof(ReceiveAsync));

            var buffer = new byte[RECEIVE_BUFFER_SIZE_BYTES];
            var sb = new StringBuilder();

            try
            {
                WebSocketReceiveResult result;

                do
                {
                    result = await _socket.ReceiveAsync(new ArraySegment<byte>(buffer), cancellationToken);

                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        await CloseGracefullyAsync(cancellationToken);
                        throw new ApiException(
                            System.Net.HttpStatusCode.Gone,
                            $"[{nameof(WebSocketClient)}.{nameof(ReceiveAsync)}] Remote closed the connection: [{result.CloseStatusDescription}].");
                    }

                    sb.Append(Encoding.UTF8.GetString(buffer, 0, result.Count));
                }
                while (!result.EndOfMessage);

                return sb.ToString();
            } catch (OperationCanceledException)
            {
                LoggingManager.LogToFile(
                    "99660da5-2813-4133-9477-7f6b972d8dd8",
                    $"[{nameof(WebSocketClient)}.{nameof(ReceiveAsync)}] cancelled while receiving.");
                throw;
            } catch (WebSocketException ex)
            {
                LoggingManager.LogToFile(
                    "dbcf8f1c-29be-4e10-9f49-881ee5a65548",
                    $"[{nameof(WebSocketClient)}.{nameof(ReceiveAsync)}] WebSocket error during receive.", ex);
                throw;
            } catch (ApiException)
            {
                throw;  // already logged above
            } catch (Exception ex)
            {
                LoggingManager.LogToFile(
                    "c391b1e0-7ceb-4c0a-a063-45fc4772f367",
                    $"[{nameof(WebSocketClient)}.{nameof(ReceiveAsync)}] Unhandled exception during receive.", ex);
                throw;
            }
        }

        // ------------------------------------------------------------------ //
        //  Public — Send + Receive (request / response)
        // ------------------------------------------------------------------ //

        /// <summary>
        /// Convenience method: sends <paramref name="jsonPayload"/> then waits
        /// for a single response message and returns it.
        /// Mirrors <see cref="HttpRestClient.PostAsync"/>.
        /// </summary>
        public async Task<string> SendAndReceiveAsync(string jsonPayload, CancellationToken cancellationToken)
        {
            await SendAsync(jsonPayload, cancellationToken);
            return await ReceiveAsync(cancellationToken);
        }

        // ------------------------------------------------------------------ //
        //  Public — Streaming (mirrors PostStreamAsync)
        // ------------------------------------------------------------------ //

        /// <summary>
        /// Sends <paramref name="jsonPayload"/> then yields each incoming message
        /// until the server sends a <c>[DONE]</c> sentinel or closes the socket.
        /// Mirrors <see cref="HttpRestClient.PostStreamAsync"/>.
        /// </summary>
        public async IAsyncEnumerable<string> SendAndStreamAsync(
            string jsonPayload,
            [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            ThrowIfDisposed();

            await SendAsync(jsonPayload, cancellationToken);

            while (true)
            {
                string message;

                try
                {
                    message = await ReceiveAsync(cancellationToken);
                } catch (OperationCanceledException)
                {
                    LoggingManager.LogToFile(
                        "cfb6e729-c2f3-4373-9c85-bf6a644d6acf",
                        $"[{nameof(WebSocketClient)}.{nameof(SendAndStreamAsync)}] cancelled while streaming.");
                    throw;
                } catch (ApiException)
                {
                    // Remote closed — treat as end-of-stream
                    yield break;
                } catch (Exception ex)
                {
                    LoggingManager.LogToFile(
                        "37b9c410-cb97-43a7-bf31-d0244d60d322",
                        $"[{nameof(WebSocketClient)}.{nameof(SendAndStreamAsync)}] Unhandled exception while streaming.", ex);
                    throw;
                }

                if (message.Contains(DONE_SENTINEL))
                    yield break;

                yield return message;
            }
        }

        // ------------------------------------------------------------------ //
        //  Public — Lifecycle
        // ------------------------------------------------------------------ //

        /// <summary>
        /// Current state of the underlying socket.
        /// Callers can gate on <see cref="WebSocketState.Open"/> before operating.
        /// </summary>
        public WebSocketState State => _socket.State;

        /// <summary>
        /// Sends a Close handshake and waits for the server to acknowledge.
        /// Safe to call even if the socket is already closed.
        /// </summary>
        public async Task CloseAsync(CancellationToken cancellationToken)
        {
            if (_socket.State is not (WebSocketState.Open or WebSocketState.CloseReceived))
                return;

            try
            {
                await _socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Client closing", cancellationToken);
            } catch (Exception ex)
            {
                LoggingManager.LogToFile(
                    "293596c6-c2eb-4e64-b5bd-fd8ced844630",
                    $"[{nameof(WebSocketClient)}.{nameof(CloseAsync)}] Exception during graceful close.", ex);
                // Don't rethrow — best-effort close
            }
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            _sendLock.Dispose();
            _socket.Dispose();
        }

        // ------------------------------------------------------------------ //
        //  Private helpers
        // ------------------------------------------------------------------ //

        /// <summary>
        /// One-way close output when the remote initiates the close handshake.
        /// </summary>
        private async Task CloseGracefullyAsync(CancellationToken cancellationToken)
        {
            try
            {
                if (_socket.State == WebSocketState.CloseReceived)
                    await _socket.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, string.Empty, cancellationToken);
            } catch (Exception ex)
            {
                LoggingManager.LogToFile(
                    "b1ae2471-f60c-4343-b455-f8308ebaef22",
                    $"[{nameof(WebSocketClient)}.{nameof(CloseGracefullyAsync)}] Exception during output close.", ex);
            }
        }

        private void EnsureConnected(string callerName)
        {
            if (_socket.State != WebSocketState.Open)
                throw new InvalidOperationException(
                    $"[{nameof(WebSocketClient)}.{callerName}] Socket is not connected (State = {_socket.State}). Call {nameof(ConnectAsync)} first.");
        }

        private void ThrowIfDisposed()
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(WebSocketClient));
        }
    }
}
