using System.Text;
using CohesiveRP.Common;
using CohesiveRP.Core.PromptContext.Abstractions;
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

        public PromptContextLoreByKeywordsBuilder(IStorageService storageService, PromptContextFormatElement promptContextFormatElement, ChatDbModel chatDbModel)
        {
            this.storageService = storageService;
            this.promptContextFormatElement = promptContextFormatElement;
            this.chatDbModel = chatDbModel;
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
            List<LorebookEntry> entriesToInclude = new();
            foreach (LorebookDbModel lorebook in lorebooks)
            {
                foreach (LorebookEntry entry in lorebook.Entries.Where(w => !w.OnlyTriggeredByRecursion).ToArray())
                {
                    List<MessageDbModel> messagesToProcess = orderedMessages.Take(entry.Depth).Where(w => !string.IsNullOrWhiteSpace(w.Content)).ToList();
                    if (!EvaluateLorebookKeyTriggers(entry, string.Join(" ", messagesToProcess.Select(s => s.Content).ToArray()), nbTotalMessages))
                    {
                        continue;
                    }

                    entriesToInclude.Add(entry);
                }

                // Now, handle all entries that triggers on recursivity
                List<LorebookEntry> entriesAllowingRecursion = entriesToInclude.Where(w => !w.PreventRecursion).ToList();
                foreach (LorebookEntry entry in lorebook.Entries.Where(w => !entriesToInclude.Contains(w) && !w.ExcludeRecursion).ToArray())
                {
                    // Second chance to this entry. We'll check if it can triggers on another entry content
                    foreach (var entryAllowingRecursion in entriesAllowingRecursion)
                    {
                        if (EvaluateLorebookKeyTriggers(entry, entryAllowingRecursion.Content, nbTotalMessages))
                        {
                            entriesToInclude.Add(entry);
                            break;
                        }
                    }

                }
            }

            // Handle probabilities
            List<LorebookEntry> finalEntries = [.. entriesToInclude.Where(w => w.ProbabilityPercentage >= 100)];

            foreach (var entry in entriesToInclude.Where(w => w.ProbabilityPercentage < 100).ToArray())
            {
                Random random = new Random(DateTime.Now.Millisecond);
                if (random.Next(0, 100) >= entry.ProbabilityPercentage)
                {
                    continue;
                }

                finalEntries.Add(entry);
            }

            // Sticky and Cooldown entries handling
            var lorebookInstances = await storageService.GetLorebookInstancesAsync(g => g.ChatId == chatDbModel.ChatId);
            var stickyOrCooldownEntries = entriesToInclude.Where(w => w.StickyForNbMessages > 0 || w.Cooldown > 0).ToArray();
            foreach (var lorebookInstance in lorebookInstances)
            {
                // Handle Sticky and Cooldown persistence
                await HandleStickyAndCooldownEntryAsync(lorebookInstance, stickyOrCooldownEntries);
            }

            // TODO: Get the sticky entries to add to the context from lorebookInstances and add it to entriesToInclude
            // TODO: Remove the entries that were in cooldown in lorebookInstances and remove them from the list of entriesToInclude

            StringBuilder str = new();
            foreach (LorebookEntry entryToInclude in entriesToInclude)
            {
                string value = promptContextFormatElement?.Options?.Format?
                        .Replace("{{item_header}}", entryToInclude.Name)
                        .Replace("{{item_description}}", entryToInclude.Content);

                if (value != null)
                {
                    str.Append(value);
                }
            }

            if (string.IsNullOrWhiteSpace(str.ToString()))
            {
                str.Append($"Infer the lore from the roleplay context.{Environment.NewLine}{Environment.NewLine}");
            }

            return ($"# Lore{Environment.NewLine}{str.Replace(Constants.USER_PLACEHOLDER, userPersonaName)}", new ShareableContextLink { LinkedBuilder = this });
        }

        private async Task HandleStickyAndCooldownEntryAsync(LorebookInstanceDbModel instanceDbModel, LorebookEntry[] entriesToStickOrCooldown)
        {
            // TODO: Add the entries that were triggered to the list of entriesToInclude
            foreach (var entryToStickOrCD in entriesToStickOrCooldown)
            {
                var alreadyInStorageEntry = instanceDbModel.Entries.FirstOrDefault(w => w.LorebookEntryId == entryToStickOrCD.EntryId);

                if(alreadyInStorageEntry != null)
                {
                    // Update the existing entry
                    alreadyInStorageEntry.RemainingCooldownAmount = entryToStickOrCD.Cooldown;
                    alreadyInStorageEntry.RemainingStickeyAmount = entryToStickOrCD.StickyForNbMessages;
                }

                // Otherwise, create the new entry
                instanceDbModel.Entries.Add(new LorebookStateEntry
                {
                    LorebookEntryId = entryToStickOrCD.EntryId,
                    RemainingCooldownAmount = entryToStickOrCD.Cooldown,
                    RemainingStickeyAmount = entryToStickOrCD.StickyForNbMessages
                });
            }

            await storageService.UpdateLorebookInstanceAsync(instanceDbModel);
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
            if (entry.CaseSensitive)
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
    }
}
