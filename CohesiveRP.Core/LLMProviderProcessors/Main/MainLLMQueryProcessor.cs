using CohesiveRP.Common.BusinessObjects;
using CohesiveRP.Common.Diagnostics;
using CohesiveRP.Common.Serialization;
using CohesiveRP.Common.Utils.Parsers;
using CohesiveRP.Core.PromptContext.Abstractions;
using CohesiveRP.Core.PromptContext.Builders;
using CohesiveRP.Core.PromptContext.Builders.Directive;
using CohesiveRP.Core.PromptContext.Builders.LoreByKeywords.BusinessObjects;
using CohesiveRP.Core.Services;
using CohesiveRP.Core.Services.LLMApiProvider;
using CohesiveRP.Core.Services.Summary;
using CohesiveRP.Storage.DataAccessLayer.AIQueries;
using CohesiveRP.Storage.DataAccessLayer.BackgroundQueries.BusinessObjects;
using CohesiveRP.Storage.DataAccessLayer.Chats;
using CohesiveRP.Storage.DataAccessLayer.Lorebooks.BusinessObjects;
using CohesiveRP.Storage.DataAccessLayer.Messages;
using CohesiveRP.Storage.DataAccessLayer.Settings;
using CohesiveRP.Storage.QueryModels.Chat;
using CohesiveRP.Storage.QueryModels.Message;

namespace CohesiveRP.Core.LLMProviderManager.Main
{
    public class MainLLMQueryProcessor : LLMQueryProcessor
    {
        public MainLLMQueryProcessor(
            ChatCompletionPresetType completionPresetType,
            BackgroundQuerySystemTags tag,
            BackgroundQueryDbModel backgroundQueryDbModel,
            IPromptContextBuilderFactory contextBuilderFactory,
            IPromptContextElementBuilderFactory promptContextElementBuilderFactory,
            IStorageService storageService,
            IHttpLLMApiProviderService httpLLMApiProviderService,
            ISummaryService summaryService) : base(
                completionPresetType,
                tag,
                backgroundQueryDbModel,
                contextBuilderFactory,
                promptContextElementBuilderFactory,
                storageService,
                httpLLMApiProviderService,
                summaryService)
        {
        }

        private async Task HandleLorebookInstancesCreationIfRequired(ChatDbModel chat, LorebookInstanceDbModel[] lorebookInstances)
        {
            var lorebookInstancesToCreate = chat.LorebookIds.Where(w => !lorebookInstances.Any(a => a.LorebookId == w)).ToArray();
            if (lorebookInstancesToCreate.Length > 0)
            {
                foreach (var lorebookInstance in lorebookInstancesToCreate)
                {
                    await storageService.AddLorebookInstanceAsync(new LorebookInstanceDbModel
                    {
                        ChatId = chat.ChatId,
                        LorebookId = lorebookInstance,
                        Entries = [],
                    });
                }
            }
        }

        private async Task HandleExistingLorebookInstanceEntries(LorebookInstanceDbModel[] lorebookInstances)
        {
            List<LorebookStateEntry> entriesToRemove = new();
            foreach (var lorebookInstance in lorebookInstances)
            {
                foreach (var lorebookInstanceEntry in lorebookInstance.Entries)
                {
                    if (lorebookInstanceEntry.RemainingStickeyAmount > 0)
                        lorebookInstanceEntry.RemainingStickeyAmount--;

                    if (lorebookInstanceEntry.RemainingCooldownAmount > 0)
                        lorebookInstanceEntry.RemainingCooldownAmount--;

                    if (lorebookInstanceEntry.RemainingStickeyAmount <= 0 && lorebookInstanceEntry.RemainingCooldownAmount <= 0)
                    {
                        entriesToRemove.Add(lorebookInstanceEntry);
                    }
                }

                // Remove the entries that are not sticky nor in cooldown
                lorebookInstance.Entries.RemoveAll(entriesToRemove.Contains);

                await storageService.UpdateLorebookInstanceAsync(lorebookInstance);
            }
        }

        private async Task HandleNewLorebookInstanceEntries(TrackedLoreEntitesShareableContext trackedLoreEntities)
        {
            if (trackedLoreEntities?.Entries == null || trackedLoreEntities.Entries.Count <= 0)
            {
                return;
            }

            // For each entries that triggered and are either sticky-able or cooldown-able, we want to track them
            var groupedByLorebookId = trackedLoreEntities.Entries.GroupBy(g => g.LorebookId);
            foreach (IGrouping<string, LoreEntryToTrack> group in groupedByLorebookId)
            {
                var currentLorebookInstances = await storageService.GetLorebookInstancesAsync(f => f.LorebookId == group.Key);
                if (currentLorebookInstances == null || currentLorebookInstances.Length != 1)
                {
                    LoggingManager.LogToFile("d96f483e-f214-4b4b-81ab-810b96d7cf61", $"Found [{currentLorebookInstances?.Length}] lorebookInstances matching lorebookId [{group.Key}]. Ignoring adding tracked lore entries using this specific lorebook.");
                    continue;
                }

                List<LorebookStateEntry> newTrackedEntries = new();
                foreach (LoreEntryToTrack entryToTrack in group)
                {
                    // Avoid duplicate on the exact same messageId for the exact same entryId
                    if (currentLorebookInstances[0].Entries.Any(a => a.LinkedMessageId == entryToTrack.LinkedMessageId && a.LorebookEntryId == entryToTrack.EntryId))
                        continue;

                    newTrackedEntries.Add(new LorebookStateEntry
                    {
                        LorebookEntryId = entryToTrack.EntryId,
                        LinkedMessageId = entryToTrack.LinkedMessageId,
                        RemainingStickeyAmount = entryToTrack.Sticky,
                        RemainingCooldownAmount = entryToTrack.Cooldown,
                    });
                }

                currentLorebookInstances[0].Entries.AddRange(newTrackedEntries);
                await storageService.UpdateLorebookInstanceAsync(currentLorebookInstances[0]);
            }
        }

        private async Task HandleStickyAndCooldownAsync(ChatDbModel chat, TrackedLoreEntitesShareableContext trackedLoreEntities)
        {
            var lorebookInstances = await storageService.GetLorebookInstancesAsync(g => g.ChatId == backgroundQueryDbModel.ChatId);

            // Create the lorebook instances if required
            await HandleLorebookInstancesCreationIfRequired(chat, lorebookInstances);

            // Decrement lifecycle of existing tracked entries for this chat and remove them when the tracking is unrequired
            await HandleExistingLorebookInstanceEntries(lorebookInstances);

            // Track new entities
            await HandleNewLorebookInstanceEntries(trackedLoreEntities);
        }

        /// <summary>
        /// Process the resulting completed query. If it was a 'main', it'll add a new AI message, if it was a sceneTracker, it'll attach the tracker, if it was a summary, it'll attach the summary to an existing message, etc.
        /// </summary>
        public override async Task ProcessCompletedQueryAsync()
        {
            if (backgroundQueryDbModel == null || backgroundQueryDbModel.Status != BackgroundQueryStatus.ProcessingFinalInstruction)
            {
                LoggingManager.LogToFile("12498826-8f44-4f5f-ac9f-51f7de6e08fa", $"Ignoring background query [{backgroundQueryDbModel?.BackgroundQueryId}]. Status was [{backgroundQueryDbModel?.Status}].");
                return;
            }

            if (string.IsNullOrWhiteSpace(backgroundQueryDbModel.Content))
            {
                LoggingManager.LogToFile("a44dbd75-61fb-46fc-98df-dcea7eaa83c6", $"Couldn't complete backgroundTask [{backgroundQueryDbModel.BackgroundQueryId}] of Type [{tag}]. The Content was null or empty. Task will be set to Pending status for re-generation.");
                backgroundQueryDbModel.Content = null;
                backgroundQueryDbModel.Status = BackgroundQueryStatus.Pending;
                return;
            }

            // Deserialize the generic content into a list of messages
            try
            {
                LLMApiResponseMessage[] messages = JsonCommonSerializer.DeserializeFromString<LLMApiResponseMessage[]>(backgroundQueryDbModel.Content);

                var chat = await storageService.GetChatAsync(backgroundQueryDbModel.ChatId);
                foreach (var message in messages)
                {
                    // Add the AI reply message to the end of the chat
                    CreateMessageQueryModel messageQueryModel = new()
                    {
                        ChatId = backgroundQueryDbModel.ChatId,
                        SourceType = MessageSourceType.AI,
                        CharacterId = chat.CharacterIds.FirstOrDefault(),
                        AvatarId = null,
                        Summarized = false,// New message, so it's not summarized yet
                        MessageContent = ChatMessageParserUtils.ParseMessage(message.Content),
                        CreatedAtUtc = DateTime.UtcNow,
                    };

                    IMessageDbModel newMessageInStorage = await storageService.AddMessageAsync(messageQueryModel);
                    if (newMessageInStorage == null)
                    {
                        LoggingManager.LogToFile("15b7b071-b3bb-4d36-9321-4353dd747797", $"Error. The message creation in storage failed. Couldn't complete backgroundTask [{backgroundQueryDbModel.BackgroundQueryId}] of Type [{tag}]. Task will be set to Pending status for re-generation.");
                        backgroundQueryDbModel.Content = null;
                        backgroundQueryDbModel.Status = BackgroundQueryStatus.Pending;
                        return;
                    }

                    backgroundQueryDbModel.LinkedId = newMessageInStorage.MessageId;
                }

                // Iterate the lorebook sticky and cooldown
                var shareableContextLink = promptContext.ShareableContextLinks.FirstOrDefault(f => f.LinkedBuilder is PromptContextLoreByKeywordsBuilder)?.Value as TrackedLoreEntitesShareableContext;
                if (shareableContextLink != null)
                {
                    await HandleStickyAndCooldownAsync(chat, shareableContextLink);
                }

                // Right before setting the query processor to completed state, we'll launch the background workers
                if (contextBuilder == null)
                {
                    await BuildContextAsync(backgroundQueryDbModel.LinkedId);
                }

                GlobalSettingsDbModel globalSettings = await storageService.GetGlobalSettingsAsync();
                _ = summaryService.EvaluateSummaryAsync(backgroundQueryDbModel.ChatId, globalSettings);

                backgroundQueryDbModel.Status = BackgroundQueryStatus.Completed;
            } catch (Exception e)
            {
                LoggingManager.LogToFile("3323ca32-a0b4-414f-a0a7-eedea88c4099", $"Couldn't complete backgroundTask [{backgroundQueryDbModel.BackgroundQueryId}]. Task will be set to Pending status for re-generation.", e);
                backgroundQueryDbModel.Content = null;
                backgroundQueryDbModel.Status = BackgroundQueryStatus.Pending;
            }
        }
    }
}
