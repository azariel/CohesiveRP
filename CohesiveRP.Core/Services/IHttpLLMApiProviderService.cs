using CohesiveRP.Core.HttpLLMApiProvider;
using CohesiveRP.Core.PromptContext.Abstractions;
using CohesiveRP.Storage.DataAccessLayer.AIQueries;
using CohesiveRP.Storage.DataAccessLayer.Settings.LLMProviders;

namespace CohesiveRP.Core.Services
{
    public interface IHttpLLMApiProviderService
    {
        Task<IHttpLLMApiQueryResponseDto> QueryApiAsync(string tag, LLMProviderConfig[] availableLLMApiProviders, IPromptContext promptContext, BackgroundQueryDbModel backgroundQueryDbModel);
    }
}
