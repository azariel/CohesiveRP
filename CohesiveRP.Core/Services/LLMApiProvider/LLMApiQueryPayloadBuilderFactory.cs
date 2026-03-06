using CohesiveRP.Core.Services.LLMApiProvider.OpenAI;
using CohesiveRP.Storage.DataAccessLayer.Settings.LLMProviders;

namespace CohesiveRP.Core.Services.LLMApiProvider
{
    public class LLMApiQueryPayloadBuilderFactory : ILLMApiQueryPayloadBuilderFactory
    {
        public ILLMApiQueryPayloadBuilder Create(LLMProviderType type)
        {
            switch (type)
            {
                case LLMProviderType.OpenAICustom:
                    return new OpenAILLMApiQueryPayloadBuilder();
                default:
                    throw new Exception($"Unhandled type [{type}].");
            }
        }
    }
}
