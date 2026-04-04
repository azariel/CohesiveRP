using CohesiveRP.Common.Serialization;
using CohesiveRP.Core.PromptContext.Abstractions;
using CohesiveRP.Core.Services.LLMApiProvider.OpenAI.BusinessObjects.Request;
using CohesiveRP.Storage.DataAccessLayer.Settings.LLMProviders;

namespace CohesiveRP.Core.Services.LLMApiProvider.OpenAI
{
    public class OpenAILLMApiQueryPayloadBuilder : ILLMApiQueryPayloadBuilder
    {
        private OpenAIChatCompletionMessage[] ConvertPromptMessagesToOpenAICompatiblePromptMessages(IPromptMessage[] messages)
        {
            if(messages == null)
            {
                return null;
            }

            return messages.Select(s=> new OpenAIChatCompletionMessage
            {
                Role = s.Role,
                Content = s.Content,
            }).ToArray();
        }

        public string BuildPayload(IPromptContext promptContext, LLMProviderConfig providerConfig)
        {
            OpenAIChatCompletionRequestDto requestDto = new OpenAIChatCompletionRequestDto
            {
                Model = providerConfig.Model,
                Messages = ConvertPromptMessagesToOpenAICompatiblePromptMessages(promptContext.Messages),
                Stream = providerConfig.Stream,
            };

            var serializedModel = JsonCommonSerializer.SerializeToString(requestDto);
            return serializedModel;
        }
    }
}
