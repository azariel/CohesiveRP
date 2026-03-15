using CohesiveRP.Common.Diagnostics;
using CohesiveRP.Storage.DataAccessLayer.AIQueries;
using CohesiveRP.Storage.DataAccessLayer.ChatCompletionPresets;
using CohesiveRP.Storage.DataAccessLayer.Chats;
using CohesiveRP.Storage.DataAccessLayer.Messages;
using CohesiveRP.Storage.DataAccessLayer.Messages.Hot;
using CohesiveRP.Storage.DataAccessLayer.SceneTracker;
using CohesiveRP.Storage.DataAccessLayer.Settings;
using CohesiveRP.Storage.DataAccessLayer.Settings.ChatCompletionPresets;
using CohesiveRP.Storage.DataAccessLayer.Summary.Short;
using CohesiveRP.Storage.DataAccessLayer.Users;
using CohesiveRP.Storage.QueryModels.BackgroundQuery;
using CohesiveRP.Storage.QueryModels.Chat;
using CohesiveRP.Storage.QueryModels.Message;
using CohesiveRP.Storage.QueryModels.Personas;
using CohesiveRP.Storage.QueryModels.SceneTracker;

namespace CohesiveRP.Core.Services
{
    /// <summary>
    /// Expose and handle operations to the Storage service.
    /// </summary>
    public class StorageService : IStorageService
    {
        private IChatsDal chatsDal;
        private ICharactersDal charactersDal;
        private IPersonasDal personasDal;
        private IMessagesDal messagesDal;
        private IGlobalSettingsDal globalSettingsDal;
        private IChatCompletionPresetsDal chatCompletionPresetsDal;
        private IBackgroundQueriesDal backgroundQueriesDal;
        private ILLMApiQueriesDal llmApiQueriesDal;
        private ISummaryDal summaryDal;
        private ISceneTrackerDal sceneTrackerDal;

        public StorageService(
            IChatsDal chatsDal,
            ICharactersDal charactersDal,
            IPersonasDal personasDal,
            IMessagesDal messagesDal,
            IGlobalSettingsDal globalSettingsDal,
            IChatCompletionPresetsDal chatCompletionPresetsDal,
            IBackgroundQueriesDal backgroundQueriesDal,
            ILLMApiQueriesDal llmApiQueriesDal,
            ISummaryDal summaryDal,
            ISceneTrackerDal sceneTrackerDal)
        {
            this.chatsDal = chatsDal;
            this.charactersDal = charactersDal;
            this.personasDal = personasDal;
            this.messagesDal = messagesDal;
            this.globalSettingsDal = globalSettingsDal;
            this.chatCompletionPresetsDal = chatCompletionPresetsDal;
            this.backgroundQueriesDal = backgroundQueriesDal;
            this.llmApiQueriesDal = llmApiQueriesDal;
            this.summaryDal = summaryDal;
            this.sceneTrackerDal = sceneTrackerDal;
        }

        // Chats
        public async Task<ChatDbModel> AddChatAsync(CreateChatQueryModel queryModel)
        {
            // If the new chat to create isn't linked to a chat completion preset configuration
            if (queryModel != null && (queryModel.SelectedChatCompletionPresets == null || queryModel.SelectedChatCompletionPresets.Count <= 0))
            {
                try
                {
                    // Fetch defaults from globalSettings
                    var globalSettings = await GetGlobalSettingsAsync();
                    var defaultChatCompletionPresets = globalSettings.ChatCompletionPresetsMap.Map.Where(w => w.IsDefault).ToArray();

                    List<ChatCompletionPresetsMapElement> defaultChatCompletionPresetsFiltered = [];
                    foreach (var defaultChatCompletionPreset in defaultChatCompletionPresets)
                    {
                        if (defaultChatCompletionPresetsFiltered.Any(a => a.Type == defaultChatCompletionPreset.Type))
                        {
                            continue;
                        }

                        defaultChatCompletionPresetsFiltered.Add(defaultChatCompletionPreset);
                    }

                    queryModel.SelectedChatCompletionPresets =
                    [
                        ..defaultChatCompletionPresetsFiltered.Select(s=> new ChatCompletionPresetSelection
                        {
                            ChatCompletionPresetId = s.ChatCompletionPresetId,
                            Type = s.Type,
                        }),
                    ];
                } catch (Exception e)
                {
                    LoggingManager.LogToFile("73d8e0f2-2e34-4624-9c79-baedde5d3e40", $"Couldn't force default ChatCompletionPresets on the new Chat. Chat can't be created without ChatCompletionPresets. Either fix the default presets or specify them.", e);
                    return null; ;
                }
            }

            return await chatsDal.CreateChatAsync(queryModel);
        }

        public async Task<ChatDbModel[]> GetAllChatsAsync() => await chatsDal.GetChatsAsync();
        public async Task<ChatDbModel> GetChatAsync(string chatId) => await chatsDal.GetChatByIdAsync(chatId);

        // Characters
        public async Task<CharacterDbModel[]> GetCharactersAsync() => await charactersDal.GetCharactersAsync();
        public async Task<CharacterDbModel> GetCharacterByIdAsync(string characterId) => await charactersDal.GetCharacterByIdAsync(characterId);
        public async Task<CharacterDbModel> ImportNewCharacterAsync(AddCharacterQueryModel queryModel) => await charactersDal.AddCharacterAsync(queryModel);
        public async Task<bool> UpdateCharacterAsync(CharacterDbModel characterDbModel) => await charactersDal.UpdateCharacterAsync(characterDbModel);
        public async Task<bool> DeleteCharacterAsync(CharacterDbModel characterDbModel) => await charactersDal.DeleteCharacterAsync(characterDbModel);

        // Personas
        public async Task<PersonaDbModel[]> GetPersonasAsync() => await personasDal.GetPersonasAsync();
        public async Task<PersonaDbModel> GetPersonaByIdAsync(string personaId) => await personasDal.GetPersonaByIdAsync(personaId);
        public async Task<PersonaDbModel> ImportNewPersonaAsync(AddPersonaQueryModel queryModel) => await personasDal.AddPersonaAsync(queryModel);
        public async Task<bool> UpdatePersonaAsync(PersonaDbModel personaDbModel) => await personasDal.UpdatePersonaAsync(personaDbModel);
        public async Task<bool> DeletePersonaAsync(PersonaDbModel personaDbModel) => await personasDal.DeletePersonaAsync(personaDbModel);

        // Messages
        public async Task<HotMessagesDbModel> GetAllHotMessagesAsync(string chatId) => await messagesDal.GetHotMessagesAsync(chatId);
        public async Task<IMessageDbModel> GetSpecificMessageAsync(string chatId, string messageId) => await messagesDal.GetMessageByIdAsync(chatId, messageId);
        public async Task<IMessageDbModel> AddMessageAsync(CreateMessageQueryModel message) => await messagesDal.CreateOrUpdateMessageAsync(message);
        public async Task<bool> UpdateHotMessagesAsync(HotMessagesDbModel messages) => await messagesDal.UpdateHotMessagesAsync(messages);
        public async Task<bool> UpdateHotMessageAsync(string chatId, MessageDbModel message) => await messagesDal.UpdateHotMessageAsync(chatId, message);
        public async Task<bool> DeleteSpecificMessageAsync(string chatId, string messageId) => await messagesDal.DeleteSpecificMessageAsync(chatId, messageId);

        // Settings
        public async Task<GlobalSettingsDbModel> GetGlobalSettingsAsync() => await globalSettingsDal.GetGlobalSettingsAsync();

        // BackgroundQueries
        public async Task<BackgroundQueryDbModel> AddBackgroundQueryAsync(CreateBackgroundQueryQueryModel queryModel) => await backgroundQueriesDal.CreateBackgroundQueryAsync(queryModel);
        public async Task<BackgroundQueryDbModel> GetBackgroundQueryAsync(string queryId) => await backgroundQueriesDal.GetBackgroundQueryAsync(queryId);
        public async Task<BackgroundQueryDbModel[]> GetPendingOrProcessingBackgroundQueryAsync() => await backgroundQueriesDal.GetPendingOrProcessingBackgroundQueryAsync();
        public async Task<BackgroundQueryDbModel[]> GetBackgroundQueriesByChatIdAsync(string chatId) => await backgroundQueriesDal.GetBackgroundQueriesByChatIdAsync(chatId);

        // ChatCompletionPresets
        public async Task<ChatCompletionPresetsDbModel> GetChatCompletionPresetAsync(string mainChatCompletionPresetId) => await chatCompletionPresetsDal.GetChatCompletionPresetsAsync(mainChatCompletionPresetId);

        // LLMQueries
        public async Task<LLMApiQueryDbModel[]> GetQueriesOnLLMApisAsync(string tag) => await llmApiQueriesDal.GetQueriesOnLLMApisAsync(tag);
        public async Task<LLMApiQueryDbModel> AddNewQueryAsync(LLMApiQueryDbModel newQuery) => await llmApiQueriesDal.AddNewQueryAsync(newQuery);
        public async Task<bool> DeleteQueryByIdAsync(string lLMApiQueryId) => await llmApiQueriesDal.DeleteQueryByIdAsync(lLMApiQueryId);

        // Summary
        public async Task<ISummaryEntryDbModel> AddShortTermSummaryAsync(CreateSummaryQueryModel queryModel) => await summaryDal.AddShortTermSummaryAsync(queryModel);
        public async Task<ISummaryEntryDbModel> AddMediumTermSummaryAsync(CreateSummaryQueryModel queryModel) => await summaryDal.AddMediumTermSummaryAsync(queryModel);
        public async Task<ISummaryEntryDbModel> AddLongTermSummaryAsync(CreateSummaryQueryModel queryModel) => await summaryDal.AddLongTermSummaryAsync(queryModel);
        public async Task<ISummaryEntryDbModel> AddExtraTermSummaryAsync(CreateSummaryQueryModel queryModel) => await summaryDal.AddExtraTermSummaryAsync(queryModel);
        public async Task<ISummaryEntryDbModel> AddOverflowTermSummaryAsync(CreateSummaryQueryModel queryModel) => await summaryDal.AddOverflowTermSummaryAsync(queryModel);
        public async Task<SummaryDbModel> GetSummaryAsync(string chatId) => await summaryDal.GetSummaryAsync(chatId);
        public async Task<bool> DeleteShortTermSummariesEntriesAsync(string chatId, string[] summariesIds) => await summaryDal.DeleteShortTermSummariesEntriesAsync(chatId, summariesIds);
        public async Task<bool> DeleteMediumTermSummariesEntriesAsync(string chatId, string[] summariesIds) => await summaryDal.DeleteMediumTermSummariesEntriesAsync(chatId, summariesIds);
        public async Task<bool> DeleteLongTermSummariesEntriesAsync(string chatId, string[] summariesIds) => await summaryDal.DeleteLongTermSummariesEntriesAsync(chatId, summariesIds);
        public async Task<bool> DeleteExtraTermSummariesEntriesAsync(string chatId, string[] summariesIds) => await summaryDal.DeleteExtraTermSummariesEntriesAsync(chatId, summariesIds);
        public async Task<bool> DeleteOverflowTermSummariesEntriesAsync(string chatId, string[] summariesIds) => await summaryDal.DeleteOverflowTermSummariesEntriesAsync(chatId, summariesIds);

        // SceneTracker
        public async Task<SceneTrackerDbModel> GetSceneTrackerAsync(string chatId) => await sceneTrackerDal.GetSceneTracker(chatId);
        public async Task<SceneTrackerDbModel> AddSceneTrackerAsync(CreateSceneTrackerQueryModel queryModel) => await sceneTrackerDal.AddSceneTracker(queryModel);
        public async Task<SceneTrackerDbModel> UpdateSceneTrackerAsync(CreateSceneTrackerQueryModel queryModel) => await sceneTrackerDal.CreateOrUpdateSceneTracker(queryModel);
    }
}
