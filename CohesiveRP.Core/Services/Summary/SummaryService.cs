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
            IMessageDbModel[] hotMessages = await storageService.GetAllHotMessages(chatId);

            await EvaluateShortTermSummaryAsync(chatId, settings, hotMessages);
            // TODO: handle medium, long and extra summary here
        }

        public async Task EvaluateShortTermSummaryAsync(string chatId, PromptContextSettings settings, IMessageDbModel[] hotMessages)
        {
            if (hotMessages.Length <= 0)
            {
                return;
            }

            // We know that the 'main' context builder is generating the last user message + {settings.LastXMessages} amount of raw messages
            // Skip the 'reserved' messages
            hotMessages = hotMessages.OrderByDescending(o => o.CreatedAtUtc).Skip(settings.LastXMessages + 1).ToArray();

            if (hotMessages.Length < settings.Summary.Short.NbMessageInChunk)
            {
                // Not enough messages to summarize to short summary module
                return;
            }

            // Create the background query to fulfill the request
            var backgroundQueryDbModel = new CreateBackgroundQueryQueryModel
            {
                ChatId = chatId,
                DependenciesTags = [BackgroundQuerySystemTags.main.ToString(), BackgroundQuerySystemTags.sceneTracker.ToString()],
                Tags = [BackgroundQuerySystemTags.shortSummary.ToString()],
                Priority = BackgroundQueryPriority.Standard,
            };

            await storageService.CreateBackgroundQueryAsync(backgroundQueryDbModel);
        }
    }
}
