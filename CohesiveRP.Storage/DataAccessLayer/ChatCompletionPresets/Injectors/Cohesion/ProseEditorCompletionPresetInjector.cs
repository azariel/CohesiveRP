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
                                Format = "\rYou are creating a roleplay (story) step by step, one reply at a time. You previously generated the text within the xml tag '<last_reply_by_AI>'. A review of the prose have the following suggestions: \r\n{{description}}.\r\n Your task is to alter the text within '<last_reply_by_AI>' and output every Find and Replace operations to bring the changes found in the provided suggestions.\r\n<last_reply_by_AI>\r\n{{last_reply_by_ai}}\r\n<last_reply_by_AI>\r\nYour output should represent a find and replace array. 'id' represent the ID matching the sentence to make the operation on. 'action' represent the action to execute on that sentence ('Delete' or 'Replace'). 'text' represent what to replace the sentence with when the action is 'Replace'. ```public enum FindReplaceAction\r\n    {\r\n        Undefined = 0,\r\n        Delete = 1,\r\n        Replace = 2,\r\n        Trim = 3,\r\n    }```\r\n"
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
