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
                    JsonSchemaDocument = StrictJsonSchemaGenerator.Generate<StyleValidationResult>(),
                    OrderedElementsWithinTheGlobalPromptContext =
                    [
                        new PromptContextFormatElement
                        {
                            Name = "Core Directive",
                            Tag = PromptContextFormatTag.Directive,
                            Enabled = true,
                            Options = new PromptContextFormatElementOptions
                            {
                                Format = "<core_directive>\r\n  Stop the roleplay. You are a profesionnal writer. Your task is to analyze the style that was generated to raise errors, incoherence or concerns about the *style* and infer suggestions on how to fix it. You are forbidden from alterating the meaning of events, actions and speech within the provided text in '<last_message_by_AI>' xml tag, but you can suggests ways to reformulate it in a better way.\r\n</core_directive>\r\n"
                            }
                        },
                        new PromptContextFormatElement
                        {
                            Name = "Sub Directive",
                            Tag = PromptContextFormatTag.Directive,
                            Enabled = true,
                            Options = new PromptContextFormatElementOptions
                            {
                                Format = "<directives>\r\n1. {{user}} is the character played by the User. The User will write the next reply to impersonate that character, so we want to avoid deciding what that character do, react or say. We will leave this flexibility to the User instead. Our role is to impersonate the other characters and the world as well as how it react to the User to make it believable and immersive.\r\n2. You are limited on suggesting modifications to the text's style.\r\n3. Keep in mind that we want the text to be short, a few paragraphs long. The goal is to show how the world and characters react to the User, perhaps add a new story development, introduce a new event or characters and continue the story, but since the User ({{user}}) can only be impersonated by the User, we want to give the User the ability to play their character and decide how the story grows as well.\r\n Try to find ways to improve the style, especially when the current text uses forbidden elements.\r\n</directives>\r\n"
                            }
                        },
                        new PromptContextFormatElement
                        {
                            Name = "Style Rules",
                            Tag = PromptContextFormatTag.Directive,
                            Enabled = true,
                            Options = new PromptContextFormatElementOptions
                            {
                                Format = "<style_rules>\r\n<scope>\r\nThese rules govern formatting and wording only. Never change events, actions, dialogue meaning, or outcomes. Never write actions, speech, thoughts, or emotions for {{user}} that are not already in the text.\r\n</scope>\r\n\r\n<narration>\r\n1. Point of view: third person limited, anchored on the viewpoint character. Do not drift into omniscient narration.\r\n2. Tense: [past tense] for narration. Dialogue may use any tense naturally.\r\n</narration>\r\n\r\n<formatting>\r\n3. Dialogue goes in \"double quotes\". Thoughts, when used, go in *italics*. No other emphasis markup.\r\n4. No headers, bullet points, bold text, or horizontal rules in the reply.\r\n5. No meta text: no \"Here is the revised version\", no summaries, no questions addressed to the player outside of OOC.\r\n6. Start a new paragraph when the speaker or the focus of the action changes.\r\n</formatting>\r\n\r\n<length>\r\n7. A reply may be shorter when the scene calls for it. Never pad to reach a length.\r\n</length>\r\n\r\n<dialogue>\r\n8. Use \"said\" or no tag when the speaker is obvious. Avoid tags that carry a manner or emotion adverb (\"she said sweetly\"), we need to show emotions through gestual.\r\n</dialogue>\r\n\r\n<variety>\r\n9. Do not open the reply with the same structure as the previous reply (same first word, a character's name followed by a verb, or a sensory opener).\r\n10. Do not end the reply with a reflective or summarizing line. End on an action, a line of dialogue, or a change in the situation.\r\n11. Do not reuse the same gesture or tic more than once per reply, or in two consecutive replies (smirking, tucking hair, sighing, swallowing hard, tilting head).\r\n12. When a character ask a question to {{user}}, avoid the context from steering away from it if we expect a reply from {{user}}, which is controlled by the User. We must avoid characters asking {{user}} a question and then steering away from it as-if they didn't just asked a question.\r\n</variety>\r\n\r\n<banned_phrases>\r\nNever use these or close variants:\r\n- \"a shiver ran down [pronoun] spine\"\r\n- \"a breath [pronoun] didn't know [pronoun] was holding\"\r\n- \"ministrations\"\r\n- \"orbs\" for eyes\r\n- \"[pronoun] heart skipped a beat\"\r\n- \"the air was thick with\"\r\n- \"time seemed to slow\"\r\n- \"a mixture of X and Y\" (as an emotion label)\r\n- \"couldn't help but\"\r\n- \"barely above a whisper\"\r\n</banned_phrases>\r\n</style_rules>\r\n"
                            }
                        },
                        new PromptContextFormatElement
                        {
                            Name = "Core Directive",
                            Tag = PromptContextFormatTag.LastAIMessageSentences,
                            Enabled = true,
                            Options = new PromptContextFormatElementOptions
                            {
                                Format = "This is the very *LAST* (most RECENT) message by the AI:\r\n{{item_description}}\r\n"
                            }
                        },
                        new PromptContextFormatElement
                        {
                            Tag = PromptContextFormatTag.BehavioralInstructions,
                            Name = "BehavioralInstructions",
                            Enabled = true,
                            Options = new PromptContextFormatElementOptions
                            {
                                Format = "Each value within the output array 'recommendations' must be a textual value representing the recommendation.You may return mutations, each on one sentence id. Allowed actions:\r\n- delete: remove a sentence that adds nothing concrete.\r\n- trim: remove a phrase or clause from a sentence. \"text\" is the exact words to remove, copied from the sentence.\r\n- replace: only to shorten a sentence, using only words already in it.\r\nNever add a person, object, sound, body sensation, or action that is not already in the sentence.\r\nNever touch [LOCKED] sentences.\r\nIf a sentence cannot be improved by cutting, leave it alone. Returning no mutations is a correct answer. Make sure that your suggestions won't make the final text incoherent or worse than the original.",
                            }
                        }
                    ]
                }
            };
        }
    }
}
