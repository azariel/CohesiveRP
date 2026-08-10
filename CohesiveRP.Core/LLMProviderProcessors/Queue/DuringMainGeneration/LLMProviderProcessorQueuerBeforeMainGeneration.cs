using CohesiveRP.Core.Services;
using CohesiveRP.Storage.DataAccessLayer.BackgroundQueries.BusinessObjects;
using CohesiveRP.Storage.DataAccessLayer.Chats;
using CohesiveRP.Storage.QueryModels.BackgroundQuery;

namespace CohesiveRP.Core.LLMProviderProcessors.Queue.AfterPostGeneration
{
    internal class LLMProviderProcessorQueuerDuringMainGeneration
    {
        private IStorageService storageService;

        internal LLMProviderProcessorQueuerDuringMainGeneration(IStorageService storageService)
        {
            this.storageService = storageService;
        }

        internal async Task<bool> QueueAll(ChatDbModel chat)
        {
            bool operationResult = true;

            operationResult &= await AddSkillChecksDescriptionBackgroundQueryAsync(chat);

            return operationResult;
        }

        /// <summary>
        /// Create a desciption of the skill checks for the current scene. For e.g.: "You notice her lip curving, a telling hint of her attempt at deceiving you".
        /// </summary>
        internal async Task<bool> AddSkillChecksDescriptionBackgroundQueryAsync(ChatDbModel chat)
        {
            // Only add if there's currently skillChecks in the chat
            var currentSkillChecks = await storageService.GetChatCharactersRollsByIdAsync(chat.ChatId);

            if(currentSkillChecks?.ChatCharactersRolls == null || currentSkillChecks.ChatCharactersRolls.Count <= 0)
            {
                // There's no description to have, this is valid
                return true;
            }

            var backgroundQueryModel = new CreateBackgroundQueryQueryModel
            {
                ChatId = chat.ChatId,
                Priority = BackgroundQueryPriority.High,
                DependenciesTags = [],// No dependencies at all
                Tags = [BackgroundQuerySystemTags.skillChecksDescriptor.ToString()],
            };

            if (await storageService.AddBackgroundQueryAsync(backgroundQueryModel) == null)
                return false;

            return true;
        }
    }
}
