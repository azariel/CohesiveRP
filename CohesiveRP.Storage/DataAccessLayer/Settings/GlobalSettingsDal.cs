using System.Text.Json;
using CohesiveRP.Common.Diagnostics;
using CohesiveRP.Storage.Common;
using CohesiveRP.Storage.DataAccessLayer.Settings;
using CohesiveRP.Storage.DataAccessLayer.Settings.ChatCompletionPresets;
using CohesiveRP.Storage.DataAccessLayer.Settings.LLMProviders;
using CohesiveRP.Storage.DataAccessLayer.Settings.LLMProviders.TimeoutStrategies;
using CohesiveRP.Storage.QueryModels.Chat;
using Microsoft.EntityFrameworkCore;

namespace CohesiveRP.Storage.DataAccessLayer.Users
{
    /// <summary>
    /// DataAccessLayer around Global Settings.
    /// </summary>
    public class GlobalSettingsDal : StorageDal, IGlobalSettingsDal
    {
        private readonly IDbContextFactory<StorageDbContext> contextFactory;

        public GlobalSettingsDal(JsonSerializerOptions jsonSerializerOptions, IDbContextFactory<StorageDbContext> contextFactory) : base(jsonSerializerOptions)
        {
            this.contextFactory = contextFactory;

            Cleanup();
        }

        private void Cleanup()
        {
            using var dbContext = contextFactory.CreateDbContext();
            dbContext.Database.EnsureCreated();

            int GlobalSettings = dbContext.GlobalSettings.Count();

            if (GlobalSettings <= 0)
            {
                // Create default settings
                dbContext.GlobalSettings.Add(new GlobalSettingsDbModel
                {
                    GlobalSettingsId = Guid.NewGuid().ToString(),
                    InsertDateTimeUtc = DateTime.UtcNow,
                    // TODO: replace this dev option
                    LLMProviders = new List<LLMProviderConfig>()
                    {
                        new LLMProviderConfig
                        {
                            ProviderConfigId = Guid.NewGuid().ToString(),
                            Name = "IntenseRP-V2-GLM",
                            Model = "glm-reasoner",
                            ApiUrl = "http://127.0.0.1:7777/v1/chat/completions",
                            Type = LLMProviderType.OpenAICustom,
                            ConcurrencyLimit = 1,
                            Tags = [ChatCompletionPresetType.Main],
                            TimeoutStrategy = new TimeoutStrategy
                            {
                                Type = LLMProviderTimeoutStrategyType.RetryXtimesThenGiveUp,
                                Retries = 3,
                            }
                        },
                        new LLMProviderConfig
                        {
                            ProviderConfigId = Guid.NewGuid().ToString(),
                            Name = "IntenseRP-V2-DS",
                            Model = "deepseek-chat",
                            ApiUrl = "http://127.0.0.1:7778/v1/chat/completions",
                            Type = LLMProviderType.OpenAICustom,
                            ConcurrencyLimit = 1,
                            Tags = [ChatCompletionPresetType.Summarize, ChatCompletionPresetType.SummariesMerge],
                            TimeoutStrategy = new TimeoutStrategy
                            {
                                Type = LLMProviderTimeoutStrategyType.RetryXtimesThenGiveUp,
                                Retries = 1,
                            }
                        }
                    },
                    ChatCompletionPresetsMap = new ChatCompletionPresetsMap()
                    {
                        Map = new List<ChatCompletionPresetsMapElement>()
                        {
                            new ChatCompletionPresetsMapElement
                            {
                                Type = ChatCompletionPresetType.Main,
                                ChatCompletionPresetId = StorageConstants.DEFAULT_CHAT_COMPLETION_PRESET,
                                IsDefault = true,
                            },
                            new ChatCompletionPresetsMapElement
                            {
                                Type = ChatCompletionPresetType.Summarize,
                                ChatCompletionPresetId = StorageConstants.DEFAULT_SUMMARIZE_COMPLETION_PRESET,
                                IsDefault = true,
                            }
                        }
                    }
                });

                dbContext.SaveChangesAsync();
                return;
            }
        }

        // ********************************************************************
        //                            Public
        // ********************************************************************
        public async Task<GlobalSettingsDbModel> GetGlobalSettingsAsync()
        {
            try
            {
                using var dbContext = await contextFactory.CreateDbContextAsync();
                return dbContext.GlobalSettings.FirstOrDefault();// TODO get by UserId eventually
            } catch (Exception ex)
            {
                LoggingManager.LogToFile("218243a4-2b6f-460e-a99c-5375e5c97ad9", $"Error when querying Db on table GlobalSettings.", ex);
                return null;
            }
        }
    }
}
