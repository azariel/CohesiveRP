using CohesiveRP.Core.PromptContext.Abstractions;
using CohesiveRP.Storage.DataAccessLayer.Settings.LLMProviders;

namespace CohesiveRP.Core.Services.LLMApiProvider
{
    public interface ILLMApiQueryPayloadBuilder
    {
        string BuildPayload(IPromptContext promptContext, LLMProviderConfig providerConfig);
        string TryGetPayloadAsSimpleString(string serializedContext);
    }
}
