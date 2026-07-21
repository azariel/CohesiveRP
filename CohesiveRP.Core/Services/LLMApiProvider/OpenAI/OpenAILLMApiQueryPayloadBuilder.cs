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
                MaxTokens = promptContext.MaxTokensToGenerate ?? 2048,
                Temperature = providerConfig.SamplingSettings.Temperature,
                MinP = providerConfig.SamplingSettings.MinP,
                DryMultiplier = providerConfig.SamplingSettings.DryMultiplier,
                DryBase = providerConfig.SamplingSettings.DryBase,
                DryAllowedLength = providerConfig.SamplingSettings.DryAllowedLength,
                DrySequenceBreakers = providerConfig.SamplingSettings.DrySequenceBreakers,
                XTCProbability = providerConfig.SamplingSettings.XTCProbability,
                XTCTreshold = providerConfig.SamplingSettings.XTCTreshold,
                TopP = providerConfig.SamplingSettings.TopP,
                TopK = providerConfig.SamplingSettings.TopK,
                //PresencePenalty = providerConfig.SamplingSettings.PresencePenalty,
                //FrequencyPenalty = providerConfig.SamplingSettings.FrequencyPenalty,
                //Stop = providerConfig.SamplingSettings.StopSequences,
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
