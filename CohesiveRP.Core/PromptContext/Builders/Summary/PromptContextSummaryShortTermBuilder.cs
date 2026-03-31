using CohesiveRP.Common.Diagnostics;
using CohesiveRP.Core.PromptContext.Abstractions;
using CohesiveRP.Core.Services;
using CohesiveRP.Storage.DataAccessLayer.ChatCompletionPresets.BusinessObjects.Format;
using CohesiveRP.Storage.DataAccessLayer.Chats;
using CohesiveRP.Storage.DataAccessLayer.Messages;
using CohesiveRP.Storage.DataAccessLayer.Settings;

namespace CohesiveRP.Core.PromptContext.Builders.Directive
{
    public class PromptContextSummaryShortTermBuilder : IPromptContextElementBuilder
    {
        private IStorageService storageService;
        private PromptContextFormatElement promptContextFormatElement;
        private ChatDbModel chatDbModel;
        private GlobalSettingsDbModel settings;

        public PromptContextSummaryShortTermBuilder(IStorageService storageService, PromptContextFormatElement promptContextFormatElement, GlobalSettingsDbModel settings, ChatDbModel chatDbModel)
        {
            this.storageService = storageService;
            this.promptContextFormatElement = promptContextFormatElement;
            this.chatDbModel = chatDbModel;
            this.settings = settings;
        }

        public async Task<(string, IShareableContextLink)> BuildAsync()
        {
            if (promptContextFormatElement == null || chatDbModel == null)
            {
                LoggingManager.LogToFile("b73fa32a-6110-4e23-b1d5-603719ce0943", $"Invalid parameters. ChatId: [{chatDbModel?.ChatId}].");
                return (null, new ShareableContextLink { LinkedBuilder = this });
            }

            SummaryDbModel summaryDbModel = await storageService.GetSummaryAsync(chatDbModel.ChatId);
            if (summaryDbModel?.ShortTermSummaries == null)
            {
                // We still don't have any summary yet, so we have nothing to add to the prompt atm
                return (null, new ShareableContextLink { LinkedBuilder = this });
            }

            // Inject that short term summary
            string output = $"<summary_short_term>{Environment.NewLine}Previous facts, events, speech and actions (Short-Term){Environment.NewLine}";
            foreach (ISummaryEntryDbModel summaryElement in summaryDbModel.ShortTermSummaries)
            {
                // TODO: add a notion of time?
                string value = $"{promptContextFormatElement.Options?.Format?.Replace("{{item_description}}", $"{summaryElement.Content}")}{Environment.NewLine}";
                output += value;
            }

            output += $"{Environment.NewLine}</summary_short_term>{Environment.NewLine}";
            return (output,
                    new ShareableContextLink
                    {
                        LinkedBuilder = this,
                        Value = summaryDbModel.ShortTermSummaries.Select(s => s.SummaryEntryId).ToArray()
                    });
        }
    }
}
