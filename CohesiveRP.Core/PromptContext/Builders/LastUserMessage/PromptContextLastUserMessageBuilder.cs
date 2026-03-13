using CohesiveRP.Common.Diagnostics;
using CohesiveRP.Core.PromptContext.Abstractions;
using CohesiveRP.Core.Services;
using CohesiveRP.Storage.DataAccessLayer.ChatCompletionPresets.BusinessObjects.Format;
using CohesiveRP.Storage.DataAccessLayer.Chats;
using CohesiveRP.Storage.DataAccessLayer.Messages;

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

            IMessageDbModel[] hotMessages = await storageService.GetAllHotMessagesAsync(chatDbModel.ChatId);
            hotMessages = hotMessages?.Where(w => w.SourceType == Common.BusinessObjects.MessageSourceType.User).ToArray();

            if (hotMessages == null || hotMessages.Length <= 0)
            {
                // TODO: if user hasn't talked in recent messages (hot), well...we could always fetch cold I guess, but that would be highly irregular for roleplay..
                return (null, new ShareableContextLink { LinkedBuilder = this });
            }

            IMessageDbModel lastUserMessage = hotMessages.OrderByDescending(o => o.CreatedAtUtc).First();
            string userPersonaName = "Azariel";// TODO: fetch from db

            if (string.IsNullOrWhiteSpace(lastUserMessage.Content))
            {
                return (string.Empty, new ShareableContextLink { LinkedBuilder = this });
            }

            return ($"# Last message by {userPersonaName}{Environment.NewLine}{promptContextFormatElement?.Options?.Format?.Replace("{{item_description}}", lastUserMessage.Content)}", 
                new ShareableContextLink
                {
                    LinkedBuilder = this,
                    Value = lastUserMessage.MessageId,
                });
        }
    }
}
