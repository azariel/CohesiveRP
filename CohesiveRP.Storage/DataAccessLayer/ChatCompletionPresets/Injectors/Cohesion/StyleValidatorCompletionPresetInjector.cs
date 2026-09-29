using CohesiveRP.Common.BusinessObjects;
using CohesiveRP.Common.Utils;
using CohesiveRP.Storage.DataAccessLayer.AIQueries;
using CohesiveRP.Storage.DataAccessLayer.ChatCompletionPresets.BusinessObjects;
using CohesiveRP.Storage.DataAccessLayer.ChatCompletionPresets.BusinessObjects.Format;

namespace CohesiveRP.Storage.DataAccessLayer.ChatCompletionPresets.Injectors.Cohesion
{
    internal class StyleValidatorCompletionPresetInjector : ICompletionPresetInjector
    {
        internal static ChatCompletionPresetsDbModel InjectPreset()
        {
            return new ChatCompletionPresetsDbModel
            {
                Name = "Default-Style-Validator-Prompt-Generator-Preset",
                ChatCompletionPresetId = StorageConstants.DEFAULT_STYLE_VALIDATOR_COMPLETION_PRESET,
                CreatedAtUtc = DateTime.UtcNow,
                Format = new GlobalPromptContextFormat()
                {
                    MaxTokensToGenerate = 4096,
                    JsonSchemaName = "style_validator_prompt_generator",
                    JsonSchemaDocument = StrictJsonSchemaGenerator.Generate<SceneTrackerValidationResult>(),
                    OrderedElementsWithinTheGlobalPromptContext =
                    [
                        new PromptContextFormatElement
                        {
                            Name = "Core Directive & World Logic",
                            Tag = PromptContextFormatTag.Directive,
                            Enabled = true,
                            Options = new PromptContextFormatElementOptions
                            {
                                Format = "\r\n"
                            }
                        },
                        new PromptContextFormatElement
                        {
                            Tag = PromptContextFormatTag.BehavioralInstructions,
                            Name = "BehavioralInstructions",
                            Enabled = true,
                            Options = new PromptContextFormatElementOptions
                            {
                                Format = "<behavioral_instruction>\r\n\r\n</behavioral_instruction>",
                            }
                        }
                    ]
                }
            };
        }
    }
}
