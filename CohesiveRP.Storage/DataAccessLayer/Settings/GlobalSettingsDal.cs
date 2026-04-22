using System.Text.Json;
using CohesiveRP.Common.Diagnostics;
using CohesiveRP.Common.Serialization;
using CohesiveRP.Storage.Common;
using CohesiveRP.Storage.DataAccessLayer.LLMApiQueries.BusinessObjects;
using CohesiveRP.Storage.DataAccessLayer.Settings;
using CohesiveRP.Storage.DataAccessLayer.Settings.ChatCompletionPresets;
using CohesiveRP.Storage.DataAccessLayer.Settings.LLMProviders;
using CohesiveRP.Storage.DataAccessLayer.Settings.LLMProviders.FallbackStrategies;
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
                            ProviderConfigId = StorageConstants.DEFAULT_LLM_PROVIDER_CONFIG_ID_1,
                            Name = "IntenseRP-V2-GLM-Think",
                            Model = "glm-reasoner",
                            Stream = true,
                            ApiUrl = "http://127.0.0.1:7777/v1/chat/completions",
                            Type = LLMProviderType.OpenAICustom,
                            Priority = LLMProviderPriority.Standard,
                            ConcurrencyLimit = 1,
                            Tags = [ChatCompletionPresetType.SceneAnalyze, ChatCompletionPresetType.DynamicCharacterSheetCreation, ChatCompletionPresetType.SPECIAL_CharacterSheetGeneration],
                            TimeoutStrategy = new TimeoutStrategy
                            {
                                Type = LLMProviderTimeoutStrategyType.RetryXtimesThenGiveUp,
                                Retries = 3,
                            },
                            ErrorsBehavior = 
                            {
                                NbErrorsBeforeTimeout = 3,
                                TimeoutInSeconds = 300,
                            },
                            FallbackStrategies = [
                              new FallbackStrategy()
                              {
                                  // After 2 errors, fallback to backup provider
                                  ErrorsTreshold = 2,
                                  ErrorsTresholdBelowXToAllowFallback = 3,
                                  ProviderConfigId = StorageConstants.DEFAULT_LLM_PROVIDER_CONFIG_ID_2,
                              },    
                              new FallbackStrategy()
                              {
                                  // After 2 errors, but after this preceding fallback (if concurrrency is too high for ex), fallback to second backup provider
                                  ErrorsTreshold = 2,
                                  ProviderConfigId = StorageConstants.DEFAULT_LLM_PROVIDER_CONFIG_ID_3,
                              },
                            ]
                        },
                        new LLMProviderConfig
                        {
                            ProviderConfigId = StorageConstants.DEFAULT_LLM_PROVIDER_CONFIG_ID_2,
                            Name = "IntenseRP-V2-DS-Think",
                            Model = "deepseek-reasoner",
                            Stream = true,
                            ApiUrl = "http://127.0.0.1:7777/v1/chat/completions",
                            Type = LLMProviderType.OpenAICustom,
                            Priority = LLMProviderPriority.Standard,
                            ConcurrencyLimit = 1,
                            Tags = [ChatCompletionPresetType.DynamicCharacterCreation],
                            TimeoutStrategy = new TimeoutStrategy
                            {
                                Type = LLMProviderTimeoutStrategyType.RetryXtimesThenGiveUp,
                                Retries = 1,
                            },
                            ErrorsBehavior = 
                            {
                                NbErrorsBeforeTimeout = 3,
                                TimeoutInSeconds = 300,
                            },
                            FallbackStrategies = [
                              new FallbackStrategy()
                              {
                                  // After 3 errors, fallback to backup provider
                                  ErrorsTreshold = 3,
                                  ErrorsTresholdBelowXToAllowFallback = 3,
                                  ProviderConfigId = StorageConstants.DEFAULT_LLM_PROVIDER_CONFIG_ID_3,
                              },
                              new FallbackStrategy()
                              {
                                  // After 2 errors, but after this preceding fallback (if concurrrency is too high for ex), fallback to second backup provider
                                  ErrorsTreshold = 2,
                                  ProviderConfigId = StorageConstants.DEFAULT_LLM_PROVIDER_CONFIG_ID_1,
                              },
                            ]
                        },
                        new LLMProviderConfig
                        {
                            ProviderConfigId = StorageConstants.DEFAULT_LLM_PROVIDER_CONFIG_ID_3,
                            Name = "IntenseRP-V2-KIMI-Think",
                            Model = "moonshot-reasoner",
                            Stream = true,
                            ApiUrl = "http://127.0.0.1:7777/v1/chat/completions",
                            Type = LLMProviderType.OpenAICustom,
                            Priority = LLMProviderPriority.Standard,
                            ConcurrencyLimit = 1,
                            Tags = [ChatCompletionPresetType.Main],
                            TimeoutStrategy = new TimeoutStrategy
                            {
                                Type = LLMProviderTimeoutStrategyType.RetryXtimesThenGiveUp,
                                Retries = 3,
                            },
                            ErrorsBehavior = 
                            {
                                NbErrorsBeforeTimeout = 3,
                                TimeoutInSeconds = 300,
                            },
                            FallbackStrategies = [
                              new FallbackStrategy()
                              {
                                  // After 3 errors, fallback to backup provider
                                  ErrorsTreshold = 3,
                                  ErrorsTresholdBelowXToAllowFallback = 3,
                                  ProviderConfigId = StorageConstants.DEFAULT_LLM_PROVIDER_CONFIG_ID_2,
                              },
                              new FallbackStrategy()
                              {
                                  // After 2 errors, but after this preceding fallback (if concurrrency is too high for ex), fallback to second backup provider
                                  ErrorsTreshold = 2,
                                  ProviderConfigId = StorageConstants.DEFAULT_LLM_PROVIDER_CONFIG_ID_1,
                              },
                            ]
                        },
                        new LLMProviderConfig
                        {
                            ProviderConfigId = StorageConstants.DEFAULT_LLM_PROVIDER_CONFIG_ID_4,
                            Name = "IntenseRP-V2-GLM-Chat",
                            Model = "glm-chat",
                            Stream = true,
                            ApiUrl = "http://127.0.0.1:7778/v1/chat/completions",
                            Type = LLMProviderType.OpenAICustom,
                            Priority = LLMProviderPriority.Standard,
                            ConcurrencyLimit = 1,
                            Tags = [ChatCompletionPresetType.Summarize, ChatCompletionPresetType.SummariesMerge],// TODO: move characterSheetCreation elsewhere?
                            TimeoutStrategy = new TimeoutStrategy
                            {
                                Type = LLMProviderTimeoutStrategyType.RetryXtimesThenGiveUp,
                                Retries = 3,
                            },
                            ErrorsBehavior = 
                            {
                                NbErrorsBeforeTimeout = 3,
                                TimeoutInSeconds = 300,
                            },
                            FallbackStrategies = [
                              new FallbackStrategy()
                              {
                                  // After 2 errors, fallback to backup provider
                                  ErrorsTreshold = 2,
                                  ErrorsTresholdBelowXToAllowFallback = 3,
                                  ProviderConfigId = StorageConstants.DEFAULT_LLM_PROVIDER_CONFIG_ID_5,
                              },    
                              new FallbackStrategy()
                              {
                                  // After 2 errors, but after this preceding fallback (if concurrrency is too high for ex), fallback to second backup provider
                                  ErrorsTreshold = 2,
                                  ProviderConfigId = StorageConstants.DEFAULT_LLM_PROVIDER_CONFIG_ID_6,
                              },
                            ]
                        },
                        new LLMProviderConfig
                        {
                            ProviderConfigId = StorageConstants.DEFAULT_LLM_PROVIDER_CONFIG_ID_5,
                            Name = "IntenseRP-V2-DS-Chat",
                            Model = "deepseek-chat",
                            Stream = true,
                            ApiUrl = "http://127.0.0.1:7778/v1/chat/completions",
                            Type = LLMProviderType.OpenAICustom,
                            Priority = LLMProviderPriority.Standard,
                            ConcurrencyLimit = 1,
                            Tags = [ChatCompletionPresetType.SceneTracker],
                            TimeoutStrategy = new TimeoutStrategy
                            {
                                Type = LLMProviderTimeoutStrategyType.RetryXtimesThenGiveUp,
                                Retries = 1,
                            },
                            ErrorsBehavior = 
                            {
                                NbErrorsBeforeTimeout = 3,
                                TimeoutInSeconds = 300,
                            },
                            FallbackStrategies = [
                              new FallbackStrategy()
                              {
                                  // After 3 errors, fallback to backup provider
                                  ErrorsTreshold = 3,
                                  ErrorsTresholdBelowXToAllowFallback = 3,
                                  ProviderConfigId = StorageConstants.DEFAULT_LLM_PROVIDER_CONFIG_ID_6,
                              },
                              new FallbackStrategy()
                              {
                                  // After 2 errors, but after this preceding fallback (if concurrrency is too high for ex), fallback to second backup provider
                                  ErrorsTreshold = 2,
                                  ProviderConfigId = StorageConstants.DEFAULT_LLM_PROVIDER_CONFIG_ID_4,
                              },
                            ]
                        },
                        new LLMProviderConfig
                        {
                            ProviderConfigId = StorageConstants.DEFAULT_LLM_PROVIDER_CONFIG_ID_6,
                            Name = "IntenseRP-V2-KIMI-Chat",
                            Model = "moonshot-chat",
                            Stream = true,
                            ApiUrl = "http://127.0.0.1:7778/v1/chat/completions",
                            Type = LLMProviderType.OpenAICustom,
                            Priority = LLMProviderPriority.Standard,
                            ConcurrencyLimit = 1,
                            Tags = [ChatCompletionPresetType.SkillChecksInitiator],
                            TimeoutStrategy = new TimeoutStrategy
                            {
                                Type = LLMProviderTimeoutStrategyType.RetryXtimesThenGiveUp,
                                Retries = 3,
                            },
                            ErrorsBehavior = 
                            {
                                NbErrorsBeforeTimeout = 3,
                                TimeoutInSeconds = 300,
                            },
                            FallbackStrategies = [
                              new FallbackStrategy()
                              {
                                  // After 3 errors, fallback to backup provider
                                  ErrorsTreshold = 3,
                                  ErrorsTresholdBelowXToAllowFallback = 3,
                                  ProviderConfigId = StorageConstants.DEFAULT_LLM_PROVIDER_CONFIG_ID_5,
                              },
                              new FallbackStrategy()
                              {
                                  // After 2 errors, but after this preceding fallback (if concurrrency is too high for ex), fallback to second backup provider
                                  ErrorsTreshold = 2,
                                  ProviderConfigId = StorageConstants.DEFAULT_LLM_PROVIDER_CONFIG_ID_4,
                              },
                            ]
                        },
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
                            },
                            new ChatCompletionPresetsMapElement
                            {
                                Type = ChatCompletionPresetType.SceneAnalyze,
                                ChatCompletionPresetId = StorageConstants.DEFAULT_SCENE_ANALYZE_COMPLETION_PRESET,
                                IsDefault = true,
                            },
                            new ChatCompletionPresetsMapElement
                            {
                                Type = ChatCompletionPresetType.DynamicCharacterCreation,
                                ChatCompletionPresetId = StorageConstants.DEFAULT_DYNAMIC_CHARACTER_CREATION_COMPLETION_PRESET,
                                IsDefault = true,
                            },
                            new ChatCompletionPresetsMapElement
                            {
                                Type = ChatCompletionPresetType.DynamicCharacterSheetCreation,
                                ChatCompletionPresetId = StorageConstants.DEFAULT_DYNAMIC_CHARACTER_SHEET_CREATION_COMPLETION_PRESET,
                                IsDefault = true,
                            },
                        }
                    },
                    Summary = new SummarySettings()
                    {
                        NbRawMessagesToKeepInContext = 5,
                        HotMessagesAmountLimit = 30,
                        ColdMessagesAmountLimit = 500,
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
