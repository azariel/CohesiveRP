using CohesiveRP.Common.BusinessObjects;
using CohesiveRP.Common.Diagnostics;
using CohesiveRP.Common.Serialization;
using CohesiveRP.Common.Utils;
using CohesiveRP.Core.PromptContext.Abstractions;
using CohesiveRP.Core.PromptContext.Utils;
using CohesiveRP.Core.Services;
using CohesiveRP.Storage.DataAccessLayer.ChatCompletionPresets.BusinessObjects.Format;
using CohesiveRP.Storage.DataAccessLayer.Chats;
using CohesiveRP.Storage.DataAccessLayer.Messages;
using CohesiveRP.Storage.DataAccessLayer.Messages.Hot;

namespace CohesiveRP.Core.PromptContext.Builders.Cohesion
{
    public class PromptContextStyleValidationEditionBuilder : IPromptContextElementBuilder
    {
        private IStorageService storageService;
        private PromptContextFormatElement promptContextFormatElement;
        private ChatDbModel chatDbModel;
        private PersonaDbModel personaLinkedToChat;
        private CharacterDbModel[] charactersLinkedToChat;

        public PromptContextStyleValidationEditionBuilder(IStorageService storageService, PromptContextFormatElement promptContextFormatElement, ChatDbModel chatDbModel, PersonaDbModel personaLinkedToChat, CharacterDbModel[] charactersLinkedToChat)
        {
            this.storageService = storageService;
            this.promptContextFormatElement = promptContextFormatElement;
            this.chatDbModel = chatDbModel;
            this.personaLinkedToChat = personaLinkedToChat;
            this.charactersLinkedToChat = charactersLinkedToChat;
        }

        public async Task<(string, IShareableContextLink)> BuildAsync()
        {
            var currentValuesFromStorage = await storageService.GetStyleCohesionsAsync(s => s.ChatId == chatDbModel.ChatId);
            var currentValueFromStorage = currentValuesFromStorage?.FirstOrDefault();
            if (currentValueFromStorage?.Content == null)
            {
                return (string.Empty, new ShareableContextLink { LinkedBuilder = this });
            }

            StyleValidationResult styleValidationResult = null;
            try
            {
                styleValidationResult = JsonCommonSerializer.DeserializeFromString<StyleValidationResult>(currentValueFromStorage.Content.Content);
            } catch (Exception e)
            {
                return (string.Empty, new ShareableContextLink { LinkedBuilder = this });
            }

            if (styleValidationResult?.Recommendations == null || styleValidationResult.Recommendations.Length <= 0)
            {
                return (string.Empty, new ShareableContextLink { LinkedBuilder = this });
            }

            string formattedRecommendations = string.Join(
                Environment.NewLine,
                styleValidationResult.Recommendations.Select(r => $"- {r}")
            );

            var lastReplyByAIResult = await GetLastReplyByAI();
            string lastReplyByAI = lastReplyByAIResult?.Content;
            lastReplyByAI = $"{JsonCommonSerializer.SerializeToString(SentenceSplitter.SplitIntoIdSentences(lastReplyByAI))}";

            return ($"{Environment.NewLine}{promptContextFormatElement?.Options?.Format?.InjectMacros(personaLinkedToChat?.Name, charactersLinkedToChat?.FirstOrDefault()?.Name)
                .Replace("{{description}}", formattedRecommendations)
                .Replace("{{last_reply_by_ai}}", lastReplyByAI)}",
            new ShareableContextLink { LinkedBuilder = this });
        }

        private async Task<IMessageDbModel> GetLastReplyByAI()
        {
            if (promptContextFormatElement == null || chatDbModel == null)
            {
                LoggingManager.LogToFile("93f358b7-42e4-4df7-9e51-bf2dd70c87ab", $"Invalid parameters. ChatId: [{chatDbModel?.ChatId}].");
                return null;
            }

            HotMessagesDbModel hotMessagesDbModel = await storageService.GetAllHotMessagesAsync(chatDbModel.ChatId);
            if (hotMessagesDbModel?.Messages == null)
            {
                return null;
            }

            hotMessagesDbModel.Messages = hotMessagesDbModel.Messages.Where(w => w.SourceType == Common.BusinessObjects.MessageSourceType.AI).ToList();
            if (hotMessagesDbModel.Messages.Count <= 0)
            {
                // TODO: if AI hasn't talked in recent messages (hot), well...we could always fetch cold I guess, but that would be highly irregular for roleplay..
                return null;
            }

            IMessageDbModel lastAIMessage = hotMessagesDbModel.Messages.OrderByDescending(o => o.CreatedAtUtc).First();

            if (string.IsNullOrWhiteSpace(lastAIMessage.Content))
            {
                return null;
            }

            return lastAIMessage;
        }
    }
}
