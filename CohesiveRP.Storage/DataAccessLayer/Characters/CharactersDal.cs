using System.Text.Json;
using CohesiveRP.Common.Diagnostics;
using CohesiveRP.Common.Serialization;
using CohesiveRP.Storage.Common;
using CohesiveRP.Storage.DataAccessLayer.Chats;
using CohesiveRP.Storage.QueryModels.Chat;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace CohesiveRP.Storage.DataAccessLayer.Users
{
    /// <summary>
    /// DataAccessLayer around Characters.
    /// </summary>
    public class CharactersDal : StorageDal, ICharactersDal
    {
        private readonly IDbContextFactory<StorageDbContext> contextFactory;

        public CharactersDal(JsonSerializerOptions jsonSerializerOptions, IDbContextFactory<StorageDbContext> contextFactory) : base(jsonSerializerOptions)
        {
            this.contextFactory = contextFactory;

            using var dbContext = contextFactory.CreateDbContext();
            dbContext.Database.EnsureCreated();
        }

        // ********************************************************************
        //                            Public
        // ********************************************************************
        public async Task<CharacterDbModel[]> GetCharactersAsync()
        {
            try
            {
                using var dbContext = await contextFactory.CreateDbContextAsync();
                return dbContext.Characters.ToArray();
            } catch (Exception ex)
            {
                LoggingManager.LogToFile("6f649aaf-04e1-4ecb-b9fc-c62325d1a572", $"Error when querying Db on table Characters.", ex);
                return null;
            }
        }

        public async Task<CharacterDbModel> GetCharacterByIdAsync(string id)
        {
            try
            {
                using var dbContext = await contextFactory.CreateDbContextAsync();
                return dbContext.Characters.FirstOrDefault(w => w.CharacterId == id);
            } catch (Exception ex)
            {
                LoggingManager.LogToFile("9d978fcb-ff82-4b0d-b9b6-8c73cb81170a", $"Error when querying Db on table Characters.", ex);
                return null;
            }
        }

        public async Task<CharacterDbModel> AddCharacterAsync(AddCharacterQueryModel queryModel)
        {
            try
            {
                using var dbContext = await contextFactory.CreateDbContextAsync();

                // Convert models
                CharacterDbModel CharacterDbModel = new CharacterDbModel
                {
                    CharacterId = Guid.NewGuid().ToString(),
                    CreatedAtUtc = DateTime.UtcNow,
                    Name = queryModel.Name,
                };

                EntityEntry<CharacterDbModel> result = await dbContext.Characters.AddAsync(CharacterDbModel);

                if (result.State != EntityState.Added)
                {
                    LoggingManager.LogToFile("dd691091-bdea-4b7b-9356-bea3c20a424b", $"Error when querying Db on table Characters. State was [{result.State}]. Result: [{JsonCommonSerializer.SerializeToString(result)}].");
                    return null;
                }

                await dbContext.SaveChangesAsync();
                return result.Entity;
            } catch (Exception ex)
            {
                LoggingManager.LogToFile("a2075ff5-1f27-4a80-82dd-615f2e454e41", $"Error when querying Db on table Characters.", ex);
                return null;
            }
        }
    }
}
