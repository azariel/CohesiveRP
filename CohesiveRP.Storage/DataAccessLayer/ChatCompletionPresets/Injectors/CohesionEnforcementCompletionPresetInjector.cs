using CohesiveRP.Storage.DataAccessLayer.AIQueries;
using CohesiveRP.Storage.DataAccessLayer.ChatCompletionPresets.BusinessObjects;
using CohesiveRP.Storage.DataAccessLayer.ChatCompletionPresets.BusinessObjects.Format;

namespace CohesiveRP.Storage.DataAccessLayer.ChatCompletionPresets.Injectors
{
    internal class CohesionEnforcementCompletionPresetInjector : ICompletionPresetInjector
    {
        internal static ChatCompletionPresetsDbModel InjectPreset()
        {
            return new ChatCompletionPresetsDbModel
            {
                Name = "Default-Cohesion-Enforcement-Prompt-Generator-Preset",
                ChatCompletionPresetId = StorageConstants.DEFAULT_COHESION_ENCORCEMENT_COMPLETION_PRESET,
                CreatedAtUtc = DateTime.UtcNow,
                Format = new GlobalPromptContextFormat()
                {
                    MaxTokensToGenerate = 4096,
                    OrderedElementsWithinTheGlobalPromptContext = new List<PromptContextFormatElement>
                        {
                            new PromptContextFormatElement
                            {
                                Tag = PromptContextFormatTag.World,
                                Name = "World",
                                Enabled = true,
                                Options = new PromptContextFormatElementOptions
                                {
                                    Format = "{{item_description}}",
                                }
                            },
                            new PromptContextFormatElement
                            {
                                Tag = PromptContextFormatTag.LoreByKeywords,
                                Name = "LoreByKeywords",
                                Enabled = true,
                                Options = new PromptContextFormatElementOptions
                                {
                                    Format = "{{item_header}}\r\n{{item_description}}",
                                }
                            },
                            new PromptContextFormatElement
                            {
                                Tag = PromptContextFormatTag.SummaryExtraTerm,
                                Name = "SummaryExtraTerm",
                                Enabled = true,
                                Options = new PromptContextFormatElementOptions
                                {
                                    Format = "- {{item_description}}",
                                }
                            },
                            new PromptContextFormatElement
                            {
                                Tag = PromptContextFormatTag.SummaryLongTerm,
                                Name = "SummaryLongTerm",
                                Enabled = true,
                                Options = new PromptContextFormatElementOptions
                                {
                                    Format = "- {{item_description}}",
                                }
                            },
                            new PromptContextFormatElement
                            {
                                Tag = PromptContextFormatTag.SummaryMediumTerm,
                                Name = "SummaryMediumTerm",
                                Enabled = true,
                                Options = new PromptContextFormatElementOptions
                                {
                                    Format = "- {{item_description}}",
                                }
                            },
                            new PromptContextFormatElement
                            {
                                Tag = PromptContextFormatTag.SummaryShortTerm,
                                Name = "SummaryShortTerm",
                                Enabled = true,
                                Options = new PromptContextFormatElementOptions
                                {
                                    Format = "- {{item_description}}",
                                }
                            },
                            new PromptContextFormatElement
                            {
                                Tag = PromptContextFormatTag.LoreByQuery,
                                Name = "LoreByQuery",
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
                                    Format = "<{{item_header}}>\r\n{{item_description}}\r\n</{{item_header}}>",
                                    IncludeKnownCharacters = true,
                                }
                            },
                            new PromptContextFormatElement
                            {
                                Tag = PromptContextFormatTag.LastUnsummarizedMessages,
                                Name = "LastUnsummarizedMessages",
                                Enabled = true,
                                Options = new PromptContextFormatElementOptions
                                {
                                    Format = "{{item_description}}",
                                }
                            },
                            new PromptContextFormatElement
                            {
                                Tag = PromptContextFormatTag.SceneTracker,
                                Name = "SceneTracker",
                                Enabled = true,
                                Options = new PromptContextFormatElementOptions
                                {
                                    Format = "{{item_description}}",
                                }
                            },
                            new PromptContextFormatElement
                            {
                                Tag = PromptContextFormatTag.CurrentObjective,
                                Name = "CurrentObjective",
                                Enabled = true,
                                Options = new PromptContextFormatElementOptions
                                {
                                    Format = "<current_objective>\r\nStory progression objective.\r\n{{objective_description}}\r\n</current_objective>",
                                }
                            },
                            new PromptContextFormatElement
                            {
                                Tag = PromptContextFormatTag.Directive,
                                Name = "Directive",
                                Enabled = true,
                                Options = new PromptContextFormatElementOptions
                                {
                                    Format = "<Original_Message>\n{{original_message}}\r\n</Original_Message>\n<Cohesion_Enforcement>\nReview the assistant's latest response against the established facts in the conversation history and flag any contradictions.\r\n\r\n    1. Character name inconsistencies or mix-ups.\r\n    2. Location contradictions: a character in place X suddenly appearing in place Y with no travel.\r\n    3. Timeline errors: events that happened \"yesterday\" drifting, or time not progressing logically.\r\n    4. Dead, absent, or departed characters appearing without explanation.\r\n    5. Items or abilities that contradict established inventory, skills, or what's been used/lost.\r\n    6. Personality inconsistencies with established behavior: a shy character suddenly delivering a confident monologue needs justification, not silence.\r\n    7. Weather, time-of-day, and environmental continuity: if it was night three messages ago with no time skip, it's still night.\r\n    \r\nWhen in doubt, default to flagging. A false positive is better than a missed contradiction.\r\nOutput format:\r\n{\r\n  \"issues\": [\r\n    {\r\n      \"severity\": \"error|warning|note\",\r\n      \"description\": \"Brief description of the contradiction\",\r\n      \"suggestion\": \"How to fix it.\"\r\n    }\r\n  ],\r\n  \"verdict\": \"clean|minor_issues|major_issues\"\r\n}\r\n\r\nIf no issues found, return: { \"issues\": [], \"verdict\": \"clean\" }\r\n</Cohesion_Enforcement>\n",
                                }
                            },
                        }
                }
            };
        }
    }
}
