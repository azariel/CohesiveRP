using System.Text;
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
            if (messages == null)
            {
                return null;
            }

            return messages.Select(s => new OpenAIChatCompletionMessage
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

        public string TryGetPayloadAsSimpleString(string serializedContext)
        {
            try
            {
                OpenAIChatCompletionRequestDto model = JsonCommonSerializer.DeserializeFromString<OpenAIChatCompletionRequestDto>(serializedContext);

                if (model == null || model.Messages.Length <= 0)
                {
                    return serializedContext;
                }

                StringBuilder str = new();
                foreach (var message in model.Messages)
                {
                    str.AppendLine($"Role: {message.Role}{Environment.NewLine}Content: {message.Content}{Environment.NewLine}");
                }

                return str.ToString();

            } catch (Exception e)
            {
                return serializedContext;
            }
        }
    }
}
