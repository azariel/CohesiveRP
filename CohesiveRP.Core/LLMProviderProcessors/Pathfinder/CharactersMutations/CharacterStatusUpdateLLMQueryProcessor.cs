using CohesiveRP.Common.Diagnostics;
using CohesiveRP.Common.Serialization;
using CohesiveRP.Common.Utils.Parsers;
using CohesiveRP.Core.LLMProviderManager;
using CohesiveRP.Core.LLMProviderProcessors.Pathfinder.CharactersMutations.BusinessObjects;
using CohesiveRP.Core.PromptContext.Abstractions;
using CohesiveRP.Core.PromptContext.Builders;
using CohesiveRP.Core.PromptContext.Builders.Pathfinder.CharactersMutations;
using CohesiveRP.Core.Services;
using CohesiveRP.Core.Services.Summary;
using CohesiveRP.Storage.DataAccessLayer.AIQueries;
using CohesiveRP.Storage.DataAccessLayer.BackgroundQueries.BusinessObjects;
using CohesiveRP.Storage.DataAccessLayer.Pathfinder.CharacterSheetInstances.BusinessObjects;
using CohesiveRP.Storage.DataAccessLayer.Pathfinder.CharacterSheets.BusinessObjects;
using CohesiveRP.Storage.QueryModels.Chat;

namespace CohesiveRP.Core.LLMProviderProcessors.Pathfinder.CharactersMutations
{
    internal class CharacterStatusUpdateLLMQueryProcessor : LLMQueryProcessor
    {
        public CharacterStatusUpdateLLMQueryProcessor(
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
        { }

        // TODO: generalize into a shared utility (duplicated from SceneTrackerLLMQueryProcessor)
        private CharacterSheetInstance FindInstanceByName(List<CharacterSheetInstance> instances, string characterName)
        {
            if (string.IsNullOrWhiteSpace(characterName))
                return null;

            string nameLower = characterName.ToLowerInvariant().Trim();
            return instances?.FirstOrDefault(f =>
                f.CharacterSheet.FirstName?.ToLowerInvariant().Trim() == nameLower ||
                f.CharacterSheet.LastName?.ToLowerInvariant().Trim() == nameLower ||
                $"{f.CharacterSheet.FirstName?.ToLowerInvariant().Trim()} {f.CharacterSheet.LastName?.ToLowerInvariant().Trim()}".Trim() == nameLower);
        }

        private static CharacterStatusEffect[] ApplyEffectDiff(CharacterStatusEffect[] current, CharacterStatusEffect[] toAdd, string[] toRemove)
        {
            List<CharacterStatusEffect> result = current?.ToList() ?? new();

            if (toRemove != null)
            {
                foreach (string contentToRemove in toRemove)
                {
                    // PERMANENT entries can never be removed, regardless of what the LLM requests
                    result.RemoveAll(r =>
                        string.Equals(r.Content, contentToRemove, StringComparison.OrdinalIgnoreCase) &&
                        !string.Equals(r.ExpiresAt, "PERMANENT", StringComparison.OrdinalIgnoreCase));
                }
            }

            if (toAdd != null)
            {
                foreach (var effect in toAdd)
                {
                    if (string.IsNullOrWhiteSpace(effect?.Content))
                        continue;

                    // Re-adding the same content replaces the old entry (e.g. an updated expiresAt) instead of duplicating it
                    result.RemoveAll(r => string.Equals(r.Content, effect.Content, StringComparison.OrdinalIgnoreCase));
                    result.Add(effect);
                }
            }

            return result.ToArray();
        }

        private static string[] ApplyStringListDiff(string[] current, string[] toAdd, string[] toRemove)
        {
            List<string> result = current?.ToList() ?? new();

            if (toRemove != null)
            {
                foreach (string valueToRemove in toRemove)
                {
                    result.RemoveAll(r => string.Equals(r, valueToRemove, StringComparison.OrdinalIgnoreCase));
                }
            }

            if (toAdd != null)
            {
                foreach (string valueToAdd in toAdd)
                {
                    if (string.IsNullOrWhiteSpace(valueToAdd))
                        continue;

                    result.RemoveAll(r => string.Equals(r, valueToAdd, StringComparison.OrdinalIgnoreCase));
                    result.Add(valueToAdd);
                }
            }

            return result.ToArray();
        }

        public override async Task<bool> ProcessCompletedQueryAsync()
        {
            if (!await base.ProcessCompletedQueryAsync())
            {
                backgroundQueryDbModel.Content = null;
                backgroundQueryDbModel.Status = BackgroundQueryStatus.Pending;// re-queue
                backgroundQueryDbModel.RetryCount++;
                return false;
            }

            try
            {
                IShareableContextLink shareableContextLink = promptContext.ShareableContextLinks.FirstOrDefault(f => f.LinkedBuilder is PromptContextCharacterStatusUpdateBuilder);
                if (shareableContextLink?.Value is not CharacterStatusUpdateShareableLinkValue linkValue || linkValue.CharacterSheetInstanceIds == null || linkValue.CharacterSheetInstanceIds.Count <= 0)
                {
                    LoggingManager.LogToFile("d3f8b6d1-0e1a-4b7f-9c3a-2b6e7f8a1c4d", $"No valid ShareableContextLink of type [{nameof(PromptContextCharacterStatusUpdateBuilder)}] found.");
                    return false;
                }

                string responseJson = LLMResponseParser.ParseOnlyJson(messages.First().Content);
                CharacterStatusUpdateLLMResponse response = JsonCommonSerializer.DeserializeFromString<CharacterStatusUpdateLLMResponse>(responseJson);

                var characterSheetInstancesObj = await storageService.GetCharacterSheetsInstanceByChatIdAsync(backgroundQueryDbModel.ChatId);
                if (characterSheetInstancesObj?.CharacterSheetInstances == null || characterSheetInstancesObj.CharacterSheetInstances.Count <= 0)
                {
                    backgroundQueryDbModel.Status = BackgroundQueryStatus.Error;
                    return false;
                }

                List<CharacterSheetInstance> checkedInstances = characterSheetInstancesObj.CharacterSheetInstances
                    .Where(w => linkValue.CharacterSheetInstanceIds.Contains(w.CharacterSheetInstanceId) && w.CharacterSheet != null)
                    .ToList();

                // Visibility only — confirms the matching below already discards these safely; this just tells us
                // how often the model still ignores the <characters_to_check> scope restriction in the prompt.
                int outOfScopeCount = response?.CharacterUpdates?.Count(u => !checkedInstances.Any(ci => FindInstanceByName(new List<CharacterSheetInstance> { ci }, u.CharacterName) != null)) ?? 0;
                if (outOfScopeCount > 0)
                {
                    LoggingManager.LogToFile("9a4c1e7b-3d6f-4b2a-8e91-7c5a0d9f2b3e", $"CharacterStatusUpdate LLM response included [{outOfScopeCount}] out-of-scope character update(s) for backgroundTask [{backgroundQueryDbModel.BackgroundQueryId}]; discarding as usual.");
                }

                foreach (var instance in checkedInstances)
                {
                    var update = response?.CharacterUpdates?.FirstOrDefault(f => FindInstanceByName(new List<CharacterSheetInstance> { instance }, f.CharacterName) != null);

                    if (update != null)
                    {
                        instance.CharacterSheet.MagicalEffects = ApplyEffectDiff(instance.CharacterSheet.MagicalEffects, update.MagicalEffectsToAdd, update.MagicalEffectsToRemove);
                        instance.CharacterSheet.BodyStatus = ApplyEffectDiff(instance.CharacterSheet.BodyStatus, update.BodyStatusToAdd, update.BodyStatusToRemove);
                        instance.CharacterSheet.Wounds = ApplyEffectDiff(instance.CharacterSheet.Wounds, update.WoundsToAdd, update.WoundsToRemove);
                        instance.CharacterSheet.GoalsForNextYear = ApplyStringListDiff(instance.CharacterSheet.GoalsForNextYear, update.GoalsForNextYearToAdd, update.GoalsForNextYearToRemove);
                        instance.CharacterSheet.Relationships = ApplyStringListDiff(instance.CharacterSheet.Relationships, update.RelationshipsToAdd, update.RelationshipsToRemove);

                        if (!string.IsNullOrWhiteSpace(update.Profession))
                        {
                            instance.CharacterSheet.Profession = update.Profession;
                        }
                    }

                    // Whether or not this character had changes, it was checked this cycle: advance its checkpoint and protect it from blueprint overwrite
                    instance.IsDirty = true;
                    instance.LastStatusCheckMessageId = linkValue.LastIncludedMessageId;
                }

                bool updateResult = await storageService.UpdateCharacterSheetsInstanceAsync(characterSheetInstancesObj);
                if (!updateResult)
                {
                    LoggingManager.LogToFile("6f1e9c2a-7d4b-4a8e-9f3c-1a5d8b6e2f70", $"Couldn't complete backgroundTask [{backgroundQueryDbModel.BackgroundQueryId}] of Type [{tag}]. Couldn't update characterSheetInstances tied to chatId [{backgroundQueryDbModel.ChatId}] in storage.");
                    backgroundQueryDbModel.Content = null;
                    backgroundQueryDbModel.Status = BackgroundQueryStatus.Pending;
                    backgroundQueryDbModel.RetryCount++;
                    return false;
                }

                backgroundQueryDbModel.EndFocusedGenerationDateTimeUtc = DateTime.UtcNow;
                backgroundQueryDbModel.Status = BackgroundQueryStatus.Completed;
                return true;
            } catch (Exception e)
            {
                LoggingManager.LogToFile("2c9f6e1a-4b8d-4c7e-a1f3-6d9b8e2c5a70", $"Couldn't complete backgroundTask [{backgroundQueryDbModel.BackgroundQueryId}]. Task will be set to Pending status for re-generation.", e);
                backgroundQueryDbModel.Content = null;
                backgroundQueryDbModel.Status = BackgroundQueryStatus.Pending;
                backgroundQueryDbModel.RetryCount++;
                return false;
            }
        }
    }
}