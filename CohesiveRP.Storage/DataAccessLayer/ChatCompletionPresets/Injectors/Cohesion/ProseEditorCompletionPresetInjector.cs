using CohesiveRP.Common.BusinessObjects;
using CohesiveRP.Common.Utils;
using CohesiveRP.Storage.DataAccessLayer.AIQueries;
using CohesiveRP.Storage.DataAccessLayer.ChatCompletionPresets.BusinessObjects;
using CohesiveRP.Storage.DataAccessLayer.ChatCompletionPresets.BusinessObjects.Format;

namespace CohesiveRP.Storage.DataAccessLayer.ChatCompletionPresets.Injectors.Cohesion
{
    internal class ProseEditorCompletionPresetInjector : ICompletionPresetInjector
    {
        internal static ChatCompletionPresetsDbModel InjectPreset()
        {
            return new ChatCompletionPresetsDbModel
            {
                Name = "Default-Prose-Edition-Prompt-Generator-Preset",
                ChatCompletionPresetId = StorageConstants.DEFAULT_PROSE_EDITION_COMPLETION_PRESET,
                CreatedAtUtc = DateTime.UtcNow,
                Format = new GlobalPromptContextFormat()
                {
                    MaxTokensToGenerate = 4096,
                    JsonSchemaName = "prose_edition_prompt_generator",
                    JsonSchemaDocument = StrictJsonSchemaGenerator.Generate<FindReplaceResultDto>(),
                    OrderedElementsWithinTheGlobalPromptContext =
                    [
                        new PromptContextFormatElement
                        {
                            Name = "Core Directive",
                            Tag = PromptContextFormatTag.ProseValidationEdition,
                            Enabled = true,
                            Options = new PromptContextFormatElementOptions
                            {
                                Format = "\rYou previously wrote the reply below. A reviewer made these suggestions:\r\n{{description}}\r\n\r\nConvert each suggestion into one operation. Output a JSON object with an array 'mutations'. Each operation has:\r\n- id: the sentence id (for example S12)\r\n- action: \"\"delete\"\", \"\"trim\"\" or \"\"replace\"\"\r\n- text: for delete use \"\"\"\"; for trim, the exact words to remove; for replace, the new sentence\r\n\r\n<reply>\r\n{{last_reply_by_ai}}\r\n</reply>\r\n"
                            }
                        },
                        new PromptContextFormatElement
                        {
                            Tag = PromptContextFormatTag.BehavioralInstructions,
                            Name = "BehavioralInstructions",
                            Enabled = true,
                            Options = new PromptContextFormatElementOptions
                            {
                                Format = "",
                            }
                        }
                    ]
                }
            };
        }
    }
}
