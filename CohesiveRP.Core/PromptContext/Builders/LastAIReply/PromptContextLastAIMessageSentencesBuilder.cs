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

namespace CohesiveRP.Core.PromptContext.Builders.Directive
{
    public class PromptContextLastAIMessageSentencesBuilder : IPromptContextElementBuilder
    {
        private IStorageService storageService;
        private PromptContextFormatElement promptContextFormatElement;
        private ChatDbModel chatDbModel;
        private PersonaDbModel personaLinkedToChat;
        private CharacterDbModel[] charactersLinkedToChat;

        public PromptContextLastAIMessageSentencesBuilder(IStorageService storageService, PromptContextFormatElement promptContextFormatElement, ChatDbModel chatDbModel, PersonaDbModel personaLinkedToChat, CharacterDbModel[] charactersLinkedToChat)
        {
            this.storageService = storageService;
            this.promptContextFormatElement = promptContextFormatElement;
            this.chatDbModel = chatDbModel;
            this.personaLinkedToChat = personaLinkedToChat;
            this.charactersLinkedToChat = charactersLinkedToChat;
        }

        public async Task<(string, IShareableContextLink)> BuildAsync()
        {
            if (promptContextFormatElement == null || chatDbModel == null)
            {
                LoggingManager.LogToFile("1958d2fc-80b7-44e6-8b43-60683b6b42ee", $"Invalid parameters. ChatId: [{chatDbModel?.ChatId}].");
                return (null, new ShareableContextLink { LinkedBuilder = this });
            }

            HotMessagesDbModel hotMessagesDbModel = await storageService.GetAllHotMessagesAsync(chatDbModel.ChatId);
            if (hotMessagesDbModel?.Messages == null)
            {
                return (null, new ShareableContextLink { LinkedBuilder = this });
            }

            hotMessagesDbModel.Messages = hotMessagesDbModel.Messages.Where(w => w.SourceType == Common.BusinessObjects.MessageSourceType.AI).ToList();
            if (hotMessagesDbModel.Messages.Count <= 0)
            {
                // TODO: if AI hasn't talked in recent messages (hot), well...we could always fetch cold I guess, but that would be highly irregular for roleplay..
                return (null, new ShareableContextLink { LinkedBuilder = this });
            }

            IMessageDbModel lastAIMessage = hotMessagesDbModel.Messages.OrderByDescending(o => o.CreatedAtUtc).First();

            if (string.IsNullOrWhiteSpace(lastAIMessage.Content))
            {
                return (string.Empty, new ShareableContextLink { LinkedBuilder = this });
            }

            string sentences = $"{JsonCommonSerializer.SerializeToString(SentenceSplitter.SplitIntoIdSentences(lastAIMessage.Content))}";
            return ($"{Environment.NewLine}{promptContextFormatElement?.Options?.Format?.Replace("{{item_description}}", sentences).InjectMacros(personaLinkedToChat?.Name, charactersLinkedToChat?.FirstOrDefault()?.Name)}{Environment.NewLine}",
                new ShareableContextLink
                {
                    LinkedBuilder = this,
                    Value = lastAIMessage.MessageId,
                });
        }
    }
}
