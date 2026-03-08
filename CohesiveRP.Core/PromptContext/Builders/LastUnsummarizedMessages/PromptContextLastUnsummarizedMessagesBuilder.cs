using CohesiveRP.Common.Diagnostics;
using CohesiveRP.Core.Services;
using CohesiveRP.Storage.DataAccessLayer.ChatCompletionPresets.BusinessObjects.Format;
using CohesiveRP.Storage.DataAccessLayer.Chats;
using CohesiveRP.Storage.DataAccessLayer.Messages;
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

        public async Task<string> BuildAsync()
        {
            if (promptContextFormatElement == null || chatDbModel == null)
            {
                LoggingManager.LogToFile("c8344a9d-ebf9-4ab2-83dc-06f949e46a5c", $"Invalid parameters. ChatId: [{chatDbModel?.ChatId}].");
                return null;
            }

            IMessageDbModel[] hotMessages = await storageService.GetAllHotMessages(chatDbModel.ChatId);
            if (hotMessages.Length <= 0)
            {
                return null;
            }

            // TODO: handle cold storage here if all hot messages are unsummarized?

            hotMessages = hotMessages.Where(w => !w.Summarized).OrderBy(o => o.CreatedAtUtc).ToArray();

            string output = $"# Last messages between you and the User{Environment.NewLine}";

            foreach (IMessageDbModel message in hotMessages)
            {
                string value = $"{promptContextFormatElement.Options?.Format?.Replace("{{item_description}}", $"{message.SourceType}:{Environment.NewLine}<message>{message.Content}</message>")}";
                output += value;
            }

            return output;
        }
    }
}
