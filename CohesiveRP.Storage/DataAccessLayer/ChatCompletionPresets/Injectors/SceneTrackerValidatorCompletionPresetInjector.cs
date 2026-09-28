using CohesiveRP.Common.BusinessObjects;
using CohesiveRP.Common.Utils;
using CohesiveRP.Storage.DataAccessLayer.AIQueries;
using CohesiveRP.Storage.DataAccessLayer.ChatCompletionPresets.BusinessObjects;
using CohesiveRP.Storage.DataAccessLayer.ChatCompletionPresets.BusinessObjects.Format;

namespace CohesiveRP.Storage.DataAccessLayer.ChatCompletionPresets.Injectors
{
    internal class SceneTrackerValidatorCompletionPresetInjector : ICompletionPresetInjector
    {
        internal static ChatCompletionPresetsDbModel InjectPreset()
        {
            return new ChatCompletionPresetsDbModel
                {
                    Name = "Default-Scene-Tracker-Validator-Preset",
                    ChatCompletionPresetId = StorageConstants.DEFAULT_SCENE_TRACKER_VALIDATOR_COMPLETION_PRESET,
                    CreatedAtUtc = DateTime.UtcNow,
                    Format = new GlobalPromptContextFormat()
                    {
                        MaxTokensToGenerate = 4096,
                        JsonSchemaName = "scene_tracker_validator",
                    JsonSchemaDocument = StrictJsonSchemaGenerator.Generate<SceneTrackerValidationResult>(),
                        OrderedElementsWithinTheGlobalPromptContext = new List<PromptContextFormatElement>
                        {
                            new PromptContextFormatElement
                            {
                                Tag = PromptContextFormatTag.Directive,
                                Name = "Directive",
                                Enabled = true,
                                Options = new PromptContextFormatElementOptions
                                {
                                    Format = "<task>\r\nStop the roleplay. You are a novels and roleplay analyst. Your role is to analyze the provided scene tracker that match the current scene within the provided story and validate that each fields contains logical, coherent and immersive values from the scene tracker. Base your reasoning on the *latest messages* that define what is going on in the *current scene*, the character sheets, listed within the '<relevant_characters>' xml tags, that define characters and how they should react. The scene tracker for the *PREVIOUS* scene is provided within the '<last_scene_tracker>' xml tag, this will help you keep clothes, state of dress, desires and mood consistent. As this is the *PREVIOUS* scene tracker, you should consider the new story elements, defined in the xml tag '<messages_after_last_scene_tracker>' since they can affect and alter previously generated values. Only raise suggestions when the value generated in the current scene tracker is *WRONG*, avoid raising suggestions when the value is incomplete or could be better.\r\n\r\nThe scene tracker for the *CURRENT* scene has been generated between the '<generated_scene_tracker>' xml tag, your task is to review every values within it, infer logical value according to the previous scene tracker and what is going on in the current scene and raise suggestions for every fields that are WRONG on what should be defined in this field instead.\r\n\r\nTo help you in your task, here's a description of the fields embedded within a scene tracker:\r\n<fieldsDescription>\r\n  \"mainThemes\": Categorize the scene in one to three strong thematic or topic keywords relevant to the scene to help trigger contextual information. (e.g., 'Romance' or 'Action,Combat')\r\n  \"nestedThemes\": Categorize the scene in one to five keywords that gives more details on the main themes/topics. (e.g., when mainCategories includes 'Romance', you may add 'Sex,Penetration,Semen' for example, if the scene include those themes).\r\n  \"currentDateTime\": Represents the current date and time in the current scene within the story. Adjust time in small increments, ideally only a few seconds per update, to reflect realistic scene progression (infer the time required to do the actions represented in the messages_after_last_scene_tracker actions and add it. Avoid large jumps unless a significant time skip (e.g., sleep, travel) is explicitly stated. Format this field as \"Day Month Year HH:MM:SS\", for example '3 March 1974 13:25:30'.\r\n  \"location\": The current location of the scene within the world. Avoid unintended reuse of specific locations from previous examples or responses. Provide specific, relevant, and detailed locations based on the context, using the format: \"preciseLocation, generalLocation, buildingOrApproximativeLocation, regionalLocation, countryOrInWorldLocation\". For example, \"Food court, second floor near east wing entrance, Madison Square Mall, Los Angeles, CA\" or \"Great hall, first floor near great staircase, Hogwarts, Scotland, Great Britain\" would be good descriptions.\r\n  \"allCharactersActiveInScene\": Array of all the characters that are active in the scene (talking, listening, thinking, physically close to {{user}}, interacting with another character, etc). List only their names (first name, last name), avoid using their titles. Avoid including the player ({{user}}). Make sure to include any characters that followed {{user}} or are in proximity to {{user}} or that could interact with {{user}}. Remove the characters that left or aren't in direct proximity to {{user}}.\r\n  \"charactersAnalysis\": characters, other than {{user}}, in the scene.\r\n  \"playerAnalysis\": {{user}} (the player) analysis.\r\n  \"name\": The name of the character. Only include first name and last name, when available. Avoid titles.\r\n  \"mood\": In what mood is the character. Analyze the character personality and make reasonable assumption on how they would feel in the current scene. Limit this description to keywords and to at most 30 words.\r\n  \"facialExpression\": Keyword for SDXL image generation. Limit your description to ONE of these choices: 'Neutral,Admiration,Amusement,Anger,Annoyance,Arousal,Arrogant,Bored,Confusion,Crying,Curiosity,Disappointment,Disapproval,Disgust,Embarrassment,Excitement,Fear,Gratitude,Grief,Jealousy,Joy,Laughing,Nervousness,Pride,Realization,Relief,Remorse,Sadness,Serious,Shy,Surprised,Sleepy,Worried'. If you're unsure, select 'Neutral'.\r\n  \"outfit\": Describe the complete outfit of this character, using specific details for color, fabric, and style (e.g., “fitted black leather jacket with silver studs on the collar”). Give a very small description (maximum 30 tokens).\r\n  \"underwear\": Describe the character's underwear (underneath clothes or visible). If underwear is intentionally missing, specify this clearly in the description (e.g., \"No bra\", \"No panties\" for female or \"no underwear\" for male).\r\n  \"stateOfDress\": Describe how put-together or disheveled the character's clothes appears, including any removed clothing. If the character is undressed, indicate where discarded items are placed.\r\n  \"exposedBodyParts\": Describe which parts of their body is exposed to the eyes of others. Choose values from this list: 'Face,Neck,Feet,Legs,Thighs,Genitals,Stomach,Chest'.\r\n  \"clothingStateOfDress\": How dressed is the character? You must choose between ONE of these choices: 'Unknown,Naked,Underwear,Clothed'. If unsure, select 'Unknown'. You may infer the character state of dress from the context and the actions taken by the characters.\r\n  \"hairStyle\": Describe the character's hairstyle (e.g., \"straight long hair\"). Give a very small description (one to five words).\r\n  \"posture\": Describe the character's posture (e.g., \"kneeling beside the cat\", \"running away\", \"leaning forward\"). Keep your description very lean (maximum 20 tokens).\r\n  \"semenOnBodyLocation\": When the current scene is erotica, after a male climax, this field determine where he came (where is semen is on his partner's body). This information is absolutely capital for the story. Select a choice between 'None,Unknown,OnFace,OnBreasts,OnStomach,OnThighsOrPelvis,InVagina,OnFeet'. When unsure, select 'Unknown'.\r\n  \"bodyPosition\": In what position is this character body currently? Choose an option from the following choices: 'Standing,Sitting,Walking,Running,Kneeling,Fighting,OnBack,OnStomach,OnAllFours,Unknown'. When unsure, select 'Unknown'. When the body position isn't available in the list, select the closest one.\r\n  \"relevantKinksInScene\": The five most relevant kinks that *belongs to this character* in the current scene in the following format: \"Kink name 1, kink name 2\", etc. e.g.: \"Domination, Marking, Bondage.\".\r\n  \"relevantSecretKinksInScene\": The five most relevant secret kinks that *belongs to this character* in the current scene in the following format: \"Kink name 1, kink name 2\", etc. e.g.: \"Domination, Marking, Bondage.\".\r\n  \"relevantPersonalityTraits\": The three most relevant personality traits of *this character* in the current scene.\r\n  \"lookingAtCharacter\": When the player is looking at a character, name this character here (full character name). Otherwise, leave it null or empty.\r\n  \"bodyPartBeingLookedAt\": When the player is looking at a character, select the body part that is being looked at here. Choose between the following choices: 'Face,Chest,Feet,Ass,Default'. Select 'Default' if you are unsure or if no other choices are applicable.\r\n</fieldsDescription>\r\n\r\n<key_instructions>\r\n- Use only details provided or logically inferred from context. Do *NOT* introduce speculative, hallucinated or unnecessary information.\r\n</key_instructions>\r\n\r\n<examplesGoodReplies>\r\n```\r\n{\r\n  \"recommendations\": []\r\n}\r\n```\r\n```\r\n{\r\n  \"recommendations\": [\r\n    \"The field 'mood' for the character 'Elena' should be 'angry' instead of 'happy'.\",\r\n\t\"The field 'outfit' for the character 'Mark' should be 'A dark coat with a blue tie'. The current value of the scene tracker is incorrect according to the previous scene tracker and should remain consistent.\"\r\n  ]\r\n}\r\n```\r\n</examplesGoodReplies>\r\n\r\n<important_reminders>\r\n- Before generating your suggestions, always consider the recent messages to ensure all changes are accurately represented.\r\n- You can only consider characters you know exist and are or may be in the scene.\r\n- Your primary objective is to ensure consistency.\r\n- Note that the player ({{user}}) may use first-person pronouns.\r\n- Your response must ONLY contain the Json representing the suggestions, which can be empty when the current scene tracker is deemed consistent.\r\n- Prefer shorter phrases and description, prefer keywords to long description.\r\n- Make sure to AVOID INCLUDING any characters that ARE NOT in the scene anymore.\r\n- The previous scene tracker may describe out of date information, make sure to consider the messages within the '<messages_after_last_scene_tracker>' tag for any changes.\r\n- *ONLY* generate new suggestions when we need to CHANGE something in the value.\r\n</important_reminders>\r\n</task>\n",
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
                                Tag = PromptContextFormatTag.SceneTrackerInstructions,
                                Name = "SceneTrackerInstructions",
                                Enabled = true,
                                Options = new PromptContextFormatElementOptions
                                {
                                    Format = "\r\n<last_scene_tracker>\r\n{{last_scene_tracker}}\r\n</last_scene_tracker>\r\n<messages_after_last_scene_tracker>\r\n{{messages_after_last_scene_tracker}}</messages_after_last_scene_tracker>\r\n",
                                }
                            },
                            new PromptContextFormatElement
                            {
                                Tag = PromptContextFormatTag.SceneTracker,
                                Name = "GeneratedSceneTracker",
                                Enabled = true,
                                Options = new PromptContextFormatElementOptions
                                {
                                    Format = "<generated_scene_tracker>\r\n{{item_description}}\r\n</generated_scene_tracker>",
                                }
                            },
                            new PromptContextFormatElement
                            {
                                Tag = PromptContextFormatTag.BehavioralInstructions,
                                Name = "BehavioralInstructions",
                                Enabled = true,
                                Options = new PromptContextFormatElementOptions
                                {
                                    Format = "\r\nHow do you respond?\r\nThink about it first. Recall the available lore, characters, scene and messages. Consider the current point in the narrative and how the characters got there as well as their goals, moods and distinct personalities.\r\n\r\nNow, continue directly with the fields to update in json.\r\n",
                                }
                            }
                        }
                    }
                };
        }
    }
}
