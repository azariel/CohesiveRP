using CohesiveRP.Common.Diagnostics;
using CohesiveRP.Storage.DataAccessLayer.AIQueries;
using CohesiveRP.Storage.DataAccessLayer.ChatCompletionPresets;
using CohesiveRP.Storage.DataAccessLayer.Chats;
using CohesiveRP.Storage.DataAccessLayer.Messages;
using CohesiveRP.Storage.DataAccessLayer.Messages.Hot;
using CohesiveRP.Storage.DataAccessLayer.Settings;
using CohesiveRP.Storage.DataAccessLayer.Settings.ChatCompletionPresets;
using CohesiveRP.Storage.DataAccessLayer.Summary.Short;
using CohesiveRP.Storage.DataAccessLayer.Users;
using CohesiveRP.Storage.QueryModels.BackgroundQuery;
using CohesiveRP.Storage.QueryModels.Chat;
using CohesiveRP.Storage.QueryModels.Message;

namespace CohesiveRP.Core.Services
{
    /// <summary>
    /// Expose and handle operations to the Storage service.
    /// </summary>
    public class StorageService : IStorageService
    {
        private IChatsDal chatsDal;
        private IMessagesDal messagesDal;
        private IGlobalSettingsDal globalSettingsDal;
        private IChatCompletionPresetsDal chatCompletionPresetsDal;
        private IBackgroundQueriesDal backgroundQueriesDal;
        private ILLMApiQueriesDal llmApiQueriesDal;
        private IShortTermSummaryDal shortTermSummaryDal;

        public StorageService(
            IChatsDal chatsDal,
            IMessagesDal messagesDal,
            IGlobalSettingsDal globalSettingsDal,
            IChatCompletionPresetsDal chatCompletionPresetsDal,
            IBackgroundQueriesDal backgroundQueriesDal,
            ILLMApiQueriesDal llmApiQueriesDal,
            IShortTermSummaryDal shortTermSummaryDal)
        {
            this.chatsDal = chatsDal;
            this.messagesDal = messagesDal;
            this.globalSettingsDal = globalSettingsDal;
            this.chatCompletionPresetsDal = chatCompletionPresetsDal;
            this.backgroundQueriesDal = backgroundQueriesDal;
            this.llmApiQueriesDal = llmApiQueriesDal;
            this.shortTermSummaryDal = shortTermSummaryDal;
        }

        // Chats
        public async Task<ChatDbModel> CreateChatAsync(CreateChatQueryModel queryModel)
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

        public async Task<ChatDbModel[]> GetAllChatsAsync()
        {
            return await chatsDal.GetChatsAsync();
        }

        public async Task<ChatDbModel> GetChatAsync(string chatId)
        {
            return await chatsDal.GetChatByIdAsync(chatId);
        }

        // Messages

        public async Task<IMessageDbModel[]> GetAllHotMessages(string chatId)
        {
            return await messagesDal.GetHotMessagesAsync(chatId);
        }

        public async Task<IMessageDbModel> GetSpecificMessageAsync(string chatId, string messageId)
        {
            return await messagesDal.GetMessageByIdAsync(chatId, messageId);
        }

        public async Task<IMessageDbModel> CreateMessageAsync(CreateMessageQueryModel message)
        {
            return await messagesDal.CreateMessageAsync(message);
        }

        public async Task<bool> UpdateHotMessagesAsync(HotMessagesDbModel messages)
        {
            return await messagesDal.UpdateHotMessagesAsync(messages);
        }

        // Settings
        public async Task<GlobalSettingsDbModel> GetGlobalSettingsAsync()
        {
            return await globalSettingsDal.GetGlobalSettingsAsync();
        }

        // BackgroundQueries
        public async Task<BackgroundQueryDbModel> CreateBackgroundQueryAsync(CreateBackgroundQueryQueryModel queryModel)
        {
            return await backgroundQueriesDal.CreateBackgroundQueryAsync(queryModel);
        }

        public async Task<BackgroundQueryDbModel> GetBackgroundQueryAsync(string queryId)
        {
            return await backgroundQueriesDal.GetBackgroundQueryAsync(queryId);
        }

        public async Task<BackgroundQueryDbModel[]> GetPendingOrProcessingBackgroundQueryAsync()
        {
            return await backgroundQueriesDal.GetPendingOrProcessingBackgroundQueryAsync();
        }

        // ChatCompletionPresets
        public async Task<ChatCompletionPresetsDbModel> GetChatCompletionPreset(string mainChatCompletionPresetId)
        {
            return await chatCompletionPresetsDal.GetChatCompletionPresetsAsync(mainChatCompletionPresetId);
        }

        // LLMQueries
        public async Task<LLMApiQueryDbModel[]> GetQueriesOnLLMApisAsync(string tag)
        {
            return await llmApiQueriesDal.GetQueriesOnLLMApisAsync(tag);
        }

        public async Task<LLMApiQueryDbModel> AddNewQueryAsync(LLMApiQueryDbModel newQuery)
        {
            return await llmApiQueriesDal.AddNewQueryAsync(newQuery);
        }

        public async Task<bool> DeleteQueryByIdAsync(string lLMApiQueryId)
        {
            return await llmApiQueriesDal.DeleteQueryByIdAsync(lLMApiQueryId);
        }

        // Summary
        // --- Short-Term
        public async Task<ISummaryDbModel> CreateShortTermSummaryAsync(CreateSummaryQueryModel queryModel)
        {
            return await shortTermSummaryDal.AddShortTermSummaryAsync(queryModel);
        }

        public async Task<ShortTermSummaryDbModel> GetShortTermSummary(string chatId)
        {
            return await shortTermSummaryDal.GetShortTermSummaryAsync(chatId);
        }
    }
}
