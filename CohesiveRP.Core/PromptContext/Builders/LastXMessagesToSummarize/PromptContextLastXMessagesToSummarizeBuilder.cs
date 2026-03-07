using CohesiveRP.Common.Diagnostics;
using CohesiveRP.Core.Services;
using CohesiveRP.Storage.DataAccessLayer.ChatCompletionPresets.BusinessObjects.Format;
using CohesiveRP.Storage.DataAccessLayer.ChatCompletionPresets.BusinessObjects.Settings;
using CohesiveRP.Storage.DataAccessLayer.Chats;
using CohesiveRP.Storage.DataAccessLayer.Messages;

namespace CohesiveRP.Core.PromptContext.Builders.Directive
{
    public class PromptContextLastXMessagesToSummarizeBuilder : IPromptContextElementBuilder
    {
        private IStorageService storageService;
        private PromptContextFormatElement promptContextFormatElement;
        private PromptContextSettings settings;
        private ChatDbModel chatDbModel;

        public PromptContextLastXMessagesToSummarizeBuilder(IStorageService storageService, PromptContextFormatElement promptContextFormatElement, PromptContextSettings settings, ChatDbModel chatDbModel)
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
                LoggingManager.LogToFile("5f5a5855-14e4-48c5-ae4f-34e78defe5c5", $"Invalid parameters. ChatId: [{chatDbModel?.ChatId}].");
                return null;
            }

            // TODO: Get the amount of messages to keep as-is from settings
            IMessageDbModel[] hotMessages = await storageService.GetAllHotMessages(chatDbModel.ChatId);
            hotMessages = hotMessages.OrderByDescending(o => o.CreatedAtUtc).Skip(settings.LastXMessages + 1).ToArray();

            if (hotMessages.Length < settings.Summary.Short.NbMessageInChunk)
            {
                // Not enough messages to summarize to short summary module
                return null;
            }

            // Filter for a chunk ordered by createdAt
            hotMessages = hotMessages.Reverse().Take(settings.Summary.Short.NbMessageInChunk).ToArray();

            string output = $"# Messages to summarize{Environment.NewLine}{promptContextFormatElement.Options?.Format}";

            string value = string.Empty;
            foreach (IMessageDbModel message in hotMessages)
            {
                value += $"{message.SourceType}:{Environment.NewLine}<message>{message.Content}</message>{Environment.NewLine}{Environment.NewLine}";
            }

            output = output.Replace("{{item_description}}", value);
            return output;
        }
    }
}
