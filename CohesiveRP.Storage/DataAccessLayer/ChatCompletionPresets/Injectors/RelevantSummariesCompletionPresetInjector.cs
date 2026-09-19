using CohesiveRP.Storage.DataAccessLayer.AIQueries;
using CohesiveRP.Storage.DataAccessLayer.ChatCompletionPresets.BusinessObjects;
using CohesiveRP.Storage.DataAccessLayer.ChatCompletionPresets.BusinessObjects.Format;

namespace CohesiveRP.Storage.DataAccessLayer.ChatCompletionPresets.Injectors
{
    internal class RelevantSummariesCompletionPresetInjector : ICompletionPresetInjector
    {
        internal static ChatCompletionPresetsDbModel InjectPreset()
        {
            return new ChatCompletionPresetsDbModel
            {
                Name = "Default-Relevant-Summaries-Prompt-Generator-Preset",
                ChatCompletionPresetId = StorageConstants.DEFAULT_RELEVANT_SUMMARIES_COMPLETION_PRESET,
                CreatedAtUtc = DateTime.UtcNow,
                Format = new GlobalPromptContextFormat()
                {
                    MaxTokensToGenerate = 8192,
                    OrderedElementsWithinTheGlobalPromptContext = new List<PromptContextFormatElement>
                    {
                        new PromptContextFormatElement
                        {
                            Tag = PromptContextFormatTag.Directive,
                            Name = "RelevantSummariesInstructions",
                            Enabled = true,
                            Options = new PromptContextFormatElementOptions
                            {
                                Format = "<task>\r\nYou are a story analyst. You must analyze the provided text to evaluate *WHICH* information are *RELEVANT* to the CURRENT SCENE. The current scene is defined by the last few messages, but most importantly the very last message as the AI will eventually generate a reply from that point forward. Our task is to reduce the amount of information found in the summaries(history) sections to enhance the adherance of the LLM that will continue the roleplay to stick to the context in a more efficient way. Your task is *NOT* to summarize the information, but to group the information that is relevant to the story context instead. Inject every relevant summaries as-is into your final output.\r\n\r\nThe sections that contain the summaries/history that we must evaluate are:\r\n- <summary_very_long_term>\r\n- <summary_long_term>\r\n- <summary_medium_term>\r\n- <summary_short_term>\r\n\r\nNote that those sections can be *missing* when there is not enough history, for example when we're dealing with a short roleplay. You can safely ignore the missing sections.\r\n\r\nTo decide *which* information is relevant to the current scene, ask yourself those questions:\r\n\r\nA) Is this information relevant to a character that is present in the scene (something about a motive, goal, past event that would influence that character, etc).\r\nB) Is this information relevant to what is happening in the scene.\r\nC) Is this information relevant to what could happen soon following the scene. This could lead to the AI influencing a character or the environment in a way that would provide immersion, coherence or logical outcomes.\r\nD) Is this information relevant for interactions between characters.\r\n\r\nWhen unsure, keep the information in your reply. We prefer to have more than not enough as this could create incoherences or break the immersion, which would be catastrophic.\r\nYou do *NOT* write prose, you do *NOT* write drafts, you are an analyst. Your output MUST be a single text output matching the refined summaries that are worth keeping.\r\n</task>\r\n\r\n<story_prose>\r\nA) *markdown italics* are to signify unspoken thoughts from a character. Other characters can't hear those thoughts except when clearly indicated (by magic or something logical within the story context).\r\nB) \"quotation marks\" are to specify spoken words\r\nC) narration is in plain text.\r\n</story_prose>\r\n<world_realism>\r\nA) The world and story are agnostic of {{user}}. The world goes on, characters interact without {{user}}'s implication.\r\nB) {{user}}'s actions can succeed or fail based on logic, not convenience. Actions have long-lasting consequences, can be rejected, make enemies, or fail spectacularly.\r\nC) Kindness is rare, especially towards strangers. Characters might be cold, dismissive, suspicious, or hostile based on context.\r\nD) Infer characters' common sense and subjectivity. They must always follow their own goals or interests and value their own life first and foremost in most situations.\r\nE) Reputation, accomplishments, achievements, and past actions/dialogues matter inexplicably; show effects through reactions, perceptions, gossip, and adaptive scenarios.\r\nF) Characters can't react directly to the story description, they may only react to what they perceives themselves within the world (eyes, touch, ears, smell, taste).\r\n</world_realism>\r\n<characters_realism>\r\nA) The world is full of authentic, multidimensional, dynamic characters. They have the capacity to be good, evil, and everything in between; with contradictions, hypocrisies, and opinions. They act of their own volition to achieve their own goals, ambitions, and desires. They do not fear going against {{user}} if their personality dictates it. They can refuse, deflect, lie, walk away, ignore, distrust, dislike, or actively work against {{user}}.\r\nB) Characters are NOT omniscient. They do NOT know other characters' secrets, thoughts, or private/unwitnessed information unless logically learned. Information travels via observations, communication, and deductions. Characters should act on false/incorrect information, producing misunderstandings or conflicts.\r\nC) Subtlety is difficult to detect; when a character hides something, it's hard for others to notice.\r\n</characters_realism>\r\nDo NOT describe characters, your role is to select the summaries entries within the specified tags that are *relevant* to the current scene.\n",
                            }
                        },
                        new PromptContextFormatElement { Tag = PromptContextFormatTag.SummaryExtraTerm, Name = "SummaryExtraTerm", Enabled = true, Options = new PromptContextFormatElementOptions { Format = "- {{item_description}}" } },
                        new PromptContextFormatElement { Tag = PromptContextFormatTag.SummaryLongTerm, Name = "SummaryLongTerm", Enabled = true, Options = new PromptContextFormatElementOptions { Format = "- {{item_description}}" } },
                        new PromptContextFormatElement { Tag = PromptContextFormatTag.SummaryMediumTerm, Name = "SummaryMediumTerm", Enabled = true, Options = new PromptContextFormatElementOptions { Format = "- {{item_description}}" } },
                        new PromptContextFormatElement { Tag = PromptContextFormatTag.SummaryShortTerm, Name = "SummaryShortTerm", Enabled = true, Options = new PromptContextFormatElementOptions { Format = "- {{item_description}}" } },
                        new PromptContextFormatElement { Tag = PromptContextFormatTag.NarrativeArchitecture, Name = "NarrativeArchitecture", Enabled = true, Options = new PromptContextFormatElementOptions { Format = "<secret_plot_state>\r\n{{description}}</secret_plot_state>\r\n" } },
                        new PromptContextFormatElement { Tag = PromptContextFormatTag.RelevantCharacters, Name = "RelevantCharacters", Enabled = true, Options = new PromptContextFormatElementRelevantCharactersOptions { Format = "<{{item_header}}>\r\n{{item_description}}\r\n</{{item_header}}>", IncludeKnownCharacters = true } },
                        new PromptContextFormatElement { Tag = PromptContextFormatTag.LastUnsummarizedMessages, Name = "LastUnsummarizedMessages", Enabled = true, Options = new PromptContextFormatElementOptions { Format = "{{item_description}}" } },
                        new PromptContextFormatElement { Tag = PromptContextFormatTag.SceneTracker, Name = "SceneTracker", Enabled = true, Options = new PromptContextFormatElementOptions { Format = "{{item_description}}" } },
                        new PromptContextFormatElement { Tag = PromptContextFormatTag.CurrentObjective, Name = "CurrentObjective", Enabled = true, Options = new PromptContextFormatElementOptions { Format = "<current_objective>\r\nStory progression objective.\r\n{{objective_description}}\r\n</current_objective>" } },
                        new PromptContextFormatElement { Tag = PromptContextFormatTag.LastUserMessage, Name = "LastUserMessage", Enabled = true, Options = new PromptContextFormatElementOptions { Format = "\r\nThis is the very *LAST* (most RECENT) message by the User:\r\n{{item_description}}\r\n" } },
                        new PromptContextFormatElement
                        {
                            Tag = PromptContextFormatTag.BehavioralInstructions,
                            Name = "BehavioralInstructions",
                            Enabled = true,
                            Options = new PromptContextFormatElementOptions
                            {
                                Format = "<behavioral_instruction>\r\nHow do you respond?\r\nThink about it first. Consider the current point in the narrative and how you got there.\r\nWrite your analysis, do *NOT* write a narrative response, we're solely looking for a trimmed down version of <history> that would keep only the events and facts that are relevant to the *CURRENT* scene.\r\n Limit your reply to 6144 tokens at maximum.\r\n</behavioral_instruction>",
                            }
                        }
                    },
                }
            };
        }
    }
}
