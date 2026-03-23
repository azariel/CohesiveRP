using System.Text.Json;
using CohesiveRP.Common.Diagnostics;
using CohesiveRP.Common.Serialization;
using CohesiveRP.Storage.Common;
using CohesiveRP.Storage.DataAccessLayer.Chats;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace CohesiveRP.Storage.DataAccessLayer.Users
{
    /// <summary>
    /// DataAccessLayer around Charactersheets.
    /// </summary>
    public class CharacterSheetsDal : StorageDal, ICharacterSheetsDal
    {
        private readonly IDbContextFactory<StorageDbContext> contextFactory;

        public CharacterSheetsDal(JsonSerializerOptions jsonSerializerOptions, IDbContextFactory<StorageDbContext> contextFactory) : base(jsonSerializerOptions)
        {
            this.contextFactory = contextFactory;

            using var dbContext = contextFactory.CreateDbContext();
            dbContext.Database.EnsureCreated();
        }

        // ********************************************************************
        //                            Public
        // ********************************************************************
        public async Task<CharacterSheetDbModel[]> GetCharacterSheetsAsync()
        {
            try
            {
                using var dbContext = await contextFactory.CreateDbContextAsync();
                return dbContext.CharacterSheets.ToArray();
            } catch (Exception ex)
            {
                LoggingManager.LogToFile("6462e761-d775-4bc7-84fa-d6a915cb73f7", $"Error when querying Db on table CharacterSheets.", ex);
                return null;
            }
        }

        public async Task<CharacterSheetDbModel[]> GetCharacterSheetsByFuncAsync(Func<CharacterSheetDbModel, bool> func)
        {
            if (func == null)
            {
                return null;
            }

            try
            {
                using var dbContext = await contextFactory.CreateDbContextAsync();
                var result = await dbContext.CharacterSheets.AsAsyncEnumerable().Where(func.Invoke).ToArrayAsync();
                return result;
            } catch (Exception ex)
            {
                LoggingManager.LogToFile("bb3dd9d4-e72a-4bc2-9d97-38b6b2d43b30", $"Error when querying Db on table CharacterSheets.", ex);
                return null;
            }
        }

        public async Task<CharacterSheetDbModel> GetCharacterSheetByCharacterIdAsync(string characterId)
        {
            var characters = await GetCharacterSheetsByFuncAsync(f => f.CharacterId == characterId);
            return characters?.FirstOrDefault();
        }

        public async Task<CharacterSheetDbModel> AddCharacterSheetAsync(CharacterSheetDbModel dbModel)
        {
            try
            {
                using var dbContext = await contextFactory.CreateDbContextAsync();

                // Override system fields
                dbModel.CreatedAtUtc = DateTime.UtcNow;
                dbModel.LastActivityAtUtc = DateTime.UtcNow;

                EntityEntry<CharacterSheetDbModel> result = await dbContext.CharacterSheets.AddAsync(dbModel);

                if (result.State != EntityState.Added)
                {
                    LoggingManager.LogToFile("98db270e-cc9c-4d3a-9280-6799a61e24af", $"Error when querying Db on table CharacterSheets. State was [{result.State}]. dbModel: [{JsonCommonSerializer.SerializeToString(dbModel)}].");
                    return null;
                }

                await dbContext.SaveChangesAsync();
                return result.Entity;
            } catch (Exception ex)
            {
                LoggingManager.LogToFile("18c7427b-5add-4755-98a2-c49314793936", $"Error when querying Db on table CharacterSheets.", ex);
                return null;
            }
        }

        public async Task<bool> UpdateCharacterSheetAsync(CharacterSheetDbModel dbModel)
        {
            try
            {
                using var dbContext = await contextFactory.CreateDbContextAsync();
                var character = dbContext.CharacterSheets.FirstOrDefault(w => w.CharacterId == dbModel.CharacterId);

                if (character == null)
                {
                    LoggingManager.LogToFile("b42f354f-1c43-4aeb-9ad8-4224894f3633", $"CharacterSheet tethered to characterId [{dbModel.CharacterId}] to update wasn't found in storage.");
                    return false;
                }

                // System fields
                character.LastActivityAtUtc = DateTime.UtcNow;

                // Update only the overridable fields
                character.CharacterSheets.Clear();
                character.CharacterSheets.AddRange(dbModel.CharacterSheets);

                var result = dbContext.CharacterSheets.Update(character);
                if (result.State != EntityState.Modified)
                {
                    LoggingManager.LogToFile("45633d31-66c6-4f06-bca9-8c61085a8cf3", $"Error when updating a CharacterSheet. State was [{result.State}]. Result: [{JsonCommonSerializer.SerializeToString(result)}]. dbModel: [{JsonCommonSerializer.SerializeToString(dbModel)}].");
                    return false;
                }

                await dbContext.SaveChangesAsync();
                return true;
            } catch (Exception ex)
            {
                LoggingManager.LogToFile("f8dd53e2-dbed-44bd-9266-c5d02f7ddb95", $"Error when querying pending queries on table CharacterSheets.", ex);
                return false;
            }
        }

        public async Task<bool> DeleteCharacterSheetAsync(CharacterSheetDbModel dbModel)
        {
            try
            {
                using var dbContext = await contextFactory.CreateDbContextAsync();
                var character = dbContext.CharacterSheets.AsNoTracking().FirstOrDefault(w => w.CharacterId == dbModel.CharacterId);

                if (character == null)
                {
                    LoggingManager.LogToFile("7b264625-86f3-4c45-9015-ad715bf99f31", $"CharacterSheet tethered to characterId [{dbModel.CharacterId}] to delete wasn't found in storage.");
                    return false;
                }

                var result = dbContext.CharacterSheets.Remove(dbModel);
                if (result.State != EntityState.Deleted)
                {
                    LoggingManager.LogToFile("e7b6efb5-bc7d-45a6-9fe7-1359dacf410c", $"Error when deleting a specific CharacterSheet. State was [{result.State}]. Result: [{JsonCommonSerializer.SerializeToString(result)}]. dbModel: [{JsonCommonSerializer.SerializeToString(dbModel)}].");
                    return false;
                }

                await dbContext.SaveChangesAsync();
                return true;
            } catch (Exception ex)
            {
                LoggingManager.LogToFile("da1c5163-4a45-4022-95f7-4815a8eb33e2", $"Error when querying pending queries on table CharacterSheets.", ex);
                return false;
            }
        }
    }
}
