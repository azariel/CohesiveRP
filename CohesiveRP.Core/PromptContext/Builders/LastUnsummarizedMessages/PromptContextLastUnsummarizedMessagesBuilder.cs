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
    public class PromptContextLastUnsummarizedMessagesBuilder : IPromptContextElementBuilder
    {
        private IStorageService storageService;
        private PromptContextFormatElement promptContextFormatElement;
        private GlobalSettingsDbModel settings;
        private ChatDbModel chatDbModel;

        public PromptContextLastUnsummarizedMessagesBuilder(IStorageService storageService, PromptContextFormatElement promptContextFormatElement, GlobalSettingsDbModel settings, ChatDbModel chatDbModel)
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
                LoggingManager.LogToFile("c8344a9d-ebf9-4ab2-83dc-06f949e46a5c", $"Invalid parameters. ChatId: [{chatDbModel?.ChatId}].");
                return (null, new ShareableContextLink { LinkedBuilder = this });
            }

            HotMessagesDbModel hotMessagesDbModel = await storageService.GetAllHotMessagesAsync(chatDbModel.ChatId);
            if (hotMessagesDbModel.Messages.Count <= 0)
            {
                return (null, new ShareableContextLink { LinkedBuilder = this });
            }

            // TODO: handle cold storage here if all hot messages are unsummarized?

            hotMessagesDbModel.Messages = hotMessagesDbModel.Messages.Where(w => !w.Summarized).OrderBy(o => o.CreatedAtUtc).ToList();

            string output = $"<last_messages>{Environment.NewLine}";

            foreach (IMessageDbModel message in hotMessagesDbModel.Messages)
            {
                string value = $"{promptContextFormatElement.Options?.Format?.Replace("{{item_description}}", $"{message.SourceType}:{Environment.NewLine}<message>{message.Content}</message>")}";
                output += value;
            }

            output += $"{Environment.NewLine}</last_messages>{Environment.NewLine}{Environment.NewLine}";
            return (output, new ShareableContextLink
            {
                LinkedBuilder = this,
                Value = hotMessagesDbModel.Messages.Select(s => s.MessageId).ToArray()
            });
        }
    }
}
