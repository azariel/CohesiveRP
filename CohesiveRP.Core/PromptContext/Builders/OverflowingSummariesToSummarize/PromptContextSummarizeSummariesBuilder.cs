using CohesiveRP.Common.Utils;
using CohesiveRP.Core.Services;
using CohesiveRP.Storage.DataAccessLayer.BackgroundQueries.BusinessObjects;
using CohesiveRP.Storage.DataAccessLayer.ChatCompletionPresets.BusinessObjects.Format;
using CohesiveRP.Storage.DataAccessLayer.Chats;
using CohesiveRP.Storage.DataAccessLayer.Messages;
using CohesiveRP.Storage.DataAccessLayer.Settings;
using CohesiveRP.Storage.DataAccessLayer.Summary;

namespace CohesiveRP.Core.PromptContext.Builders.Directive
{
    public class PromptContextSummarizeSummariesBuilder : IPromptContextElementBuilder
    {
        private IStorageService storageService;
        private PromptContextFormatElement promptContextFormatElement;
        private GlobalSettingsDbModel settings; 
        private ChatDbModel chatDbModel;
        private BackgroundQuerySystemTags tag;

        public PromptContextSummarizeSummariesBuilder(IStorageService storageService, PromptContextFormatElement promptContextFormatElement, GlobalSettingsDbModel settings, ChatDbModel chatDbModel, BackgroundQuerySystemTags tag)
        {
            this.storageService = storageService;
            this.promptContextFormatElement = promptContextFormatElement;
            this.settings = settings;
            this.chatDbModel = chatDbModel;
            this.tag = tag;
        }

        public async Task<string> BuildAsync()
        {
            if (promptContextFormatElement == null || chatDbModel == null)
            {
                throw new Exception($"Invalid parameters. ChatId: [{chatDbModel?.ChatId}].");
            }

            if(tag == BackgroundQuerySystemTags.overflowSummary)
            {
                // TODO
                //return await HandleOverflowSummaryAsync();
                return null;
            }

            SummaryDbModel summaryDbModel = await storageService.GetSummaryAsync(chatDbModel.ChatId);
            List<SummaryEntryDbModel> summariesToParse = null;
            int summarizeLastXTokens = 0;

            switch (tag)
            {
                case BackgroundQuerySystemTags.shortSummary:
                    summariesToParse = summaryDbModel.ShortTermSummaries;
                    summarizeLastXTokens = settings.Summary.Medium.SummarizeLastXTokens;
                    break;
                case BackgroundQuerySystemTags.mediumSummary:
                    summariesToParse = summaryDbModel.ShortTermSummaries;
                    summarizeLastXTokens = settings.Summary.Long.SummarizeLastXTokens;
                    break;
                case BackgroundQuerySystemTags.longSummary:
                    summariesToParse = summaryDbModel.MediumTermSummaries;
                    summarizeLastXTokens = settings.Summary.Extra.SummarizeLastXTokens;
                    break;
                case BackgroundQuerySystemTags.extraSummary:
                    summariesToParse = summaryDbModel.LongTermSummaries;
                    summarizeLastXTokens = settings.Summary.Overflow.SummarizeLastXTokens;
                    break;
                default:
                    throw new Exception($"Unhandled tag [{tag}] in .");
            }

            summariesToParse ??= new();

            List<SummaryEntryDbModel> lowerTermSummariesToSummarize = new();
            int tokensCounter = 0;
            var orderedSummariesToParse = summariesToParse.OrderBy(o => o.CreatedAtUtc);
            foreach (SummaryEntryDbModel lowerTermSummary in orderedSummariesToParse)
            {
                lowerTermSummariesToSummarize.Add(lowerTermSummary);
                tokensCounter += TokensUtils.Count(lowerTermSummary.Content);

                if (tokensCounter >= summarizeLastXTokens)
                {
                    break;
                }
            }

            // You can't summarize a single chunk, so despite what the config says, we'll do at least 2
            if(lowerTermSummariesToSummarize.Count < 2 && summariesToParse.Count > 1)
            {
                lowerTermSummariesToSummarize = orderedSummariesToParse.Take(2).ToList();
            }

            string value = string.Empty;
            foreach (SummaryEntryDbModel summaryItem in lowerTermSummariesToSummarize)
            {
                value += $"<message>{summaryItem.Content}</message>{Environment.NewLine}{Environment.NewLine}";
            }

            var output = promptContextFormatElement.Options?.Format.Replace("{{item_description}}", value);
            return output;
        }
    }
}
