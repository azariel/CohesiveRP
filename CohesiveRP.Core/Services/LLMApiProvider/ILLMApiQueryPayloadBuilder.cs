using CohesiveRP.Core.PromptContext.Abstractions;

namespace CohesiveRP.Core.Services.LLMApiProvider
{
    public interface ILLMApiQueryPayloadBuilder
    {
        string BuildPayload(IPromptContext promptContext,string model);
    }
}
