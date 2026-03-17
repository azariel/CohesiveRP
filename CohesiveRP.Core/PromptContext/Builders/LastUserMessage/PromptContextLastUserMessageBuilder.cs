using CohesiveRP.Common.Diagnostics;
using CohesiveRP.Core.PromptContext.Abstractions;
using CohesiveRP.Core.Services;
using CohesiveRP.Storage.DataAccessLayer.ChatCompletionPresets.BusinessObjects.Format;
using CohesiveRP.Storage.DataAccessLayer.Chats;
using CohesiveRP.Storage.DataAccessLayer.Messages;
using CohesiveRP.Storage.DataAccessLayer.Messages.Hot;

namespace CohesiveRP.Core.PromptContext.Builders.Directive
{
    public class PromptContextLastUserMessageBuilder : IPromptContextElementBuilder
    {
        private IStorageService storageService;
        private PromptContextFormatElement promptContextFormatElement;
        private ChatDbModel chatDbModel;

        public PromptContextLastUserMessageBuilder(IStorageService storageService, PromptContextFormatElement promptContextFormatElement, ChatDbModel chatDbModel)
        {
            this.storageService = storageService;
            this.promptContextFormatElement = promptContextFormatElement;
            this.chatDbModel = chatDbModel;
        }

        public async Task<(string, IShareableContextLink)> BuildAsync()
        {
            if (promptContextFormatElement == null || chatDbModel == null)
            {
                LoggingManager.LogToFile("8a023c77-6471-4185-9308-fdab9267d658", $"Invalid parameters. ChatId: [{chatDbModel?.ChatId}].");
                return (null, new ShareableContextLink { LinkedBuilder = this });
            }

            // TODO: Get the amount of messages to keep as-is from settings
            const int nbMessages = 5;// For Debug

            HotMessagesDbModel hotMessagesDbModel = await storageService.GetAllHotMessagesAsync(chatDbModel.ChatId);
            if (hotMessagesDbModel?.Messages == null)
            {
                return (null, new ShareableContextLink { LinkedBuilder = this });
            }

            hotMessagesDbModel.Messages = hotMessagesDbModel.Messages.Where(w => w.SourceType == Common.BusinessObjects.MessageSourceType.User).ToList();
            if (hotMessagesDbModel.Messages.Count <= 0)
            {
                // TODO: if user hasn't talked in recent messages (hot), well...we could always fetch cold I guess, but that would be highly irregular for roleplay..
                return (null, new ShareableContextLink { LinkedBuilder = this });
            }

            IMessageDbModel lastUserMessage = hotMessagesDbModel.Messages.OrderByDescending(o => o.CreatedAtUtc).First();
            PersonaDbModel linkedPersona = await storageService.GetPersonaByIdAsync(chatDbModel?.PersonaId);

            if (string.IsNullOrWhiteSpace(lastUserMessage.Content))
            {
                return (string.Empty, new ShareableContextLink { LinkedBuilder = this });
            }

            return ($"# Last message by {linkedPersona.Name}{Environment.NewLine}{promptContextFormatElement?.Options?.Format?.Replace("{{item_description}}", lastUserMessage.Content)}",
                new ShareableContextLink
                {
                    LinkedBuilder = this,
                    Value = lastUserMessage.MessageId,
                });
        }
    }
}
