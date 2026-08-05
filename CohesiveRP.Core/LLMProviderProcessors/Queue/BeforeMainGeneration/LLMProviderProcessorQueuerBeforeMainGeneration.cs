using CohesiveRP.Core.Services;
using CohesiveRP.Storage.DataAccessLayer.BackgroundQueries.BusinessObjects;
using CohesiveRP.Storage.DataAccessLayer.Chats;
using CohesiveRP.Storage.DataAccessLayer.Messages.Hot;
using CohesiveRP.Storage.QueryModels.BackgroundQuery;

namespace CohesiveRP.Core.LLMProviderProcessors.Queue.AfterPostGeneration
{
    internal class LLMProviderProcessorQueuerBeforeMainGeneration
    {
        private IStorageService storageService;

        internal LLMProviderProcessorQueuerBeforeMainGeneration(IStorageService storageService)
        {
            this.storageService = storageService;
        }

        internal async Task<bool> QueueAll(ChatDbModel chat)
        {
            bool operationResult = true;

            // Only generate a request for a sceneTracker once we have a decent amount of messages in the conversation/story. Otherwise, the model may get confused and blabber something irrelevant or that will induce corruption
            HotMessagesDbModel hotMessagesDbModel = await storageService.GetAllHotMessagesAsync(chat.ChatId);
            if (hotMessagesDbModel != null && hotMessagesDbModel.Messages.Count > 4)
            {
                operationResult &= await AddSceneTrackerBackgroundQueryAsync(chat);
            }

            operationResult &= await AddSkillChecksInitiatorBackgroundQueryAsync(chat);


            var currentNarrativeDirections = await storageService.GetNarrativeDirectionsAsync(s => s.ChatId == chat.ChatId);
            if ((currentNarrativeDirections == null || !currentNarrativeDirections.Any()) && hotMessagesDbModel?.Messages?.Count > 5)
            {
                operationResult &= await AddNarrativeDirectionBackgroundQueryAsync(chat);
            } else if (currentNarrativeDirections != null && currentNarrativeDirections.Any() && currentNarrativeDirections.First().RefreshCooldown <= 0)
            {
                operationResult &= await AddNarrativeDirectionBackgroundQueryAsync(chat);
            }

            return operationResult;
        }

        internal async Task<bool> AddSceneTrackerBackgroundQueryAsync(ChatDbModel chat)
        {
            var backgroundQueryModel = new CreateBackgroundQueryQueryModel
            {
                ChatId = chat.ChatId,
                Priority = BackgroundQueryPriority.Highest,// User is waiting!
                DependenciesTags = [],// No dependencies at all
                Tags = [BackgroundQuerySystemTags.sceneTracker.ToString()],
            };

            if (await storageService.AddBackgroundQueryAsync(backgroundQueryModel) == null)
                return false;

            return true;
        }

        internal async Task<bool> AddSkillChecksInitiatorBackgroundQueryAsync(ChatDbModel chat)
        {
            var backgroundQueryModel = new CreateBackgroundQueryQueryModel
            {
                ChatId = chat.ChatId,
                Priority = BackgroundQueryPriority.Highest,// User is waiting!
                DependenciesTags = [],// No dependencies at all
                Tags = [BackgroundQuerySystemTags.skillChecksInitiator.ToString()],
            };

            if (await storageService.AddBackgroundQueryAsync(backgroundQueryModel) == null)
                return false;

            return true;
        }

        internal async Task<bool> AddNarrativeDirectionBackgroundQueryAsync(ChatDbModel chat)
        {
            var backgroundQueryModel = new CreateBackgroundQueryQueryModel
            {
                ChatId = chat.ChatId,
                Priority = BackgroundQueryPriority.Highest,// User is waiting!
                DependenciesTags = [],// No dependencies at all
                Tags = [BackgroundQuerySystemTags.narrativeDirection.ToString()],
            };

            if (await storageService.AddBackgroundQueryAsync(backgroundQueryModel) == null)
                return false;

            return true;
        }
    }
}
