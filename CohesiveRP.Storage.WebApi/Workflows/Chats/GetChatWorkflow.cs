using CohesiveRP.Common.Exceptions;
using CohesiveRP.Common.WebApi;
using CohesiveRP.Storage.DataAccessLayer.Users;
using CohesiveRP.Storage.WebApi.RequestDtos.Chat;
using CohesiveRP.Storage.WebApi.ResponseDtos;

namespace CohesiveRP.Storage.WebApi.Workflows.Chats
{
    public class GetChatWorkflow : IGetChatWorkflow
    {
        private IChatsDal chatsDal;

        public GetChatWorkflow(IChatsDal chatsDal)
        {
            this.chatsDal = chatsDal;
        }

        public async Task<IWebApiReponseDto> GetChatByIdAsync(GetChatByIdRequestDto getChatByIdRequestDto)
        {
            var chat = await chatsDal.GetChatByIdAsync(getChatByIdRequestDto.ChatId);

            if (chat == null)
            {
                return new WebApiException
                {
                    HttpResultCode = System.Net.HttpStatusCode.NotFound,
                    Message = $"Chat with id {getChatByIdRequestDto.ChatId} was not found."
                };
            }

            // Create the model that will be returned to the client based on the data access layer model
            var responseModel = new GetChatResponseDto
            {
                ChatId = chat.ChatId,
            };

            return responseModel;
        }
    }
}

