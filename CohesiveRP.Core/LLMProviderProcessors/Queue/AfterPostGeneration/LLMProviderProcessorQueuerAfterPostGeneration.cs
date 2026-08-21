using CohesiveRP.Core.Services;
using CohesiveRP.Storage.DataAccessLayer.BackgroundQueries.BusinessObjects;
using CohesiveRP.Storage.DataAccessLayer.Chats;
using CohesiveRP.Storage.QueryModels.BackgroundQuery;

namespace CohesiveRP.Core.LLMProviderProcessors.Queue.AfterPostGeneration
{
    internal class LLMProviderProcessorQueuerAfterPostGeneration
    {
        private IStorageService storageService;

        internal LLMProviderProcessorQueuerAfterPostGeneration(IStorageService storageService)
        {
            this.storageService = storageService;
        }

        internal async Task<bool> QueueAll(ChatDbModel chat)
        {
          bool operationResult = true;

            var hotMessagesDbModel = await storageService.GetAllHotMessagesAsync(chat.ChatId);
            var currentNarrativeArchitectures = await storageService.GetNarrativeArchitecturesAsync(s => s.ChatId == chat.ChatId);
            if ((currentNarrativeArchitectures == null || !currentNarrativeArchitectures.Any()) && hotMessagesDbModel?.Messages?.Count > 50 ||
                (currentNarrativeArchitectures != null && currentNarrativeArchitectures.Any() && currentNarrativeArchitectures.First().RefreshCooldown <= 0))
            {
                operationResult &= await QueueNarrativeArchitectureAsync(chat);
            }

            return operationResult;
        }

        internal async Task<bool> QueueNarrativeArchitectureAsync(ChatDbModel chat)
        {
            var backgroundQueryModel = new CreateBackgroundQueryQueryModel
            {
                ChatId = chat.ChatId,
                Priority = BackgroundQueryPriority.VeryLow,// user is not waiting, we're simply generation and iterating over secret plots and narrative arcs in the background, so we can set it to very low priority
                DependenciesTags = [
                    BackgroundQuerySystemTags.main.ToString(),
                    BackgroundQuerySystemTags.skillChecksInitiator.ToString(),
                    BackgroundQuerySystemTags.sceneTracker.ToString()
                ],
                Tags = [BackgroundQuerySystemTags.narrativeArchitecture.ToString()],
            };

            if (await storageService.AddBackgroundQueryAsync(backgroundQueryModel) == null)
                return false;

            return true;
        }
    }
}
