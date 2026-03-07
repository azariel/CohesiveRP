using CohesiveRP.Common.Diagnostics;
using CohesiveRP.Storage.DataAccessLayer.BackgroundQueries.BusinessObjects;
using CohesiveRP.Storage.DataAccessLayer.ChatCompletionPresets.BusinessObjects.Settings;
using CohesiveRP.Storage.DataAccessLayer.Messages;
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

        public async Task EvaluateSummaryAsync(string chatId, PromptContextSettings settings)
        {
            // If there's already a background query that is currently processing a summary, delay
            var pendingRequests = await storageService.GetPendingOrProcessingBackgroundQueryAsync();

            if (pendingRequests == null)
            {
                LoggingManager.LogToFile("2e9ba10a-e856-4e42-b44a-f290b4c5bdaa", $"Couldn't fetch backgroundQueries.");
                return;
            }

            IMessageDbModel[] hotMessages = await storageService.GetAllHotMessages(chatId);
            if (pendingRequests.Where(w => w.Tags.Contains(BackgroundQuerySystemTags.shortSummary.ToString())).ToArray().Length <= 0)
            {
                await EvaluateShortTermSummaryAsync(chatId, settings, hotMessages);
            } else
            {
                LoggingManager.LogToFile("07b58dae-333d-4235-b02b-143bcf963e69", $"A short-term summary background query is already processing. Delaying.");
            }

            // TODO: handle medium, long and extra summary here
        }

        public async Task EvaluateShortTermSummaryAsync(string chatId, PromptContextSettings settings, IMessageDbModel[] hotMessages)
        {
            if (hotMessages == null)
            {
                return;
            }

            var hotMessagesCopy = new List<IMessageDbModel>();
            hotMessagesCopy.AddRange(hotMessages);

            hotMessagesCopy = hotMessagesCopy?.Where(w => !w.Summarized).ToList();

            if (hotMessagesCopy.Count < settings.LastXMessages + 1 + settings.Summary.Short.NbMessageInChunk)
            {
                return;
            }

            // We know that the 'main' context builder is generating the last user message + {settings.LastXMessages} amount of raw messages
            // Skip the 'reserved' messages
            hotMessagesCopy = hotMessagesCopy.OrderByDescending(o => o.CreatedAtUtc).Skip(settings.LastXMessages + 1).ToList();

            if (hotMessagesCopy.Count < settings.Summary.Short.NbMessageInChunk)
            {
                // Not enough messages to summarize to short summary module
                return;
            }

            // Filter for a chunk ordered by createdAt
            hotMessagesCopy.Reverse();
            hotMessagesCopy = hotMessagesCopy.Take(settings.Summary.Short.NbMessageInChunk).ToList();

            // Create the background query to fulfill the request
            var backgroundQueryDbModel = new CreateBackgroundQueryQueryModel
            {
                ChatId = chatId,
                LinkedId = hotMessagesCopy.Last().MessageId,
                DependenciesTags = [BackgroundQuerySystemTags.main.ToString(), BackgroundQuerySystemTags.sceneTracker.ToString()],
                Tags = [BackgroundQuerySystemTags.shortSummary.ToString()],
                Priority = BackgroundQueryPriority.Standard,
            };

            await storageService.CreateBackgroundQueryAsync(backgroundQueryDbModel);
        }
    }
}
