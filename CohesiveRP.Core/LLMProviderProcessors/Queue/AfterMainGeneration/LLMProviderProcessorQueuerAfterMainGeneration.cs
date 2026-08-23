using CohesiveRP.Core.Services;
using CohesiveRP.Storage.DataAccessLayer.BackgroundQueries.BusinessObjects;
using CohesiveRP.Storage.DataAccessLayer.Chats;
using CohesiveRP.Storage.QueryModels.BackgroundQuery;

namespace CohesiveRP.Core.LLMProviderProcessors.Queue.AfterPostGeneration
{
    internal class LLMProviderProcessorQueuerAfterMainGeneration
    {
        private IStorageService storageService;

        internal LLMProviderProcessorQueuerAfterMainGeneration(IStorageService storageService)
        {
            this.storageService = storageService;
        }

        internal async Task<bool> QueueAll(ChatDbModel chat)
        {
            bool operationResult = true;
            //operationResult &= await QueueCohesionEnforcerAsync(chat);
            operationResult &= await QueueProseGuardianAsync(chat);

            return operationResult;
        }

        internal async Task<bool> QueueCohesionEnforcerAsync(ChatDbModel chat)
        {
            var backgroundQueryModel = new CreateBackgroundQueryQueryModel
            {
                ChatId = chat.ChatId,
                Priority = BackgroundQueryPriority.Highest,
                DependenciesTags = [BackgroundQuerySystemTags.main.ToString()],// must run directly after main without delay
                Tags = [BackgroundQuerySystemTags.cohesionEnforcement.ToString()],
            };

            if (await storageService.AddBackgroundQueryAsync(backgroundQueryModel) == null)
                return false;

            return true;
        }

        internal async Task<bool> QueueProseGuardianAsync(ChatDbModel chat)
        {
            var backgroundQueryModel = new CreateBackgroundQueryQueryModel
            {
                ChatId = chat.ChatId,
                Priority = BackgroundQueryPriority.High,// will block the next 'main'
                DependenciesTags = [],// no block
                Tags = [BackgroundQuerySystemTags.proseGuardian.ToString()],
            };

            if (await storageService.AddBackgroundQueryAsync(backgroundQueryModel) == null)
                return false;

            return true;
        }
    }
}
