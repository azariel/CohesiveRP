using CohesiveRP.Common.Diagnostics;
using CohesiveRP.Common.Serialization;
using CohesiveRP.Common.Utils.Parsers;
using CohesiveRP.Core.PromptContext.Abstractions;
using CohesiveRP.Core.PromptContext.Builders;
using CohesiveRP.Core.PromptContext.Builders.Directive;
using CohesiveRP.Core.Services;
using CohesiveRP.Core.Services.LLMApiProvider;
using CohesiveRP.Core.Services.Summary;
using CohesiveRP.Storage.DataAccessLayer.AIQueries;
using CohesiveRP.Storage.DataAccessLayer.BackgroundQueries.BusinessObjects;
using CohesiveRP.Storage.DataAccessLayer.Messages;
using CohesiveRP.Storage.DataAccessLayer.Settings;
using CohesiveRP.Storage.DataAccessLayer.Summary;
using CohesiveRP.Storage.QueryModels.Chat;
using CohesiveRP.Storage.QueryModels.Message;

namespace CohesiveRP.Core.LLMProviderManager.Main
{
    public class SummaryMergerLLMQueryProcessor : LLMQueryProcessor
    {
        public SummaryMergerLLMQueryProcessor(
            ChatCompletionPresetType completionPresetType,
            BackgroundQuerySystemTags tag,
            BackgroundQueryDbModel backgroundQueryDbModel,
            IPromptContextBuilderFactory contextBuilderFactory,
            IPromptContextElementBuilderFactory promptContextElementBuilderFactory,
            IStorageService storageService,
            IHttpLLMApiProviderService httpLLMApiProviderService,
            ISummaryService summaryService) : base(
                completionPresetType,
                tag,
                backgroundQueryDbModel,
                contextBuilderFactory,
                promptContextElementBuilderFactory,
                storageService,
                httpLLMApiProviderService,
                summaryService)
        { }

        /// <summary>
        /// Process the resulting completed query. If it was a 'main', it'll add a new AI message, if it was a sceneTracker, it'll attach the tracker, if it was a summary, it'll attach the summary to an existing message, etc.
        /// </summary>
        public override async Task<bool> ProcessCompletedQueryAsync()
        {
            if (!await base.ProcessCompletedQueryAsync())
            {
                backgroundQueryDbModel.Content = null;
                backgroundQueryDbModel.Status = BackgroundQueryStatus.Pending;// re-queue
                return false;
            }

            // Deserialize the generic content into a list of messages
            try
            {
                if (messages.Length != 1)
                {
                    LoggingManager.LogToFile("215dc539-06ba-4b1b-9850-b3d49a8b44c4", $"Couldn't complete backgroundTask [{backgroundQueryDbModel.BackgroundQueryId}] of Type [{tag}]. The Content embedding [{messages.Length}] messages generated from the inference server. One message was expected (no more, no less). Task will be set to Pending status for re-generation.");
                    backgroundQueryDbModel.Content = null;
                    backgroundQueryDbModel.Status = BackgroundQueryStatus.Pending;// re-queue
                }

                if (!await ExecuteCompletingProcess())
                    return false;

                backgroundQueryDbModel.EndFocusedGenerationDateTimeUtc = DateTime.UtcNow;
                backgroundQueryDbModel.Status = BackgroundQueryStatus.Completed;
                return true;
            } catch (Exception e)
            {
                LoggingManager.LogToFile("e6db3317-7952-4ba9-900c-39d97ec510d3", $"Couldn't complete backgroundTask [{backgroundQueryDbModel.BackgroundQueryId}]. Task will be set to Pending status for re-generation.", e);
                backgroundQueryDbModel.Content = null;
                backgroundQueryDbModel.Status = BackgroundQueryStatus.Pending;
                return false;
            }
        }

        private async Task<bool> ExecuteCompletingProcess()
        {
            GlobalSettingsDbModel settings = await storageService.GetGlobalSettingsAsync();
            SummaryDbModel summaryDbModel = await storageService.GetSummaryAsync(backgroundQueryDbModel.ChatId);
            List<SummaryEntryDbModel> lowerTermSummariesToSummarize = new();

            IShareableContextLink shareableContextLink = promptContext.ShareableContextLinks.FirstOrDefault(f => f.LinkedBuilder is PromptContextSummarizeSummariesBuilder);
            if (shareableContextLink == null)
            {
                LoggingManager.LogToFile("b41cdb3d-4013-4fe7-bb09-6104afc0b3f3", $"No ShareableContextLink of type [{nameof(PromptContextSummarizeSummariesBuilder)} found.]");
                return false;
            }

            // This one is special
            if (tag == BackgroundQuerySystemTags.overflowSummary)
            {
                return await HandleOverflowSummaryAsync(backgroundQueryDbModel.ChatId, settings, summaryDbModel, shareableContextLink);
            }

            switch (tag)
            {
                case BackgroundQuerySystemTags.mediumSummary:
                    lowerTermSummariesToSummarize = summaryDbModel.ShortTermSummaries.Where(w => ((string[])shareableContextLink.Value).Contains(w.SummaryEntryId)).ToList();
                    break;
                case BackgroundQuerySystemTags.longSummary:
                    lowerTermSummariesToSummarize = summaryDbModel.MediumTermSummaries.Where(w => ((string[])shareableContextLink.Value).Contains(w.SummaryEntryId)).ToList();
                    break;
                case BackgroundQuerySystemTags.extraSummary:
                    lowerTermSummariesToSummarize = summaryDbModel.LongTermSummaries.Where(w => ((string[])shareableContextLink.Value).Contains(w.SummaryEntryId)).ToList();
                    break;
                default:
                    throw new Exception($"Unhandled tag [{tag}] in .");
            }

            // We now need to remove the summaries that were merged and add the merged summary to the next tier
            // Add new summary to higher tier
            LLMApiResponseMessage[] messages = JsonCommonSerializer.DeserializeFromString<LLMApiResponseMessage[]>(backgroundQueryDbModel.Content);

            if (messages.Length != 1)
            {
                LoggingManager.LogToFile("aa1dc4b6-635f-4d42-91c8-b525c16b0684", $"Couldn't complete backgroundTask [{backgroundQueryDbModel.BackgroundQueryId}] of Type [{tag}]. The Content embedding [{messages.Length}] messages generated from the inference server. One message was expected (no more, no less). Task will be set to Pending status for re-generation.");
                backgroundQueryDbModel.Content = null;
                backgroundQueryDbModel.Status = BackgroundQueryStatus.Pending;// re-queue
                return false;
            }

            CreateSummaryQueryModel messageQueryModel = new()
            {
                ChatId = backgroundQueryDbModel.ChatId,
                MessageIdTracker = backgroundQueryDbModel.LinkedId,
                Content = ChatMessageParserUtils.ParseMessage(messages[0].Content),
                CreatedAtUtc = DateTime.UtcNow,
            };

            switch (tag)
            {
                case BackgroundQuerySystemTags.mediumSummary:
                    if (await storageService.AddMediumTermSummaryAsync(messageQueryModel) == null)
                        return false;

                    if (!await storageService.DeleteShortTermSummariesEntriesAsync(backgroundQueryDbModel.ChatId, [.. lowerTermSummariesToSummarize.Select(s => s.SummaryEntryId)]))
                        return false;
                    break;
                case BackgroundQuerySystemTags.longSummary:
                    if (await storageService.AddLongTermSummaryAsync(messageQueryModel) == null)
                        return false;

                    if (!await storageService.DeleteMediumTermSummariesEntriesAsync(backgroundQueryDbModel.ChatId, [.. lowerTermSummariesToSummarize.Select(s => s.SummaryEntryId)]))
                        return false;
                    break;
                case BackgroundQuerySystemTags.extraSummary:
                    if (await storageService.AddExtraTermSummaryAsync(messageQueryModel) == null)
                        return false;

                    if (!await storageService.DeleteLongTermSummariesEntriesAsync(backgroundQueryDbModel.ChatId, [.. lowerTermSummariesToSummarize.Select(s => s.SummaryEntryId)]))
                        return false;
                    break;
                default:
                    throw new Exception($"Unhandled tag [{tag}] in .");
            }

            return true;
        }

        private async Task<bool> HandleOverflowSummaryAsync(string chatId, GlobalSettingsDbModel settings, SummaryDbModel summaryDbModel, IShareableContextLink shareableContextLink)
        {
            var summariesProcessed = summaryDbModel.ExtraTermSummaries.Where(w => ((string[])shareableContextLink.Value).Contains(w.SummaryEntryId)).ToList();

            // We now need to remove the summaries that were merged and add the merged summary to the next tier
            // REPLACE new summary to higher tier
            LLMApiResponseMessage[] messages = JsonCommonSerializer.DeserializeFromString<LLMApiResponseMessage[]>(backgroundQueryDbModel.Content);

            if (messages.Length != 1)
            {
                LoggingManager.LogToFile("dcd06d1e-a7f4-4147-8f2d-6671d976b57e", $"Couldn't complete backgroundTask [{backgroundQueryDbModel.BackgroundQueryId}] of Type [{tag}]. The Content embedding [{messages.Length}] messages generated from the inference server. One message was expected (no more, no less). Task will be set to Pending status for re-generation.");
                backgroundQueryDbModel.Content = null;
                backgroundQueryDbModel.Status = BackgroundQueryStatus.Pending;// re-queue
                return false;
            }

            CreateSummaryQueryModel messageQueryModel = new()
            {
                ChatId = backgroundQueryDbModel.ChatId,
                MessageIdTracker = backgroundQueryDbModel.LinkedId,
                Content = ChatMessageParserUtils.ParseMessage(messages[0].Content),
                CreatedAtUtc = DateTime.UtcNow,
            };

            if (summaryDbModel.OverflowTermSummaries != null && summaryDbModel.OverflowTermSummaries.Count > 0)
            {
                if (!await storageService.DeleteOverflowTermSummariesEntriesAsync(chatId, summaryDbModel.OverflowTermSummaries.Select(s => s.SummaryEntryId).ToArray()))
                    return false;
            }

            if (await storageService.AddOverflowTermSummaryAsync(messageQueryModel) == null)
                return false;

            if (!await storageService.DeleteExtraTermSummariesEntriesAsync(backgroundQueryDbModel.ChatId, [.. summariesProcessed.Select(s => s.SummaryEntryId)]))
                return false;

            return true;
        }
    }
}
