using System.Text.Json;
using CohesiveRP.Storage.Common;
using CohesiveRP.Storage.DataAccessLayer.Users.Requests;
using CohesiveRP.Storage.Users;
using Microsoft.EntityFrameworkCore;

namespace CohesiveRP.Storage.DataAccessLayer.Users
{
    /// <summary>
    /// DataAccessLayer around Users.
    /// </summary>
    public class UsersDal : StorageDal, IUsersDal, IDisposable
    {
        private StorageDbContext storageDbContext;

        public UsersDal(JsonSerializerOptions jsonSerializerOptions) : base(jsonSerializerOptions)
        {
            storageDbContext = new StorageDbContext();
            storageDbContext.Users.Load();
        }

        // ********************************************************************
        //                            Public
        // ********************************************************************
        public void Dispose()
        {
            storageDbContext?.Dispose();
            GC.SuppressFinalize(this);
        }

        public async Task<UserDbModel> CreateNewUserAsync(CreateDbUserRequest userToCreate)
        {
            // Create the User
            var newUser = new UserDbModel()
            {
                UserName = userToCreate.UserName,
                Password = userToCreate.Password,
                Type = UserDbModel.UserType.User
            };

            var addedUser = await storageDbContext.Users.AddAsync(newUser);
            await storageDbContext.SaveChangesAsync();
            return addedUser.Entity;
        }

        public async Task<UserDbModel> GetUserByUsernameAsync(string userName)
        {
            string lowerInvariantUsername = userName.ToLowerInvariant();
            return await storageDbContext.Users.Where(w => w.UserName.ToLowerInvariant().Equals(lowerInvariantUsername)).FirstOrDefaultAsync();
        }
    }
}
