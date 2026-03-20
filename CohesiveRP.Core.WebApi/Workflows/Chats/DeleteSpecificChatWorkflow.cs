using CohesiveRP.Common.Exceptions;
using CohesiveRP.Common.WebApi;
using CohesiveRP.Core.Services;
using CohesiveRP.Core.WebApi.ResponseDtos;
using CohesiveRP.Core.WebApi.Workflows.Chats.Abstractions;
using CohesiveRP.Storage.DataAccessLayer.Chats;

namespace CohesiveRP.Core.WebApi.Workflows.Chats
{
    public class DeleteSpecificChatWorkflow : IDeleteSpecificChatWorkflow
    {
        private IStorageService storageService;

        public DeleteSpecificChatWorkflow(IStorageService storageService)
        {
            this.storageService = storageService;
        }

        public async Task<IWebApiResponseDto> DeleteChatById(string chatId)
        {
            ChatDbModel chat = await storageService.GetChatAsync(chatId);

            if (chat == null)
            {
                return new WebApiException
                {
                    HttpResultCode = System.Net.HttpStatusCode.NotFound,
                    Message = $"Chat with id {chatId} was not found. Aborting deletion."
                };
            }

            // TODO: Delete lorebooks that exists solely for this chat (not global lorebooks that are selectable by any, delete only the ones that are embedded by this chat)
            // TODO: Delete characters that exists solely for this chat (not global characters that are selectable by any, delete only the ones that are embedded by this chat)
            // TODO: Delete personas that exists solely for this chat (not global personas that are selectable by any, delete only the ones that are embedded by this chat)
            
            // delete all backgroundqueries tied to this chat
            bool deleteBackgroundQueries = await storageService.DeleteBackgroundQueriesByChatIdAsync(chatId);

            // delete sceneTracker tied to this chat
            bool deleteSceneTracker = await storageService.DeleteSceneTrackerAsync(chatId);

            // delete summaries tied to this chat
            bool deleteSummaries = await storageService.DeleteSummaryFromChatIdAsync(chatId);
            
            // delete all cold and hot messages tied to this chat
            bool deleteColdMessages = await storageService.DeleteColdMessagesAsync(chatId);
            bool deleteHotMessages = await storageService.DeleteHotMessagesAsync(chatId);

            // delete the actual chat
            bool deleteChatResult = await storageService.DeleteChatAsync(chatId);
            if (!deleteChatResult)
            {
                return new WebApiException
                {
                    HttpResultCode = System.Net.HttpStatusCode.InternalServerError,
                    Message = $"Couldn't delete Chat with Id [{chatId}]. Chat deletion is in half-deletion state, please try again."
                };
            }

            BasicResponseDto responseDto = new()
            {
                HttpResultCode = System.Net.HttpStatusCode.OK,
            };

            return responseDto;
        }
    }
}
