using CohesiveRP.Common.BusinessObjects;
using CohesiveRP.Common.Utils;
using CohesiveRP.Storage.DataAccessLayer.AIQueries;
using CohesiveRP.Storage.DataAccessLayer.ChatCompletionPresets.BusinessObjects;
using CohesiveRP.Storage.DataAccessLayer.ChatCompletionPresets.BusinessObjects.Format;

namespace CohesiveRP.Storage.DataAccessLayer.ChatCompletionPresets.Injectors.Cohesion
{
    internal class ProseValidatorCompletionPresetInjector : ICompletionPresetInjector
    {
        internal static ChatCompletionPresetsDbModel InjectPreset()
        {
            return new ChatCompletionPresetsDbModel
            {
                Name = "Default-Prose-Validator-Prompt-Generator-Preset",
                ChatCompletionPresetId = StorageConstants.DEFAULT_PROSE_VALIDATOR_COMPLETION_PRESET,
                CreatedAtUtc = DateTime.UtcNow,
                Format = new GlobalPromptContextFormat()
                {
                    MaxTokensToGenerate = 4096,
                    JsonSchemaName = "prose_validator_prompt_generator",
                    JsonSchemaDocument = StrictJsonSchemaGenerator.Generate<ProseValidationResult>(),
                    OrderedElementsWithinTheGlobalPromptContext =
                    [
                        new PromptContextFormatElement
                        {
                            Name = "Core Directive",
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
