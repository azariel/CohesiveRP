using CohesiveRP.Storage.DataAccessLayer.Users.Requests;
using CohesiveRP.Storage.Users;

namespace CohesiveRP.Storage.DataAccessLayer.Users
{
    public interface IUsersDal
    {
        Task<UserDbModel> GetUserByUsernameAsync(string lowerInvariantUsername);
        Task<UserDbModel> CreateNewUserAsync(CreateDbUserRequest userToCreate);
    }
}
