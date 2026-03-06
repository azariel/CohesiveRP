using CohesiveRP.Storage.DataAccessLayer.Settings.LLMProviders;

namespace CohesiveRP.Core.Services.LLMApiProvider
{
    public interface ILLMApiQueryPayloadBuilderFactory
    {
        ILLMApiQueryPayloadBuilder Create(LLMProviderType type);
    }
}
