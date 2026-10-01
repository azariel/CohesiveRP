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
                                Format = "<core_directive>\r\n  Stop the roleplay. You are a profesionnal writer. Your task is to analyze the prose that was generated to raise errors, incoherence or concerns about the *prose* and infer suggestions on how to improve it. You are forbidden from alterating the meaning of events, actions and speech within the provided text in '<last_message_by_AI>' xml tag, but you can suggests ways to reformulate it in a better way.\r\n</core_directive>\r\n"
                            }
                        },
                        new PromptContextFormatElement
                        {
                            Name = "Sub Directive",
                            Tag = PromptContextFormatTag.Directive,
                            Enabled = true,
                            Options = new PromptContextFormatElementOptions
                            {
                                Format = "<directives>\r\n1. {{user}} is the character played by the User. The User will write the next reply to impersonate that character, so we want to avoid deciding what that character do, react or say. We will leave this flexibility to the User instead. Our role is to impersonate the other characters and the world as well as how it react to the User to make it believable and immersive.\r\n2. You are limited on suggesting modifications to the prose.\r\n3. Keep in mind that we want the text to be short, a few paragraphs long. The goal is to show how the world and characters react to the User, perhaps add a new story development, introduce a new event or characters and continue the story, but since the User ({{user}}) can only be impersonated by the User, we want to give the User the ability to play their character and decide how the story grows as well.\r\n Try to find ways to improve the prose. The end goal here is to have text that could be found in teenage or young adults novels.\r\n</directives>\r\n"
                            }
                        },
                        new PromptContextFormatElement
                        {
                            Name = "Prose Rules",
                            Tag = PromptContextFormatTag.Directive,
                            Enabled = true,
                            Options = new PromptContextFormatElementOptions
                            {
                                Format = "<prose_rules>\r\n1. Avoid antithesis and contrastive construction.\r\n2. Keep a good balance between lean text and details (at most a few paragraphs, otherwise it gets too convulated). Keep crucial details, but remove poetic language, metaphors and details that adds nothing concrete or usable in the story context. You may leave one or two for diversity purpose when they're creative, but they must be anchored in reality and in the current scene context.\r\n3. Prefer concrete physical reactions.\r\n4. Stay inside the current scene.\r\n5. Don't introduce incidental sensory events just to make prose feel alive. They must have an impact or use by at least one character.\r\n6. Don't replace concrete information with abstract emotional interpretation.\r\n7. Don't inflate simple actions into elaborate descriptions.\r\n8. Prefer observable behavior over authorial interpretation.\r\n9. Don't mechanically apply \"not X, but Y\" or similar contrast structures.\r\n10. Show emotions through actions when possible.\r\n11. When an emotion, reaction, or motivation is genuinely unclear from the existing prose, a brief glimpse of the character's thoughts may be suggested. Do not use internal thoughts merely to restate an emotion that is already apparent.\r\n12. Do not alter the meaning, sequence, outcome, or implications of events, actions, or dialogue. Suggestions may improve how something is expressed, but must preserve what actually happens.\r\n13. Do not modify {{user}}'s character. Do not suggest actions, dialogue, thoughts, emotions, decisions, or reactions for {{user}} unless they are already explicitly present in the provided text.\r\n14. Prefer one strong, relevant detail over several weaker decorative details.\r\n</prose_rules>\r\n"
                            }
                        },
                        new PromptContextFormatElement
                        {
                            Name = "Core Directive",
                            Tag = PromptContextFormatTag.LastAIMessage,
                            Enabled = true,
                            Options = new PromptContextFormatElementOptions
                            {
                                Format = "This is the very *LAST* (most RECENT) message by the AI:\r\n<last_message_by_AI>{{item_description}}</last_message_by_AI>\r\n"
                            }
                        },
                        new PromptContextFormatElement
                        {
                            Tag = PromptContextFormatTag.BehavioralInstructions,
                            Name = "BehavioralInstructions",
                            Enabled = true,
                            Options = new PromptContextFormatElementOptions
                            {
                                Format = "Each recommendation must be an actionable suggestion that you make that will enhance the prose. This could mean to remove something, alter a phrase, add details, etc.",
                            }
                        }
                    ]
                }
            };
        }
    }
}
