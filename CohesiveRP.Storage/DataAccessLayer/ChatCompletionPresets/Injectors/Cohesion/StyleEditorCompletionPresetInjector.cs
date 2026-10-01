using CohesiveRP.Common.BusinessObjects;
using CohesiveRP.Common.Utils;
using CohesiveRP.Storage.DataAccessLayer.AIQueries;
using CohesiveRP.Storage.DataAccessLayer.ChatCompletionPresets.BusinessObjects;
using CohesiveRP.Storage.DataAccessLayer.ChatCompletionPresets.BusinessObjects.Format;

namespace CohesiveRP.Storage.DataAccessLayer.ChatCompletionPresets.Injectors.Cohesion
{
    internal class StyleEditorCompletionPresetInjector : ICompletionPresetInjector
    {
        internal static ChatCompletionPresetsDbModel InjectPreset()
        {
            return new ChatCompletionPresetsDbModel
            {
                Name = "Default-Style-Edition-Prompt-Generator-Preset",
                ChatCompletionPresetId = StorageConstants.DEFAULT_STYLE_EDITION_COMPLETION_PRESET,
                CreatedAtUtc = DateTime.UtcNow,
                Format = new GlobalPromptContextFormat()
                {
                    MaxTokensToGenerate = 4096,
                    //JsonSchemaName = "style_edition_prompt_generator",
                    //JsonSchemaDocument = StrictJsonSchemaGenerator.Generate<SceneTrackerValidationResult>(),
                    OrderedElementsWithinTheGlobalPromptContext =
                    [
                        new PromptContextFormatElement
                        {
                            Name = "Core Directive",
                            Tag = PromptContextFormatTag.StyleValidationEdition,
                            Enabled = true,
                            Options = new PromptContextFormatElementOptions
                            {
                                Format = "\rYou are creating a roleplay (story) step by step, one reply at a time. You previously generated the text within the xml tag '<last_reply_by_AI>'. A review of the final text style have the following suggestions: \r\n{{description}}.\r\n Your task is to alter the text within '<last_reply_by_AI>' and output a modified version that includes the revised text after applying the suggestions.\r\n<last_reply_by_AI>\r\n{{last_reply_by_ai}}\r\n<last_reply_by_AI>\r\n"
                            }
                        },
                        new PromptContextFormatElement
                        {
                            Tag = PromptContextFormatTag.BehavioralInstructions,
                            Name = "BehavioralInstructions",
                            Enabled = true,
                            Options = new PromptContextFormatElementOptions
                            {
                                Format = "Output *only* the final text representing the story, basing yourself in the text within '<last_reply_by_AI>' xml tag modified with the provided suggestions. Your reply will be injected directly into the story, so it must be solely the revised and complete story content.",
                            }
                        }
                    ]
                }
            };
        }
    }
}
