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
                    Creator = queryModel.Creator,
                    CreatorNotes = queryModel.CreatorNotes,
                    Description = queryModel.Description,
                    Tags = queryModel.Tags,
                    FirstMessage = queryModel.FirstMessage,
                    AlternateGreetings = queryModel.AlternateGreetings,
                    LastActivityAtUtc = DateTime.UtcNow,
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

        public async Task<bool> UpdateCharacterAsync(CharacterDbModel characterDbModel)
        {
            try
            {
                using var dbContext = await contextFactory.CreateDbContextAsync();
                var character = dbContext.Characters.AsNoTracking().FirstOrDefault(w => w.CharacterId == characterDbModel.CharacterId);

                if (character == null)
                {
                    LoggingManager.LogToFile("20737745-2e05-4620-959c-92df2e03312c", $"Character [{characterDbModel.CharacterId}] to update wasn't found in storage.");
                    return false;
                }

                EntityEntry<CharacterDbModel> result = dbContext.Characters.Update(characterDbModel);
                if (result.State != EntityState.Modified)
                {
                    LoggingManager.LogToFile("b3cd56a6-f7b7-4bd5-be9f-a30748fec1d8", $"Error when updating LastActivity on table Characters. State was [{result.State}]. Result: [{JsonCommonSerializer.SerializeToString(result)}]. dbModel: [{JsonCommonSerializer.SerializeToString(characterDbModel)}].");
                    return false;
                }

                await dbContext.SaveChangesAsync();
                return true;
            } catch (Exception ex)
            {
                LoggingManager.LogToFile("1bdebfc9-b832-49ea-be6d-eff2027d83e6", $"Error when querying pending queries on table Characters.", ex);
                return false;
            }
        }

        public async Task<bool> DeleteCharacterAsync(CharacterDbModel characterDbModel)
        {
            try
            {
                using var dbContext = await contextFactory.CreateDbContextAsync();
                var character = dbContext.Characters.AsNoTracking().FirstOrDefault(w => w.CharacterId == characterDbModel.CharacterId);

                if (character == null)
                {
                    LoggingManager.LogToFile("8aff54b1-92ef-4bc9-b594-6da73992c982", $"Character [{characterDbModel.CharacterId}] to delete wasn't found in storage.");
                    return false;
                }

                EntityEntry<CharacterDbModel> result = dbContext.Characters.Remove(characterDbModel);
                if (result.State != EntityState.Deleted)
                {
                    LoggingManager.LogToFile("9ece8490-e7c5-4f30-93e2-6794953d0afb", $"Error when deleting a specific Character. State was [{result.State}]. Result: [{JsonCommonSerializer.SerializeToString(result)}]. dbModel: [{JsonCommonSerializer.SerializeToString(characterDbModel)}].");
                    return false;
                }

                await dbContext.SaveChangesAsync();
                return true;
            } catch (Exception ex)
            {
                LoggingManager.LogToFile("2d6e9259-8fe1-4448-8d88-8dfb846dd2d3", $"Error when querying pending queries on table Characters.", ex);
                return false;
            }
        }
    }
}
