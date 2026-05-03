using System.Text.Json;
using CohesiveRP.Common.Diagnostics;
using CohesiveRP.Common.Serialization;
using CohesiveRP.Storage.Common;
using CohesiveRP.Storage.DataAccessLayer.Chats;
using CohesiveRP.Storage.DataAccessLayer.Pathfinder.ChatCharactersRolls.BusinessObjects;
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

        private async Task<bool> UpdateCharacterSheetInstancesAsync(CharacterSheetDbModel dbModel)
        {
            try
            {
                using var dbContext = await contextFactory.CreateDbContextAsync();
                var characterSheetInstancesObjs = dbContext.CharacterSheetInstances;
                if (characterSheetInstancesObjs == null || !characterSheetInstancesObjs.Any())
                {
                    return false;
                }

                foreach (var characterSheetInstancesObj in characterSheetInstancesObjs)
                {
                    if(characterSheetInstancesObj.CharacterSheetInstances == null || characterSheetInstancesObj.CharacterSheetInstances.Count <= 0)
                    {
                        continue;
                    }

                    var matchingInstances = characterSheetInstancesObj.CharacterSheetInstances.Where(w => w.CharacterSheetId == dbModel.CharacterSheetId && !w.IsDirty).ToArray();

                    if(matchingInstances.Length <= 0)
                    {
                        continue;
                    }

                    foreach (var instance in matchingInstances)
                    {
                        instance.CharacterSheet = new CharacterSheet
                        {
                            AgeGroup = dbModel.CharacterSheet.AgeGroup,
                            Attractiveness = dbModel.CharacterSheet.Attractiveness,
                            Behavior = dbModel.CharacterSheet.Behavior,
                            BirthdayDate = dbModel.CharacterSheet.BirthdayDate,
                            BodyType = dbModel.CharacterSheet.BodyType,
                            BreastsSize = dbModel.CharacterSheet.BreastsSize,
                            ClothesPreference = dbModel.CharacterSheet.ClothesPreference,
                            CombatAffinityAttack = dbModel.CharacterSheet.CombatAffinityAttack,
                            CombatAffinityDefense = dbModel.CharacterSheet.CombatAffinityDefense,
                            Dislikes = dbModel.CharacterSheet.Dislikes,
                            EarShape = dbModel.CharacterSheet.EarShape,
                            EyeColor = dbModel.CharacterSheet.EyeColor,
                            Fears = dbModel.CharacterSheet.Fears,
                            FirstName = dbModel.CharacterSheet.FirstName,
                            Gender = dbModel.CharacterSheet.Gender,
                            Genitals = dbModel.CharacterSheet.Genitals,
                            PenisSize = dbModel.CharacterSheet.PenisSize,
                            GoalsForNextYear = dbModel.CharacterSheet.GoalsForNextYear,
                            HairColor = dbModel.CharacterSheet.HairColor,
                            HairStyle = dbModel.CharacterSheet.HairStyle,
                            Height = dbModel.CharacterSheet.Height,
                            Kinks = dbModel.CharacterSheet.Kinks,
                            LastName = dbModel.CharacterSheet.LastName,
                            Likes = dbModel.CharacterSheet.Likes,
                            LongTermGoals = dbModel.CharacterSheet.LongTermGoals,
                            Mannerisms = dbModel.CharacterSheet.Mannerisms,
                            PathfinderAttributesValues = dbModel.CharacterSheet.PathfinderAttributesValues,
                            PathfinderSkillsValues = dbModel.CharacterSheet.PathfinderSkillsValues,
                            PersonalityTraits = dbModel.CharacterSheet.PersonalityTraits,
                            PreferredCombatStyle = dbModel.CharacterSheet.PreferredCombatStyle,
                            Profession = dbModel.CharacterSheet.Profession,
                            Race = dbModel.CharacterSheet.Race,
                            Relationships = dbModel.CharacterSheet.Relationships,
                            Reputation = dbModel.CharacterSheet.Reputation,
                            SecretKinks = dbModel.CharacterSheet.SecretKinks,
                            Secrets = dbModel.CharacterSheet.Secrets,
                            Sexuality = dbModel.CharacterSheet.Sexuality,
                            Skills = dbModel.CharacterSheet.Skills,
                            SkinColor = dbModel.CharacterSheet.SkinColor,
                            SocialAnxiety = dbModel.CharacterSheet.SocialAnxiety,
                            SpeechImpairment = dbModel.CharacterSheet.SpeechImpairment,
                            SpeechPattern = dbModel.CharacterSheet.SpeechPattern,
                            Weaknesses = dbModel.CharacterSheet.Weaknesses,
                            WeaponsProficiency = dbModel.CharacterSheet.WeaponsProficiency,
                        };
                    }

                    // System fields
                    characterSheetInstancesObj.LastActivityAtUtc = DateTime.UtcNow;
                    var result = dbContext.CharacterSheetInstances.Update(characterSheetInstancesObj);
                    if (result.State != EntityState.Modified)
                    {
                        LoggingManager.LogToFile("6b45ca46-206c-4a5d-9b65-b879d4b260c6", $"Error when updating a CharacterSheetInstancesObj. State was [{result.State}]. Result: [{JsonCommonSerializer.SerializeToString(result)}]. dbModel: [{JsonCommonSerializer.SerializeToString(dbModel)}].");
                        return false;
                    }

                    await dbContext.SaveChangesAsync();
                }

                return true;
            } catch (Exception ex)
            {
                LoggingManager.LogToFile("7cd0b4c2-27dd-4d80-a45c-aafbb2cc2392", $"Error when querying pending queries on table CharacterSheetInstances.", ex);
                return false;
            }
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
                dbModel.CharacterSheetId = Guid.NewGuid().ToString();

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
                var character = dbContext.CharacterSheets.FirstOrDefault(w => w.CharacterSheetId == dbModel.CharacterSheetId);

                if (character == null)
                {
                    LoggingManager.LogToFile("b42f354f-1c43-4aeb-9ad8-4224894f3633", $"CharacterSheet tethered to characterId [{dbModel.CharacterSheetId}] to update wasn't found in storage.");
                    return false;
                }

                // System fields
                character.LastActivityAtUtc = DateTime.UtcNow;

                // Update only the overridable fields
                character.CharacterSheet = dbModel.CharacterSheet;

                var result = dbContext.CharacterSheets.Update(character);
                if (result.State != EntityState.Modified)
                {
                    LoggingManager.LogToFile("45633d31-66c6-4f06-bca9-8c61085a8cf3", $"Error when updating a CharacterSheet. State was [{result.State}]. Result: [{JsonCommonSerializer.SerializeToString(result)}]. dbModel: [{JsonCommonSerializer.SerializeToString(dbModel)}].");
                    return false;
                }

                await dbContext.SaveChangesAsync();

                // In a second time, update the characterSheetInstances related to this characterSheet ONLY if the instance isn't 'dirty' aka was updated by the backend with chat updates
                await UpdateCharacterSheetInstancesAsync(dbModel);

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
                var character = dbContext.CharacterSheets.AsNoTracking().FirstOrDefault(w => w.CharacterSheetId == dbModel.CharacterSheetId);

                if (character == null)
                {
                    LoggingManager.LogToFile("7b264625-86f3-4c45-9015-ad715bf99f31", $"CharacterSheet tethered to characterId [{dbModel.CharacterSheetId}] to delete wasn't found in storage.");
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
