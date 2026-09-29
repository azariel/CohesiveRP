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

            // Queue cohesion queries to refine the AI reply
            //operationResult &= await QueueCharactersAdherenceAsync(chat);
            operationResult &= await QueueProseValidatorAsync(chat);// Will trigger ProseEditor, StyleValidator and StyleEditor when required

            // Queue a query that'll run after the final reply is sent to the User so that we can reflect on the replied prose for the next turn
            operationResult &= await QueueProseGuardianAsync(chat);

            return operationResult;
        }

        internal async Task<bool> QueueCharactersAdherenceAsync(ChatDbModel chat)
        {
            var backgroundQueryModel = new CreateBackgroundQueryQueryModel
            {
                ChatId = chat.ChatId,
                Priority = BackgroundQueryPriority.Highest,
                DependenciesTags = [BackgroundQuerySystemTags.main.ToString()],// must run directly after main without delay
                Tags = [BackgroundQuerySystemTags.charactersAdherenceEnforcement.ToString()],
            };

            if (await storageService.AddBackgroundQueryAsync(backgroundQueryModel) == null)
                return false;

            return true;
        }

        internal async Task<bool> QueueProseValidatorAsync(ChatDbModel chat)
        {
            var backgroundQueryModel = new CreateBackgroundQueryQueryModel
            {
                ChatId = chat.ChatId,
                Priority = BackgroundQueryPriority.Highest,
                DependenciesTags = [
                    BackgroundQuerySystemTags.main.ToString(),
                ],
                Tags = [BackgroundQuerySystemTags.proseValidator.ToString()],
            };

            if (await storageService.AddBackgroundQueryAsync(backgroundQueryModel) == null)
                return false;

            return true;
        }

        internal async Task<bool> QueueProseEditorAsync(ChatDbModel chat)
        {
            var backgroundQueryModel = new CreateBackgroundQueryQueryModel
            {
                ChatId = chat.ChatId,
                Priority = BackgroundQueryPriority.Highest,
                DependenciesTags = [
                    BackgroundQuerySystemTags.main.ToString(),
                    BackgroundQuerySystemTags.proseValidator.ToString(),
                ],
                Tags = [BackgroundQuerySystemTags.proseEdition.ToString()],
            };

            if (await storageService.AddBackgroundQueryAsync(backgroundQueryModel) == null)
                return false;

            return true;
        }

        internal async Task<bool> QueueStyleValidatorAsync(ChatDbModel chat)
        {
            var backgroundQueryModel = new CreateBackgroundQueryQueryModel
            {
                ChatId = chat.ChatId,
                Priority = BackgroundQueryPriority.Highest,
                DependenciesTags = [
                    BackgroundQuerySystemTags.main.ToString(),
                    BackgroundQuerySystemTags.proseValidator.ToString(),
                    BackgroundQuerySystemTags.proseEdition.ToString(),
                ],
                Tags = [BackgroundQuerySystemTags.styleValidator.ToString()],
            };

            if (await storageService.AddBackgroundQueryAsync(backgroundQueryModel) == null)
                return false;

            return true;
        }

        internal async Task<bool> QueueStyleEditorAsync(ChatDbModel chat)
        {
            var backgroundQueryModel = new CreateBackgroundQueryQueryModel
            {
                ChatId = chat.ChatId,
                Priority = BackgroundQueryPriority.Highest,
                DependenciesTags = [
                    BackgroundQuerySystemTags.main.ToString(),
                    BackgroundQuerySystemTags.proseValidator.ToString(),
                    BackgroundQuerySystemTags.proseEdition.ToString(),
                    BackgroundQuerySystemTags.styleValidator.ToString(),
                ],
                Tags = [BackgroundQuerySystemTags.styleEdition.ToString()],
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
                Priority = BackgroundQueryPriority.High,// will block the *next* 'main', but doesn't block the current one
                DependenciesTags = [
                    BackgroundQuerySystemTags.main.ToString(),
                    BackgroundQuerySystemTags.proseValidator.ToString(),
                    BackgroundQuerySystemTags.proseEdition.ToString(),
                    BackgroundQuerySystemTags.styleValidator.ToString(),
                    BackgroundQuerySystemTags.styleEdition.ToString(),
                ],
                Tags = [BackgroundQuerySystemTags.proseGuardian.ToString()],
            };

            if (await storageService.AddBackgroundQueryAsync(backgroundQueryModel) == null)
                return false;

            return true;
        }
    }
}
