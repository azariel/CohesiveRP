using CohesiveRP.Storage.DataAccessLayer.AIQueries;
using CohesiveRP.Storage.DataAccessLayer.ChatCompletionPresets.BusinessObjects;
using CohesiveRP.Storage.DataAccessLayer.ChatCompletionPresets.BusinessObjects.Format;

namespace CohesiveRP.Storage.DataAccessLayer.ChatCompletionPresets.Injectors
{
    internal class SummaryCompletionPresetInjector : ICompletionPresetInjector
    {
        internal static ChatCompletionPresetsDbModel InjectPreset()
        {
            return new ChatCompletionPresetsDbModel
                {
                    Name = "Default-Summarize-Preset",
                    ChatCompletionPresetId = StorageConstants.DEFAULT_SUMMARIZE_COMPLETION_PRESET,
                    CreatedAtUtc = DateTime.UtcNow,
                    Format = new GlobalPromptContextFormat()
                    {
                        MaxTokensToGenerate = 2048,
                        OrderedElementsWithinTheGlobalPromptContext = new List<PromptContextFormatElement>
                        {
                            new PromptContextFormatElement
                            {
                                Tag = PromptContextFormatTag.Directive,
                                Name = "Directive",
                                Enabled = true,
                                Options = new PromptContextFormatElementOptions
                                {
                                    Format = "Role:Summarizer | Goal:summarize the provided text into a cohesive summary | MaxLen:512\r\n\r\n<task>\r\nStop your roleplay. You are now an helpful assistant. Your task is to summarize a roleplay session. You will be given a fictional narrative which you need to summarize into a single, short and concise statement of facts, events, speech and actions. Create a detailed beat-by-beat summary in narrative prose in past tense. You must ignore the roleplay, your role isn't to continue the roleplay, but to summarize the text.\r\n\r\nNote that {{user}} will always use first-person pronouns.\r\nFirst, note the dates and time in the roleplay. Each message if prefixed by the date and time. Then capture this scene accurately without losing ANY important information EXCEPT FOR [OOC] conversation/interaction. All [OOC] conversation/interaction is not useful for summaries.\r\nThis summary will go in a vectorized database, so include:\r\n- All important story beats/events that happened\r\n- Key interaction highlights and character developments\r\n- Notable details, memorable quotes, and revelations\r\n- Outcome and anything else important for future interactions between {{user}} and other characters\r\nCapture ALL nuance without repeating verbatim. Make it comprehensive yet digestible.\r\nResponses should be no more than 200 words an a single paragraph long.\r\nInclude names when possible.\r\nYour response must ONLY contain the summary.\r\n</<task>",
                                }
                            },
                            new PromptContextFormatElement
                            {
                                Tag = PromptContextFormatTag.World,
                                Name = "World",
                                Enabled = true,
                                Options = new PromptContextFormatElementOptions
                                {
                                    Format = "{{item_header}}\r\n{{item_description}}",
                                }
                            },
                            new PromptContextFormatElement
                            {
                                Tag = PromptContextFormatTag.RelevantCharacters,
                                Name = "RelevantCharacters",
                                Enabled = true,
                                Options = new PromptContextFormatElementRelevantCharactersOptions
                                {
                                    Format = "{{item_header}}\r\n{{item_description}}",
                                }
                            },
                            new PromptContextFormatElement
                            {
                                Tag = PromptContextFormatTag.LastXMessagesToSummarize,
                                Name = "LastXMessagesToSummarize",
                                Enabled = true,
                                Options = new PromptContextFormatElementOptions
                                {
                                    Format = "<messages_to_summarize>\r\n{{item_description}}</messages_to_summarize>",
                                }
                            },
                            new PromptContextFormatElement
                            {
                                Tag = PromptContextFormatTag.BehavioralInstructions,
                                Name = "BehavioralInstructions",
                                Enabled = true,
                                Options = new PromptContextFormatElementOptions
                                {
                                    Format = "<behavioral_instruction>\r\nHow do you respond?\r\nThink about it first. Recall the available lore, characters, objective, scene and messages. Consider the current point in the narrative and how the characters got there.\r\nWrite in past tense third-person omniscient narration.\r\nFocus your summary on what happened in the messages_to_summarize instead of lore, world or characters.\r\nKeep in your summary the details about what exactly {{user}} said.\r\nKeep in your summary the details about what {{user}}'s actions were.\r\n\r\nNow, continue directly with your summarization of the messages_to_summarize content only.\r\n<behavioral_instruction>",
                                }
                            }
                        }
                    }
                };
        }
    }
}
