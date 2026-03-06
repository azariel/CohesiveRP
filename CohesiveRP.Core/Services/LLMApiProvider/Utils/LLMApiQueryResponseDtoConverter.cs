using CohesiveRP.Common.Diagnostics;
using CohesiveRP.Common.Serialization;
using CohesiveRP.Core.HttpLLMApiProvider;
using CohesiveRP.Core.Services.LLMApiProvider.OpenAI.BusinessObjects.Response;
using CohesiveRP.Storage.DataAccessLayer.Settings.LLMProviders;

namespace CohesiveRP.Core.Services.LLMApiProvider.Utils
{
    public static class LLMApiQueryResponseDtoConverter
    {
        public static IHttpLLMApiQueryResponseDto Convert(LLMProviderType type, string rawResponse)
        {
            try
            {
                switch (type)
                {
                    case LLMProviderType.OpenAICustom:
                        return JsonCommonSerializer.DeserializeFromString<OpenAIChatCompletionResponseDto>(rawResponse);
                    default:
                        LoggingManager.LogToFile("4ad0a33f-3538-496c-8b3e-2ece63c402d6", $"Unhandled {nameof(LLMProviderType)} [{type}].");
                        return null;
                }
            } catch (Exception e)
            {
                LoggingManager.LogToFile("3baa99e8-c178-4271-8199-61594f6d3fcb", $"Couldn't deserialize the LLM API response model into a valid {nameof(LLMProviderType)} [{type}]. Raw response: [{rawResponse}].", e);
                return null;
            }
        }
    }
}
