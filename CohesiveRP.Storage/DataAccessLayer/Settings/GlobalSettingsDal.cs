using System.Text.Json;
using CohesiveRP.Common.Diagnostics;
using CohesiveRP.Common.Serialization;
using CohesiveRP.Storage.Common;
using CohesiveRP.Storage.DataAccessLayer.Settings;
using CohesiveRP.Storage.DataAccessLayer.Settings.ChatCompletionPresets;
using CohesiveRP.Storage.DataAccessLayer.Settings.LLMProviders;
using CohesiveRP.Storage.DataAccessLayer.Settings.LLMProviders.TimeoutStrategies;
using CohesiveRP.Storage.DataAccessLayer.Settings.Summary;
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
                    CreatedAtUtc = DateTime.UtcNow,
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
                            Tags = [ChatCompletionPresetType.Summarize, ChatCompletionPresetType.SummariesMerge, ChatCompletionPresetType.SceneTracker, ChatCompletionPresetType.SkillChecksInitiator],// TODO: move SkillChecksInitiator elsewhere, it conflicts with SceneTracker
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
                            },
                            new ChatCompletionPresetsMapElement
                            {
                                Type = ChatCompletionPresetType.SummariesMerge,
                                ChatCompletionPresetId = StorageConstants.DEFAULT_SUMMARIZES_MERGER_COMPLETION_PRESET,
                                IsDefault = true,
                            },
                            new ChatCompletionPresetsMapElement
                            {
                                Type = ChatCompletionPresetType.SceneTracker,
                                ChatCompletionPresetId = StorageConstants.DEFAULT_SCENE_TRACKER_COMPLETION_PRESET,
                                IsDefault = true,
                            },
                            new ChatCompletionPresetsMapElement
                            {
                                Type = ChatCompletionPresetType.SkillChecksInitiator,
                                ChatCompletionPresetId = StorageConstants.DEFAULT_PATHFINDER_SKILLS_CHECKS_INITIATOR_COMPLETION_PRESET,
                                IsDefault = true,
                            }
                        }
                    },
                    Summary = new SummarySettings()
                    {
                        NbRawMessagesToKeepInContext = 5,
                        HotMessagesAmountLimit = 30,
                        ColdMessagesAmountLimit = 200,
                        Short = new ShortSummaryConfig
                        {
                            NbMessageInChunk = 3,
                            MaxShortTermSummaryTokens = 4096,// Standard would probably be 4096
                        },
                        Medium = new ExtensibleSummaryConfig
                        {
                            SummarizeLastXTokens = 1024,// Standard would probably be 1024 (about 25% of Short.MaxShortTermSummaryTokens)
                            MaxTotalSummariesTokens = 2048,// Standard would probably be 2048
                        },
                        Long = new ExtensibleSummaryConfig
                        {
                            SummarizeLastXTokens = 512,// Standard would probably be 512 (about 25% of Medium.MaxShortTermSummaryTokens)
                            MaxTotalSummariesTokens = 2048,// Standard would probably be 2048
                        },
                        Extra = new ExtensibleSummaryConfig
                        {
                            SummarizeLastXTokens = 512,// Standard would probably be 512 (about 25% of Long.MaxShortTermSummaryTokens)
                            MaxTotalSummariesTokens = 2048,// Standard would probably be 2048
                        },
                        Overflow = new OverflowSummaryConfig
                        {
                            SummarizeLastXTokens = 512,// Standard would probably be 512 (about 25% of Extra.MaxShortTermSummaryTokens)
                            MaxOverflowSummaryTokens = 2048,// Standard would probably be 1024
                        },
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

        public async Task<bool> UpdateGlobalSettingsAsync(GlobalSettingsDbModel dbModel)
        {
            try
            {
                using var dbContext = await contextFactory.CreateDbContextAsync();
                var globalSettings = dbContext.GlobalSettings.FirstOrDefault();

                if (globalSettings == null)
                {
                    LoggingManager.LogToFile("d10b2253-933f-44d2-ae34-e97b0bdd6662", $"GlobalSettings to update wasn't found in storage.");
                    return false;
                }

                // Only handle overridable fields
                globalSettings.LLMProviders = dbModel.LLMProviders;
                globalSettings.ChatCompletionPresetsMap = dbModel.ChatCompletionPresetsMap;
                globalSettings.Summary = dbModel.Summary;

                var result = dbContext.GlobalSettings.Update(globalSettings);
                if (result.State != EntityState.Modified)
                {
                    LoggingManager.LogToFile("7003830b-7053-4cb2-9a7a-0f428a169c93", $"Error when updating GlobalSettings. State was [{result.State}]. Result: [{JsonCommonSerializer.SerializeToString(result)}]. dbModel: [{JsonCommonSerializer.SerializeToString(dbModel)}].");
                    return false;
                }

                await dbContext.SaveChangesAsync();
                return true;
            } catch (Exception ex)
            {
                LoggingManager.LogToFile("15ba9469-d33d-4cd8-969e-dd50621f518c", $"Error when querying pending queries on table GlobalSettings.", ex);
                return false;
            }
        }
    }
}
