using CohesiveRP.Storage.DataAccessLayer.AIQueries;
using CohesiveRP.Storage.DataAccessLayer.ChatCompletionPresets.BusinessObjects;
using CohesiveRP.Storage.DataAccessLayer.ChatCompletionPresets.BusinessObjects.Format;

namespace CohesiveRP.Storage.DataAccessLayer.ChatCompletionPresets.Injectors
{
    internal class DynamicCharacterCreatorCompletionPresetInjector : ICompletionPresetInjector
    {
        internal static ChatCompletionPresetsDbModel InjectPreset()
        {
            return new ChatCompletionPresetsDbModel
                {
                    Name = "Default-Dynamic-Character-Creator-Preset",
                    ChatCompletionPresetId = StorageConstants.DEFAULT_DYNAMIC_CHARACTER_CREATION_COMPLETION_PRESET,
                    CreatedAtUtc = DateTime.UtcNow,
                    Format = new GlobalPromptContextFormat()
                    {
                        MaxTokensToGenerate = 8196,
                        OrderedElementsWithinTheGlobalPromptContext = new List<PromptContextFormatElement>
                        {
                            new PromptContextFormatElement
                            {
                                Tag = PromptContextFormatTag.Directive,
                                Name = "Directive - Task",
                                Enabled = true,
                                Options = new PromptContextFormatElementOptions
                                {
                                    Format = "<task>\r\n  Role:Omniscient story scene analyst | Goal:Provide clear, consistent, logical and cohesive analysis on a character within a webnovel story | MaxLen:4096\r\n  \r\n  You are an helpful assistant, trained in analyzing and categorizing characters in books, novels, fanfictions and stories. You are tasked with providing a structured analysis of a specific character. You must start by understanding the character from the information provided. Please note that the User is impersonating {{user}} in the story.\r\n  Your reply must be structured into a strict JSON representing the following c# model:\r\n  ```\r\n  public class CharacterSheet\r\n  {\r\n      [JsonPropertyName(\"name\")]\r\n      public string Name { get; set; }\r\n      \r\n      [JsonPropertyName(\"description\")]\r\n      public string Description { get; set; }\r\n  }\r\n  ```\r\n</task>\n",
                                }
                            },
                            new PromptContextFormatElement
                            {
                                Tag = PromptContextFormatTag.Directive,
                                Name = "Directive - information_on_properties",
                                Enabled = true,
                                Options = new PromptContextFormatElementOptions
                                {
                                    Format = "<information_on_properties>\r\n  To help you in your task, here is a description of what is expected from each properties:\r\n  \r\n  <name>\r\n    The name of the character. Use their full name (first name + last name) if available. Do NOT include their title or profession. Limit it to their first name.\r\n  <name>\r\n  <description>\r\n    A description of this character using as much details as possible.\r\n  <description>\r\n</information_on_properties>\n",
                                }
                            },
                            new PromptContextFormatElement
                            {
                                Tag = PromptContextFormatTag.Directive,
                                Name = "Directive - example_of_good_replies",
                                Enabled = true,
                                Options = new PromptContextFormatElementOptions
                                {
                                    Format = "<example_of_good_replies>\r\n```\r\n{\r\n  \"name\": \"Marcus Flint\",\r\n  \"description\": \"Marcus Flint was born in 1977 to a pure-blood family with a long tradition of valuing strength, status, and competitive prowess. At fifteen, he is a Slytherin student of imposing height and build, already broader than most boys his age. His face is striking in its harshness: a heavy jaw, prominent brow, and a crooked, predatory grin that reveals slightly uneven teeth. His complexion is sallow from long hours spent outdoors on the Quidditch pitch, and his dark hair—thick, coarse, and perpetually wind-tossed—sits in a disorderly mop that refuses to be tamed. His eyes are a cold, steely gray, sharp with calculation and a constant undercurrent of aggression. His robes are functional rather than neat, often smelling faintly of broom polish and damp grass, with sleeves frayed from careless use. He moves with a confident, athletic swagger, shoulders squared and chin tilted forward, broadcasting dominance without a word. When he speaks, his voice is gravelly and forceful, punctuated by curt commands or clipped remarks, as though he’s accustomed to being obeyed rather than questioned. Marcus’s temperament is competitive, ambitious, and unapologetically aggressive. He thrives on physicality, confrontation, and the thrill of pushing boundaries. Though not academically inclined, he possesses a shrewd instinct for hierarchies and power plays, aligning himself with those who can advance his goals. His loyalty, though selective, is fierce; he protects his teammates with a near-feral intensity and treats Quidditch not simply as a sport but as a battlefield where reputation is won or lost. He values dominance, victory, and the prestige of Slytherin House. Beneath his swagger lies a relentless determination to excel, a driving need to prove himself through strength, endurance, and sheer force of will.\"\r\n}\r\n```\r\n</example_of_good_replies>\n",
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
                                }
                            },
                            new PromptContextFormatElement
                            {
                                Tag = PromptContextFormatTag.DirectCharactersDescription,
                                Name = "DirectCharactersDescription",
                                Enabled = true,
                                Options = new PromptContextFormatElementOptions
                                {
                                    Format = "<character_instruction>\r\nInformation about {{character_name}}\r\n{{character_description}}\r\n</characters>\r\n\r\n",
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
                                Tag = PromptContextFormatTag.CharacterCreation,
                                Name = "CharacterCreationInstructions",
                                Enabled = true,
                                Options = new PromptContextFormatElementOptions
                                {
                                    Format = "<behavioral_instruction>\r\n<important_reminders>\r\nYour objective is to ensure a consistent, coherent and logical description of the character '{{character_name}}', providing complete details even when specifics are not explicitly stated.\r\nYour response must ONLY contain the strict JSON representing the c# model.\r\nPrefer keywords to long description.\r\n</important_reminders>\r\n  <behavioral_instruction>\r\n",
                                }
                            }
                        }
                    }
                };
        }
    }
}
