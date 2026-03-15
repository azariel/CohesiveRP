using CohesiveRP.Common.Diagnostics;
using CohesiveRP.Core.PromptContext.Abstractions;
using CohesiveRP.Core.Services;
using CohesiveRP.Storage.DataAccessLayer.ChatCompletionPresets.BusinessObjects.Format;
using CohesiveRP.Storage.DataAccessLayer.Chats;
using CohesiveRP.Storage.DataAccessLayer.Messages;
using CohesiveRP.Storage.DataAccessLayer.Messages.Hot;
using CohesiveRP.Storage.DataAccessLayer.Settings;

namespace CohesiveRP.Core.PromptContext.Builders.Directive
{
    public class PromptContextLastXMessagesBuilder : IPromptContextElementBuilder
    {
        private IStorageService storageService;
        private PromptContextFormatElement promptContextFormatElement;
        private GlobalSettingsDbModel settings;
        private ChatDbModel chatDbModel;

        public PromptContextLastXMessagesBuilder(IStorageService storageService, PromptContextFormatElement promptContextFormatElement, GlobalSettingsDbModel settings, ChatDbModel chatDbModel)
        {
            this.storageService = storageService;
            this.promptContextFormatElement = promptContextFormatElement;
            this.settings = settings;
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
            HotMessagesDbModel hotMessagesDbModel = await storageService.GetAllHotMessagesAsync(chatDbModel.ChatId);
            if (hotMessagesDbModel?.Messages == null || hotMessagesDbModel.Messages.Count <= 0)
            {
                return (null, new ShareableContextLink { LinkedBuilder = this });
            }

            // TODO: If user configured for more than Hot messages, fetch the cold ones as well... Really not efficient, really not as-designed, but if they really want to, we'll handle it :shrug:

            IOrderedEnumerable<IMessageDbModel> orderedMessagesByMostRecent = hotMessagesDbModel.Messages.OrderByDescending(o => o.CreatedAtUtc);

            // Remove the very most recent message if it was made by the user as this is handled by the LastUserMessage builder
            int skipNb = 0;
            if (orderedMessagesByMostRecent.First().SourceType == Common.BusinessObjects.MessageSourceType.User)
            {
                skipNb = 1;
            }

            List<IMessageDbModel> selectedMessages = orderedMessagesByMostRecent.Skip(skipNb).Take(settings.Summary.Short.NbMessageInChunk).Reverse().ToList();// TODO: check LastXMessages - 1?

            if (selectedMessages.Count <= 0)
            {
                return (string.Empty, new ShareableContextLink { LinkedBuilder = this });
            }

            string output = $"# Last messages between you and the User{Environment.NewLine}";

            foreach (IMessageDbModel message in selectedMessages)
            {
                string value = $"{promptContextFormatElement.Options?.Format?.Replace("{{item_description}}", $"{message.SourceType}:{Environment.NewLine}<message>{message.Content}</message>")}";
                output += value;
            }

            return (output,
                    new ShareableContextLink
                    {
                        LinkedBuilder = this,
                        Value = selectedMessages.Select(s => s.MessageId).ToArray()
                    });
        }
    }
}
