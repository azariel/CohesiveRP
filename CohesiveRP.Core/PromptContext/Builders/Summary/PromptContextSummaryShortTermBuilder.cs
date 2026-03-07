using CohesiveRP.Common.Diagnostics;
using CohesiveRP.Core.Services;
using CohesiveRP.Storage.DataAccessLayer.ChatCompletionPresets.BusinessObjects.Format;
using CohesiveRP.Storage.DataAccessLayer.ChatCompletionPresets.BusinessObjects.Settings;
using CohesiveRP.Storage.DataAccessLayer.Chats;
using CohesiveRP.Storage.DataAccessLayer.Messages;

namespace CohesiveRP.Core.PromptContext.Builders.Directive
{
    public class PromptContextSummaryShortTermBuilder : IPromptContextElementBuilder
    {
        private IStorageService storageService;
        private PromptContextFormatElement promptContextFormatElement;
        private ChatDbModel chatDbModel;
        private PromptContextSettings settings;

        public PromptContextSummaryShortTermBuilder(IStorageService storageService, PromptContextFormatElement promptContextFormatElement, PromptContextSettings settings, ChatDbModel chatDbModel)
        {
            this.storageService = storageService;
            this.promptContextFormatElement = promptContextFormatElement;
            this.chatDbModel = chatDbModel;
            this.settings = settings;
        }

        public async Task<string> BuildAsync()
        {
            if (promptContextFormatElement == null || chatDbModel == null)
            {
                LoggingManager.LogToFile("b73fa32a-6110-4e23-b1d5-603719ce0943", $"Invalid parameters. ChatId: [{chatDbModel?.ChatId}].");
                return null;
            }

            ShortTermSummaryDbModel shortTermSummary = await storageService.GetShortTermSummary(chatDbModel.ChatId);
            if (shortTermSummary == null)
            {
                return null;
            }

            // Inject that short term summary

            string output = $"# Previous facts, events, speech and actions (Short-Term){Environment.NewLine}";

            foreach (ISummaryDbModel summaryElement in shortTermSummary.Summaries)
            {
                // TODO: add a notion of time?
                string value = $"{promptContextFormatElement.Options?.Format?.Replace("{{item_description}}", $"{summaryElement.Content}")}";
                output += value;
            }

            return output;
        }
    }
}
