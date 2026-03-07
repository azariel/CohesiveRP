using CohesiveRP.Common.Diagnostics;
using CohesiveRP.Core.Services;
using CohesiveRP.Storage.DataAccessLayer.ChatCompletionPresets.BusinessObjects.Format;
using CohesiveRP.Storage.DataAccessLayer.ChatCompletionPresets.BusinessObjects.Settings;
using CohesiveRP.Storage.DataAccessLayer.Chats;
using CohesiveRP.Storage.DataAccessLayer.Messages;

namespace CohesiveRP.Core.PromptContext.Builders.Directive
{
    public class PromptContextLastXMessagesBuilder : IPromptContextElementBuilder
    {
        private IStorageService storageService;
        private PromptContextFormatElement promptContextFormatElement;
        private PromptContextSettings settings;
        private ChatDbModel chatDbModel;

        public PromptContextLastXMessagesBuilder(IStorageService storageService, PromptContextFormatElement promptContextFormatElement, PromptContextSettings settings, ChatDbModel chatDbModel)
        {
            this.storageService = storageService;
            this.promptContextFormatElement = promptContextFormatElement;
            this.settings = settings;
            this.chatDbModel = chatDbModel;
        }

        public async Task<string> BuildAsync()
        {
            if (promptContextFormatElement == null || chatDbModel == null)
            {
                LoggingManager.LogToFile("8a023c77-6471-4185-9308-fdab9267d658", $"Invalid parameters. ChatId: [{chatDbModel?.ChatId}].");
                return null;
            }

            // TODO: Get the amount of messages to keep as-is from settings
            IMessageDbModel[] hotMessages = await storageService.GetAllHotMessages(chatDbModel.ChatId);
            if(hotMessages.Length <= 0)
            {
                return null;
            }

            // TODO: If user configured for more than Hot messages, fetch the cold ones as well... Really not efficient, really not as-designed, but if they really want to, we'll handle it :shrug:

            IOrderedEnumerable<IMessageDbModel> orderedMessagesByMostRecent = hotMessages.OrderByDescending(o => o.CreatedAtUtc);

            // Remove the very most recent message if it was made by the user as this is handled by the LastUserMessage builder
            int skipNb = 0;
            if(orderedMessagesByMostRecent.First().SourceType == Common.BusinessObjects.MessageSourceType.User)
            {
                skipNb = 1;
            }

            List<IMessageDbModel> selectedMessages = orderedMessagesByMostRecent.Skip(skipNb).Take(settings.LastXMessages).Reverse().ToList();

            if (selectedMessages.Count <= 0)
            {
                return string.Empty;
            }

            string output = $"# Last messages between you and the User{Environment.NewLine}";

            foreach (IMessageDbModel message in selectedMessages)
            {
                string value = $"{promptContextFormatElement.Options?.Format?.Replace("{{item_description}}", $"{message.SourceType}:{Environment.NewLine}<message>{message.Content}</message>")}";
                output += value;
            }

            return output;
        }
    }
}
