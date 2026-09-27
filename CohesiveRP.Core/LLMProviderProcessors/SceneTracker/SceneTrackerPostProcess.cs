using CohesiveRP.Common.BusinessObjects;
using CohesiveRP.Common.Diagnostics;
using CohesiveRP.Common.Serialization;
using CohesiveRP.Common.Utils.Parsers;
using CohesiveRP.Core.LLMProviderProcessors.Pathfinder.CharactersMutations.BusinessObjects;
using CohesiveRP.Core.PromptContext.Abstractions;
using CohesiveRP.Core.Services;
using CohesiveRP.Storage.DataAccessLayer.AIQueries;
using CohesiveRP.Storage.DataAccessLayer.BackgroundQueries.BusinessObjects;
using CohesiveRP.Storage.DataAccessLayer.InteractiveUserInputQueries;
using CohesiveRP.Storage.DataAccessLayer.InteractiveUserInputQueries.BusinessObjects;
using CohesiveRP.Storage.DataAccessLayer.Messages;
using CohesiveRP.Storage.DataAccessLayer.Pathfinder.CharacterSheetInstances.BusinessObjects;
using CohesiveRP.Storage.DataAccessLayer.SceneTracker.BusinessObjects;
using CohesiveRP.Storage.DataAccessLayer.SceneTracker.BusinessObjects.Visual;
using CohesiveRP.Storage.QueryModels.BackgroundQuery;
using CohesiveRP.Storage.QueryModels.Chat;
using CohesiveRP.Storage.Utils.Characters;

namespace CohesiveRP.Core.LLMProviderProcessors.SceneTracker
{
    public class SceneTrackerPostProcess : ISceneTrackerPostProcess
    {
        IStorageService storageService;

        public SceneTrackerPostProcess(
            //IPromptContextBuilderFactory contextBuilderFactory,
            //IPromptContextElementBuilderFactory promptContextElementBuilderFactory,
            IStorageService storageService
            //IHttpLLMApiProviderService httpLLMApiProviderService,
            //ISummaryService summaryService
            )
        {
            this.storageService = storageService;
        }


        public async Task<bool> Process(ChatCompletionPresetType completionPresetType, BackgroundQuerySystemTags? tag, BackgroundQueryDbModel backgroundQueryDbModel, IShareableContextLink shareableContextLink, string sceneTrackerContent)
        {
            try
            {
                // This contains the sceneTracker result
                string sceneTrackerJson = LLMResponseParser.ParseOnlyJson(sceneTrackerContent);

                try
                {
                    JsonCommonSerializer.DeserializeFromString<VisualSceneTracker>(sceneTrackerJson);
                } catch (Exception e)
                {
                    LoggingManager.LogToFile("129fa22b-fc50-4764-89a8-0904bda8faf4", $"Couldn't complete backgroundTask [{backgroundQueryDbModel.BackgroundQueryId}] of Type [{tag}]. Invalid sceneTrackerJson. Skipping.", e);
                    backgroundQueryDbModel.Status = BackgroundQueryStatus.Pending;
                    backgroundQueryDbModel.RetryCount++;
                    return false;
                }

                VisualSceneTracker validJson = JsonCommonSerializer.DeserializeFromString<VisualSceneTracker>(sceneTrackerJson);

                string linkedMessageId = shareableContextLink.Value as string;
                SceneTrackerDbModel queryModel = new()
                {
                    ChatId = backgroundQueryDbModel.ChatId,
                    LinkMessageId = linkedMessageId,
                    Content = sceneTrackerJson,
                };

                SceneTrackerDbModel sceneTrackerDbModel = await storageService.CreateOrUpdateSceneTrackerAsync(queryModel);
                if (sceneTrackerDbModel == null)
                {
                    LoggingManager.LogToFile("62444cb8-831f-4d23-b21a-c60313fd0938", $"Couldn't complete backgroundTask [{backgroundQueryDbModel.BackgroundQueryId}] of Type [{tag}]. Couldn't update storage. Skipping.");
                    backgroundQueryDbModel.Status = BackgroundQueryStatus.Error;
                    return false;
                }

                // Queue the task to enforce coherence for characters
                await QueueCharactersCoherenceEnforcementBackgroundQuery(backgroundQueryDbModel.ChatId);

                await UpdateMessagesTimeTrackerAsync(sceneTrackerDbModel);
                await CreateNewCharactersWhenRequired(sceneTrackerDbModel);

                var sceneTrackerModelForPresence = JsonCommonSerializer.DeserializeFromString<VisualSceneTracker>(sceneTrackerDbModel.Content);
                await UpdateCharacterScenePresenceAndQueueStatusUpdatesAsync(sceneTrackerDbModel, sceneTrackerModelForPresence, linkedMessageId);

                backgroundQueryDbModel.EndFocusedGenerationDateTimeUtc = DateTime.UtcNow;
                backgroundQueryDbModel.Status = BackgroundQueryStatus.Completed;
                return true;
            } catch (Exception e)
            {
                LoggingManager.LogToFile("8da8321f-9c6d-44e5-8f26-9b5104f81464", $"Couldn't complete backgroundTask [{backgroundQueryDbModel.BackgroundQueryId}]. Task will be set to Pending status for re-generation.", e);
                backgroundQueryDbModel.Content = null;
                backgroundQueryDbModel.Status = BackgroundQueryStatus.Pending;
                backgroundQueryDbModel.RetryCount++;
                return false;
            }
        }

        /// <summary>
        /// Analyze the characters found in the sceneTracker and infer characters that don't already have a CharacterSheet. It'll then queue an InteractiveUserInputQuery for each of those characters, so the user can decide if we should analyze the story to create a new CharacterSheet for them or not.
        /// The issue here and the reason why we ask the player to decide is that the sceneTracker can track irregular or temporary characters such as 'the woman', 'the guard', etc. CharacterSheets are specific to ONE individual.
        /// </summary>
        /// <param name="sceneTrackerDbModel"></param>
        /// <returns></returns>
        private async Task CreateNewCharactersWhenRequired(SceneTrackerDbModel sceneTrackerDbModel)
        {
            var visualSceneTracker = JsonCommonSerializer.DeserializeFromString<VisualSceneTracker>(sceneTrackerDbModel.Content);
            if ((visualSceneTracker?.AllCharacterNamesActiveInScene == null || visualSceneTracker.AllCharacterNamesActiveInScene.Length <= 0) &&
                (visualSceneTracker?.CharactersAnalysis == null || visualSceneTracker.CharactersAnalysis.Length <= 0))
            {
                return;
            }

            var characterSheetInstances = await storageService.GetCharacterSheetsInstanceByChatIdAsync(sceneTrackerDbModel.ChatId);
            var allCurrentInteractiveUserInputQueries = await storageService.GetInteractiveUserInputQueriesAsync(c => c.ChatId == sceneTrackerDbModel.ChatId);
            var everyCharacterNames = GetAllCharacterNamesFromSceneTracker(visualSceneTracker);

            foreach (string characterName in everyCharacterNames)
            {
                var characterSheetInstance = CharacterSheetInstanceHelper.FindCharacterSheetInstanceFromCharacterName(characterSheetInstances?.CharacterSheetInstances, characterName);

                if (characterSheetInstances?.CharacterSheetInstances == null || characterSheetInstances.CharacterSheetInstances.Count <= 0 || characterSheetInstance == null)
                {
                    if (allCurrentInteractiveUserInputQueries == null || allCurrentInteractiveUserInputQueries.Length <= 0 || allCurrentInteractiveUserInputQueries.All(a => a == null || !string.IsNullOrWhiteSpace(a.Metadata) && !a.Metadata.ToLowerInvariant().Trim().Contains(characterName.ToLowerInvariant().Trim())))
                    {
                        await storageService.AddInteractiveUserInputQueryAsync(new InteractiveUserInputDbModel
                        {
                            ChatId = sceneTrackerDbModel.ChatId,
                            SceneTrackerId = sceneTrackerDbModel.SceneTrackerId,
                            Status = InteractiveUserInputStatus.WaitingUserInput,
                            Type = InteractiveUserInputType.NewCharacterDetected,
                            Metadata = characterName,
                        });
                    }
                }
            }
        }

        private List<string> GetAllCharacterNamesFromSceneTracker(VisualSceneTracker visualSceneTracker)
        {
            var everyCharacterNames = new List<string>();

            var allCharactersInScene = visualSceneTracker?.AllCharacterNamesActiveInScene?.Where(w => !string.IsNullOrWhiteSpace(w));
            if (allCharactersInScene != null && allCharactersInScene.Any())
                everyCharacterNames.AddRange(allCharactersInScene);

            var allDetailedCharactersInScene = visualSceneTracker?.CharactersAnalysis?.Select(s => s.Name).Where(w => !string.IsNullOrWhiteSpace(w) && everyCharacterNames.All(a => a.ToLowerInvariant().Trim() != w.ToLowerInvariant().Trim()));
            if (allDetailedCharactersInScene != null && allDetailedCharactersInScene.Any())
                everyCharacterNames.AddRange(allDetailedCharactersInScene);

            return everyCharacterNames;
        }

        private async Task UpdateMessagesTimeTrackerAsync(SceneTrackerDbModel sceneTracker)
        {
            if (sceneTracker == null)
                return;

            var hotMessagesObj = await storageService.GetAllHotMessagesAsync(sceneTracker.ChatId);
            var hotMessages = hotMessagesObj.Messages.OrderByDescending(m => m.CreatedAtUtc).ToList();
            if (hotMessages.Count <= 0)
                return;

            var indexOfUserMessageToCutChunk = hotMessages.IndexOf(hotMessages.FirstOrDefault(m => m.SourceType == MessageSourceType.User));
            if (hotMessages.First().SourceType == MessageSourceType.User)
            {
                var selection = hotMessages.Skip(1).ToArray();
                indexOfUserMessageToCutChunk = selection.IndexOf(selection.FirstOrDefault(m => m.SourceType == MessageSourceType.User));
            }

            var chunkOfMessagesToUpdate = hotMessages.Take(indexOfUserMessageToCutChunk + 1).ToList();

            var deserializedSceneTracker = JsonCommonSerializer.DeserializeFromString<BasicInformationSceneTracker>(sceneTracker.Content);
            if (deserializedSceneTracker?.CurrentDateTime == null)
                return;

            if (!DateTime.TryParse(deserializedSceneTracker.CurrentDateTime, out DateTime sceneTrackerParsedDate))
                return;

            foreach (var message in chunkOfMessagesToUpdate)
            {
                message.InRoleplayDateTime = sceneTrackerParsedDate;
            }

            var resultUpdate = await storageService.UpdateHotMessagesAsync(hotMessagesObj);

            if (!resultUpdate)
            {
                LoggingManager.LogToFile("ac42a2e5-0f77-4f15-90f1-b4956fbfcffe", $"Failed to update messages inRoleplayDateTime with sceneTracker dateTime.");
            }
        }

        private async Task UpdateCharacterScenePresenceAndQueueStatusUpdatesAsync(SceneTrackerDbModel sceneTrackerDbModel, VisualSceneTracker sceneTrackerModel, string currentMessageId)
        {
            if (sceneTrackerDbModel == null)
                return;

            var characterSheetInstancesObj = await storageService.GetCharacterSheetsInstanceByChatIdAsync(sceneTrackerDbModel.ChatId);
            if (characterSheetInstancesObj?.CharacterSheetInstances == null || characterSheetInstancesObj.CharacterSheetInstances.Count <= 0)
                return;

            // Needed to evaluate the message-count threshold against the SAME "stable" (non-mutable-tail) boundary
            // the builder itself will use, instead of approximating it with a per-cycle counter alone.
            var hotMessagesDbModel = await storageService.GetAllHotMessagesAsync(sceneTrackerDbModel.ChatId);
            List<IMessageDbModel> orderedMessages = hotMessagesDbModel?.Messages?.Cast<IMessageDbModel>().OrderBy(o => o.CreatedAtUtc).ToList() ?? new();

            int stableMessageCount = Math.Max(0, orderedMessages.Count - CharacterStatusUpdateConstants.RECENT_ACTIVITY_STABILITY_WINDOW);
            int ResolveIndex(string messageId) => string.IsNullOrWhiteSpace(messageId) ? -1 : orderedMessages.FindIndex(f => f.MessageId == messageId);

            var visualSceneTracker = JsonCommonSerializer.DeserializeFromString<VisualSceneTracker>(sceneTrackerDbModel.Content);

            var namesInScene = GetAllCharacterNamesFromSceneTracker(visualSceneTracker);
            List<CharacterStatusUpdateTarget> targetsNeedingUpdate = new();
            bool anyChange = false;

            foreach (var instance in characterSheetInstancesObj.CharacterSheetInstances.Where(w => w.CharacterSheet != null))
            {
                bool inScene = namesInScene.Any(n => CharacterSheetInstanceHelper.FindCharacterSheetInstanceFromCharacterName(new List<CharacterSheetInstance> { instance }, n) != null);

                if (inScene)
                {
                    instance.ConsecutiveMessagesAbsentFromScene = 0;
                    instance.ConsecutiveMessagesInScene++;
                    anyChange = true;

                    // The cycle counter is just an outer "don't bother checking yet" gate — a cycle can bundle a
                    // variable number of raw messages, so once the gate opens we verify against the REAL stable
                    // message count since checkpoint. This guarantees the builder's window won't come back empty
                    // (still fully inside the tail buffer), and avoids wasting an LLM call that couldn't advance
                    // the checkpoint anyway.
                    if (instance.ConsecutiveMessagesInScene >= CharacterStatusUpdateConstants.CHARACTER_STATUS_UPDATE_MESSAGE_THRESHOLD)
                    {
                        int checkpointIndex = Math.Max(ResolveIndex(instance.LastStatusCheckMessageId), ResolveIndex(instance.LastConfirmedAbsentMessageId));
                        int newStableMessagesSinceCheckpoint = stableMessageCount - (checkpointIndex + 1);

                        if (newStableMessagesSinceCheckpoint > 0)
                        {
                            targetsNeedingUpdate.Add(new CharacterStatusUpdateTarget
                            {
                                CharacterSheetInstanceId = instance.CharacterSheetInstanceId,
                                LastStatusCheckMessageId = instance.LastStatusCheckMessageId,
                                LastConfirmedAbsentMessageId = instance.LastConfirmedAbsentMessageId,
                            });

                            instance.ConsecutiveMessagesInScene = 0;
                        }
                        // else: the buffer hasn't released any new stable content yet — leave the counter elevated
                        // and re-check next cycle instead of restarting a fresh wait.
                    }
                } else if (instance.ConsecutiveMessagesInScene > 0)
                {
                    // The sceneTracker only tracks its most important characters per cycle, so one missed cycle is
                    // expected flicker, not a real exit. Require a run of consecutive absent cycles before
                    // finalizing this as a genuine departure.
                    instance.ConsecutiveMessagesAbsentFromScene++;
                    anyChange = true;

                    if (instance.ConsecutiveMessagesAbsentFromScene >= CharacterStatusUpdateConstants.RECENT_ACTIVITY_STABILITY_WINDOW)
                    {
                        int checkpointIndex = Math.Max(ResolveIndex(instance.LastStatusCheckMessageId), ResolveIndex(instance.LastConfirmedAbsentMessageId));
                        int newStableMessagesSinceCheckpoint = stableMessageCount - (checkpointIndex + 1);

                        if (newStableMessagesSinceCheckpoint > 0)
                        {
                            targetsNeedingUpdate.Add(new CharacterStatusUpdateTarget
                            {
                                CharacterSheetInstanceId = instance.CharacterSheetInstanceId,
                                LastStatusCheckMessageId = instance.LastStatusCheckMessageId,
                                LastConfirmedAbsentMessageId = instance.LastConfirmedAbsentMessageId,
                            });
                        }
                        // else: rare — the character's entire presence session was still inside the tail buffer at
                        // the moment departure got confirmed. Nothing to analyze; still finalize the departure below.

                        instance.ConsecutiveMessagesInScene = 0;
                        instance.ConsecutiveMessagesAbsentFromScene = 0;
                        instance.LastConfirmedAbsentMessageId = currentMessageId;
                    }
                } else if (instance.ConsecutiveMessagesAbsentFromScene == 0)
                {
                    // Steady-state absence: keep the checkpoint fresh so a future return doesn't pull in messages
                    // from a scene (possibly hundreds of messages back) this character wasn't part of.
                    instance.LastConfirmedAbsentMessageId = currentMessageId;
                    anyChange = true;
                }
            }

            if (anyChange)
            {
                await storageService.UpdateCharacterSheetsInstanceAsync(characterSheetInstancesObj);
            }

            if (targetsNeedingUpdate.Count > 0)
            {
                await QueueCharacterStatusUpdateAsync(sceneTrackerDbModel.ChatId, targetsNeedingUpdate);
            }
        }

        private async Task QueueCharacterStatusUpdateAsync(string chatId, List<CharacterStatusUpdateTarget> targets)
        {
            CreateBackgroundQueryQueryModel queryModel = new()
            {
                ChatId = chatId,
                Priority = BackgroundQueryPriority.Lowest,
                LinkedId = JsonCommonSerializer.SerializeToString(new CharacterStatusUpdateLinks { Targets = targets }),
                Tags = [BackgroundQuerySystemTags.characterStatusUpdate.ToString()],
                DependenciesTags = [
                    BackgroundQuerySystemTags.main.ToString(),
                    BackgroundQuerySystemTags.charactersCohesionEnforcement.ToString(),
                    BackgroundQuerySystemTags.reflection.ToString(),
                    BackgroundQuerySystemTags.relevantSummaries.ToString(),
                    BackgroundQuerySystemTags.sceneTracker.ToString(),
                    BackgroundQuerySystemTags.skillChecksInitiator.ToString(),
                ],
            };

            await storageService.AddBackgroundQueryAsync(queryModel);
        }

        private async Task QueueCharactersCoherenceEnforcementBackgroundQuery(string chatId)
        {
            CreateBackgroundQueryQueryModel queryModel = new()
            {
                ChatId = chatId,
                Priority = BackgroundQueryPriority.Highest,
                LinkedId = null,
                Tags = [BackgroundQuerySystemTags.charactersCohesionEnforcement.ToString()],
                DependenciesTags = [
                    // logical, but commented so that the worker doesn't filter out that backgroundQuery (concurrency)
                    //BackgroundQuerySystemTags.sceneTracker.ToString(),
                    //BackgroundQuerySystemTags.sceneTrackerValidator.ToString(),

                    BackgroundQuerySystemTags.skillChecksInitiator.ToString(),
                ],
            };

            await storageService.AddBackgroundQueryAsync(queryModel);
        }
    }
}
