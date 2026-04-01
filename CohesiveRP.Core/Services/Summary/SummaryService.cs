using CohesiveRP.Common.Diagnostics;
using CohesiveRP.Common.Utils;
using CohesiveRP.Storage.DataAccessLayer.BackgroundQueries.BusinessObjects;
using CohesiveRP.Storage.DataAccessLayer.Messages;
using CohesiveRP.Storage.DataAccessLayer.Messages.Hot;
using CohesiveRP.Storage.DataAccessLayer.Settings;
using CohesiveRP.Storage.DataAccessLayer.Summary;
using CohesiveRP.Storage.QueryModels.BackgroundQuery;

namespace CohesiveRP.Core.Services.Summary
{
    public class SummaryService : ISummaryService
    {
        private IStorageService storageService;

        public SummaryService(IStorageService storageService)
        {
            this.storageService = storageService;
        }

        public async Task EvaluateSummaryAsync(string chatId, GlobalSettingsDbModel settings)
        {
            // If there's already a background query that is currently processing a summary, delay
            var pendingRequests = await storageService.GetPendingOrProcessingBackgroundQueryAsync();
            if (pendingRequests == null)
            {
                LoggingManager.LogToFile("2e9ba10a-e856-4e42-b44a-f290b4c5bdaa", $"Couldn't fetch backgroundQueries.");
                return;
            }

            SummaryDbModel summaryDbModel = await storageService.GetSummaryAsync(chatId);
            HotMessagesDbModel hotMessagesDbModel = await storageService.GetAllHotMessagesAsync(chatId);
            if (pendingRequests.Where(w => w.Tags.Contains(BackgroundQuerySystemTags.shortSummary.ToString())).ToArray().Length <= 0)
            {
                await EvaluateShortTermSummaryAsync(chatId, settings, hotMessagesDbModel.Messages.ToArray());
            }

            if (pendingRequests.Where(w => w.Tags.Contains(BackgroundQuerySystemTags.mediumSummary.ToString())).ToArray().Length <= 0)
            {
                await EvaluateMediumTermSummaryAsync(chatId, settings, summaryDbModel);
            }

            if (pendingRequests.Where(w => w.Tags.Contains(BackgroundQuerySystemTags.longSummary.ToString())).ToArray().Length <= 0)
            {
                await EvaluateLongTermSummaryAsync(chatId, settings, summaryDbModel);
            }

            if (pendingRequests.Where(w => w.Tags.Contains(BackgroundQuerySystemTags.extraSummary.ToString())).ToArray().Length <= 0)
            {
                await EvaluateExtraTermSummaryAsync(chatId, settings, summaryDbModel);
            }

            if (pendingRequests.Where(w => w.Tags.Contains(BackgroundQuerySystemTags.overflowSummary.ToString())).ToArray().Length <= 0)
            {
                await EvaluateOverflowTermSummaryAsync(chatId, settings, summaryDbModel);
            }
        }

        /// <summary>
        /// Validate if we need to create a background query to generate a new Short-term summary.
        /// </summary>
        public async Task EvaluateShortTermSummaryAsync(string chatId, GlobalSettingsDbModel settings, IMessageDbModel[] hotMessages)
        {
            if (hotMessages == null)
            {
                return;
            }

            var hotMessagesCopy = new List<IMessageDbModel>();
            hotMessagesCopy.AddRange(hotMessages);
            hotMessagesCopy = hotMessagesCopy?.Where(w => !w.Summarized).ToList();

            if (hotMessagesCopy.Count < settings.Summary.NbRawMessagesToKeepInContext + settings.Summary.Short.NbMessageInChunk)
            {
                return;
            }

            // We know that the 'main' context builder is generating the last user message + {settings.LastXMessages} amount of raw messages
            // Skip the 'reserved' messages
            hotMessagesCopy = hotMessagesCopy.OrderByDescending(o => o.CreatedAtUtc).Skip(settings.Summary.NbRawMessagesToKeepInContext).ToList();

            // Filter for a chunk ordered by createdAt
            hotMessagesCopy.Reverse();
            hotMessagesCopy = hotMessagesCopy.Take(settings.Summary.Short.NbMessageInChunk).ToList();

            // Create the background query to fulfill the request
            var backgroundQueryDbModel = new CreateBackgroundQueryQueryModel
            {
                ChatId = chatId,
                LinkedId = hotMessagesCopy.Last().MessageId,
                DependenciesTags = [BackgroundQuerySystemTags.sceneTracker.ToString(), BackgroundQuerySystemTags.sceneAnalyze.ToString()],
                Tags = [BackgroundQuerySystemTags.shortSummary.ToString()],
                Priority = BackgroundQueryPriority.Low,
            };

            await storageService.AddBackgroundQueryAsync(backgroundQueryDbModel);
        }

        /// <summary>
        /// Validate if we need to create a background query to generate a new Medium-term summary by rollovering short-term some summaries.
        /// In other words, we'll take a few short-term summaries and smash them together into a single medium-term summary.
        /// </summary>
        public async Task EvaluateMediumTermSummaryAsync(string chatId, GlobalSettingsDbModel settings, SummaryDbModel summaryDbModel)
        {
            if (summaryDbModel == null)
            {
                return;
            }

            if (TokensUtils.Count(string.Join(Environment.NewLine, summaryDbModel.ShortTermSummaries.Select(s => s.Content).ToArray())) < settings.Summary.Short.MaxShortTermSummaryTokens)
            {
                return;
            }

            // Create the background query to fulfill the request
            var backgroundQueryDbModel = new CreateBackgroundQueryQueryModel
            {
                ChatId = chatId,
                DependenciesTags = [BackgroundQuerySystemTags.sceneTracker.ToString(), BackgroundQuerySystemTags.shortSummary.ToString(), BackgroundQuerySystemTags.sceneAnalyze.ToString()],
                Tags = [BackgroundQuerySystemTags.mediumSummary.ToString()],
                Priority = BackgroundQueryPriority.VeryLow,
            };

            await storageService.AddBackgroundQueryAsync(backgroundQueryDbModel);
        }

        /// <summary>
        /// Validate if we need to create a background query to generate a new Long-term summary by rollovering medium-term some summaries.
        /// In other words, we'll take a few medium-term summaries and smash them together into a single long-term summary.
        /// </summary>
        public async Task EvaluateLongTermSummaryAsync(string chatId, GlobalSettingsDbModel settings, SummaryDbModel summaryDbModel)
        {
            if (summaryDbModel == null)
            {
                return;
            }

            if (TokensUtils.Count(string.Join(Environment.NewLine, summaryDbModel.MediumTermSummaries.Select(s => s.Content).ToArray())) < settings.Summary.Medium.MaxTotalSummariesTokens)
            {
                return;
            }

            // Create the background query to fulfill the request
            var backgroundQueryDbModel = new CreateBackgroundQueryQueryModel
            {
                ChatId = chatId,
                DependenciesTags = [BackgroundQuerySystemTags.sceneTracker.ToString(), BackgroundQuerySystemTags.shortSummary.ToString(), BackgroundQuerySystemTags.mediumSummary.ToString(), BackgroundQuerySystemTags.sceneAnalyze.ToString()],
                Tags = [BackgroundQuerySystemTags.longSummary.ToString()],
                Priority = BackgroundQueryPriority.VeryLow,
            };

            await storageService.AddBackgroundQueryAsync(backgroundQueryDbModel);
        }

        /// <summary>
        /// Validate if we need to create a background query to generate a new Extra-term summary by rollovering Long-term some summaries.
        /// In other words, we'll take a few Long-term summaries and smash them together into a single Extra-term summary.
        /// </summary>
        public async Task EvaluateExtraTermSummaryAsync(string chatId, GlobalSettingsDbModel settings, SummaryDbModel summaryDbModel)
        {
            if (summaryDbModel == null)
            {
                return;
            }

            if (TokensUtils.Count(string.Join(Environment.NewLine, summaryDbModel.LongTermSummaries.Select(s => s.Content).ToArray())) < settings.Summary.Long.MaxTotalSummariesTokens)
            {
                return;
            }

            // Create the background query to fulfill the request
            var backgroundQueryDbModel = new CreateBackgroundQueryQueryModel
            {
                ChatId = chatId,
                DependenciesTags = [BackgroundQuerySystemTags.sceneTracker.ToString(), BackgroundQuerySystemTags.shortSummary.ToString(), BackgroundQuerySystemTags.mediumSummary.ToString(), BackgroundQuerySystemTags.longSummary.ToString(), BackgroundQuerySystemTags.sceneAnalyze.ToString()],
                Tags = [BackgroundQuerySystemTags.extraSummary.ToString()],
                Priority = BackgroundQueryPriority.VeryLow,
            };

            await storageService.AddBackgroundQueryAsync(backgroundQueryDbModel);
        }

        /// <summary>
        /// Validate if we need to create a background query to generate a new Overflow-term summary by rollovering Extra-term some summaries.
        /// In other words, we'll take a few Extra-term summaries and smash them together into a single Overflow-term summary.
        /// Note that we always keep a SINGLE overflow summary, contrary to the other types of summaries. This is the final, fading summary.
        /// </summary>
        public async Task EvaluateOverflowTermSummaryAsync(string chatId, GlobalSettingsDbModel settings, SummaryDbModel summaryDbModel)
        {
            if (summaryDbModel == null)
            {
                return;
            }

            if (TokensUtils.Count(string.Join(Environment.NewLine, summaryDbModel.ExtraTermSummaries.Select(s => s.Content).ToArray())) < settings.Summary.Extra.MaxTotalSummariesTokens)
            {
                return;
            }

            // Create the background query to fulfill the request
            var backgroundQueryDbModel = new CreateBackgroundQueryQueryModel
            {
                ChatId = chatId,
                DependenciesTags = [BackgroundQuerySystemTags.sceneTracker.ToString(), BackgroundQuerySystemTags.shortSummary.ToString(), BackgroundQuerySystemTags.mediumSummary.ToString(), BackgroundQuerySystemTags.longSummary.ToString(), BackgroundQuerySystemTags.extraSummary.ToString(), BackgroundQuerySystemTags.sceneAnalyze.ToString()],
                Tags = [BackgroundQuerySystemTags.overflowSummary.ToString()],
                Priority = BackgroundQueryPriority.VeryLow,
            };

            await storageService.AddBackgroundQueryAsync(backgroundQueryDbModel);
        }
    }
}
