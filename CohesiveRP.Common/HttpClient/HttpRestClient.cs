using System.Net.Security;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using CohesiveRP.Common.Diagnostics;
using CohesiveRP.Common.Exceptions;

namespace CohesiveRP.Common.HttpClient
{
    // TODO: implement async
    public class HttpRestClient : IDisposable
    {
        private const bool IGNORE_CERTIFICATE_ERRORS = true; // TODO: make this configurable. This is useful for development and testing environments, but should be false in production...although we may accept self-sign certs in production as well, so we should just make this configurable?
        private System.Net.Http.HttpClient httpClient;

        public HttpRestClient()
        {
            var clientHandler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (message, cert, chain, sslPolicyErrors) => sslPolicyErrors == SslPolicyErrors.None || IGNORE_CERTIFICATE_ERRORS
            };

            httpClient = new System.Net.Http.HttpClient(clientHandler)
            {
                Timeout = new TimeSpan(0, 1, 0, 0)
            };

            // Add default headers
            httpClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
        }

        // ********************************************************************
        //                            Public
        // ********************************************************************
        public void Dispose()
        {
            httpClient?.Dispose();
        }

        /// <summary>
        /// Asynchronous get
        /// </summary>
        public async Task<string> GetAsync(string url, CancellationToken cancellationToken)
        {
            HttpResponseMessage response = null;

            try
            {
                response = await httpClient.GetAsync(url, cancellationToken);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    return responseContent;
                }
            } catch (OperationCanceledException operationCanceled)
            {
                LoggingManager.LogToFile("7639240a-77e5-4cee-aa43-c97a29736cb5", $"[{nameof(HttpRestClient)}.{nameof(PostAsync)}] failed due to cancellationToken reaching its time limit.");
                throw;
            } catch (AggregateException aggregateException) when (aggregateException.InnerExceptions.Any(a => a.GetType() == typeof(HttpRequestException)))
            {
                LoggingManager.LogToFile("270b1291-8da6-4f20-8f73-30464927fe6e", $"[{nameof(HttpRestClient)}.{nameof(GetAsync)}] failed.", aggregateException);
                throw;
            }
            //catch(TaskCanceledException)
            catch (Exception ex)
            {
                LoggingManager.LogToFile("16f4baea-ecba-4631-a4a0-65a2653cf63d", $"Unhandled exception. [{nameof(HttpRestClient)}.{nameof(GetAsync)}] failed.", ex);
                throw;
            }

            throw new ApiException(response.StatusCode, $"[{nameof(HttpRestClient)}.{nameof(GetAsync)}] request failed when querying [{url}]. Error: [{await response.Content.ReadAsStringAsync()}].");// TODO: wrap error
        }

        public async Task<string> PostAsync(string url, string jsonPayload, CancellationToken cancellationToken)
        {
            try
            {
                HttpResponseMessage response = await httpClient.PostAsync(url, new StringContent(jsonPayload, Encoding.UTF8, "application/json"), cancellationToken);
                string responseMessageContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                    return responseMessageContent;

                string errorMessage = $"Couldn't post async to dependent service. Url = [{url}]. HTTP Response Code = [{response.StatusCode}]. Response Message = [{responseMessageContent}].";
                LoggingManager.LogToFile("b423f45b-7032-4eb4-b674-6296a95783db", errorMessage);
                throw new Exception(errorMessage);
            } catch (OperationCanceledException operationCanceled)
            {
                LoggingManager.LogToFile("1d7350e0-f550-4209-8687-b6f935ebb210", $"[{nameof(HttpRestClient)}.{nameof(PostAsync)}] failed due to cancellationToken reaching its time limit.");
                throw;
            } catch (AggregateException aggregateException) when (aggregateException.InnerExceptions.Any(a => a.GetType() == typeof(HttpRequestException)))
            {
                LoggingManager.LogToFile("157c8016-86d0-41bf-8649-3a176bb6e3a8", $"[{nameof(HttpRestClient)}.{nameof(PostAsync)}] failed.", aggregateException);
                throw;
            } catch (Exception ex)
            {
                LoggingManager.LogToFile("74f24fa2-c994-40ec-9703-007bd0739cef", $"Unhandled exception. [{nameof(HttpRestClient)}.{nameof(GetAsync)}] failed.", ex);
                throw;
            }

            throw new Exception($"[{nameof(HttpRestClient)}.{nameof(PostAsync)}] Unhandled exception when querying [{url}].");// TODO: wrap error
        }

        public async IAsyncEnumerable<string> PostStreamAsync(string url, string jsonPayload, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            HttpResponseMessage response;

            try
            {
                var request = new HttpRequestMessage(HttpMethod.Post, url)
                {
                    Content = new StringContent(jsonPayload, Encoding.UTF8, "application/json")
                };

                response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
            } catch (OperationCanceledException)
            {
                LoggingManager.LogToFile("0ea3ebe0-e24a-4948-8d80-3718e6c28236", $"[{nameof(HttpRestClient)}.{nameof(PostStreamAsync)}] cancelled.");
                throw;
            } catch (Exception ex)
            {
                LoggingManager.LogToFile("d7687757-7197-4097-a04d-43c8996b00f7", $"[{nameof(HttpRestClient)}.{nameof(PostStreamAsync)}] request failed.", ex);
                throw;
            }

            if (!response.IsSuccessStatusCode)
            {
                string body = await response.Content.ReadAsStringAsync();
                string error = $"Stream request failed. Url=[{url}] Status=[{response.StatusCode}] Body=[{body}]";
                LoggingManager.LogToFile("6336ca57-4b9d-41e7-aa6d-5cacddd248c0", error);
                throw new Exception(error);
            }

            using HttpResponseMessage _ = response;
            await using Stream stream = await response.Content.ReadAsStreamAsync();
            using var reader = new StreamReader(stream);

            while (true)
            {
                string line;

                try
                {
                    line = await reader.ReadLineAsync(cancellationToken);
                } catch (OperationCanceledException)
                {
                    LoggingManager.LogToFile("6d55d521-3adc-42b2-a76a-6f67e73103d3", $"[{nameof(HttpRestClient)}.{nameof(PostStreamAsync)}] cancelled while reading stream.");
                    throw;
                } catch (Exception ex)
                {
                    LoggingManager.LogToFile("73b9f140-1f05-41de-be68-0c58428393dc", $"[{nameof(HttpRestClient)}.{nameof(PostStreamAsync)}] failed while reading stream.", ex);
                    throw;
                }

                if (line is null)
                    break;

                if(line.Contains($"data: [DONE]"))
                    break;

                yield return line;
            }
        }

        public async Task<string> PatchAsync(string url, string jsonPayload, CancellationToken cancellationToken)
        {
            try
            {
                HttpResponseMessage response = await httpClient.PatchAsync(url, new StringContent(jsonPayload, Encoding.UTF8, "application/json"), cancellationToken);

                if (response.IsSuccessStatusCode)
                    return await response.Content.ReadAsStringAsync();
            } catch (OperationCanceledException operationCanceled)
            {
                LoggingManager.LogToFile("1850e585-1ee8-41b3-a76b-fa8b125ebc3c", $"[{nameof(HttpRestClient)}.{nameof(PostAsync)}] failed due to cancellationToken reaching its time limit.");
                throw;
            } catch (AggregateException aggregateException) when (aggregateException.InnerExceptions.Any(a => a.GetType() == typeof(HttpRequestException)))
            {
                LoggingManager.LogToFile("6b477a34-a5d7-47e8-86f7-4fb8caa0f6ff", $"[{nameof(HttpRestClient)}.{nameof(PatchAsync)}] failed.", aggregateException);
                throw;
            } catch (Exception ex)
            {
                LoggingManager.LogToFile("71601440-192c-4479-bbab-4a67e292338d", $"Unhandled exception. [{nameof(HttpRestClient)}.{nameof(PatchAsync)}] failed.", ex);
                throw;
            }

            throw new Exception($"[{nameof(HttpRestClient)}.{nameof(PatchAsync)}] Unhandled exception when querying [{url}].");// TODO: wrap error
        }
    }
}
