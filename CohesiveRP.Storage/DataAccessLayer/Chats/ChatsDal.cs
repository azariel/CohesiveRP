using System.Text.Json;
using CohesiveRP.Common.Diagnostics;
using CohesiveRP.Common.Serialization;
using CohesiveRP.Storage.Common;
using CohesiveRP.Storage.DataAccessLayer.AIQueries;
using CohesiveRP.Storage.DataAccessLayer.ChatAdditions.NarrativeArchitecture;
using CohesiveRP.Storage.DataAccessLayer.ChatAdditions.NarrativeDirection;
using CohesiveRP.Storage.DataAccessLayer.ChatAdditions.ProseGuardian;
using CohesiveRP.Storage.DataAccessLayer.Chats;
using CohesiveRP.Storage.DataAccessLayer.InteractiveUserInputQueries;
using CohesiveRP.Storage.DataAccessLayer.LorebookInstances;
using CohesiveRP.Storage.DataAccessLayer.Messages;
using CohesiveRP.Storage.DataAccessLayer.SceneTracker;
using CohesiveRP.Storage.DataAccessLayer.Settings;
using CohesiveRP.Storage.DataAccessLayer.Summary.Short;
using CohesiveRP.Storage.QueryModels.Chat;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace CohesiveRP.Storage.DataAccessLayer.Users
{
    /// <summary>
    /// DataAccessLayer around Chats.
    /// </summary>
    public class ChatsDal : StorageDal, IChatsDal
    {
        private readonly IDbContextFactory<StorageDbContext> contextFactory;
        private readonly IBackgroundQueriesDal backgroundQueriesDal;
        private readonly IMessagesDal messagesDal;
        private readonly IIllustrationQueryDal illustrationQueryDal;
        private readonly IInteractiveUserInputDal interactiveUserInputDal;
        private readonly ILorebookInstanceDal lorebookInstanceDal;
        private readonly INarrativeArchitecturesDal narrativeArchitecturesDal;
        private readonly INarrativeDirectionsDal narrativeDirectionsDal;
        private readonly ICharacterSheetInstancesDal characterSheetInstancesDal;
        private readonly IChatCharactersRollsDal chatCharactersRollsDal;
        private readonly IProseGuardiansDal proseGuardiansDal;
        private readonly ISceneTrackerDal sceneTrackerDal;
        private readonly ISummaryDal summaryDal;
        private readonly IGlobalSettingsDal globalSettingsDal;

        public ChatsDal(JsonSerializerOptions jsonSerializerOptions, IDbContextFactory<StorageDbContext> contextFactory,
            IBackgroundQueriesDal backgroundQueriesDal, IMessagesDal messagesDal, IIllustrationQueryDal illustrationQueryDal,
            IInteractiveUserInputDal interactiveUserInputDal, ILorebookInstanceDal lorebookInstanceDal,
            INarrativeArchitecturesDal narrativeArchitecturesDal, INarrativeDirectionsDal narrativeDirectionsDal,
            ICharacterSheetInstancesDal characterSheetInstancesDal, IChatCharactersRollsDal chatCharactersRollsDal, IGlobalSettingsDal globalSettingsDal,
            IProseGuardiansDal proseGuardiansDal, ISceneTrackerDal sceneTrackerDal, ISummaryDal summaryDal) : base(jsonSerializerOptions)
        {
            this.contextFactory = contextFactory;
            this.backgroundQueriesDal = backgroundQueriesDal;
            this.messagesDal = messagesDal;
            this.illustrationQueryDal = illustrationQueryDal;
            this.interactiveUserInputDal = interactiveUserInputDal;
            this.lorebookInstanceDal = lorebookInstanceDal;
            this.narrativeArchitecturesDal = narrativeArchitecturesDal;
            this.narrativeDirectionsDal = narrativeDirectionsDal;
            this.characterSheetInstancesDal = characterSheetInstancesDal;
            this.chatCharactersRollsDal = chatCharactersRollsDal;
            this.proseGuardiansDal = proseGuardiansDal;
            this.sceneTrackerDal = sceneTrackerDal;
            this.summaryDal = summaryDal;
            this.globalSettingsDal = globalSettingsDal;

            using var dbContext = contextFactory.CreateDbContext();
            dbContext.Database.EnsureCreated();
        }

        // ********************************************************************
        //                            Public
        // ********************************************************************
        public async Task<ChatDbModel[]> GetChatsAsync()
        {
            try
            {
                using var dbContext = await contextFactory.CreateDbContextAsync();
                return dbContext.Chats.ToArray();
            } catch (Exception ex)
            {
                LoggingManager.LogToFile("0a39887a-ea58-4e78-b44b-afad7e5fc340", $"Error when querying Db on table Chat.", ex);
                return null;
            }
        }

        public async Task<ChatDbModel> GetChatByIdAsync(string id)
        {
            try
            {
                using var dbContext = await contextFactory.CreateDbContextAsync();

                var selectedChat = dbContext.Chats.FirstOrDefault(w => w.ChatId == id);

                // If selectedCompletionPresets field is null, re-generate it
                if (selectedChat != null && (selectedChat.SelectedChatCompletionPresets == null || selectedChat.SelectedChatCompletionPresets.Count <= 0))
                {
                    //globalSettings.ChatCompletionPresetsMap.Map.Where(w => w.IsDefault).ToArray();
                    var globalSettings = await globalSettingsDal.GetGlobalSettingsAsync();
                    var defaultCompletionPresets = globalSettings?.ChatCompletionPresetsMap?.Map?.Where(w => w.IsDefault)?.ToArray();

                    if (defaultCompletionPresets != null && defaultCompletionPresets.Length > 0)
                    {
                        selectedChat.SelectedChatCompletionPresets = [..defaultCompletionPresets.Select(s => new ChatCompletionPresetSelection()
                        {
                            Type = s.Type,
                            ChatCompletionPresetId = s.ChatCompletionPresetId,
                        })];

                        await dbContext.SaveChangesAsync();
                    }
                }

                return selectedChat;
            } catch (Exception ex)
            {
                LoggingManager.LogToFile("ed1b481f-463b-4854-acac-222965ef3601", $"Error when querying Db on table Chat.", ex);
                return null;
            }
        }

        public async Task<ChatDbModel> CreateChatAsync(CreateChatQueryModel queryModel)
        {
            try
            {
                using var dbContext = await contextFactory.CreateDbContextAsync();

                // Convert models
                ChatDbModel chatDbModel = new ChatDbModel
                {
                    ChatId = Guid.NewGuid().ToString(),
                    Name = queryModel.Name,
                    CreatedAtUtc = DateTime.UtcNow,
                    SelectedChatCompletionPresets = queryModel.SelectedChatCompletionPresets,
                    CharacterIds = queryModel.CharacterIds,
                    LorebookIds = queryModel.LorebookIds,
                    PersonaId = queryModel.PersonaId,
                    AvatarFilePath = queryModel.AvatarFilePath,
                    LastActivityAtUtc = DateTime.UtcNow,
                };

                EntityEntry<ChatDbModel> result = await dbContext.Chats.AddAsync(chatDbModel);

                if (result.State != EntityState.Added)
                {
                    LoggingManager.LogToFile("970fa367-1778-45b0-9c75-8243d331f3ea", $"Error when querying Db on table Chat. State was [{result.State}]. Result: [{JsonCommonSerializer.SerializeToString(result)}].");
                    return null;
                }

                await dbContext.SaveChangesAsync();
                return result.Entity;
            } catch (Exception ex)
            {
                LoggingManager.LogToFile("0a39887a-ea58-4e78-b44b-afad7e5fc340", $"Error when querying Db on table Chat.", ex);
                return null;
            }
        }

        public async Task<bool> UpdateChatAsync(ChatDbModel dbModel)
        {
            try
            {
                using var dbContext = await contextFactory.CreateDbContextAsync();
                var chat = dbContext.Chats.FirstOrDefault(w => w.ChatId == dbModel.ChatId);

                if (chat == null)
                {
                    LoggingManager.LogToFile("e89002b8-9b09-4ddc-b1fd-53b5cea3327f", $"Chat [{dbModel.ChatId}] to update wasn't found in storage.");
                    return false;
                }

                // Update only the overridable fields
                chat.SelectedChatCompletionPresets = dbModel.SelectedChatCompletionPresets;
                chat.LorebookIds = dbModel.LorebookIds;
                chat.CharacterIds = dbModel.CharacterIds;
                chat.Name = dbModel.Name;
                chat.AvatarFilePath = dbModel.AvatarFilePath;
                chat.PersonaId = dbModel.PersonaId;

                var result = dbContext.Chats.Update(chat);
                if (result.State != EntityState.Modified)
                {
                    LoggingManager.LogToFile("5c3ae68d-9c28-44b0-9f50-d816e735ff78", $"Error when updating a Chat. State was [{result.State}]. Result: [{JsonCommonSerializer.SerializeToString(result)}]. dbModel: [{JsonCommonSerializer.SerializeToString(dbModel)}].");
                    return false;
                }

                await dbContext.SaveChangesAsync();
                return true;
            } catch (Exception ex)
            {
                LoggingManager.LogToFile("eeecbf0f-eb8d-4191-848c-5d1de62f9fa8", $"Error when querying pending queries on table Chats.", ex);
                return false;
            }
        }

        public async Task<bool> DeleteChatAsync(string chatId)
        {
            try
            {
                using var dbContext = await contextFactory.CreateDbContextAsync();
                var chat = dbContext.Chats.FirstOrDefault(w => w.ChatId == chatId);

                if (chat == null)
                {
                    LoggingManager.LogToFile("f23deb92-034e-43dc-a7c4-4e0ca1c8c5a5", $"Chat [{chatId}] to delete wasn't found in storage.");
                    return false;
                }

                var result = dbContext.Chats.Remove(chat);
                if (result.State != EntityState.Deleted)
                {
                    LoggingManager.LogToFile("cbc84e32-b1ed-4c3d-8a4a-03810a0afddc", $"Error when deleting a specific Chat. State was [{result.State}]. Result: [{JsonCommonSerializer.SerializeToString(result)}]. dbModel: [{JsonCommonSerializer.SerializeToString(chat)}].");
                    return false;
                }

                // Delete orphan objects in storage
                // backgroundQueries
                var BackgroundQueriesTiedToDeletedChat = await backgroundQueriesDal.GetBackgroundQueriesByChatIdAsync(chatId);

                if (BackgroundQueriesTiedToDeletedChat != null && BackgroundQueriesTiedToDeletedChat.Length > 0)
                {
                    await backgroundQueriesDal.DeleteBackgroundQueriesByChatIdAsync(chatId);
                }

                // TODO CohesionEnforcements
                // TODO SceneAnalyzers

                // Hot Messages
                var hotMessagesToDelete = await messagesDal.GetHotMessagesAsync(chatId);
                if (hotMessagesToDelete != null && hotMessagesToDelete.Messages.Any())
                {
                    await messagesDal.DeleteHotMessageAsync(chatId);
                }

                // Cold Messages
                var coldMessagesToDelete = await messagesDal.GetColdMessagesAsync(chatId);
                if (coldMessagesToDelete != null && coldMessagesToDelete.Messages.Any())
                {
                    await messagesDal.DeleteColdMessageAsync(chatId);
                }

                // Illustration Queries
                var illustrationQueriesToDelete = await illustrationQueryDal.GetIllustrationQueriesAsync(f => f.ChatId == chatId);
                if (illustrationQueriesToDelete != null && illustrationQueriesToDelete.Any())
                {
                    foreach (var illustrationQueryToDelete in illustrationQueriesToDelete)
                    {
                        await illustrationQueryDal.DeleteIllustrationQueryAsync(illustrationQueryToDelete.IllustrationQueryId);
                    }
                }

                // InteractiveUserInputQueries
                var interactiveUserQueriesToDelete = await interactiveUserInputDal.GetInteractiveUserInputQueriesAsync(f => f.ChatId == chatId);
                if (interactiveUserQueriesToDelete != null && interactiveUserQueriesToDelete.Any())
                {
                    foreach (var interactiveUserQueryToDelete in interactiveUserQueriesToDelete)
                    {
                        await interactiveUserInputDal.DeleteInteractiveUserInputQueryAsync(interactiveUserQueryToDelete.InteractiveUserInputQueryId);
                    }
                }

                // LorebookInstances
                var lorebookInstancesToDelete = await lorebookInstanceDal.GetLorebookInstancesAsync(f => f.ChatId == chatId);
                if (lorebookInstancesToDelete != null && lorebookInstancesToDelete.Any())
                {
                    await lorebookInstanceDal.DeleteLorebookInstanceAsync(chatId);
                }

                // NarrativeArchitecture
                var narrativeArchitecturesToDelete = await narrativeArchitecturesDal.GetNarrativeArchitecturesAsync(f => f.ChatId == chatId);
                if (narrativeArchitecturesToDelete != null && narrativeArchitecturesToDelete.Any())
                {
                    foreach (var narrativeArchitectureToDelete in narrativeArchitecturesToDelete)
                    {
                        await narrativeArchitecturesDal.DeleteNarrativeArchitectureAsync(f => f.NarrativeArchitectureId == narrativeArchitectureToDelete.NarrativeArchitectureId);
                    }
                }

                // NarrativeDirections
                var narrativeDirectionsToDelete = await narrativeDirectionsDal.GetNarrativeDirectionsAsync(f => f.ChatId == chatId);
                if (narrativeDirectionsToDelete != null && narrativeDirectionsToDelete.Any())
                {
                    foreach (var narrativeDirectionToDelete in narrativeDirectionsToDelete)
                    {
                        await narrativeDirectionsDal.DeleteNarrativeDirectionAsync(f => f.NarrativeDirectionId == narrativeDirectionToDelete.NarrativeDirectionId);
                    }
                }

                // CharacterSheetInstances
                var characterSheetInstancesToDelete = await characterSheetInstancesDal.GetCharacterSheetsInstanceByChatIdAsync(chatId);
                if (characterSheetInstancesToDelete != null)
                {
                    await characterSheetInstancesDal.DeleteCharacterSheetsInstanceAsync(characterSheetInstancesToDelete);
                }

                // ChatCharactersRolls
                var chatCharactersRollsToDelete = await chatCharactersRollsDal.GetChatCharactersRollsByFuncAsync(f => f.ChatId == chatId);
                if (chatCharactersRollsToDelete != null)
                {
                    foreach (var chatCharactersRollToDelete in chatCharactersRollsToDelete)
                    {
                        await chatCharactersRollsDal.DeleteChatCharactersRollsAsync(chatCharactersRollToDelete);
                    }
                }

                // ProseGuardians
                var proseGuardiansToDelete = await proseGuardiansDal.GetProseGuardiansAsync(f => f.ChatId == chatId);
                if (proseGuardiansToDelete != null)
                {
                    await proseGuardiansDal.DeleteProseGuardianAsync(f => f.ChatId == chatId);
                }

                // SceneTracker
                var sceneTrackersToDelete = await sceneTrackerDal.GetSceneTrackerAsync(chatId);
                if (sceneTrackersToDelete != null)
                {
                    await sceneTrackerDal.DeleteSceneTrackerAsync(chatId);
                }

                // Summaries
                var summariesToDelete = await summaryDal.GetSummaryAsync(chatId);
                if (summariesToDelete != null)
                {
                    // Delete the WHOLE summary, this includes short, medium, long, extraLong, etc.
                    await summaryDal.DeleteSummaryFromChatIdAsync(chatId);
                }

                await dbContext.SaveChangesAsync();
                return true;
            } catch (Exception ex)
            {
                LoggingManager.LogToFile("8173a545-f6d2-43c8-8caf-6b1bcf5e497c", $"Error when querying pending queries on table Chats.", ex);
                return false;
            }
        }
    }
}
