using System.Text.Json;
using CohesiveRP.Common.Serialization;
using CohesiveRP.Core.LLMProviderProcessors.Pathfinder.CharactersMutations.BusinessObjects;
using CohesiveRP.Core.PromptContext.Abstractions;
using CohesiveRP.Core.PromptContext.Utils;
using CohesiveRP.Core.Services;
using CohesiveRP.Storage.DataAccessLayer.ChatCompletionPresets.BusinessObjects.Format;
using CohesiveRP.Storage.DataAccessLayer.Chats;
using CohesiveRP.Storage.DataAccessLayer.Messages;
using CohesiveRP.Storage.DataAccessLayer.Messages.Hot;
using CohesiveRP.Storage.DataAccessLayer.Pathfinder.CharacterSheetInstances.BusinessObjects;
using CohesiveRP.Storage.DataAccessLayer.Pathfinder.CharacterSheets.BusinessObjects;
using CohesiveRP.Storage.DataAccessLayer.SceneTracker.BusinessObjects;

namespace CohesiveRP.Core.PromptContext.Builders.Pathfinder.CharactersMutations
{
    public class PromptContextCharacterStatusUpdateBuilder : IPromptContextElementBuilder
    {
        private IStorageService storageService;
        private PromptContextFormatElement promptContextFormatElement;
        private ChatDbModel chatDbModel;
        private string linkedId;
        private PersonaDbModel personaLinkedToChat;
        private CharacterDbModel[] charactersLinkedToChat;

        public PromptContextCharacterStatusUpdateBuilder(IStorageService storageService, PromptContextFormatElement promptContextFormatElement, ChatDbModel chatDbModel, string linkedId, PersonaDbModel personaLinkedToChat, CharacterDbModel[] charactersLinkedToChat)
        {
            this.storageService = storageService;
            this.promptContextFormatElement = promptContextFormatElement;
            this.chatDbModel = chatDbModel;
            this.linkedId = linkedId;
            this.personaLinkedToChat = personaLinkedToChat;
            this.charactersLinkedToChat = charactersLinkedToChat;
        }

        private class CharacterCurrentStatus
        {
            public string CharacterName { get; set; }
            public string Profession { get; set; }
            public string[] Relationships { get; set; }
            public string LastInteractionWithPlayer { get; set; }
            public string LatentMoodForNextInteractionWithPlayer { get; set; }
            public string[] RecentImportantEvents { get; set; }
            public string[] GoalsForNextYear { get; set; }
            public CharacterStatusEffect[] MagicalEffects { get; set; }
            public CharacterStatusEffect[] BodyStatus { get; set; }
            public CharacterStatusEffect[] Wounds { get; set; }
        }

        public async Task<(string, IShareableContextLink)> BuildAsync()
        {
            if (chatDbModel == null || string.IsNullOrWhiteSpace(linkedId))
            {
                return (null, new ShareableContextLink { LinkedBuilder = this });
            }

            CharacterStatusUpdateLinks links = JsonCommonSerializer.DeserializeFromString<CharacterStatusUpdateLinks>(linkedId);
            if (links?.Targets == null || links.Targets.Count <= 0)
            {
                return (null, new ShareableContextLink { LinkedBuilder = this });
            }

            var characterSheetInstancesObj = await storageService.GetCharacterSheetsInstanceByChatIdAsync(chatDbModel.ChatId);
            if (characterSheetInstancesObj?.CharacterSheetInstances == null || characterSheetInstancesObj.CharacterSheetInstances.Count <= 0)
            {
                return (null, new ShareableContextLink { LinkedBuilder = this });
            }

            List<string> targetInstanceIds = links.Targets.Select(s => s.CharacterSheetInstanceId).ToList();
            CharacterSheetInstance[] instancesToUpdate = characterSheetInstancesObj.CharacterSheetInstances
                .Where(w => targetInstanceIds.Contains(w.CharacterSheetInstanceId) && w.CharacterSheet != null)
                .ToArray();

            if (instancesToUpdate.Length <= 0)
            {
                return (null, new ShareableContextLink { LinkedBuilder = this });
            }

            // Snapshot ONLY the fields this task is allowed to touch
            var currentStatuses = instancesToUpdate.Select(s => new CharacterCurrentStatus
            {
                CharacterName = GetCharacterFullName(s.CharacterSheet.FirstName, s.CharacterSheet.LastName),
                Profession = s.CharacterSheet.Profession,
                LatentMoodForNextInteractionWithPlayer = s.CharacterSheet.LatentMoodForNextInteractionWithPlayer,
                LastInteractionWithPlayer = s.CharacterSheet.LastInteractionWithPlayer,
                RecentImportantEvents = s.CharacterSheet.RecentImportantEvents,
                Relationships = s.CharacterSheet.Relationships,
                GoalsForNextYear = s.CharacterSheet.GoalsForNextYear,
                MagicalEffects = s.CharacterSheet.MagicalEffects,
                BodyStatus = s.CharacterSheet.BodyStatus,
                Wounds = s.CharacterSheet.Wounds,
            }).ToArray();

            string charactersCurrentStatusJson = JsonSerializer.Serialize(currentStatuses, new JsonSerializerOptions { WriteIndented = true });

            // Explicit, easy-to-scan scope list — separate from the JSON snapshot above so the model has a plain
            // target list to check itself against, rather than having to infer scope from which characterName
            // values happen to appear inside a JSON blob.
            string charactersToCheckList = string.Join(Environment.NewLine, currentStatuses.Select(s => $"- {s.CharacterName}"));

            HotMessagesDbModel hotMessagesDbModel = await storageService.GetAllHotMessagesAsync(chatDbModel.ChatId);
            List<IMessageDbModel> orderedMessages = hotMessagesDbModel?.Messages?
                .Cast<IMessageDbModel>()
                .OrderBy(o => o.CreatedAtUtc)
                .ToList() ?? new();

            int ResolveIndex(string messageId) => string.IsNullOrWhiteSpace(messageId) ? -1 : orderedMessages.FindIndex(f => f.MessageId == messageId);

            List<int> perCharacterStartIndexes = links.Targets.Select(target =>
            {
                int checkIndex = ResolveIndex(target.LastStatusCheckMessageId);
                int absentIndex = ResolveIndex(target.LastConfirmedAbsentMessageId);
                return Math.Max(checkIndex, absentIndex) + 1;
            }).ToList();

            int startIndex = Math.Max(0, perCharacterStartIndexes.Count > 0 ? perCharacterStartIndexes.Min() : 0);

            int safeEndIndexExclusive = Math.Max(0, orderedMessages.Count - CharacterStatusUpdateConstants.RECENT_ACTIVITY_STABILITY_WINDOW);
            int windowCount = Math.Max(0, safeEndIndexExclusive - startIndex);

            List<IMessageDbModel> messagesInWindow = orderedMessages.Skip(startIndex).Take(windowCount).ToList();
            string messagesContent = "";
            foreach (var message in messagesInWindow)
            {
                messagesContent += $"{message.SourceType}:{Environment.NewLine}<message>{message.Content}</message>{Environment.NewLine}";
            }
            messagesContent = messagesContent.Trim().TrimEnd(Environment.NewLine.ToCharArray());

            var sceneTracker = await storageService.GetSceneTrackerAsync(chatDbModel.ChatId);
            string currentStoryDateTime = "Unknown";
            if (sceneTracker != null && !string.IsNullOrWhiteSpace(sceneTracker.Content))
            {
                var basicInfo = JsonCommonSerializer.DeserializeFromString<BasicInformationSceneTracker>(sceneTracker.Content);
                if (!string.IsNullOrWhiteSpace(basicInfo?.CurrentDateTime))
                {
                    currentStoryDateTime = basicInfo.CurrentDateTime;
                }
            }

            string formatted = promptContextFormatElement?.Options?.Format?
                .Replace("{{characters_to_check}}", charactersToCheckList)
                .Replace("{{messages_since_last_status_check}}", messagesContent)
                .Replace("{{current_story_datetime}}", currentStoryDateTime)
                .Replace("{{characters_current_status}}", charactersCurrentStatusJson);

            return ($"{formatted?.InjectMacros(personaLinkedToChat?.Name, charactersLinkedToChat?.FirstOrDefault()?.Name)}{Environment.NewLine}{Environment.NewLine}",
                new ShareableContextLink
                {
                    LinkedBuilder = this,
                    Value = new CharacterStatusUpdateShareableLinkValue
                    {
                        CharacterSheetInstanceIds = instancesToUpdate.Select(s => s.CharacterSheetInstanceId).ToList(),
                        LastIncludedMessageId = messagesInWindow.LastOrDefault()?.MessageId,
                    }
                });
        }

        private static string GetCharacterFullName(string firstName, string lastName)
        {
            string name = firstName?.Trim();
            if (!string.IsNullOrWhiteSpace(lastName))
            {
                name = $"{firstName} {lastName}".Trim();
            }
            return name;
        }
    }
}