using CohesiveRP.Common.WebApi;
using CohesiveRP.Storage.DataAccessLayer.Users;
using CohesiveRP.Storage.WebApi.RequestDtos.Chat;

namespace CohesiveRP.Storage.WebApi.Workflows.Chats
{
    public class GetChatWorkflow : IGetChatWorkflow
    {
        private IChatsDal chatsDal;

        public GetChatWorkflow(IChatsDal chatsDal)
        {
            this.chatsDal = chatsDal;
        }

        public string GetById(string chatId)
        {
            throw new NotImplementedException();
        }

        public Task<IWebApiReponseDto> GetChatByIdAsync(GetChatByIdRequestDto getChatByIdRequestDto)
        {
            return null;
        }

        //public async Task<IWebApiReponseDto> CreateNewUser(UserCreationRequestDto userCreationRequestDto)
        //{
        //    var existingUser = await usersDal.GetUserByUsernameAsync(userCreationRequestDto.Username);

        //    if (existingUser == null)
        //    {
        //        throw new StorageException("f7033238-d192-4d1c-95eb-1b13c387f8e2", $"Couldn't create user with Username [{userCreationRequestDto.Username}] as it already exists.");
        //    }

        //    // TODO: remove this temp code
        //    return new UserCreationResponseDto();
        //}
    }
}

