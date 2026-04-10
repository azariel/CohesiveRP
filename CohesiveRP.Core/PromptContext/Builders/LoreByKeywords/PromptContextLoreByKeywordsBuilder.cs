using System.Text;
using CohesiveRP.Common;
using CohesiveRP.Common.BusinessObjects;
using CohesiveRP.Core.PromptContext.Abstractions;
using CohesiveRP.Core.PromptContext.Builders.LoreByKeywords.BusinessObjects;
using CohesiveRP.Core.PromptContext.Utils;
using CohesiveRP.Core.Services;
using CohesiveRP.Storage.DataAccessLayer.ChatCompletionPresets.BusinessObjects.Format;
using CohesiveRP.Storage.DataAccessLayer.Chats;
using CohesiveRP.Storage.DataAccessLayer.Lorebooks.BusinessObjects;
using CohesiveRP.Storage.DataAccessLayer.Messages;
using CohesiveRP.Storage.DataAccessLayer.Messages.Hot;

namespace CohesiveRP.Core.PromptContext.Builders.Directive
{
    public class PromptContextLoreByKeywordsBuilder : IPromptContextElementBuilder
    {
        private IStorageService storageService;
        private PromptContextFormatElement promptContextFormatElement;
        private ChatDbModel chatDbModel;
        private PersonaDbModel personaLinkedToChat;
        private CharacterDbModel[] charactersLinkedToChat;

        public PromptContextLoreByKeywordsBuilder(IStorageService storageService, PromptContextFormatElement promptContextFormatElement, ChatDbModel chatDbModel, PersonaDbModel personaLinkedToChat, CharacterDbModel[] charactersLinkedToChat)
        {
            this.storageService = storageService;
            this.promptContextFormatElement = promptContextFormatElement;
            this.chatDbModel = chatDbModel;
        }

        private async Task HandleStickyEntryAsync(LorebookDbModel lorebook, LorebookInstanceDbModel instanceDbModel, List<TrackableLorebookEntry> finalEntries)
        {
            if (lorebook == null || instanceDbModel?.Entries == null || instanceDbModel.Entries.Count <= 0)
            {
                return;
            }

            foreach (var entry in instanceDbModel.Entries.Where(w => w.RemainingStickeyAmount > 0).ToArray())
            {
                if (finalEntries.Any(a => a.Entry.EntryId == entry.LorebookEntryId))
                    continue;

                var entryDbModel = lorebook.Entries.FirstOrDefault(f => f.EntryId == entry.LorebookEntryId);
                if (entryDbModel != null)
                {
                    finalEntries.Add(new TrackableLorebookEntry
                    {
                        Entry = lorebook.Entries.FirstOrDefault(f => f.EntryId == entry.LorebookEntryId),
                        LinkedMessageId = entry.LinkedMessageId,
                        LorebookId = entry.LorebookEntryId,
                    });
                }
            }
        }

        private async Task HandleCooldownEntryAsync(LorebookDbModel lorebook, LorebookInstanceDbModel instanceDbModel, List<TrackableLorebookEntry> finalEntries)
        {
            if (lorebook == null || instanceDbModel?.Entries == null || instanceDbModel.Entries.Count <= 0)
            {
                return;
            }

            foreach (var entry in instanceDbModel.Entries.Where(w => w.RemainingCooldownAmount > 0).ToArray())
            {
                finalEntries.RemoveAll(a => a.Entry.EntryId == entry.LorebookEntryId);
            }
        }

        /// <summary>
        /// Only inject the key/value when relevant in context.
        /// </summary>
        private bool EvaluateLorebookKeyTriggers(LorebookEntry entry, string contentToEvaluate, int nbTotalMessages)
        {
            // TODO: handle insertionOrder
            // TODO: handle Vectorized
            // TODO: handle PositionInPrompt, but this looks incompatible with cohesiveRP
            // TODO: handle sticky AND cooldown.. this will most likely require db interface for persistence..
            // TODO: handle IgnoreTokensBudget

            if (!entry.Enabled ||
                entry.Depth <= 0 ||
                (entry.Delay > 0 && nbTotalMessages < entry.Delay) ||
                ((entry.Keys == null || entry.Keys.Count <= 0) && entry.SelectiveLogicBetweenKeysAndSecondaryKeys == KeysEvaluationLogicGate.MainKeysOnly) ||
                ((entry.Keys == null || entry.Keys.Count <= 0) && (entry.SecondaryKeys == null || entry.SecondaryKeys.Count <= 0)))
            {
                return false;
            }

            if (entry.Constant)
            {
                return true;
            }

            StringComparison stringComparison = StringComparison.InvariantCulture;
            StringComparer stringComparer = StringComparer.InvariantCulture;
            if (!entry.CaseSensitive)
            {
                stringComparison = StringComparison.InvariantCultureIgnoreCase;
                stringComparer = StringComparer.InvariantCultureIgnoreCase;
            }

            List<string> words = contentToEvaluate.Split(" ").Select(a => a.Trim()).ToList();
            switch (entry.SelectiveLogicBetweenKeysAndSecondaryKeys)
            {
                case KeysEvaluationLogicGate.MainKeysOnly:
                    if (entry.MatchWholeWord)
                    {
                        return entry.Keys.Any(a => words.Contains(a, stringComparer));
                    } else
                    {
                        return entry.Keys.Any(a => contentToEvaluate.Contains(a, stringComparison));
                    }
                case KeysEvaluationLogicGate.OR:
                    if (entry.MatchWholeWord)
                    {
                        return entry.Keys.Any(a => words.Contains(a, stringComparer)) ||
                        entry.SecondaryKeys.Any(a => words.Contains(a, stringComparer));
                    } else
                    {
                        return entry.Keys.Any(a => contentToEvaluate.Contains(a, stringComparison)) ||
                        entry.SecondaryKeys.Any(a => contentToEvaluate.Contains(a, stringComparison));
                    }
                case KeysEvaluationLogicGate.AND:
                    if (entry.MatchWholeWord)
                    {
                        return entry.Keys.Any(a => words.Contains(a, stringComparer)) &&
                        entry.SecondaryKeys.Any(a => words.Contains(a, stringComparer));
                    } else
                    {
                        return entry.Keys.Any(a => contentToEvaluate.Contains(a, stringComparison)) &&
                        entry.SecondaryKeys.Any(a => contentToEvaluate.Contains(a, stringComparison));
                    }
            }

            return false;
        }

        public async Task<(string, IShareableContextLink)> BuildAsync()
        {
            ChatDbModel chat = await storageService.GetChatAsync(chatDbModel?.ChatId);

            if (chat?.LorebookIds == null || chat.LorebookIds.Count <= 0)
            {
                return (null, new ShareableContextLink { LinkedBuilder = this });
            }

            PersonaDbModel defaultPersona = await storageService.GetPersonaByIdAsync(chatDbModel?.PersonaId);
            string userPersonaName = defaultPersona.Name;

            // Fetch the lorebooks tethered to this chat
            var lorebooks = await storageService.GetLorebooksAsync(f => chat.LorebookIds.Contains(f.LorebookId));
            if (lorebooks == null || lorebooks.Length <= 0)
            {
                return (null, new ShareableContextLink { LinkedBuilder = this });
            }

            HotMessagesDbModel hotMessages = await storageService.GetAllHotMessagesAsync(chat.ChatId);
            IOrderedEnumerable<MessageDbModel> orderedMessages = hotMessages.Messages.OrderByDescending(o => o.CreatedAtUtc);

            int nbTotalMessages = hotMessages.NbColdMessages + hotMessages.Messages.Count;
            List<TrackableLorebookEntry> entriesToInclude = new();
            foreach (LorebookDbModel lorebook in lorebooks)
            {
                foreach (LorebookEntry entry in lorebook.Entries.Where(w => !w.OnlyTriggeredByRecursion).ToArray())
                {
                    // always start by most recent messages to know if we trigger or not
                    // Note: we should technically use Take(entry.Depth) here, but I think it's more logical to take any messages until the last player message so that we're certain we have everything, especially if there's multiple AI messages after the player message
                    var nbMessagesDepth = 0;
                    try
                    {
                        int nbMessagesToSkip = -1;
                        if(orderedMessages.FirstOrDefault()?.SourceType == MessageSourceType.User)
                            nbMessagesToSkip = 1;

                        var fileteredMessages = orderedMessages.Skip(nbMessagesToSkip);
                        nbMessagesDepth = fileteredMessages.ToList().IndexOf(fileteredMessages.FirstOrDefault(f => f.SourceType == MessageSourceType.User)) + nbMessagesToSkip;
                    } catch (Exception) { }

                    List<MessageDbModel> messagesToProcess = orderedMessages.Take(nbMessagesDepth <= 0 ? entry.Depth : nbMessagesDepth).Where(w => !string.IsNullOrWhiteSpace(w.Content)).OrderByDescending(o => o.CreatedAtUtc).ToList();
                    foreach (var messageToProcess in messagesToProcess)
                    {
                        if (!EvaluateLorebookKeyTriggers(entry, messageToProcess.Content, nbTotalMessages))
                        {
                            continue;
                        }

                        if (entriesToInclude.Any(a => a.LinkedMessageId == messageToProcess.MessageId && a.Entry == entry))
                            continue;

                        entriesToInclude.Add(new TrackableLorebookEntry
                        {
                            LorebookId = lorebook.LorebookId,
                            LinkedMessageId = messageToProcess.MessageId,
                            Entry = entry,
                        });

                        // No need to process older messages, we already have a match (that is more recent and thus more relevant for stickyness and CD)
                        break;
                    }
                }

                // Now, handle all entries that triggers on recursivity
                List<TrackableLorebookEntry> entriesAllowingRecursion = entriesToInclude.Where(w => !w.Entry.PreventRecursion).ToList();
                foreach (LorebookEntry entry in lorebook.Entries.Where(w => !entriesToInclude.Select(s => s.Entry).Contains(w) && !w.ExcludeRecursion).ToArray())
                {
                    // Second chance to this entry. We'll check if it can triggers on another entry content
                    foreach (var entryAllowingRecursion in entriesAllowingRecursion)
                    {
                        if (EvaluateLorebookKeyTriggers(entry, entryAllowingRecursion.Entry.Content, nbTotalMessages))
                        {
                            entriesToInclude.Add(entryAllowingRecursion);
                            break;
                        }
                    }

                }
            }

            // Handle probabilities
            List<TrackableLorebookEntry> finalEntries = [.. entriesToInclude.Where(w => w.Entry.ProbabilityPercentage >= 100)];
            foreach (var entry in entriesToInclude.Where(w => w.Entry.ProbabilityPercentage < 100).ToArray())
            {
                Random random = new Random(DateTime.Now.Millisecond);
                if (random.Next(0, 100) >= entry.Entry.ProbabilityPercentage)
                {
                    continue;
                }

                finalEntries.Add(entry);
            }

            var trackedLoreEntitesShareableContext = new TrackedLoreEntitesShareableContext();

            // Sticky and Cooldown entries handling
            //var lorebookInstances = await storageService.GetLorebookInstancesAsync(g => g.ChatId == chatDbModel.ChatId);
            //var stickyOrCooldownEntries = finalEntries.Where(w => w.Entry.StickyForNbMessages > 0 || w.Entry.Cooldown > 0).ToArray();
            //foreach (var lorebookInstance in lorebookInstances)
            //{
            //    // Handle Sticky and Cooldown persistence
            //    await HandleStickyAndCooldownEntryAsync(lorebookInstance, stickyOrCooldownEntries);
            //}

            // Add tracked entries so that the process can add them to the tracking table after the request is generated
            trackedLoreEntitesShareableContext.Entries ??= new();
            trackedLoreEntitesShareableContext.Entries.AddRange(finalEntries.Where(w => w.Entry.StickyForNbMessages > 0 || w.Entry.Cooldown > 0).Select(s => new LoreEntryToTrack
            {
                EntryId = s.Entry.EntryId,
                LinkedMessageId = s.LinkedMessageId,
                LorebookId = s.LorebookId,
                Cooldown = s.Entry.Cooldown,
                Sticky = s.Entry.StickyForNbMessages,
            }));

            // Get the sticky entries to add to the context from lorebookInstances and add it to finalEntries
            var lorebookInstances = await storageService.GetLorebookInstancesAsync(g => g.ChatId == chatDbModel.ChatId);
            foreach (var lorebookInstance in lorebookInstances)
            {
                // Handle Sticky
                await HandleStickyEntryAsync(lorebooks.FirstOrDefault(f => f.LorebookId == lorebookInstance.LorebookId), lorebookInstance, finalEntries);
            }

            // Remove the entries that were in cooldown in lorebookInstances and remove them from the list of finalEntries
            foreach (var lorebookInstance in lorebookInstances)
            {
                // Handle Cooldown
                await HandleCooldownEntryAsync(lorebooks.FirstOrDefault(f => f.LorebookId == lorebookInstance.LorebookId), lorebookInstance, finalEntries);
            }

            StringBuilder str = new();
            foreach (LorebookEntry entryToInclude in finalEntries.Select(s => s.Entry).Distinct().ToArray())
            {
                string value = promptContextFormatElement?.Options?.Format?
                        .Replace("{{item_header}}", entryToInclude.Name)
                        .Replace("{{item_description}}", entryToInclude.Content);

                if (value != null)
                {
                    value += Environment.NewLine;
                    str.Append(value);
                }
            }

            if (string.IsNullOrWhiteSpace(str.ToString()))
            {
                str.Append($"Infer the lore from the roleplay context.");
            }

            return ($"<lore>{Environment.NewLine}{str.ToString().InjectMacros(personaLinkedToChat?.Name, charactersLinkedToChat?.FirstOrDefault()?.Name)}{Environment.NewLine}</lore>{Environment.NewLine}{Environment.NewLine}",
                new ShareableContextLink
                {
                    LinkedBuilder = this,
                    Value = trackedLoreEntitesShareableContext,
                });
        }
    }
}
