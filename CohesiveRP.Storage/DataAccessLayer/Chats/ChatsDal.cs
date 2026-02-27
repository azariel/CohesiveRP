using System.Text.Json;
using CohesiveRP.Common.Diagnostics;
using CohesiveRP.Storage.Common;
using CohesiveRP.Storage.Users;
using Microsoft.EntityFrameworkCore;

namespace CohesiveRP.Storage.DataAccessLayer.Users
{
    /// <summary>
    /// DataAccessLayer around Chats.
    /// </summary>
    public class ChatsDal : StorageDal, IChatsDal, IDisposable
    {
        private StorageDbContext storageDbContext;

        public ChatsDal(JsonSerializerOptions jsonSerializerOptions) : base(jsonSerializerOptions)
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

        public async Task<ChatDbModel> GetChatByIdAsync(string id)
        {
            try
            {
                storageDbContext.Chats.Load();
                return storageDbContext.Chats.FirstOrDefault(w => w.ChatId == id);
            } catch (Exception ex)
            {
                LoggingManager.LogToFile("ed1b481f-463b-4854-acac-222965ef3601", $"Error when querying Db on table Chat.", ex);
                return null;
            }
        }
    }
}
