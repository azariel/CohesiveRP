using System.Text.Json;
using CohesiveRP.Common.Diagnostics;
using CohesiveRP.Common.Serialization;
using CohesiveRP.Storage.Common;
using CohesiveRP.Storage.DataAccessLayer.AIQueries;
using CohesiveRP.Storage.DataAccessLayer.ChatCompletionPresets;
using CohesiveRP.Storage.DataAccessLayer.ChatCompletionPresets.Injectors;
using Microsoft.EntityFrameworkCore;

namespace CohesiveRP.Storage.DataAccessLayer.Users
{
    /// <summary>
    /// DataAccessLayer around Global Settings.
    /// </summary>
    /// - **Mood**: Describe the character's mood or current emotion (e.g., \"embarrassed\", \"angry\", \"sad\", \"playful\") in a short phrase. Base that character's mood on that character's personality and how they would realistically and immersively react in the current story context.
    public class ChatCompletionPresetsDal : StorageDal, IChatCompletionPresetsDal
    {
        private readonly IDbContextFactory<StorageDbContext> contextFactory;

        public ChatCompletionPresetsDal(JsonSerializerOptions jsonSerializerOptions, IDbContextFactory<StorageDbContext> contextFactory) : base(jsonSerializerOptions)
        {
            this.contextFactory = contextFactory;

            Cleanup();
        }

        private void Cleanup()
        {
            using var dbContext = contextFactory.CreateDbContext();
            dbContext.Database.EnsureCreated();
            int nbPresets = dbContext.ChatCompletionPresets.Count();

            if (nbPresets <= 0)
            {
                // Create default settings
                // Generate the main reply of the AI (its actual roleplay)
                dbContext.ChatCompletionPresets.Add(MainCompletionPresetInjector.InjectPreset());

                // Summarize raw messages from the User and AI
                dbContext.ChatCompletionPresets.Add(SummaryCompletionPresetInjector.InjectPreset());

                // Summarize summaries
                dbContext.ChatCompletionPresets.Add(SummaryMergerCompletionPresetInjector.InjectPreset());

                // Generate a scene tracker object
                dbContext.ChatCompletionPresets.Add(SceneTrackerCompletionPresetInjector.InjectPreset());

                // Determine if a character needs to do a skill check
                dbContext.ChatCompletionPresets.Add(SkillChecksInitiatorCompletionPresetInjector.InjectPreset());

                // Character Creation
                dbContext.ChatCompletionPresets.Add(DynamicCharacterCreatorCompletionPresetInjector.InjectPreset());
                dbContext.ChatCompletionPresets.Add(DynamicCharacterSheetCreatorCompletionPresetInjector.InjectPreset());
                dbContext.ChatCompletionPresets.Add(DynamicCharacterAvatarPromptIllustrationCompletionPresetInjector.InjectPreset());

                // Chat Additions
                dbContext.ChatCompletionPresets.Add(NarrativeDirectionCompletionPresetInjector.InjectPreset());
                dbContext.ChatCompletionPresets.Add(ProseGuardianCompletionPresetInjector.InjectPreset());
                dbContext.ChatCompletionPresets.Add(CohesionEnforcementCompletionPresetInjector.InjectPreset());
                dbContext.ChatCompletionPresets.Add(NarrativeArchitectureCompletionPresetInjector.InjectPreset());

                // Poke the AI to update character status alterations (magical effects, body status, wounds)
                // and the other slow-changing CharacterSheetInstance fields, outputting only the diff.
                dbContext.ChatCompletionPresets.Add(CharacterStatusUpdateCompletionPresetInjector.InjectPreset());

                dbContext.SaveChanges();
                return;
            }
        }

        // ********************************************************************
        //                            Public
        // ********************************************************************
        public async Task<ChatCompletionPresetsDbModel> GetChatCompletionPresetAsync(string chatCompletionPresetId)
        {
            try
            {
                using var dbContext = await contextFactory.CreateDbContextAsync();
                return dbContext.ChatCompletionPresets.FirstOrDefault(f => f.ChatCompletionPresetId == chatCompletionPresetId);
            } catch (Exception ex)
            {
                LoggingManager.LogToFile("60a3ceac-3614-4031-96e4-bc2f36aa7f27", $"Error when querying Db on table ChatCompletionPresets.", ex);
                return null;
            }
        }

        public async Task<ChatCompletionPresetsDbModel[]> GetChatCompletionPresetsAsync()
        {
            try
            {
                using var dbContext = await contextFactory.CreateDbContextAsync();
                return await dbContext.ChatCompletionPresets.ToArrayAsync();
            } catch (Exception ex)
            {
                LoggingManager.LogToFile("a9c89721-5a45-4484-9894-dd1b263c4047", $"Error when querying Db on table ChatCompletionPresets.", ex);
                return null;
            }
        }

        public async Task<ChatCompletionPresetsDbModel> AddChatCompletionPresetAsync(ChatCompletionPresetsDbModel dbModel)
        {
            try
            {
                using var dbContext = await contextFactory.CreateDbContextAsync();

                // Force those fields
                dbModel.ChatCompletionPresetId = Guid.NewGuid().ToString();
                dbModel.CreatedAtUtc = DateTime.UtcNow;

                var result = await dbContext.ChatCompletionPresets.AddAsync(dbModel);

                if (result.State != EntityState.Added)
                {
                    LoggingManager.LogToFile("eebd4b6b-c8b3-4acd-a2ed-3ed5fb87318e", $"Error when querying Db on table ChatCompletionPresets. State was [{result.State}]. Result: [{JsonCommonSerializer.SerializeToString(result)}].");
                    return null;
                }

                await dbContext.SaveChangesAsync();
                return result.Entity;
            } catch (Exception ex)
            {
                LoggingManager.LogToFile("1452962e-da77-4b13-9cd4-c5aa8de74fb9", $"Error when querying Db on table ChatCompletionPresets.", ex);
                return null;
            }
        }

        public async Task<bool> UpdateChatCompletionPresetAsync(ChatCompletionPresetsDbModel dbModel)
        {
            try
            {
                using var dbContext = await contextFactory.CreateDbContextAsync();
                var chatCompletionPreset = dbContext.ChatCompletionPresets.FirstOrDefault(w => w.ChatCompletionPresetId == dbModel.ChatCompletionPresetId);

                if (chatCompletionPreset == null)
                {
                    LoggingManager.LogToFile("f1be577a-6f5c-4ece-aad9-f04fef028488", $"ChatCompletionPreset [{dbModel.ChatCompletionPresetId}] to update wasn't found in storage.");
                    return false;
                }

                // Only handle overridable fields
                chatCompletionPreset.Name = dbModel.Name;
                chatCompletionPreset.Format = dbModel.Format;

                var result = dbContext.ChatCompletionPresets.Update(chatCompletionPreset);
                if (result.State != EntityState.Modified)
                {
                    LoggingManager.LogToFile("76514bc6-0aff-407b-9455-e4719e9fb6b0", $"Error when updating ChatCompletionPreset. State was [{result.State}]. Result: [{JsonCommonSerializer.SerializeToString(result)}]. dbModel: [{JsonCommonSerializer.SerializeToString(dbModel)}].");
                    return false;
                }

                await dbContext.SaveChangesAsync();
                return true;
            } catch (Exception ex)
            {
                LoggingManager.LogToFile("21f773d1-ac34-4794-8fdd-5589d2e149b3", $"Error when querying pending queries on table ChatCompletionPresets.", ex);
                return false;
            }
        }

        public async Task<bool> DeleteChatCompletionPresetAsync(string chatCompletionPresetId)
        {
            try
            {
                using var dbContext = await contextFactory.CreateDbContextAsync();
                var chatCompletionPreset = dbContext.ChatCompletionPresets.AsNoTracking().FirstOrDefault(w => w.ChatCompletionPresetId == chatCompletionPresetId);

                if (chatCompletionPreset == null)
                {
                    LoggingManager.LogToFile("01b4a3bc-94ba-483d-898d-32e211dc7df7", $"ChatCompletionPreset [{chatCompletionPresetId}] to delete wasn't found in storage.");
                    return false;
                }

                var result = dbContext.ChatCompletionPresets.Remove(chatCompletionPreset);
                if (result.State != EntityState.Deleted)
                {
                    LoggingManager.LogToFile("d537ffd3-1236-426b-81eb-f61bd74cb6bc", $"Error when deleting a specific ChatCompletionPreset [{chatCompletionPresetId}]. State was [{result.State}]. Result: [{JsonCommonSerializer.SerializeToString(result)}]..");
                    return false;
                }

                await dbContext.SaveChangesAsync();
                return true;
            } catch (Exception ex)
            {
                LoggingManager.LogToFile("282d43c9-5eab-470d-a0f9-211376eb3e7a", $"Error when querying pending queries on table ChatCompletionPresets.", ex);
                return false;
            }
        }
    }
}
