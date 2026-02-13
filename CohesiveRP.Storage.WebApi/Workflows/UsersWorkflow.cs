using CohesiveRP.Common.WebApi;
using CohesiveRP.Storage.Common;
using CohesiveRP.Storage.WebApi.DataAccessLayer;
using CohesiveRP.Storage.WebApi.ResponseDtos;

namespace CohesiveRP.Storage.WebApi.Workflows
{
    /// <summary>
    /// Workflow around Users storage.
    /// </summary>
    public class UsersWorkflow : IUsersWorkflow
    {
        private IUsersDal usersDal;

        public UsersWorkflow(IUsersDal usersDal)
        {
            this.usersDal = usersDal;
        }

        public async Task<IWebApiReponseDto> CreateNewUser(UserCreationRequestDto userCreationRequestDto)
        {
            var existingUser = await usersDal.GetUserByUsernameAsync(userCreationRequestDto.Username);

            if (existingUser == null)
            {
                throw new StorageException("f7033238-d192-4d1c-95eb-1b13c387f8e2", $"Couldn't create user with Username [{userCreationRequestDto.Username}] as it already exists.");
            }

            // TODO: remove this temp code
            return new UserCreationResponseDto();
        }
    }
}
