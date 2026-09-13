using CohesiveRP.Storage.DataAccessLayer.AIQueries;
using CohesiveRP.Storage.DataAccessLayer.ChatCompletionPresets.BusinessObjects;
using CohesiveRP.Storage.DataAccessLayer.ChatCompletionPresets.BusinessObjects.Format;

namespace CohesiveRP.Storage.DataAccessLayer.ChatCompletionPresets.Injectors
{
    internal class ReflectionCompletionPresetInjector : ICompletionPresetInjector
    {
        internal static ChatCompletionPresetsDbModel InjectPreset()
        {
            return new ChatCompletionPresetsDbModel
            {
                Name = "Default-Reflection-Prompt-Generator-Preset",
                ChatCompletionPresetId = StorageConstants.DEFAULT_REFLECTION_COMPLETION_PRESET,
                CreatedAtUtc = DateTime.UtcNow,
                Format = new GlobalPromptContextFormat()
                {
                    MaxTokensToGenerate = 4096,
                    OrderedElementsWithinTheGlobalPromptContext = new List<PromptContextFormatElement>
                        {
                            new PromptContextFormatElement
                            {
                                Tag = PromptContextFormatTag.Directive,
                                Name = "ReflectionInstructions",
                                Enabled = true,
                                Options = new PromptContextFormatElementOptions
                                {
                                    Format = "<task>\r\nYou are a story analyst. You must analyze the provided text to evaluate the following points:\r\n\r\n1. Quick summary of the current scene and situation. What is going on? Where are each of the characters in the scene? What just happened? What a question raised? Etc. Each information *must* be backed with information found in the story context. Try to select information that are *RELEVANT* for the next message/action.\r\n2. Relevant speech, actions, tensions, goals and innuendos in the current scene.\r\n3. The current dice rolls provided in the context (in the 'pathfinder_characters_rolls' tag), if any. The impact of these rolls on other characters, taking into consideration the 'otherCharactersRollAgainstTheseActions' tag when provided.\r\n4. How other characters in the scene may react from those rolls according to their motives, personality and mood.\r\n5. The list of every characters scene information. A character scene information include the character current mood, their relevant personality traits, their arousal (when the story context is nfsw or romantic), the kinks relevant to the scene (when the story context is nfsw), how this character is personally affected by the situation, what do they *know* about the current intrigue (if applicable), what they *don't* know about the current intrigue (such as secrets, when applicable), .\r\n6. The logical outcome (success/failure) of the attempted action applied to every characters considering the scene definition of each character. You should list the key points for each characters to achieve a logical outcome.\r\n7. The emotional state and goals of the active characters.\r\n8. How should the scene continue? How should it flows? What could the characters do or say to contribute meaningfully and logically to the scene? Give ideas, do not continue the roleplay, you are an analyst.\r\n9. Prose styles reminder, key writing guidelines.\r\n10. Quick custom reflection on the task to think about other considerations you may have.\r\n\r\nNote that you should always validate your analysis against the world state, lore, previous events and information so that you rely on logical and truthful facts and information in your analysis.\r\n\r\n<social_roll_clarification>\r\nWhen a player's social roll (Charisma, Deception, Intimidation, etc.) fails against an NPC:\r\n- The NPC actively rejects, dismisses, mocks, or redirects the attempt.\r\n- Show this through body language, tone, and choice of words.\r\n- Do NOT have the NPC simply comply anyway.\r\n- A failed roll means the NPC is not persuaded, deceived or intimidated.\r\n\r\nWhen a social roll succeeds:\r\n- Show the NPC's internal shift through subtle cues (a pause, softened tone, change in posture) before any verbal agreement.\r\n- Even on success, the NPC may have conditions or reservations based on personality, but they are more open to be persuaded, deceived or intimidated.\r\n</social_roll_clarification>\r\n\r\nYou do *NOT* write prose, you do *NOT* write drafts, you are an analyst. Your output MUST be a numbered list matching the task.\r\n</task>\r\n\r\n<story_prose>\r\nA) *markdown italics* are to signify unspoken thoughts from a character. Other characters can't hear those thoughts except when clearly indicated (by magic or something logical within the story context).\r\nB) \"quotation marks\" are to specify spoken words\r\nC) narration is in plain text.\r\n</story_prose>\r\n<world_realism>\r\nA) The world and story are agnostic of {{user}}. The world goes on, characters interact without {{user}}'s implication.\r\nB) {{user}}'s actions can succeed or fail based on logic, not convenience. Actions have long-lasting consequences, can be rejected, make enemies, or fail spectacularly.\r\nC) Kindness is rare, especially towards strangers. Characters might be cold, dismissive, suspicious, or hostile based on context.\r\nD) Incorporate realistic, cynical, and grounded historical/modern realities when applicable.\r\nE) Infer characters' common sense and subjectivity. They must always follow their own goals or interests and value their own life first and foremost in most situations.\r\nF) Reputation, accomplishments, achievements, and past actions/dialogues matter inexplicably; show effects through reactions, perceptions, gossip, and adaptive scenarios.\r\nG) Characters can't react directly to the story description, they may only react to what they perceives themselves within the world (eyes, touch, ears, smell, taste).\r\n</world_realism>\r\n<characters_realism>\r\nA) The world is full of authentic, multidimensional, dynamic characters. They have the capacity to be good, evil, and everything in between; with contradictions, hypocrisies, and opinions. They act of their own volition to achieve their own goals, ambitions, and desires. They do not fear going against {{user}} if their personality dictates it. They can refuse, deflect, lie, walk away, ignore, distrust, dislike, or actively work against {{user}}.\r\nB) Characters are NOT omniscient. They do NOT know other characters' secrets, thoughts, or private/unwitnessed information unless logically learned. Information travels via observations, communication, and deductions. Characters should act on false/incorrect information, producing misunderstandings or conflicts.\r\nC) Subtlety is difficult to detect; when a character hides something, it's hard for others to notice.\r\n</characters_realism>\n",
                                }
                            },
                            new PromptContextFormatElement
                        {
                            Name = "Pathfinder Module",
                            Tag = PromptContextFormatTag.Directive,
                            Enabled = true,
                            Options = new PromptContextFormatElementOptions
                            {
                                Format = "<pathfinder_module>\r\nTo enhance variability and keep the story roleplay fresh, please adhere to these rules, which are very similar to 'dungeons and dragons' Skills rules. There is a section 'definitions' defined to explain what does each skills and attributes.\r\n  <attributes>\r\n    <Fortitude>Ability to sustain physical damage, illnesses or poisons. The higher, the better a character is to deal with pain and the longer they will remain functional when wounded, ill or poisoned. This doesn't make them superhuman, but may give them a slight edge. Inversely, when lower than 10, the effect are negative.</Fortitude>\r\n    <Reflex>Ability to react quickly enough to either dodge attacks or to react in time for timed based actions.</Reflex>\r\n    <Willpower>Ability to resist influence, mental afflictions or compulsions. The higher, the easier it is for this character to defend their opinions or to assert themselves.</Willpower>\r\n    <Stamina>Ability to exert physical effort for a longer time without getting out of breath, sluggish or fainting from exertion.</Stamina>\r\n    <MagicalStamina>Ability to exert magical effort (casting magic or spells) for a longer time without getting out of breath, sluggish or fainting from exertion. This attribute only applies if the story world and context allow for magical abilities.</MagicalStamina>\r\n    <MagicalPower>Ability to cast strong magic. A value of 10 is average. The higher the value, the stronger the spells or magic. This attribute only applies when the story world and context allow for magical abilities.</MagicalPower>\r\n    <Intelligence>Ability to retain information and to link bits of information with each other.</Intelligence>\r\n    <Discernment>Ability to detect lies or what is true from what isn't.</Discernment>\r\n    <Perception>Ability to detect details and people that are trying to avoid attention.</Perception>\r\n  </attributes>\r\n</pathfinder_module>\n"
                            }
                        },
                        new PromptContextFormatElement { Tag = PromptContextFormatTag.World, Name = "World", Enabled = true, Options = new PromptContextFormatElementOptions { Format = "{{item_description}}" } },
                        new PromptContextFormatElement
                        {
                            Tag = PromptContextFormatTag.LoreByKeywords,
                            Name = "LoreByKeywords",
                            Enabled = true,
                            Options = new PromptContextFormatElementOptions
                            {
                                Format = "<{{item_header}}>\r\n{{item_description}}\r\n</{{item_header}}>"
                            }
                        },
                        new PromptContextFormatElement { Tag = PromptContextFormatTag.SummaryExtraTerm, Name = "SummaryExtraTerm", Enabled = true, Options = new PromptContextFormatElementOptions { Format = "- {{item_description}}" } },
                        new PromptContextFormatElement { Tag = PromptContextFormatTag.SummaryLongTerm, Name = "SummaryLongTerm", Enabled = true, Options = new PromptContextFormatElementOptions { Format = "- {{item_description}}" } },
                        new PromptContextFormatElement { Tag = PromptContextFormatTag.SummaryMediumTerm, Name = "SummaryMediumTerm", Enabled = true, Options = new PromptContextFormatElementOptions { Format = "- {{item_description}}" } },
                        new PromptContextFormatElement { Tag = PromptContextFormatTag.SummaryShortTerm, Name = "SummaryShortTerm", Enabled = true, Options = new PromptContextFormatElementOptions { Format = "- {{item_description}}" } },
                        new PromptContextFormatElement { Tag = PromptContextFormatTag.LoreByQuery, Name = "LoreByQuery", Enabled = true, Options = new PromptContextFormatElementOptions { Format = "{{item_header}}\r\n{{item_description}}" } },
                        new PromptContextFormatElement { Tag = PromptContextFormatTag.NarrativeArchitecture, Name = "NarrativeArchitecture", Enabled = true, Options = new PromptContextFormatElementOptions { Format = "<secret_plot_state>\r\n{{description}}</secret_plot_state>\r\n" } },
                        new PromptContextFormatElement { Tag = PromptContextFormatTag.RelevantCharacters, Name = "RelevantCharacters", Enabled = true, Options = new PromptContextFormatElementRelevantCharactersOptions { Format = "<{{item_header}}>\r\n{{item_description}}\r\n</{{item_header}}>", IncludeKnownCharacters = true } },
                        new PromptContextFormatElement { Tag = PromptContextFormatTag.DirectCharactersDescription, Name = "DirectCharactersDescription", Enabled = true, Options = new PromptContextFormatElementOptions { Format = "<character_instruction>\r\nInformation about {{character_name}}\r\n{{character_description}}\r\n</characters>\r\n\r\n" } },
                        new PromptContextFormatElement { Tag = PromptContextFormatTag.LastUnsummarizedMessages, Name = "LastUnsummarizedMessages", Enabled = true, Options = new PromptContextFormatElementOptions { Format = "{{item_description}}" } },
                        new PromptContextFormatElement { Tag = PromptContextFormatTag.SceneTracker, Name = "SceneTracker", Enabled = true, Options = new PromptContextFormatElementOptions { Format = "{{item_description}}" } },
                        new PromptContextFormatElement { Tag = PromptContextFormatTag.PathfinderSkillChecksResults, Name = "PathfinderSkillChecksResults", Enabled = true, Options = new PromptContextFormatElementOptions { Format = "{{item_description}}" } },
                        new PromptContextFormatElement { Tag = PromptContextFormatTag.CurrentObjective, Name = "CurrentObjective", Enabled = true, Options = new PromptContextFormatElementOptions { Format = "<current_objective>\r\nStory progression objective.\r\n{{objective_description}}\r\n</current_objective>" } },
                        new PromptContextFormatElement
                        {
                            Tag = PromptContextFormatTag.BehavioralInstructions,
                            Name = "BehavioralInstructions",
                            Enabled = true,
                            Options = new PromptContextFormatElementOptions
                            {
                                Format = "<behavioral_instruction>\r\nHow do you respond?\r\nThink about it first. Recall the available lore and rules and remember to apply them. Consider the current point in the narrative and how you got there.\r\nWrite your analysis, do *NOT* write a narrative response.\r\n Limit your reply to 2000 words at maximum.\r\n</behavioral_instruction>",
                            }
                        }
                    },
                }
            };
        }
    }
}
