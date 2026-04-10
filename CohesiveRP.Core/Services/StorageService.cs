using CohesiveRP.Common.Diagnostics;
using CohesiveRP.Storage.DataAccessLayer.AIQueries;
using CohesiveRP.Storage.DataAccessLayer.ChatCompletionPresets;
using CohesiveRP.Storage.DataAccessLayer.Chats;
using CohesiveRP.Storage.DataAccessLayer.LorebookInstances;
using CohesiveRP.Storage.DataAccessLayer.Messages;
using CohesiveRP.Storage.DataAccessLayer.Messages.Hot;
using CohesiveRP.Storage.DataAccessLayer.SceneAnalyzer;
using CohesiveRP.Storage.DataAccessLayer.SceneTracker;
using CohesiveRP.Storage.DataAccessLayer.Settings;
using CohesiveRP.Storage.DataAccessLayer.Settings.ChatCompletionPresets;
using CohesiveRP.Storage.DataAccessLayer.Summary.Short;
using CohesiveRP.Storage.DataAccessLayer.Users;
using CohesiveRP.Storage.QueryModels.BackgroundQuery;
using CohesiveRP.Storage.QueryModels.Chat;
using CohesiveRP.Storage.QueryModels.Lorebooks;
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
        private ICharacterSheetsDal characterSheetsDal;
        private ICharacterSheetInstancesDal characterSheetInstancesDal;
        private IChatCharactersRollsDal chatCharactersRollsDal;
        private IPersonasDal personasDal;
        private ILorebooksDal lorebooksDal;
        private ILorebookInstanceDal lorebookInstancesDal;
        private IMessagesDal messagesDal;
        private IGlobalSettingsDal globalSettingsDal;
        private IChatCompletionPresetsDal chatCompletionPresetsDal;
        private IBackgroundQueriesDal backgroundQueriesDal;
        private ILLMApiQueriesDal llmApiQueriesDal;
        private ISummaryDal summaryDal;
        private ISceneTrackerDal sceneTrackerDal;
        private ISceneAnalyzerDal sceneAnalyzerDal;

        public StorageService(
            IChatsDal chatsDal,
            ICharactersDal charactersDal,
            ICharacterSheetsDal characterSheetsDal,
            ICharacterSheetInstancesDal characterSheetInstancesDal,
            IChatCharactersRollsDal chatCharactersRollsDal,
            IPersonasDal personasDal,
            ILorebooksDal lorebooksDal,
            ILorebookInstanceDal lorebookInstancesDal,
            IMessagesDal messagesDal,
            IGlobalSettingsDal globalSettingsDal,
            IChatCompletionPresetsDal chatCompletionPresetsDal,
            IBackgroundQueriesDal backgroundQueriesDal,
            ILLMApiQueriesDal llmApiQueriesDal,
            ISummaryDal summaryDal,
            ISceneTrackerDal sceneTrackerDal,
            ISceneAnalyzerDal sceneAnalyzerDal)
        {
            this.chatsDal = chatsDal;
            this.charactersDal = charactersDal;
            this.characterSheetsDal = characterSheetsDal;
            this.characterSheetInstancesDal = characterSheetInstancesDal;
            this.chatCharactersRollsDal = chatCharactersRollsDal;
            this.personasDal = personasDal;
            this.lorebooksDal = lorebooksDal;
            this.lorebookInstancesDal = lorebookInstancesDal;
            this.messagesDal = messagesDal;
            this.globalSettingsDal = globalSettingsDal;
            this.chatCompletionPresetsDal = chatCompletionPresetsDal;
            this.backgroundQueriesDal = backgroundQueriesDal;
            this.llmApiQueriesDal = llmApiQueriesDal;
            this.summaryDal = summaryDal;
            this.sceneTrackerDal = sceneTrackerDal;
            this.sceneAnalyzerDal = sceneAnalyzerDal;
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
        public async Task<bool> UpdateChatAsync(ChatDbModel dbModel) => await chatsDal.UpdateChatAsync(dbModel);
        public async Task<bool> DeleteChatAsync(string chatId) => await chatsDal.DeleteChatAsync(chatId);

        // Characters
        public async Task<CharacterDbModel[]> GetCharactersAsync() => await charactersDal.GetCharactersAsync();
        public async Task<CharacterDbModel> GetCharacterByIdAsync(string characterId) => await charactersDal.GetCharacterByIdAsync(characterId);
        public async Task<CharacterDbModel> ImportNewCharacterAsync(AddCharacterQueryModel queryModel) => await charactersDal.AddCharacterAsync(queryModel);
        public async Task<bool> UpdateCharacterAsync(CharacterDbModel characterDbModel) => await charactersDal.UpdateCharacterAsync(characterDbModel);
        public async Task<bool> DeleteCharacterAsync(CharacterDbModel characterDbModel) => await charactersDal.DeleteCharacterAsync(characterDbModel);

        // Pathfinder.CharacterSheets
        public async Task<CharacterSheetDbModel[]> GetCharacterSheetsAsync() => await characterSheetsDal.GetCharacterSheetsAsync();
        public async Task<CharacterSheetDbModel[]> GetCharacterSheetsByFuncAsync(Func<CharacterSheetDbModel, bool> func) => await characterSheetsDal.GetCharacterSheetsByFuncAsync(func);
        //public async Task<CharacterSheetDbModel> GetCharacterSheetByFuncAsync(Func<CharacterSheetDbModel, bool> func) => throw new NotImplementedException();
        public async Task<CharacterSheetDbModel> GetCharacterSheetByCharacterIdAsync(string characterId) => await characterSheetsDal.GetCharacterSheetByCharacterIdAsync(characterId);
        public async Task<CharacterSheetDbModel> AddCharacterSheetAsync(CharacterSheetDbModel dbModel) => await characterSheetsDal.AddCharacterSheetAsync(dbModel);
        public async Task<bool> UpdateCharacterSheetAsync(CharacterSheetDbModel dbModel) => await characterSheetsDal.UpdateCharacterSheetAsync(dbModel);
        public async Task<bool> DeleteCharacterSheetAsync(CharacterSheetDbModel dbModel) => await characterSheetsDal.DeleteCharacterSheetAsync(dbModel);

        // Pathfinder.CharacterSheetInstances
        public async Task<CharacterSheetInstancesDbModel[]> GetCharacterSheetInstancesAsync() => await characterSheetInstancesDal.GetCharacterSheetInstancesAsync();
        public async Task<CharacterSheetInstancesDbModel[]> GetCharacterSheetsInstanceByFuncAsync(Func<CharacterSheetInstancesDbModel, bool> func) => await characterSheetInstancesDal.GetCharacterSheetInstancesByFuncAsync(func);
        public async Task<CharacterSheetInstancesDbModel> GetCharacterSheetsInstanceByChatIdAsync(string chatId) => await characterSheetInstancesDal.GetCharacterSheetsInstanceByChatIdAsync(chatId);
        public async Task<CharacterSheetInstancesDbModel> AddCharacterSheetsInstanceAsync(CharacterSheetInstancesDbModel dbModel) => await characterSheetInstancesDal.AddCharacterSheetsInstanceAsync(dbModel);
        public async Task<bool> UpdateCharacterSheetsInstanceAsync(CharacterSheetInstancesDbModel dbModel) => await characterSheetInstancesDal.UpdateCharacterSheetsInstanceAsync(dbModel);
        public async Task<bool> DeleteCharacterSheetsInstanceAsync(CharacterSheetInstancesDbModel dbModel) => await characterSheetInstancesDal.DeleteCharacterSheetsInstanceAsync(dbModel);

        // Pathfinder.ChatCharactersRolls
        public async Task<ChatCharactersRollsDbModel[]> GetChatCharactersRollsAsync() => await chatCharactersRollsDal.GetChatCharactersRollsAsync();
        public async Task<ChatCharactersRollsDbModel[]> GetChatCharactersRollsByFuncAsync(Func<ChatCharactersRollsDbModel, bool> func) => await chatCharactersRollsDal.GetChatCharactersRollsByFuncAsync(func);
        public async Task<ChatCharactersRollsDbModel> GetChatCharactersRollsByIdAsync(string chatId) => await chatCharactersRollsDal.GetChatCharactersRollsEntryAsync(chatId);
        public async Task<ChatCharactersRollsDbModel> AddChatCharactersRollsAsync(ChatCharactersRollsDbModel dbModel) => await chatCharactersRollsDal.AddChatCharactersRollsAsync(dbModel);
        public async Task<bool> UpdateChatCharactersRollsAsync(ChatCharactersRollsDbModel dbModel) => await chatCharactersRollsDal.UpdateChatCharactersRollsAsync(dbModel);
        public async Task<bool> DeleteChatCharactersRollsAsync(ChatCharactersRollsDbModel dbModel) => await chatCharactersRollsDal.DeleteChatCharactersRollsAsync(dbModel);

        // Personas
        public async Task<PersonaDbModel[]> GetPersonasAsync() => await personasDal.GetPersonasAsync();
        public async Task<PersonaDbModel> GetPersonaByIdAsync(string personaId) => await personasDal.GetPersonaByIdAsync(personaId);
        public async Task<bool> UpdatePersonaAsync(PersonaDbModel personaDbModel) => await personasDal.UpdatePersonaAsync(personaDbModel);
        public async Task<bool> DeletePersonaAsync(PersonaDbModel personaDbModel) => await personasDal.DeletePersonaAsync(personaDbModel);
        public async Task<PersonaDbModel> AddPersonaAsync(AddPersonaQueryModel queryModel) => await personasDal.AddPersonaAsync(queryModel);

        // Lorebooks
        public async Task<LorebookDbModel[]> GetLorebooksAsync() => await lorebooksDal.GetLorebooksAsync();
        public async Task<LorebookDbModel[]> GetLorebooksAsync(Func<LorebookDbModel, bool> func) => await lorebooksDal.GetLorebooksByFuncAsync(func);
        public async Task<LorebookDbModel> GetLorebookByIdAsync(string lorebookId) => await lorebooksDal.GetLorebookByIdAsync(lorebookId);
        public async Task<bool> UpdateLorebookAsync(LorebookDbModel lorebookDbModel) => await lorebooksDal.UpdateLorebookAsync(lorebookDbModel);
        public async Task<bool> DeleteLorebookAsync(LorebookDbModel lorebookDbModel) => await lorebooksDal.DeleteLorebookAsync(lorebookDbModel);
        public async Task<LorebookDbModel> AddEmptyLorebookAsync() => await lorebooksDal.AddLorebookAsync(new AddLorebookQueryModel { Name = "New Lorebook", Entries = [] });
        public async Task<LorebookDbModel> AddLorebookAsync(LorebookDbModel dbModel) => await lorebooksDal.AddLorebookAsync(dbModel);

        // Lorebook Instances
        public async Task<LorebookInstanceDbModel[]> GetLorebookInstancesAsync() => await lorebookInstancesDal.GetLorebookInstancesAsync();
        public async Task<LorebookInstanceDbModel[]> GetLorebookInstancesAsync(Func<LorebookInstanceDbModel, bool> func) => await lorebookInstancesDal.GetLorebookInstancesAsync(func);
        public async Task<LorebookInstanceDbModel> AddLorebookInstanceAsync(LorebookInstanceDbModel dbModel) => await lorebookInstancesDal.AddLorebookInstanceAsync(dbModel);
        public async Task<bool> UpdateLorebookInstanceAsync(LorebookInstanceDbModel lorebookDbModel) => await lorebookInstancesDal.UpdateLorebookInstanceAsync(lorebookDbModel);
        public async Task<bool> DeleteLorebookInstanceAsync(string chatId) => await lorebookInstancesDal.DeleteLorebookInstanceAsync(chatId);

        // Messages
        public async Task<HotMessagesDbModel> GetAllHotMessagesAsync(string chatId) => await messagesDal.GetHotMessagesAsync(chatId);
        public async Task<IMessageDbModel> GetSpecificMessageAsync(string chatId, string messageId) => await messagesDal.GetMessageByIdAsync(chatId, messageId);
        public async Task<IMessageDbModel> AddMessageAsync(CreateMessageQueryModel message) => await messagesDal.CreateOrUpdateMessageAsync(message);
        public async Task<bool> UpdateHotMessagesAsync(HotMessagesDbModel messages) => await messagesDal.UpdateHotMessagesAsync(messages);
        public async Task<bool> UpdateHotMessageAsync(string chatId, MessageDbModel message) => await messagesDal.UpdateHotMessageAsync(chatId, message);
        public async Task<bool> DeleteSpecificMessageAsync(string chatId, string messageId) => await messagesDal.DeleteSpecificMessageAsync(chatId, messageId);
        public async Task<bool> DeleteColdMessagesAsync(string chatId)  => await messagesDal.DeleteColdMessageAsync(chatId);
        public async Task<bool> DeleteHotMessagesAsync(string chatId)  => await messagesDal.DeleteHotMessageAsync(chatId);

        // Settings
        public async Task<GlobalSettingsDbModel> GetGlobalSettingsAsync() => await globalSettingsDal.GetGlobalSettingsAsync();
        public async Task<bool> UpdateGlobalSettingsAsync(GlobalSettingsDbModel dbModel) => await globalSettingsDal.UpdateGlobalSettingsAsync(dbModel);

        // BackgroundQueries
        public async Task<BackgroundQueryDbModel> AddBackgroundQueryAsync(CreateBackgroundQueryQueryModel queryModel) => await backgroundQueriesDal.CreateBackgroundQueryAsync(queryModel);
        public async Task<BackgroundQueryDbModel> GetBackgroundQueryAsync(string queryId) => await backgroundQueriesDal.GetBackgroundQueryAsync(queryId);
        public async Task<BackgroundQueryDbModel[]> GetPendingOrProcessingBackgroundQueryAsync() => await backgroundQueriesDal.GetPendingOrProcessingBackgroundQueryAsync();
        public async Task<BackgroundQueryDbModel[]> GetBackgroundQueriesByChatIdAsync(string chatId) => await backgroundQueriesDal.GetBackgroundQueriesByChatIdAsync(chatId);
        public async Task<bool> UpdateBackgroundQueryAsync(BackgroundQueryDbModel backgroundQueryDbModel) => await backgroundQueriesDal.UpdateBackgroundQueryAsync(backgroundQueryDbModel);
        public async Task<bool> DeleteBackgroundQueriesByChatIdAsync(string chatId) => await backgroundQueriesDal.DeleteBackgroundQueriesByChatIdAsync(chatId);

        // ChatCompletionPresets
        public async Task<ChatCompletionPresetsDbModel[]> GetChatCompletionPresetsAsync() => await chatCompletionPresetsDal.GetChatCompletionPresetsAsync();
        public async Task<ChatCompletionPresetsDbModel> GetChatCompletionPresetAsync(string chatCompletionPresetId) => await chatCompletionPresetsDal.GetChatCompletionPresetAsync(chatCompletionPresetId);
        public async Task<ChatCompletionPresetsDbModel> AddChatCompletionPresetAsync(ChatCompletionPresetsDbModel dbModel) => await chatCompletionPresetsDal.AddChatCompletionPresetAsync(dbModel);
        public async Task<bool> UpdateChatCompletionPresetAsync(ChatCompletionPresetsDbModel currentChatCompletionPreset) => await chatCompletionPresetsDal.UpdateChatCompletionPresetAsync(currentChatCompletionPreset);
        public async Task<bool> DeleteChatCompletionPresetAsync(string chatCompletionPresetId) => await chatCompletionPresetsDal.DeleteChatCompletionPresetAsync(chatCompletionPresetId);

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
        public async Task<bool> DeleteSummaryFromChatIdAsync(string chatId) => await summaryDal.DeleteSummaryFromChatIdAsync(chatId);

        // SceneTracker
        public async Task<SceneTrackerDbModel> GetSceneTrackerAsync(string chatId) => await sceneTrackerDal.GetSceneTrackerAsync(chatId);
        public async Task<SceneTrackerDbModel> AddSceneTrackerAsync(CreateSceneTrackerQueryModel queryModel) => await sceneTrackerDal.AddSceneTrackerAsync(queryModel);
        public async Task<SceneTrackerDbModel> CreateOrUpdateSceneTrackerAsync(CreateSceneTrackerQueryModel queryModel) => await sceneTrackerDal.CreateOrUpdateSceneTrackerAsync(queryModel);
        public async Task<bool> DeleteSceneTrackerAsync(string chatId) => await sceneTrackerDal.DeleteSceneTrackerAsync(chatId);

        // SceneAnalyzer
        public async Task<SceneAnalyzerDbModel> GetSceneAnalyzerAsync(string chatId) => await sceneAnalyzerDal.GetSceneAnalyzerAsync(chatId);
        public async Task<SceneAnalyzerDbModel> AddSceneAnalyzerAsync(SceneAnalyzerDbModel dbModel) => await sceneAnalyzerDal.AddSceneAnalyzerAsync(dbModel);
        public async Task<SceneAnalyzerDbModel> CreateOrUpdateSceneAnalyzerAsync(SceneAnalyzerDbModel dbModel) => await sceneAnalyzerDal.CreateOrUpdateSceneAnalyzerAsync(dbModel);
        public async Task<bool> DeleteSceneAnalyzerAsync(string chatId) => await sceneAnalyzerDal.DeleteSceneAnalyzerAsync(chatId);
    }
}
