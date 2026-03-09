using CohesiveRP.Common.Diagnostics;
using CohesiveRP.Core.PromptContext.Abstractions;
using CohesiveRP.Core.Services;
using CohesiveRP.Storage.DataAccessLayer.ChatCompletionPresets.BusinessObjects.Format;
using CohesiveRP.Storage.DataAccessLayer.Chats;
using CohesiveRP.Storage.DataAccessLayer.Messages;
using CohesiveRP.Storage.DataAccessLayer.Settings;

namespace CohesiveRP.Core.PromptContext.Builders.Directive
{
    public class PromptContextSummaryExtraTermBuilder : IPromptContextElementBuilder
    {
        private IStorageService storageService;
        private PromptContextFormatElement promptContextFormatElement;
        private ChatDbModel chatDbModel;
        private GlobalSettingsDbModel settings;

        public PromptContextSummaryExtraTermBuilder(IStorageService storageService, PromptContextFormatElement promptContextFormatElement, GlobalSettingsDbModel settings, ChatDbModel chatDbModel)
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
                LoggingManager.LogToFile("725022ce-942f-4d83-bea6-c85c400d603b", $"Invalid parameters. ChatId: [{chatDbModel?.ChatId}].");
                return (null, new ShareableContextLink { LinkedBuilder = this });
            }

            SummaryDbModel summaryDbModel = await storageService.GetSummaryAsync(chatDbModel.ChatId);
            if (summaryDbModel?.ExtraTermSummaries == null)
            {
                // We still don't have any summary yet, so we have nothing to add to the prompt atm
                return (null, new ShareableContextLink { LinkedBuilder = this });
            }

            // Inject that short term summary
            string output = $"# Previous facts, events, speech and actions (Very Long-Term){Environment.NewLine}";

            foreach (ISummaryEntryDbModel summaryElement in summaryDbModel.ExtraTermSummaries)
            {
                // TODO: add a notion of time?
                string value = $"{promptContextFormatElement.Options?.Format?.Replace("{{item_description}}", $"{summaryElement.Content}")}";
                output += value;
            }

            output += Environment.NewLine;
            return (output,
                    new ShareableContextLink
                    {
                        LinkedBuilder = this,
                        Value = summaryDbModel.ExtraTermSummaries.Select(s => s.SummaryEntryId).ToArray()
                    });
        }
    }
}
