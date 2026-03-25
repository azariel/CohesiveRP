using System.Text.Json;
using CohesiveRP.Common.Diagnostics;
using CohesiveRP.Common.Serialization;
using CohesiveRP.Storage.Common;
using CohesiveRP.Storage.DataAccessLayer.Chats;
using Microsoft.EntityFrameworkCore;

namespace CohesiveRP.Storage.DataAccessLayer.Users
{
    /// <summary>
    /// DataAccessLayer around CharacterSheetInstances.
    /// </summary>
    public class CharacterSheetInstancesDal : StorageDal, ICharacterSheetInstancesDal
    {
        private readonly IDbContextFactory<StorageDbContext> contextFactory;

        public CharacterSheetInstancesDal(JsonSerializerOptions jsonSerializerOptions, IDbContextFactory<StorageDbContext> contextFactory) : base(jsonSerializerOptions)
        {
            this.contextFactory = contextFactory;

            using var dbContext = contextFactory.CreateDbContext();
            dbContext.Database.EnsureCreated();
        }

        // ********************************************************************
        //                            Public
        // ********************************************************************
        public async Task<CharacterSheetInstancesDbModel[]> GetCharacterSheetInstancesAsync()
        {
            try
            {
                using var dbContext = await contextFactory.CreateDbContextAsync();
                return dbContext.CharacterSheetInstances.ToArray();
            } catch (Exception ex)
            {
                LoggingManager.LogToFile("975a683e-452f-4c08-a804-a9d7abcddfd0", $"Error when querying Db on table CharacterSheetInstances.", ex);
                return null;
            }
        }

        public async Task<CharacterSheetInstancesDbModel[]> GetCharacterSheetInstancesByFuncAsync(Func<CharacterSheetInstancesDbModel, bool> func)
        {
            if (func == null)
            {
                return null;
            }

            try
            {
                using var dbContext = await contextFactory.CreateDbContextAsync();
                var result = await dbContext.CharacterSheetInstances.AsAsyncEnumerable().Where(func.Invoke).ToArrayAsync();
                return result;
            } catch (Exception ex)
            {
                LoggingManager.LogToFile("e7490b8e-4933-4cba-ba8c-1d31b527d246", $"Error when querying Db on table CharacterSheetInstances.", ex);
                return null;
            }
        }

        public async Task<CharacterSheetInstancesDbModel> GetCharacterSheetsInstanceByChatIdAsync(string chatId)
        {
            var characters = await GetCharacterSheetInstancesByFuncAsync(f => f.ChatId == chatId);
            return characters?.FirstOrDefault();
        }

        public async Task<CharacterSheetInstancesDbModel> AddCharacterSheetsInstanceAsync(CharacterSheetInstancesDbModel dbModel)
        {
            try
            {
                using var dbContext = await contextFactory.CreateDbContextAsync();

                // Override system fields
                dbModel.CreatedAtUtc = DateTime.UtcNow;
                dbModel.LastActivityAtUtc = DateTime.UtcNow;

                var result = await dbContext.CharacterSheetInstances.AddAsync(dbModel);

                if (result.State != EntityState.Added)
                {
                    LoggingManager.LogToFile("76b81edf-df4b-4356-a760-e4e55e5f7787", $"Error when querying Db on table CharacterSheetInstances. State was [{result.State}]. dbModel: [{JsonCommonSerializer.SerializeToString(dbModel)}].");
                    return null;
                }

                await dbContext.SaveChangesAsync();
                return result.Entity;
            } catch (Exception ex)
            {
                LoggingManager.LogToFile("05e20f72-2096-490a-b7a4-6a998cc8ac18", $"Error when querying Db on table CharacterSheetInstances.", ex);
                return null;
            }
        }

        public async Task<bool> UpdateCharacterSheetsInstanceAsync(CharacterSheetInstancesDbModel dbModel)
        {
            try
            {
                using var dbContext = await contextFactory.CreateDbContextAsync();
                var character = dbContext.CharacterSheetInstances.FirstOrDefault(w => w.ChatId == dbModel.ChatId);

                if (character == null)
                {
                    LoggingManager.LogToFile("71ef043b-e042-4685-86ad-f9f126d47380", $"CharacterSheetInstances tethered to chatId [{dbModel.ChatId}] to update wasn't found in storage.");
                    return false;
                }

                // System fields
                character.LastActivityAtUtc = DateTime.UtcNow;

                // Update only the overridable fields
                character.CharacterSheetInstances.Clear();
                character.CharacterSheetInstances.AddRange(dbModel.CharacterSheetInstances);

                var result = dbContext.CharacterSheetInstances.Update(character);
                if (result.State != EntityState.Modified)
                {
                    LoggingManager.LogToFile("3206db23-41d4-4556-ae1b-ce869d04d434", $"Error when updating a CharacterSheetInstances. State was [{result.State}]. Result: [{JsonCommonSerializer.SerializeToString(result)}]. dbModel: [{JsonCommonSerializer.SerializeToString(dbModel)}].");
                    return false;
                }

                await dbContext.SaveChangesAsync();
                return true;
            } catch (Exception ex)
            {
                LoggingManager.LogToFile("5f9f529b-c4a0-49a2-b9e3-59ab94e8e1ed", $"Error when querying pending queries on table CharacterSheetInstances.", ex);
                return false;
            }
        }

        public async Task<bool> DeleteCharacterSheetsInstanceAsync(CharacterSheetInstancesDbModel dbModel)
        {
            try
            {
                using var dbContext = await contextFactory.CreateDbContextAsync();
                var character = dbContext.CharacterSheetInstances.AsNoTracking().FirstOrDefault(w => w.ChatId == dbModel.ChatId);

                if (character == null)
                {
                    LoggingManager.LogToFile("183dd80c-7157-4c7f-a0bc-ef41d3817fb4", $"CharacterSheetsInstance tethered to chatId [{dbModel.ChatId}] to delete wasn't found in storage.");
                    return false;
                }

                var result = dbContext.CharacterSheetInstances.Remove(dbModel);
                if (result.State != EntityState.Deleted)
                {
                    LoggingManager.LogToFile("957607e6-3e08-4414-a162-5df2958561bd", $"Error when deleting a specific CharacterSheetsInstance. State was [{result.State}]. Result: [{JsonCommonSerializer.SerializeToString(result)}]. dbModel: [{JsonCommonSerializer.SerializeToString(dbModel)}].");
                    return false;
                }

                await dbContext.SaveChangesAsync();
                return true;
            } catch (Exception ex)
            {
                LoggingManager.LogToFile("ab52efe9-cb42-4bdd-8373-130ad789a125", $"Error when querying pending queries on table CharacterSheetInstances.", ex);
                return false;
            }
        }
    }
}
