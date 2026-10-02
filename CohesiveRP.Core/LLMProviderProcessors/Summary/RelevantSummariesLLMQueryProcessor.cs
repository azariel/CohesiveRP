using CohesiveRP.Common.Diagnostics;
using CohesiveRP.Common.Serialization;
using CohesiveRP.Common.Utils.Parsers;
using CohesiveRP.Core.PromptContext.Abstractions;
using CohesiveRP.Core.PromptContext.Builders;
using CohesiveRP.Core.Services;
using CohesiveRP.Core.Services.Summary;
using CohesiveRP.Storage.DataAccessLayer.AIQueries;
using CohesiveRP.Storage.DataAccessLayer.BackgroundQueries.BusinessObjects;
using CohesiveRP.Storage.DTOs;
using CohesiveRP.Storage.QueryModels.Chat;

namespace CohesiveRP.Core.LLMProviderManager.Main
{
    public class RelevantSummariesLLMQueryProcessor : LLMQueryProcessor
    {
        public RelevantSummariesLLMQueryProcessor(
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

        public override async Task<bool> ProcessCompletedQueryAsync()
        {
            if (!await base.ProcessCompletedQueryAsync())
            {
                backgroundQueryDbModel.Content = null;
                backgroundQueryDbModel.Status = BackgroundQueryStatus.Pending;// re-queue
                backgroundQueryDbModel.RetryCount++;
                return false;
            }

            try
            {
                if (messages.Length != 1)
                {
                    LoggingManager.LogToFile("858c51c1-1c13-42be-a7cc-6d5c25416bdf", $"Couldn't complete backgroundTask [{backgroundQueryDbModel.BackgroundQueryId}] of Type [{tag}]. The Content embedding [{messages.Length}] messages generated from the inference server. One message was expected (no more, no less). Task will be set to Pending status for re-generation.");
                    backgroundQueryDbModel.Content = null;
                    backgroundQueryDbModel.Status = BackgroundQueryStatus.Pending;// re-queue
                    backgroundQueryDbModel.RetryCount++;
                    return false;
                }

                string relevantSummariesResponseContent = messages.First().Content;
                string relevantSummariesJson = LLMResponseParser.ParseOnlyJson(relevantSummariesResponseContent);
                LLMRelevantSummariesResponseDto responseDto = null;

                try
                {
                    responseDto = JsonCommonSerializer.DeserializeFromString<LLMRelevantSummariesResponseDto>(relevantSummariesJson);
                } catch (Exception e)
                {
                    LoggingManager.LogToFile("5139be1f-735e-4abb-bdc1-9fb5d04f7057", $"Couldn't complete backgroundTask [{backgroundQueryDbModel.BackgroundQueryId}] of Type [{tag}]. Invalid relevantSummariesJson. Skipping.", e);
                    backgroundQueryDbModel.Status = BackgroundQueryStatus.Pending;
                    backgroundQueryDbModel.RetryCount++;
                    return false;
                }

                var summaryDbModel = await storageService.GetSummaryAsync(backgroundQueryDbModel.ChatId);
                if (summaryDbModel == null)
                {
                    backgroundQueryDbModel.Status = BackgroundQueryStatus.Error;
                    return false;
                }

                summaryDbModel.RelevantSummaryInformationFromMostRecentStoryContext = $"<summary_short_term>{Environment.NewLine}{responseDto.ShortTermSummaries}{Environment.NewLine}</summary_short_term>{Environment.NewLine}<summary_medium_term>{Environment.NewLine}{responseDto.MediumTermSummaries}{Environment.NewLine}</summary_medium_term>{Environment.NewLine}<summary_long_term>{Environment.NewLine}{responseDto.LongTermSummaries}{Environment.NewLine}</summary_long_term>{Environment.NewLine}<summary_very_long_term>{Environment.NewLine}{responseDto.LongTermSummaries}{Environment.NewLine}</summary_very_long_term>{Environment.NewLine}";
                await storageService.UpdateSummaryAsync(summaryDbModel);

                backgroundQueryDbModel.EndFocusedGenerationDateTimeUtc = DateTime.UtcNow;
                backgroundQueryDbModel.Status = BackgroundQueryStatus.Completed;
                return true;
            } catch (Exception e)
            {
                LoggingManager.LogToFile("2f5203bb-a0e5-45b7-b141-8f84be30f230", $"Couldn't complete backgroundTask [{backgroundQueryDbModel.BackgroundQueryId}]. Task will be set to Pending status for re-generation.", e);
                backgroundQueryDbModel.Content = null;
                backgroundQueryDbModel.Status = BackgroundQueryStatus.Pending;
                backgroundQueryDbModel.RetryCount++;
                return false;
            }
        }

        //private async Task<bool> UpdateSummarizedMessagesAsync(string chatId)
        //{
        //    HotMessagesDbModel hotMessagesDbModel = await storageService.GetAllHotMessagesAsync(chatId);

        //    hotMessagesDbModel.Messages = hotMessagesDbModel.Messages.OrderByDescending(o => o.CreatedAtUtc).ToList();
        //    string[] messageIdsProcessedAgainstLLM = promptContext.ShareableContextLinks.FirstOrDefault(f => f.LinkedBuilder is PromptContextLastXMessagesToSummarizeBuilder)?.Value as string[];
        //    if (messageIdsProcessedAgainstLLM == null)
        //    {
        //        LoggingManager.LogToFile("6653a400-18f7-4d98-a0ed-b8959e4049f6", $"No ShareableContextLink of type [{nameof(PromptContextLastXMessagesToSummarizeBuilder)} found.]");
        //        return false;
        //    }

        //    // Process the messages
        //    hotMessagesDbModel.Messages = hotMessagesDbModel.Messages.OrderBy(o => o.CreatedAtUtc).ToList();

        //    foreach (var message in messageIdsProcessedAgainstLLM)
        //    {
        //        hotMessagesDbModel.Messages.FirstOrDefault(w => w.MessageId == message)?.Summarized = true;
        //    }

        //    HotMessagesDbModel request = new HotMessagesDbModel
        //    {
        //        ChatId = chatId,
        //        Messages = hotMessagesDbModel.Messages.Cast<MessageDbModel>().ToList(),
        //    };

        //    await storageService.UpdateHotMessagesAsync(request);
        //    return true;
        //}
    }
}
