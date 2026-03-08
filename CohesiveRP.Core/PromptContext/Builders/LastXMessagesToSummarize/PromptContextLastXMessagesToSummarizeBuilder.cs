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
        private string contextLinkedId;

        public PromptContextLastXMessagesToSummarizeBuilder(IStorageService storageService, PromptContextFormatElement promptContextFormatElement, PromptContextSettings settings, ChatDbModel chatDbModel, string contextLinkedId)
        {
            this.storageService = storageService;
            this.promptContextFormatElement = promptContextFormatElement;
            this.settings = settings;
            this.chatDbModel = chatDbModel;
            this.contextLinkedId = contextLinkedId;
        }

        public async Task<string> BuildAsync()
        {
            if (promptContextFormatElement == null || chatDbModel == null || contextLinkedId == null)
            {
                throw new Exception($"Invalid parameters. ChatId: [{chatDbModel?.ChatId}].");
            }

            // TODO: Get the amount of messages to keep as-is from settings
            IMessageDbModel[] hotMessages = await storageService.GetAllHotMessages(chatDbModel.ChatId);
            hotMessages = hotMessages.OrderByDescending(o => o.CreatedAtUtc).ToArray();

            int indexOfTargetLastMessage = hotMessages.IndexOf(hotMessages.FirstOrDefault(f => f.MessageId == contextLinkedId));
            if (indexOfTargetLastMessage < 0)
            {
                throw new Exception($"Invalid contextLinkedId [{contextLinkedId}]. ChatId: [{chatDbModel?.ChatId}] hot messages did not contain this messageId.");
            }

            IMessageDbModel[] messagesToProcess = hotMessages.Skip(indexOfTargetLastMessage).Take(settings.Summary.Short.NbMessageInChunk).ToArray();

            if (messagesToProcess.Length < settings.Summary.Short.NbMessageInChunk)
            {
                throw new Exception($"Not enough messages to summarize to short summary module. ContextLinkedId: [{contextLinkedId}]. ChatId: [{chatDbModel?.ChatId}].");
            }

            // Filter for a chunk ordered by createdAt
            messagesToProcess = messagesToProcess.Reverse().ToArray();

            string output = $"# Messages to summarize{Environment.NewLine}{promptContextFormatElement.Options?.Format}";

            string value = string.Empty;
            foreach (IMessageDbModel message in messagesToProcess)
            {
                value += $"{message.SourceType}:{Environment.NewLine}<message>{message.Content}</message>{Environment.NewLine}{Environment.NewLine}";
            }

            output = output.Replace("{{item_description}}", value);
            return output;
        }
    }
}
