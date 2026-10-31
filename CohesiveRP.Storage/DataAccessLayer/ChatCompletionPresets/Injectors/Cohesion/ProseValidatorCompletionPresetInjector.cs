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
                                Format = "<core_directive>\r\nStop the roleplay. You are a line editor for a young-adult novel. Below is a reply, one sentence per line. Most of it is fine. Suggest a small cut only where a sentence clearly hurts the text. An empty list is a correct answer. {{user}} is played by the User: never suggest anything about what {{user}} does, says or thinks. Never suggest anything that changes the meaning of events, actions or speech."
                            }
                        },
                        new PromptContextFormatElement
                        {
                            Name = "Look For",
                            Tag = PromptContextFormatTag.Directive,
                            Enabled = true,
                            Options = new PromptContextFormatElementOptions
                            {
                                Format = "<look_for priority=\"in order\">\r\n1. redundancy: repeats information already stated earlier in this message (speech included). Set \"duplicateOf\" to the earlier sentence id.\r\n2. clarity: confusing or unexplained statement. Smallest possible fix.\r\n3. Not story text: planning notes, rules, or instructions the writer gave itself (\"I need to...\", \"I must NOT...\", \"I'll start with...\"). DELETE every one, however many there are.\r\n4. A stacked adjective pair (\"a single, sharp click\") or a detail nobody reacts to. TRIM it.\r\n5. missing_reaction: only if no side character reacts. Use insert_after once, max 20 words, one observable action by a character from <present_characters>.\r\n</look_for>\r\n"
                            }
                        },
                        new PromptContextFormatElement
                        {
                            Name = "Never",
                            Tag = PromptContextFormatTag.Directive,
                            Enabled = true,
                            Options = new PromptContextFormatElementOptions
                            {
                                Format = "<never>\r\n- Modify the last line that hands the scene back to {{user}}.\r\n- Touch a [LOCKED] sentence or the last sentence.\r\n- Delete a sentence that introduces a person or object the next sentence refers to (\"He\", \"She\", \"it\").\r\n- Reword a sentence just to reword it.\r\n- Add anything that is not already in the sentence..\r\n</never>\r\n"
                            }
                        },
                        //new PromptContextFormatElement
                        //{
                        //    Name = "Sub Directive",
                        //    Tag = PromptContextFormatTag.Directive,
                        //    Enabled = true,
                        //    Options = new PromptContextFormatElementOptions
                        //    {
                        //        Format = "<directives>\r\n1. {{user}} is the character played by the User. The User will write the next reply to impersonate that character, so we want to avoid deciding what that character do, react or say. We will leave this flexibility to the User instead. Our role is to impersonate the other characters and the world as well as how it react to the User to make it believable and immersive.\r\n2. You are limited on suggesting modifications to the prose.\r\n3. Keep in mind that we want the text to be short, a few paragraphs long.\r\n Try to find ways to improve the prose. The end goal here is to have text that could be found in young-adults novels.\r\n</directives>\r\n"
                        //    }
                        //},
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
                                Format = "<examples><example>\r\n[S1] I need to show the aftermath without repeating the dialogue.\r\n[S2] I will end on Mara's silence.\r\n[S3] The kettle clicked off and Mara poured two cups.\r\n[S4] She didn't sit; she stood by the window with her cup.\r\n[S5] Joren watched her over the rim.\r\n[S6] [LOCKED] \"\"You knew,\"\" he said.\r\nAnswer:\r\n[\"\"S1: DELETE -- planning note, not story text.\"\",\r\n \"\"S2: DELETE -- planning note, not story text.\"\",\r\n \"\"S4: REPLACE: She stood by the window with her cup. -- removes the 'didn't X; she Y' contrast.\"\"]\r\n</example>\r\n\r\n<example>\r\n[S1] The courier burst in, mud to his knees.\r\n[S2] He dropped a sealed letter on the table.\r\n[S3] [LOCKED] \"\"From the border,\"\" he panted. \"\"The pass is closed.\"\"\r\n[S4] The captain read it twice, then folded it.\r\n[S5] The courier repeated that the pass was closed.\r\n[S6] [LOCKED] \"\"How long?\"\"\r\nAnswer:\r\n[\"\"S5: DELETE -- repeats what the courier already said in S3.\"\"]\r\n</example>\r\n\r\n<example>\r\n[S1] Rain ticked against the shutters.\r\n[S2] Old Bram counted the coins twice and slid half across the table.\r\n[S3] [LOCKED] \"\"That's the last of it.\"\"\r\n[S4] Wren pocketed her share without a word.\r\n[S5] [LOCKED] \"\"Well? Are you coming or not?\"\"\r\nAnswer:\r\n[]\r\n</example></examples>\r\n"
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
                                Format = "Answer with a JSON array of strings in 'recommendations', one suggestion per string, in exactly one of these formats:\r\nS<number>: DELETE -- reason\r\nS<number>: TRIM: exact words to remove -- reason\r\nS<number>: REPLACE: shorter sentence using only its own words -- reason\r\nMaximum 8 suggestions. Planning notes at the top of the reply do not count toward the maximum.",
                            }
                        }
                    ]
                }
            };
        }
    }
}
