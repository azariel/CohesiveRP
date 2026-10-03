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
                                Format = "<core_directive>\r\nStop the roleplay. You are a line editor for a young-adult novel. The text below is already good. Find only the few places where a small edit clearly helps. Returning no mutations is a correct answer. You are forbidden from alterating the meaning of events, actions and speech within the provided text in '<last_message_by_AI>' xml tag, but you can suggests ways to reformulate it in a better way.\r\n</core_directive>\r\n"
                            }
                        },
                        new PromptContextFormatElement
                        {
                            Name = "Look For",
                            Tag = PromptContextFormatTag.Directive,
                            Enabled = true,
                            Options = new PromptContextFormatElementOptions
                            {
                                Format = "<look_for priority=\"in order\">\r\n1. redundancy: repeats information already stated earlier in this message (speech included). Set \"duplicateOf\" to the earlier sentence id.\r\n2. clarity: confusing or unexplained statement. Smallest possible fix.\r\n3. thought: a *thought* whose emotion is already visible in nearby behavior. Delete it. Otherwise keep it.\r\n4. filler: a detail nobody reacts to or uses.\r\n5. tic: comma-stacked adjective pairs (\"a steady, measured rhythm\") and \"almost imperceptible\". Use one adjective or \"X and Y\".\r\n6. missing_reaction: only if no side character reacts. Use insert_after once, max 20 words, one observable action by a character from <present_characters>.\r\n</look_for>\r\n"
                            }
                        },
                        new PromptContextFormatElement
                        {
                            Name = "Leave Alone",
                            Tag = PromptContextFormatTag.Directive,
                            Enabled = true,
                            Options = new PromptContextFormatElementOptions
                            {
                                Format = "<leave_alone>\r\n- The last line that hands the scene back to {{user}}.\r\n- Figurative language that reveals character or plot (keep one or two).\r\n- Sounds or details that mark tension or silence.\r\n- Anything that changes events, outcomes, or what {{user}} does.\r\n- A rewording that is not clearly better. Do not swap one phrase for an equivalent one.\r\n</leave_alone>\r\n"
                            }
                        },
                        new PromptContextFormatElement
                        {
                            Name = "Sub Directive",
                            Tag = PromptContextFormatTag.Directive,
                            Enabled = true,
                            Options = new PromptContextFormatElementOptions
                            {
                                Format = "<directives>\r\n1. {{user}} is the character played by the User. The User will write the next reply to impersonate that character, so we want to avoid deciding what that character do, react or say. We will leave this flexibility to the User instead. Our role is to impersonate the other characters and the world as well as how it react to the User to make it believable and immersive.\r\n2. You are limited on suggesting modifications to the prose.\r\n3. Keep in mind that we want the text to be short, a few paragraphs long.\r\n Try to find ways to improve the prose. The end goal here is to have text that could be found in young-adults novels.\r\n</directives>\r\n"
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
                            Name = "Examples",
                            Tag = PromptContextFormatTag.Directive,
                            Enabled = true,
                            Options = new PromptContextFormatElementOptions
                            {
                                Format = "<examples>\r\n<example>\r\n<text>\r\n[S1] Lysa slid the tray of loaves onto the counter.\r\n[S2] A dog barked somewhere down the street.\r\n[S3] [LOCKED] \"Fresh this morning,\" she said.\r\n[S4] She wiped her floury hands on her apron, then wiped them again on a towel.\r\n[S5] The soldier eyed the bread, then the coins in his palm.\r\n[S6] [LOCKED] \"How many can I get for this?\"\r\n</text>\r\n<output>{\"delete\":[\"S2\",\"S4\"]}</output>\r\n</example>\r\n\r\n<example>\r\n<text>\r\n[S1] The gate creaked open and Warden Pike stepped out.\r\n[S2] [LOCKED] \"The bridge is out,\" he said. \"Nobody crosses tonight.\"\r\n[S3] He told them again that the bridge had collapsed and no one would be crossing.\r\n[S4] Rennick set his jaw and glanced at the river.\r\n[S5] The torches along the wall flickered in the wind.\r\n[S6] [LOCKED] \"Well, traveler? What will it be?\"\r\n</text>\r\n<output>{\"delete\":[\"S3\"]}</output>\r\n</example>\r\n\r\n<example>\r\n<text>\r\n[S1] Rain ticked against the shutters.\r\n[S2] Old Bram counted the coins twice and slid half across the table.\r\n[S3] [LOCKED] \"That's the last of it.\"\r\n[S4] Wren pocketed her share without a word.\r\n[S5] [LOCKED] \"Well? Are you coming or not?\"\r\n</text>\r\n<output>{\"delete\":[]}</output>\r\n</example>\r\n</examples>\r\n"
                            }
                        },
                        new PromptContextFormatElement
                        {
                            Name = "AI message",
                            Tag = PromptContextFormatTag.LastAIMessageSentences,
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
                                Format = "Each value within the output array 'recommendations' must be a textual value representing the recommendation. You may return mutations, each on one sentence id. At most 5 mutations. Each one has: category, id, action, text, duplicateOf (or null). Allowed actions:\r\n- delete: remove a sentence that adds nothing concrete.\r\n- trim: remove a phrase or clause from a sentence. \"text\" is the exact words to remove, copied from the sentence.\r\n- replace: only to shorten a sentence, using only words already in it.\r\nNever add a person, object, sound, body sensation, or action that is not already in the sentence.\r\nNever touch [LOCKED] sentences.\r\nIf a sentence cannot be improved by cutting, leave it alone. Returning no mutations is a correct answer. Make sure that your suggestions won't make the final text incoherent or worse than the original.",
                            }
                        }
                    ]
                }
            };
        }
    }
}
