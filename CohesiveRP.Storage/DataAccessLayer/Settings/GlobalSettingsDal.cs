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
    /// 
    /// Pre-Main: SceneTracker, SkillChecksInitiator, ProseGuardian, NarrativeDirection
    /// Main: Main
    /// After-Main: CohesionEnforcement
    /// Downtime: Summarize, SummariesMerge, NarrativeArchitecture
    /// </summary>
    public class GlobalSettingsDal : StorageDal, IGlobalSettingsDal
    {
        private readonly IDbContextFactory<StorageDbContext> contextFactory;

        // Dev-01
        // --- Local
        //private const string LOCAL_MAIN_INFERENCE_SERVER_URL = "http://192.168.0.237:5001/v1/chat/completions";
        //private const string LOCAL_SECONDARY_INFERENCE_SERVER_URL = "http://127.0.0.1:5001/v1/chat/completions";

        //private readonly List<ChatCompletionPresetType> localInferenceServerMainMachineCompletionPresets =
        //[
        //    ChatCompletionPresetType.ProseGuardian,// PRE ~50s
        //    ChatCompletionPresetType.NarrativeDirection,// PRE ~15s
        //    ChatCompletionPresetType.Summarize,// POST++ ~45s
        //    ChatCompletionPresetType.SummariesMerge,// POST++ ~50s
        //];

        //private readonly List<ChatCompletionPresetType> localInferenceServerSecondaryMachineCompletionPresets =
        //[
        //];

        //// --- IntenseRP
        //private readonly List<ChatCompletionPresetType> GLMthinkCompletionPresets =
        //[
        //    ChatCompletionPresetType.Main,
        //];

        //private readonly List<ChatCompletionPresetType> DSthinkCompletionPresets =
        //[
        //    ChatCompletionPresetType.CharacterStatusUpdate,// POST++
        //    ChatCompletionPresetType.IllustrationPromptInjectionForCharacterAvatar,
        //    ChatCompletionPresetType.DynamicCharacterCreation,
        //    ChatCompletionPresetType.DynamicCharacterSheetCreation,
        //    ChatCompletionPresetType.SPECIAL_CharacterSheetGeneration
        //];

        //private readonly List<ChatCompletionPresetType> KimithinkCompletionPresets = [];

        //private readonly List<ChatCompletionPresetType> GLMchatCompletionPresets =
        //[
        //    ChatCompletionPresetType.SkillChecksInitiator,// PRE
        //    //ChatCompletionPresetType.NarrativeArchitecture,// POST++ (secretPlot) NOT WORKING YET
        //];

        //private readonly List<ChatCompletionPresetType> DSchatCompletionPresets =
        //[
        //    ChatCompletionPresetType.SceneTracker,// PRE
        //    //ChatCompletionPresetType.CohesionEnforcement,// POST NOT WORKING YET
        //];

        //private readonly List<ChatCompletionPresetType> KimichatCompletionPresets = [];

        // Dev-02
        // --- Local
        private const string LOCAL_MAIN_INFERENCE_SERVER_URL = "http://192.168.100.1:5001/v1/chat/completions";
        private const string LOCAL_SECONDARY_INFERENCE_SERVER_URL = "http://192.168.100.1:5001/v1/chat/completions";

        private readonly List<ChatCompletionPresetType> localInferenceServerMainMachineCompletionPresets =
        [
            ChatCompletionPresetType.NarrativeDirection,// PRE
            ChatCompletionPresetType.ProseGuardian,// PRE
            ChatCompletionPresetType.SkillChecksInitiator,// PRE
            ChatCompletionPresetType.SceneTracker,// PRE
            ChatCompletionPresetType.CharacterStatusUpdate,// POST
            //ChatCompletionPresetType.CohesionEnforcement,// POST NOT WORKING YET
            //ChatCompletionPresetType.NarrativeArchitecture,// POST++ (secretPlot) NOT WORKING YET
            ChatCompletionPresetType.Summarize,// POST++
            ChatCompletionPresetType.SummariesMerge,// POST++
            ChatCompletionPresetType.Main,
            ChatCompletionPresetType.IllustrationPromptInjectionForCharacterAvatar,
            ChatCompletionPresetType.DynamicCharacterCreation,
            ChatCompletionPresetType.DynamicCharacterSheetCreation,
            ChatCompletionPresetType.SPECIAL_CharacterSheetGeneration,
        ];

        private readonly List<ChatCompletionPresetType> localInferenceServerSecondaryMachineCompletionPresets = [];
        private readonly List<ChatCompletionPresetType> GLMthinkCompletionPresets = [];
        private readonly List<ChatCompletionPresetType> DSthinkCompletionPresets = [];
        private readonly List<ChatCompletionPresetType> KimithinkCompletionPresets = [];
        private readonly List<ChatCompletionPresetType> GLMchatCompletionPresets = [];
        private readonly List<ChatCompletionPresetType> DSchatCompletionPresets = [];
        private readonly List<ChatCompletionPresetType> KimichatCompletionPresets = [];
        // -----------

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
                            ProviderConfigId = StorageConstants.DEFAULT_LLM_PROVIDER_CONFIG_ID_7,
                            Name = "Local-Inference-Server-Main",
                            Model = "gemma-4-26b-a4b-heretic-styletune-v2-head.i1-Q4_K_M",
                            Stream = true,
                            ApiUrl = LOCAL_MAIN_INFERENCE_SERVER_URL,
                            Type = LLMProviderType.OpenAICustom,
                            Priority = LLMProviderPriority.Standard,
                            ConcurrencyLimit = 1,
                            Tags = localInferenceServerMainMachineCompletionPresets,
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
                                  ProviderConfigId = StorageConstants.DEFAULT_LLM_PROVIDER_CONFIG_ID_8,
                              },
                              new FallbackStrategy()
                              {
                                  // After 2 errors, but after this preceding fallback (if concurrrency is too high for ex), fallback to second backup provider
                                  ErrorsTreshold = 2,
                                  ProviderConfigId = StorageConstants.DEFAULT_LLM_PROVIDER_CONFIG_ID_5,
                              },
                            ]
                        },
                        new LLMProviderConfig
                        {
                            ProviderConfigId = StorageConstants.DEFAULT_LLM_PROVIDER_CONFIG_ID_8,
                            Name = "Local-Inference-Server-Secondary",
                            Model = "gemma-4-26b-a4b-heretic-styletune-v2-head.i1-Q4_K_M",
                            Stream = true,
                            ApiUrl = LOCAL_SECONDARY_INFERENCE_SERVER_URL,
                            Type = LLMProviderType.OpenAICustom,
                            Priority = LLMProviderPriority.Standard,
                            ConcurrencyLimit = 1,
                            Tags = localInferenceServerSecondaryMachineCompletionPresets,
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
                                  ProviderConfigId = StorageConstants.DEFAULT_LLM_PROVIDER_CONFIG_ID_7,
                              },
                              new FallbackStrategy()
                              {
                                  // After 2 errors, but after this preceding fallback (if concurrrency is too high for ex), fallback to second backup provider
                                  ErrorsTreshold = 2,
                                  ProviderConfigId = StorageConstants.DEFAULT_LLM_PROVIDER_CONFIG_ID_5,
                              },
                            ]
                        },
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
                            Tags = GLMthinkCompletionPresets,
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
                            Tags = DSthinkCompletionPresets,
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
                            Tags = KimithinkCompletionPresets,
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
                            Tags = GLMchatCompletionPresets,
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
                            Tags = DSchatCompletionPresets,
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
                            Tags = KimichatCompletionPresets,
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
                            //new ChatCompletionPresetsMapElement
                            //{
                            //    Type = ChatCompletionPresetType.SceneAnalyze,
                            //    ChatCompletionPresetId = StorageConstants.DEFAULT_SCENE_ANALYZE_COMPLETION_PRESET,
                            //    IsDefault = true,
                            //},
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
                            new ChatCompletionPresetsMapElement
                            {
                                Type = ChatCompletionPresetType.IllustrationPromptInjectionForCharacterAvatar,
                                ChatCompletionPresetId = StorageConstants.DEFAULT_ILLUSTRATION_PROMPT_INJECTION_FOR_CHARACTER_AVATAR_COMPLETION_PRESET,
                                IsDefault = true,
                            },
                            new ChatCompletionPresetsMapElement
                            {
                                Type = ChatCompletionPresetType.NarrativeDirection,
                                ChatCompletionPresetId = StorageConstants.DEFAULT_NARRATIVE_DIRECTION_COMPLETION_PRESET,
                                IsDefault = true,
                            },
                            new ChatCompletionPresetsMapElement
                            {
                                Type = ChatCompletionPresetType.ProseGuardian,
                                ChatCompletionPresetId = StorageConstants.DEFAULT_PROSE_GUARDIAN_COMPLETION_PRESET,
                                IsDefault = true,
                            },
                            new ChatCompletionPresetsMapElement
                            {
                                Type = ChatCompletionPresetType.CharacterStatusUpdate,
                                ChatCompletionPresetId = StorageConstants.DEFAULT_CHARACTER_STATUS_UPDATE_COMPLETION_PRESET,
                                IsDefault = true,
                            },
                            // Still working on those below
                            new ChatCompletionPresetsMapElement
                            {
                                Type = ChatCompletionPresetType.CohesionEnforcement,
                                ChatCompletionPresetId = StorageConstants.DEFAULT_COHESION_ENCORCEMENT_COMPLETION_PRESET,
                                IsDefault = true,
                            },
                            new ChatCompletionPresetsMapElement
                            {
                                Type = ChatCompletionPresetType.NarrativeArchitecture,
                                ChatCompletionPresetId = StorageConstants.DEFAULT_NARRATIVE_ARCHITECTURE_COMPLETION_PRESET,
                                IsDefault = true,
                            },
                        }
                    },
                    Summary = new SummarySettings()
                    {
                        NbRawMessagesToKeepInContext = 10,
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
