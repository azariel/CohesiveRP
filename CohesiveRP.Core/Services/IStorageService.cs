using CohesiveRP.Storage.DataAccessLayer.AIQueries;
using CohesiveRP.Storage.DataAccessLayer.Chats;
using CohesiveRP.Storage.DataAccessLayer.Messages;
using CohesiveRP.Storage.DataAccessLayer.Messages.Hot;
using CohesiveRP.Storage.DataAccessLayer.Settings;
using CohesiveRP.Storage.QueryModels.BackgroundQuery;
using CohesiveRP.Storage.QueryModels.Chat;
using CohesiveRP.Storage.QueryModels.Message;
using CohesiveRP.Storage.QueryModels.SceneTracker;

namespace CohesiveRP.Core.Services
{
    public interface IStorageService
    {
        Task<ChatDbModel> AddChatAsync(CreateChatQueryModel queryModel);
        Task<ChatDbModel[]> GetAllChatsAsync();
        Task<ChatDbModel> GetChatAsync(string chatId);
        Task<HotMessagesDbModel> GetAllHotMessagesAsync(string chatId);
        Task<IMessageDbModel> GetSpecificMessageAsync(string chatId, string messageId);
        Task<IMessageDbModel> AddMessageAsync(CreateMessageQueryModel message);
        Task<bool> DeleteSpecificMessageAsync(string chatId, string messageId);
        Task<bool> UpdateHotMessagesAsync(HotMessagesDbModel messages);
        Task<bool> UpdateHotMessageAsync(string chatId, MessageDbModel message);
        Task<GlobalSettingsDbModel> GetGlobalSettingsAsync();
        Task<BackgroundQueryDbModel> AddBackgroundQueryAsync(CreateBackgroundQueryQueryModel queryModel);
        Task<BackgroundQueryDbModel> GetBackgroundQueryAsync(string queryId);
        Task<BackgroundQueryDbModel[]> GetBackgroundQueriesByChatIdAsync(string chatId);
        Task<BackgroundQueryDbModel[]> GetPendingOrProcessingBackgroundQueryAsync();
        Task<ChatCompletionPresetsDbModel> GetChatCompletionPresetAsync(string mainChatCompletionPresetId);
        Task<LLMApiQueryDbModel[]> GetQueriesOnLLMApisAsync(string tag);
        Task<LLMApiQueryDbModel> AddNewQueryAsync(LLMApiQueryDbModel newQuery);
        Task<bool> DeleteQueryByIdAsync(string lLMApiQueryId);
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
        Task<SceneTrackerDbModel> GetSceneTrackerAsync(string chatId);
        Task<SceneTrackerDbModel> AddSceneTrackerAsync(CreateSceneTrackerQueryModel queryModel);
        Task<SceneTrackerDbModel> UpdateSceneTrackerAsync(CreateSceneTrackerQueryModel queryModel);
        Task<CharacterDbModel[]> GetCharactersAsync();
        Task<CharacterDbModel> GetCharacterByIdAsync(string characterId);
        Task<CharacterDbModel> ImportNewCharacterAsync(AddCharacterQueryModel queryModel);
        Task<bool> UpdateCharacterAsync(CharacterDbModel characterDbModel);
        Task<bool> DeleteCharacterAsync(CharacterDbModel characterDbModel);
        Task<PersonaDbModel[]> GetPersonasAsync();
        Task<PersonaDbModel> GetPersonaByIdAsync(string personaId);
        Task<bool> UpdatePersonaAsync(PersonaDbModel personaDbModel);
        Task<bool> DeletePersonaAsync(PersonaDbModel personaDbModel);
        Task<PersonaDbModel> AddEmptyPersonaAsync();
    }
}
