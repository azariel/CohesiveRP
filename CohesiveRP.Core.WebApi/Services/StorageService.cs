using CohesiveRP.Common.Diagnostics;
using CohesiveRP.Common.Exceptions;
using CohesiveRP.Common.HttpClient;
using CohesiveRP.Common.Serialization;
using CohesiveRP.Core.WebApi.ResponseDtos.Storage;

namespace CohesiveRP.Core.WebApi.Services
{
    /// <summary>
    /// Expose and handle operations to the Storage service.
    /// </summary>
    public class StorageService : IStorageService, IDisposable
    {
        private const string STORAGE_BASE_URL = "https://127.0.0.1:7298";// TODO: make configurable
        private const string CHAT_CONTROLLER_BASE_URL = "api/chats";// TODO: make configurable
        HttpRestClient httpRestClient = new();

        public void Dispose()
        {
            httpRestClient?.Dispose();
        }

        public async Task<ChatResponseDto> GetChatAsync(string chatId)
        {
            string url = $"{STORAGE_BASE_URL}/{CHAT_CONTROLLER_BASE_URL}/{chatId}";
            string response = null;
            try
            {
                response = await httpRestClient.GetAsync(url);
            } catch (Exception ex)
            {
                string message = $"Couldn't get Chat with id [{chatId}] from Storage WebApi.";
                LoggingManager.LogToFile("6141bd7a-5060-4ada-82c8-8eb181159051", message, ex);
                throw new ApiException(System.Net.HttpStatusCode.InternalServerError, message, ex);
            }

            return JsonCommonSerializer.DeserializeFromString<ChatResponseDto>(response);
        }
    }
}
