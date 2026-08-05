using CohesiveRP.Core.LLMProviderProcessors.Queue.AfterPostGeneration;
using CohesiveRP.Core.Services;
using CohesiveRP.Storage.DataAccessLayer.Chats;

namespace CohesiveRP.Core.LLMProviderProcessors.Queue
{
    public class LLMProviderProcessorQueuer : ILLMProviderProcessorQueuer
    {
        LLMProviderProcessorQueuerBeforeMainGeneration beforeMainGenerationProcessorQueuer;
        LLMProviderProcessorQueuerAfterMainGeneration afterMainGenerationProcessorQueuer;
        LLMProviderProcessorQueuerAfterPostGeneration afterPostGenerationProcessorQueuer;

        public LLMProviderProcessorQueuer(IStorageService storageService)
        {
            beforeMainGenerationProcessorQueuer = new(storageService);
            afterMainGenerationProcessorQueuer = new(storageService);
            afterPostGenerationProcessorQueuer = new(storageService);
        }

        // PreGeneration
        public async Task<bool> QueueProcessorsOnBeforeMainGeneration(ChatDbModel chat) => await beforeMainGenerationProcessorQueuer.QueueAll(chat);

        // PostGeneration
        public async Task<bool> QueueProcessorsOnAfterMainGeneration(ChatDbModel chat) => await afterMainGenerationProcessorQueuer.QueueAll(chat);

        // SecondPostGeneration
        public async Task<bool> QueueProcessorsOnAfterPostGeneration(ChatDbModel chat) => await afterPostGenerationProcessorQueuer.QueueAll(chat);
    }
}
