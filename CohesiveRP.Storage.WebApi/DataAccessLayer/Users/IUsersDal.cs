using CohesiveRP.Storage.RequestDtos;
using CohesiveRP.Storage.Users;

namespace CohesiveRP.Storage.WebApi.DataAccessLayer
{
    public interface IUsersDal
    {
        Task<UserDbModel> GetUserByUsernameAsync(string lowerInvariantUsername);
        Task<UserDbModel> CreateNewUserAsync(CreateUserRequestDto userToCreate);
    }
}
