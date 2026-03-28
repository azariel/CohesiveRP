using CohesiveRP.Storage.DataAccessLayer.AIQueries;
using CohesiveRP.Storage.DataAccessLayer.Chats;
using CohesiveRP.Storage.DataAccessLayer.Messages;
using CohesiveRP.Storage.DataAccessLayer.Messages.Hot;
using CohesiveRP.Storage.DataAccessLayer.Settings;
using CohesiveRP.Storage.QueryModels.BackgroundQuery;
using CohesiveRP.Storage.QueryModels.Chat;
using CohesiveRP.Storage.QueryModels.Message;
using CohesiveRP.Storage.QueryModels.Personas;
using CohesiveRP.Storage.QueryModels.SceneTracker;

namespace CohesiveRP.Core.Services
{
    public interface IStorageService
    {
        // Chats
        Task<ChatDbModel> AddChatAsync(CreateChatQueryModel queryModel);
        Task<ChatDbModel[]> GetAllChatsAsync();
        Task<ChatDbModel> GetChatAsync(string chatId);
        Task<bool> UpdateChatAsync(ChatDbModel chat);
        Task<bool> DeleteChatAsync(string chatId);

        // Messages
        Task<HotMessagesDbModel> GetAllHotMessagesAsync(string chatId);
        Task<IMessageDbModel> GetSpecificMessageAsync(string chatId, string messageId);
        Task<bool> DeleteSpecificMessageAsync(string chatId, string messageId);
        Task<IMessageDbModel> AddMessageAsync(CreateMessageQueryModel message);
        Task<bool> UpdateHotMessagesAsync(HotMessagesDbModel messages);
        Task<bool> UpdateHotMessageAsync(string chatId, MessageDbModel message);
        Task<bool> DeleteColdMessagesAsync(string chatId);
        Task<bool> DeleteHotMessagesAsync(string chatId);

        // GlobalSettings
        Task<GlobalSettingsDbModel> GetGlobalSettingsAsync();
        Task<bool> UpdateGlobalSettingsAsync(GlobalSettingsDbModel dbModel);

        // CompletionPresets
        Task<ChatCompletionPresetsDbModel> GetChatCompletionPresetAsync(string chatCompletionPresetId);
        Task<ChatCompletionPresetsDbModel[]> GetChatCompletionPresetsAsync();
        Task<ChatCompletionPresetsDbModel> AddChatCompletionPresetAsync(ChatCompletionPresetsDbModel dbModel);
        Task<bool> UpdateChatCompletionPresetAsync(ChatCompletionPresetsDbModel currentChatCompletionPreset);
        Task<bool> DeleteChatCompletionPresetAsync(string chatCompletionPresetId);

        // BackgroundQueries
        Task<BackgroundQueryDbModel> AddBackgroundQueryAsync(CreateBackgroundQueryQueryModel queryModel);
        Task<BackgroundQueryDbModel> GetBackgroundQueryAsync(string queryId);
        Task<BackgroundQueryDbModel[]> GetBackgroundQueriesByChatIdAsync(string chatId);
        Task<BackgroundQueryDbModel[]> GetPendingOrProcessingBackgroundQueryAsync();
        Task<LLMApiQueryDbModel[]> GetQueriesOnLLMApisAsync(string tag);
        Task<LLMApiQueryDbModel> AddNewQueryAsync(LLMApiQueryDbModel newQuery);
        Task<bool> DeleteQueryByIdAsync(string lLMApiQueryId);
        Task<bool> DeleteBackgroundQueriesByChatIdAsync(string chatId);

        // Summary
        Task<ISummaryEntryDbModel> AddShortTermSummaryAsync(CreateSummaryQueryModel queryModel);
        Task<ISummaryEntryDbModel> AddMediumTermSummaryAsync(CreateSummaryQueryModel queryModel);
        Task<ISummaryEntryDbModel> AddLongTermSummaryAsync(CreateSummaryQueryModel queryModel);
        Task<ISummaryEntryDbModel> AddExtraTermSummaryAsync(CreateSummaryQueryModel queryModel);
        Task<ISummaryEntryDbModel> AddOverflowTermSummaryAsync(CreateSummaryQueryModel queryModel);
        Task<SummaryDbModel> GetSummaryAsync(string chatId);
        Task<bool> DeleteShortTermSummariesEntriesAsync(string chatId, string[] summariesIds);
        Task<bool> DeleteMediumTermSummariesEntriesAsync(string chatId, string[] summariesIds);
        Task<bool> DeleteLongTermSummariesEntriesAsync(string chatId, string[] summariesIds);
        Task<bool> DeleteExtraTermSummariesEntriesAsync(string chatId, string[] summariesIds);
        Task<bool> DeleteOverflowTermSummariesEntriesAsync(string chatId, string[] summariesId);
        Task<bool> DeleteSummaryFromChatIdAsync(string chatId);

        // SceneTracker
        Task<SceneTrackerDbModel> GetSceneTrackerAsync(string chatId);
        Task<SceneTrackerDbModel> AddSceneTrackerAsync(CreateSceneTrackerQueryModel queryModel);
        Task<SceneTrackerDbModel> CreateOrUpdateSceneTrackerAsync(CreateSceneTrackerQueryModel queryModel);
        Task<bool> DeleteSceneTrackerAsync(string chatId);

        // SceneAnalyzer
        Task<SceneAnalyzerDbModel> GetSceneAnalyzerAsync(string chatId);
        Task<SceneAnalyzerDbModel> AddSceneAnalyzerAsync(SceneAnalyzerDbModel dbModel);
        Task<SceneAnalyzerDbModel> CreateOrUpdateSceneAnalyzerAsync(SceneAnalyzerDbModel dbModel);
        Task<bool> DeleteSceneAnalyzerAsync(string chatId);

        // Characters
        Task<CharacterDbModel[]> GetCharactersAsync();
        Task<CharacterDbModel> GetCharacterByIdAsync(string characterId);
        Task<CharacterDbModel> ImportNewCharacterAsync(AddCharacterQueryModel queryModel);
        Task<bool> UpdateCharacterAsync(CharacterDbModel characterDbModel);
        Task<bool> DeleteCharacterAsync(CharacterDbModel characterDbModel);

        // Pathfinder.CharacterSheets
        Task<CharacterSheetDbModel[]> GetCharacterSheetsAsync();
        Task<CharacterSheetDbModel[]> GetCharacterSheetsByFuncAsync(Func<CharacterSheetDbModel, bool> func);
        //Task<CharacterSheetDbModel> GetCharacterSheetByFuncAsync(Func<CharacterSheetDbModel, bool> func);
        Task<CharacterSheetDbModel> GetCharacterSheetByCharacterIdAsync(string characterId);
        Task<CharacterSheetDbModel> AddCharacterSheetAsync(CharacterSheetDbModel dbModel);
        Task<bool> UpdateCharacterSheetAsync(CharacterSheetDbModel dbModel);
        Task<bool> DeleteCharacterSheetAsync(CharacterSheetDbModel dbModel);

        // Pathfinder.CharacterSheetInstances
        Task<CharacterSheetInstancesDbModel[]> GetCharacterSheetInstancesAsync();
        Task<CharacterSheetInstancesDbModel> GetCharacterSheetsInstanceByChatIdAsync(string chatId);
        Task<CharacterSheetInstancesDbModel> AddCharacterSheetsInstanceAsync(CharacterSheetInstancesDbModel dbModel);
        Task<bool> UpdateCharacterSheetsInstanceAsync(CharacterSheetInstancesDbModel dbModel);
        Task<bool> DeleteCharacterSheetsInstanceAsync(CharacterSheetInstancesDbModel dbModel);

        // Pathfinder.ChatCharactersRolls
        Task<ChatCharactersRollsDbModel[]> GetChatCharactersRollsAsync();
        Task<ChatCharactersRollsDbModel> GetChatCharactersRollsByIdAsync(string chatId);
        Task<ChatCharactersRollsDbModel> AddChatCharactersRollsAsync(ChatCharactersRollsDbModel dbModel);
        Task<bool> UpdateChatCharactersRollsAsync(ChatCharactersRollsDbModel dbModel);
        Task<bool> DeleteChatCharactersRollsAsync(ChatCharactersRollsDbModel dbModel);

        // Personas
        Task<PersonaDbModel[]> GetPersonasAsync();
        Task<PersonaDbModel> GetPersonaByIdAsync(string personaId);
        Task<bool> UpdatePersonaAsync(PersonaDbModel personaDbModel);
        Task<bool> DeletePersonaAsync(PersonaDbModel personaDbModel);
        Task<PersonaDbModel> AddPersonaAsync(AddPersonaQueryModel queryModel);

        // Lorebooks
        Task<LorebookDbModel[]> GetLorebooksAsync();
        Task<LorebookDbModel[]> GetLorebooksAsync(Func<LorebookDbModel, bool> func);
        Task<LorebookDbModel> GetLorebookByIdAsync(string lorebookId);
        Task<bool> UpdateLorebookAsync(LorebookDbModel lorebookDbModel);
        Task<bool> DeleteLorebookAsync(LorebookDbModel lorebookDbModel);
        Task<LorebookDbModel> AddEmptyLorebookAsync();
        Task<LorebookDbModel> AddLorebookAsync(LorebookDbModel dbModel);

        // Lorebook Instances
        Task<LorebookInstanceDbModel[]> GetLorebookInstancesAsync();
        Task<LorebookInstanceDbModel[]> GetLorebookInstancesAsync(Func<LorebookInstanceDbModel, bool> func);
        Task<LorebookInstanceDbModel> AddLorebookInstanceAsync(LorebookInstanceDbModel dbModel);
        Task<bool> UpdateLorebookInstanceAsync(LorebookInstanceDbModel lorebookDbModel);
        Task<bool> DeleteLorebookInstanceAsync(string chatId);
    }
}
