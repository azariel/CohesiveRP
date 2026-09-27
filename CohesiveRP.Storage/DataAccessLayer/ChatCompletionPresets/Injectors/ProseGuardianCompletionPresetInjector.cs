using CohesiveRP.Storage.DataAccessLayer.AIQueries;
using CohesiveRP.Storage.DataAccessLayer.ChatCompletionPresets.BusinessObjects;
using CohesiveRP.Storage.DataAccessLayer.ChatCompletionPresets.BusinessObjects.Format;

namespace CohesiveRP.Storage.DataAccessLayer.ChatCompletionPresets.Injectors
{
    internal class ProseGuardianCompletionPresetInjector : ICompletionPresetInjector
    {
        internal static ChatCompletionPresetsDbModel InjectPreset()
        {
            return new ChatCompletionPresetsDbModel
                {
                    Name = "Default-Prose-Guardian-Prompt-Generator-Preset",
                    ChatCompletionPresetId = StorageConstants.DEFAULT_PROSE_GUARDIAN_COMPLETION_PRESET,
                    CreatedAtUtc = DateTime.UtcNow,
                    Format = new GlobalPromptContextFormat()
                    {
                        MaxTokensToGenerate = 4096,
                        OrderedElementsWithinTheGlobalPromptContext = new List<PromptContextFormatElement>
                        {
                            new PromptContextFormatElement
                            {
                                Tag = PromptContextFormatTag.ProseGuardianInstructions,
                                Name = "ProseGuardianInstructions",
                                Enabled = true,
                                Options = new PromptContextFormatElementOptions
                                {
                                    Format = "<Prose_Guardian>\r\nStudy the last few AI messages and produce concrete, actionable writing directives for the next generation. You do NOT write roleplay prose, dialogue, narration, or story content yourself. You only produce directives.\r\n\r\nAnalyze recent messages and for each category, provide a correction only when one is genuinely useful. Otherwise output \"No correction needed.\":\r\n1. REPETITION TO AVOID:\r\n  Identify up to 6 noticeably repetitive phrases, imagery patterns, gestures, or sentence constructions. There may be fewer than 6, or none. Do not ban ordinary vocabulary merely because it appears frequently. Do not ban character-defining physical traits, important objects, recurring environmental details, or necessary vocabulary merely because they recur. Flag repetition only when the same description, gesture, or phrasing is becoming stylistically monotonous.\r\n  1a. List each repetition to avoid explicitly.\r\n  1b. Include repeated wording, distinctive descriptions, gestures, imagery, and emotional beats when their repetition is stylistically noticeable. Do not flag ordinary grammatical vocabulary solely because of frequency.\r\n2. SENTENCE / PARAGRAPH REPETITION:\r\n  Identify any noticeably repetitive sentence openings, paragraph rhythms,\r\n  or structural patterns from recent messages.\r\n  If a pattern is genuinely repetitive, tell the model to vary it naturally.\r\n  Do not prescribe sentence lengths, sentence types, opening structures,\r\n  rhetorical devices, or specific paragraph structures.\r\n  Natural pacing and grammatical correctness take priority.\r\n3. VOCABULARY:\r\nAvoid repeating distinctive wording when it has become noticeable or monotonous. Do not force synonyms when repetition is natural or necessary. Prefer natural wording appropriate to the scene. Do not deliberately introduce unusual vocabulary.\r\n4. SENSORY VARIETY:\r\n  If recent prose has noticeably overused one sensory channel, mention it.\r\n  Otherwise output \"No sensory correction needed.\"\r\n  Never require a particular sense or invent sensory details merely for variety.\r\n5. SHOW-DON'T-TELL:\r\n  Prefer concrete behavior, dialogue, and context over explicitly naming emotions\r\n  when appropriate. Avoid repetitive stock gestures and do not manufacture\r\n  physical reactions merely to demonstrate an emotion.\r\n6. EXCEPTIONS:\r\n  6a. Names (characters, locations, etc) repetitions are accepted, do NOT list them in the 'REPETITION TO AVOID' list.\r\nOutput format: output directly, no wrapping tags:\r\nREPETITION TO AVOID: ...\r\nSENTENCE STRUCTURE: ...\r\nVOCABULARY: ...\r\nSENSORY FOCUS: ...\r\nSHOW-DON'T-TELL: ...\r\nReference actual text from the recent messages when flagging repetition. Keep total output compact, normally under 120 words. Be precise and conservative. Only flag patterns that are genuinely noticeable. When uncertain, do not flag.\r\nAvoid instructions on the User ({{user}}) since this character is controlled by the player and the prose isn't enforceable.\r\n</Prose_Guardian>\r\n<Example>\r\nREPETITION TO AVOID: \"let out a breath\" — repeated 4 times; \"looked toward\" —\r\nrepeated 5 times; repeated use of characters touching their own face when\r\nreacting to tension.\r\n\r\nSENTENCE STRUCTURE: The last three responses repeatedly used dialogue followed\r\nby a brief action beat. Vary naturally in the next response if the scene allows.\r\n\r\nVOCABULARY: Avoid repeating the distinctive phrase \"for a moment\" from the\r\nprevious two responses. Otherwise no vocabulary correction is needed.\r\n\r\nSENSORY FOCUS: No sensory correction needed.\r\n\r\nSHOW-DON'T-TELL: Emotional reactions have recently relied heavily on physical\r\ngestures. Prefer behavior or dialogue that conveys the emotion naturally,\r\nwithout inserting another stock gesture.\r\n\r\n</Example>\r\n{{recent_messages}}\n",
                                }
                            },
                        }
                    }
                };
        }
    }
}
