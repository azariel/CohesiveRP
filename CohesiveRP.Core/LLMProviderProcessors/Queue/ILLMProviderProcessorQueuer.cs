using CohesiveRP.Storage.DataAccessLayer.Chats;

namespace CohesiveRP.Core.LLMProviderProcessors.Queue
{
    public interface ILLMProviderProcessorQueuer
    {
         Task<bool> QueueProcessorsOnBeforeMainGeneration(ChatDbModel chat);
         Task<bool> QueueProcessorsOnAfterMainGeneration(ChatDbModel chat);
         Task<bool> QueueProcessorsOnAfterPostGeneration(ChatDbModel chat);
    }
}
