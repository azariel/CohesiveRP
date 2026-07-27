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
                                    Format = "<Prose_Guardian>\r\nStudy the last few AI messages and produce concrete, actionable writing directives for the next generation. You do NOT write roleplay prose, dialogue, narration, or story content yourself. You only produce directives.\r\n\r\nAnalyze recent messages and produce directives covering ALL of these categories:\r\n1. REPETITION BAN LIST:\r\n  Scan the last messages for overused words, phrases, imagery, gestures, actions, body parts and descriptors. Anything appearing 5+ times across recent messages is BANNED.\r\n  1a. List each banned element explicitly (e.g., \"BANNED: eyes, gaze, smirk, let out a breath, heart pounding, fingers traced, raised an eyebrow\").\r\n  1b. Include overused verbs, adjectives, adverbs, physical descriptions, and emotional beats (\"heart skipped a beat\" appearing multiple times).\r\n  1c. PERMANENT BANS (ALWAYS include these in EVERY output, regardless of repetition detection): single-word sentences that are not dialogue, floating adjectives (e.g., \"Flat. Final.\" or \"Not a moan. Weirder.\"), sentence fragments, any non-dialogue sentence under 6 words, comma splices (joining two independent clauses with only a comma), missing spaces after commas or punctuation.\r\n2. RHETORICAL DEVICE ROTATION:\r\n  From this master list, identify which devices WERE used and which were NOT:\r\n  Simile, Metaphor, Personification, Hyperbole, Understatement/Litotes, Irony, Rhetorical question, Anaphora, Asyndeton, Polysyndeton, Chiasmus, Antithesis, Alliteration, Onomatopoeia, Synecdoche, Metonymy, Oxymoron, Paradox, Epistrophe, Aposiopesis (trailing off…)\r\n  2a. \"USED RECENTLY (avoid): [devices found].\"\r\n  2b. \"USE THIS TURN (pick 1–2): [devices NOT yet used, with a brief note on how to apply them to the current scene].\"\r\n3. SENTENCE STRUCTURE:\r\nAnalyze sentence patterns in recent messages:\r\n  3a. Average sentence length; if long, demand shorter (but complete) sentences. If short, demand at least 1–2 complex/compound sentences.\r\n  3b. If mostly declarative, demand interrogative or exclamatory variation.\r\n  3c. If paragraphs follow the same rhythm (e.g., action → dialogue → thought every time), prescribe a DIFFERENT structure.\r\n  3d. Specify: \"This turn: open with [short/long/dialogue]. Vary between 8 and 25 word sentences. Break at least one expected rhythm. CRITICAL: NEVER demand or use single-word sentences, fragments, or floating adjectives. ALWAYS demand complete, grammatically correct sentences. If a descriptor like 'flat' or 'final' is needed, attach it to a complete sentence (e.g., 'Her voice sounded flat and final').\"\r\n4. VOCABULARY FRESHNESS:\r\nList 3–5 specific, fresh words or phrases the model should use this turn: vivid, unexpected, and genre-appropriate. Not purple prose, just precise and evocative. Keep the words simple enough for a person for which english is their second language to be able to understand.\r\n  4a. Example: Instead of \"walked slowly\" → \"ambled\", \"drifted\", \"picked their way through.\"\r\n5. SENSORY CHANNEL ROTATION:\r\nCheck which senses appeared in recent messages: Sight, Sound, Smell, Touch/Texture, Taste, Temperature, Proprioception (body position/movement), Interoception (internal body feelings).\r\n  5a. \"OVERUSED: [sight, sound].\"\r\n  5b. \"PRIORITIZE THIS TURN: [smell, texture, temperature].\"\r\n6. SHOW-DON'T-TELL ENFORCEMENT:\r\nIf recent messages TOLD emotions directly (e.g., \"she felt angry\", \"he was nervous\"), demand the next turn SHOW them through:\r\n  6a. Micro-actions (fidgeting, jaw clenching, shifting weight).\r\n  6b. Environmental interaction (kicking a stone, gripping a cup tighter).\r\n  6c. Physiological responses (dry mouth, heat in chest, cold fingers).\r\n  6d. Dialogue subtext — what's NOT said matters as much as what is said.\r\nOutput format: output directly, no wrapping tags:\r\nBANNED ELEMENTS: ... (ALWAYS include permanent bans from 1c)\r\nRHETORICAL DEVICES — Used recently: ... | Use this turn: ...\r\nSENTENCE STRUCTURE: ...\r\nFRESH VOCABULARY: ...\r\nSENSORY FOCUS: ...\r\nSHOW-DON'T-TELL: ...\r\nBe brutally specific. Reference actual text from the recent messages when flagging repetition. Keep total output compact (150–250 words).\r\n</Prose_Guardian>\r\n<Previous_Prose_Guardian>\r\nHere is the previous prose guardian description to guide you into a coherent narration:\r\n```\r\n{{description}}\r\n```\r\n</Previous_Prose_Guardian>\n{{recent_messages}}",
                                }
                            },
                        }
                    }
                };
        }
    }
}
