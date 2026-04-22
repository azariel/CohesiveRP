using CohesiveRP.Common.Diagnostics;
using CohesiveRP.Common.Utils.Parsers;
using CohesiveRP.Core.LLMProviderManager;
using CohesiveRP.Core.LLMProviderProcessors.Pathfinder.SkillChecksInitiator.BusinessObjects;
using CohesiveRP.Core.PromptContext.Abstractions;
using CohesiveRP.Core.PromptContext.Builders;
using CohesiveRP.Core.Services;
using CohesiveRP.Core.Services.LLMApiProvider;
using CohesiveRP.Core.Services.Summary;
using CohesiveRP.Storage.DataAccessLayer.AIQueries;
using CohesiveRP.Storage.DataAccessLayer.BackgroundQueries.BusinessObjects;
using CohesiveRP.Storage.DataAccessLayer.Chats;
using CohesiveRP.Storage.DataAccessLayer.Pathfinder.CharacterSheetInstances.BusinessObjects;
using CohesiveRP.Storage.DataAccessLayer.Pathfinder.ChatCharactersRolls.BusinessObjects;
using CohesiveRP.Storage.QueryModels.Chat;

namespace CohesiveRP.Core.LLMProviderProcessors.Pathfinder.SkillChecksInitiator
{
    public class SkillChecksInitiatorLLMQueryProcessor : LLMQueryProcessor
    {
        public SkillChecksInitiatorLLMQueryProcessor(
            ChatCompletionPresetType completionPresetType,
            BackgroundQuerySystemTags tag,
            BackgroundQueryDbModel backgroundQueryDbModel,
            IPromptContextBuilderFactory contextBuilderFactory,
            IPromptContextElementBuilderFactory promptContextElementBuilderFactory,
            IStorageService storageService,
            IHttpLLMApiProviderService httpLLMApiProviderService,
            ISummaryService summaryService) : base(
                completionPresetType,
                tag,
                backgroundQueryDbModel,
                contextBuilderFactory,
                promptContextElementBuilderFactory,
                storageService,
                httpLLMApiProviderService,
                summaryService)
        { }

        /* Examples:
        * [
            {
            "characterName": "Linota",
            "actionCategory": "Performance",
            "reasoning": "Linota is attempting to project calm and control despite visible signs of agitation like trembling fingers and a betraying tail swish, masking her intense emotions under a composed facade."
            },
            {
            "characterName": "Linota",
            "actionCategory": "Charisma",
            "reasoning": "Linota is engaging in a persuasive and challenging dialogue, setting terms and gauging Edward's understanding, using her words to negotiate the intimacy of touching her horns."
            },
            {
            "characterName": "Linota",
            "actionCategory": "Sex",
            "reasoning": "The scene is charged with sexual tension; the discussion about touching sensitive horns, her hunger, and the implied physical intimacy afterward fall under sensuality and sexual context."
            },
            {
            "characterName": "Edward",
            "actionCategory": "Charisma",
            "reasoning": "Edward is responding to Linota's challenge with confident persuasion, asserting his capability to handle her intensity in a flirtatious, persuasive manner."
            }
        ]
        * */
        private async Task<bool> HandlePathfinderSkillRollsAsync(string LLMrawResponse)
        {
            try
            {
                // TODO: Actually call the backend to DO the skill checks, compute them and find a way to ADD them to the MAIN backgroundQuery
                // javais pensé à ne garder que les skill checks de chaque chars dans une row par chat, mais pour la partie injection dans le contexte, il nous faudrait le reasoning
                // peut-être garder les 3 derniers reasonings pour chaque skill + le résultat du Dice pendant genre 20 messages. après 20 messages, on garde les 3 reasonings (puisqu'ils rollover), on clear le Dice et that's it. Quand on va rerouler un Dice pour ce skill pour ce char la prochaine fois, les reasonings vont rollover, on va avoir 2 vieux et un récent, on roll et paf, on garde pendant 20 messagees

                // Start by validating the llm response. It should be a valid json
                LLMPathfinderCharactersSkillChecksScene CharactersSkillChecks = null;

                try
                {
                    CharactersSkillChecks = LLMResponseParser.ParseFromApiMessageContent<LLMPathfinderCharactersSkillChecksScene>(LLMrawResponse);
                } catch (Exception ex)
                {
                    LoggingManager.LogToFile("8dae9f1a-2a6b-43b8-a2ad-c28776909dd6", $"The response from the LLM for the Pathfinder CharactersSkillChecksInitiator failed. The Json structure is incorrect. Ignoring.");
                    return false;
                }

                ChatCharactersRollsDbModel chatCharactersRollsDbModel = await storageService.GetChatCharactersRollsByIdAsync(backgroundQueryDbModel.ChatId);

                // Update the characters in scene in the SceneTracker
                chatCharactersRollsDbModel = await CreateOrUpdateCharactersInSceneAsync(chatCharactersRollsDbModel, CharactersSkillChecks?.AllCharactersByName, backgroundQueryDbModel.ChatId);

                if(chatCharactersRollsDbModel == null)
                { 
                    LoggingManager.LogToFile("27331e02-10a1-4101-83c0-307fbaf1d99e", $"The CharactersRolls tied to this chat was empty.");
                    return false;
                }

                // Get the list of characters known for this chat from characterSheetInstances
                var chatDbModel = await storageService.GetChatAsync(backgroundQueryDbModel.ChatId);

                if (chatDbModel == null)
                {
                    LoggingManager.LogToFile("41d7828a-2bd4-4c7b-b9f2-e7c2e7e9f14f", $"The chat tied to the Id [{backgroundQueryDbModel.ChatId}] couldn't be found.");
                    return false;
                }

                CharacterSheetInstancesDbModel characterSheetInstancesDbModel = await storageService.GetCharacterSheetsInstanceByChatIdAsync(backgroundQueryDbModel.ChatId);

                if (characterSheetInstancesDbModel == null)
                {
                    // Create a basic one tied to this chat
                    var newCharacterSheetsInstance = new CharacterSheetInstancesDbModel
                    {
                        ChatId = chatDbModel.ChatId,
                        CharacterSheetInstances = [],
                    };

                    characterSheetInstancesDbModel = await storageService.AddCharacterSheetsInstanceAsync(newCharacterSheetsInstance);
                }

                if (!await CreateMissingCharacterSheetInstancesMatchingACharacterAsync(chatDbModel, characterSheetInstancesDbModel))
                {
                    return false;
                }

                // TODO: SEND A NOTIFICATION TO THE PLAYER TO KNOW IF WE ADD IT OR NOT
                // Handle characterSheetsInstances
                //foreach (string characterName in CharactersSkillChecks.AllCharactersByName)
                //{
                //    if (!await CreateMissingCharacterSheetInstancesAsync(chatDbModel, characterSheetInstancesDbModel, characterName))
                //    {
                //        return false;
                //    }
                //}

                if (CharactersSkillChecks?.Actions == null || CharactersSkillChecks.Actions.Length <= 0)
                {
                    return true;
                }

                // Order the information we got from the LLM and then process them against storage
                foreach (IGrouping<string, LLMPathfinderCharactersSkillChecksActions> skillChecksByCharacter in CharactersSkillChecks.Actions
                    .GroupBy(g => g.CharacterName.ToLowerInvariant().Trim())
                    .Where(w => !string.IsNullOrWhiteSpace(w.Key))
                    .ToArray())
                {
                    List<SkillCheckQuery> queries = new();
                    foreach (LLMPathfinderCharactersSkillChecksActions element in skillChecksByCharacter)
                    {
                        if (!Enum.TryParse(element.ActionCategory, out PathfinderSkills actionCategory))
                        {
                            // Just skip
                            continue;
                        }

                        var existingQuery = queries.FirstOrDefault(a => a.ActionCategory == actionCategory);
                        if (existingQuery != null)
                        {
                            // Simply add the reasoning to the list and that's it
                            existingQuery.Reasonings.Add(element.Reasoning);
                            continue;
                        }

                        queries.Add(new SkillCheckQuery
                        {
                            ActionCategory = actionCategory,
                            Reasonings = [element.Reasoning],
                        });
                    }

                    // Finally, process the structured information against the backend. We want keep track of the reasonings and roll the dice when required. The final info will get persisted in storage
                    await ProcessSkillCheckQueriesAsync(chatDbModel, chatCharactersRollsDbModel, characterSheetInstancesDbModel, skillChecksByCharacter.Key, queries, CharactersSkillChecks.AllCharactersByName);
                }

                return true;

            } catch (Exception)
            {
                return false;
            }
        }

        private async Task<ChatCharactersRollsDbModel> CreateOrUpdateCharactersInSceneAsync(ChatCharactersRollsDbModel chatCharactersRollsDbModel, string[] allCharactersByName, string chatId)
        {
            if(allCharactersByName == null || allCharactersByName.Length <= 0)
                return chatCharactersRollsDbModel;

            if (chatCharactersRollsDbModel == null)
            {
                chatCharactersRollsDbModel = new ChatCharactersRollsDbModel
                {
                    ChatId = chatId,
                    CharacterNamesInScene = allCharactersByName.ToList(),
                    ChatCharactersRolls = [],
                };

                chatCharactersRollsDbModel = await storageService.AddChatCharactersRollsAsync(chatCharactersRollsDbModel);
                if (chatCharactersRollsDbModel == null)
                {
                    LoggingManager.LogToFile("460d25ff-8c75-48c2-96da-f84447213b22", $"Couldn't Add a new ChatCharactersRolls in storage.");
                    return chatCharactersRollsDbModel;
                }

                return chatCharactersRollsDbModel;
            }

            chatCharactersRollsDbModel.CharacterNamesInScene = allCharactersByName.ToList();
            await storageService.UpdateChatCharactersRollsAsync(chatCharactersRollsDbModel);
            return chatCharactersRollsDbModel;
        }

        private CharacterSheetInstance FindCharacterSheetInstanceFromCharacterName(List<CharacterSheetInstance> characterSheetInstances, string characterName)
        {
            string characterNameLower = characterName.ToLowerInvariant().Trim();
            var selectedCharacterSheetInstance = characterSheetInstances?.FirstOrDefault(f =>
            f.CharacterSheet.FirstName?.ToLowerInvariant().Trim() == characterNameLower ||
            f.CharacterSheet.LastName?.ToLowerInvariant().Trim() == characterNameLower ||
            $"{f.CharacterSheet.FirstName?.ToLowerInvariant().Trim()} {f.CharacterSheet.LastName?.ToLowerInvariant().Trim()}" == characterNameLower);

            return selectedCharacterSheetInstance;
        }

        private async Task<bool> CreateMissingCharacterSheetInstancesAsync(ChatDbModel chatDbModel, CharacterSheetInstancesDbModel characterSheetInstancesDbModel, string characterName)
        {
            var selectedCharacterSheetInstance = FindCharacterSheetInstanceFromCharacterName(characterSheetInstancesDbModel.CharacterSheetInstances, characterName);
            if (selectedCharacterSheetInstance == null)
            {
                var newCharacterInstance = new CharacterSheetInstance
                {
                    CharacterSheetInstanceId = Guid.NewGuid().ToString(),
                    CharacterId = null,// Not matching a 'character', we are dealing with an NPC (that doesn't have a characterCard, it's a in-roleplay NPC)
                    CharacterSheet = new CharacterSheet()
                    {
                        FirstName = characterName,// We'll assume this since we must
                    },// Just the default values, especially around Attributes and Skills, the rest should default to null or zero until the background query updates it
                };

                characterSheetInstancesDbModel.CharacterSheetInstances.Add(newCharacterInstance);

                var result = await storageService.UpdateCharacterSheetsInstanceAsync(characterSheetInstancesDbModel);
                if (!result)
                {
                    LoggingManager.LogToFile("18d5f61d-36b0-4034-8526-317cfb11b354", $"Couldn't Update the characterSheetInstance tied to chat [{chatDbModel.ChatId}] in storage to add new character [{characterName}].");
                    return false;
                }

                // TODO: add a backgroundQuery to update this newly create characterSheetInstance. We want the AI to scan the story and generate values for each fields automatically
            }

            return true;
        }

        private async Task<bool> CreateMissingCharacterSheetInstancesMatchingACharacterAsync(ChatDbModel chatDbModel, CharacterSheetInstancesDbModel characterSheetInstancesDbModel)
        {
            if (chatDbModel == null || characterSheetInstancesDbModel?.CharacterSheetInstances == null)
            {
                return false;
            }

            // TODO: move this into when we ADD or REMOVE a character(s) to a specific chat instead! It takes more performance by putting it here since we're evaluating it every time
            // Get the characters currently linked to the chat
            var charactersSheetsInChat = await storageService.GetCharacterSheetsAsync();// TODO: func give the array of characters
            charactersSheetsInChat = charactersSheetsInChat.Where(w => chatDbModel.CharacterIds.Any(a => a == w.CharacterId) || chatDbModel.PersonaId == w.PersonaId).ToArray();

            List<CharacterSheetInstance> instancesToAdd = new();
            foreach (string characterIdLinkedToTheChat in chatDbModel.CharacterIds)
            {
                // Make sure that we have a CharacterSheetInstance for this character
                if (characterSheetInstancesDbModel.CharacterSheetInstances.Any(a => a.CharacterId == characterIdLinkedToTheChat))
                {
                    continue;
                }

                // Get the blueprint if any
                var characterSheetObj = charactersSheetsInChat.FirstOrDefault(w => w.CharacterId == characterIdLinkedToTheChat);

                // Create a new characterSheetInstance
                if (characterSheetObj != null)
                {
                    // We have a characterSheet (blueprint), use that to spawn an instance out of it for our chat
                    instancesToAdd.Add(new CharacterSheetInstance
                    {
                        CharacterId = characterIdLinkedToTheChat,
                        CharacterSheetInstanceId = Guid.NewGuid().ToString(),
                        CharacterSheet = characterSheetObj.CharacterSheet,
                    });
                } else
                {
                    // We DON'T have a blueprint, create an average characterSheet for this character
                    // TODO: queue a backgroundQuery to update this characterSheetInstance by scanning the persona information + chat ?
                    var character = await storageService.GetCharacterByIdAsync(characterIdLinkedToTheChat);

                    if (!string.IsNullOrWhiteSpace(character?.Name))
                    {
                        instancesToAdd.Add(new CharacterSheetInstance
                        {
                            CharacterId = characterIdLinkedToTheChat,
                            CharacterSheetInstanceId = Guid.NewGuid().ToString(),
                            CharacterSheet = new CharacterSheet()
                            {
                                FirstName = character.Name,
                            },
                        });
                    }
                }
            }

            // Also handle the persona one if required
            var personaCharacterSheet = charactersSheetsInChat.FirstOrDefault(f => f.PersonaId == chatDbModel.PersonaId);
            var persona = await storageService.GetPersonaByIdAsync(chatDbModel.PersonaId);

            if (persona != null)
            {
                var existingCharacterSheetInstance = characterSheetInstancesDbModel.CharacterSheetInstances.FirstOrDefault(f => f.PersonaId == persona.PersonaId);

                if (existingCharacterSheetInstance == null)
                {
                    // We don't have a characterSheetInstance in storage, we need one
                    if (personaCharacterSheet?.CharacterSheet != null)
                    {
                        // We have a characterSheet (blueprint), use that to spawn an instance out of it for our chat
                        instancesToAdd.Add(new CharacterSheetInstance
                        {
                            PersonaId = persona.PersonaId,
                            CharacterSheetInstanceId = Guid.NewGuid().ToString(),
                            CharacterSheet = personaCharacterSheet.CharacterSheet,
                        });
                    } else
                    {
                        // We DON'T have a blueprint, create an average characterSheet for this persona
                        // TODO: queue a backgroundQuery to update this characterSheetInstance by scanning the persona information + chat ?
                        instancesToAdd.Add(new CharacterSheetInstance
                        {
                            PersonaId = persona.PersonaId,
                            CharacterSheetInstanceId = Guid.NewGuid().ToString(),
                            CharacterSheet = new CharacterSheet()
                            {
                                FirstName = persona.Name,
                            },
                        });
                    }
                }
            }

            if (instancesToAdd.Count > 0)
            {
                characterSheetInstancesDbModel.CharacterSheetInstances.AddRange(instancesToAdd);
                return await storageService.UpdateCharacterSheetsInstanceAsync(characterSheetInstancesDbModel);
            }

            return true;
        }

        private async Task<bool> ProcessSkillCheckQueriesAsync(ChatDbModel chatDbModel, ChatCharactersRollsDbModel chatCharactersRollsDbModel, CharacterSheetInstancesDbModel characterSheetInstancesDbModel, string characterName, List<SkillCheckQuery> queries, string[] charactersInScene)
        {
            // Find the character from the available character sheet instances
            var selectedCharacterSheetInstance = FindCharacterSheetInstanceFromCharacterName(characterSheetInstancesDbModel?.CharacterSheetInstances, characterName);
            if (selectedCharacterSheetInstance == null)
            {
                return false;
            }

            if (chatCharactersRollsDbModel == null)
            {
                var addNewRollDbModel = new ChatCharactersRollsDbModel
                {
                    ChatId = backgroundQueryDbModel.ChatId,
                    ChatCharactersRolls = [new ChatCharacterRolls { CharacterSheetInstanceId = selectedCharacterSheetInstance.CharacterSheetInstanceId, Rolls = [] }],
                };

                var queryResult = await storageService.AddChatCharactersRollsAsync(addNewRollDbModel);
                if (queryResult == null)
                {
                    LoggingManager.LogToFile("545bc836-005e-4085-bb59-600a06e4f7f4", $"Couldn't Add a new ChatCharactersRolls row for CharacterSheetInstanceId [{selectedCharacterSheetInstance.CharacterSheetInstanceId}] in storage for new character [{characterName}].");
                    return false;
                }

                chatCharactersRollsDbModel = queryResult;
            }

            ChatCharacterRolls persistentCharacterRolls = chatCharactersRollsDbModel.ChatCharactersRolls?.FirstOrDefault(a => a.CharacterSheetInstanceId == selectedCharacterSheetInstance.CharacterSheetInstanceId);

            // If we dont' have an entry in the chat characters rolls row, we'll add an empty for (for this specific character sheet instance)
            if (persistentCharacterRolls == null)
            {
                chatCharactersRollsDbModel.ChatCharactersRolls = new List<ChatCharacterRolls>(){
                    new ChatCharacterRolls
                    {
                        CharacterSheetInstanceId = selectedCharacterSheetInstance.CharacterSheetInstanceId,
                        Rolls = [],
                    }
                };

                if (!await storageService.UpdateChatCharactersRollsAsync(chatCharactersRollsDbModel))
                {
                    LoggingManager.LogToFile("ae823f24-ddd3-48bb-8bd6-ba3f82efed47", $"Couldn't Update chatCharactersRolls tied to chat [{chatCharactersRollsDbModel.ChatId}] in storage for new character [{characterName}].");
                    return false;
                }

                persistentCharacterRolls = chatCharactersRollsDbModel.ChatCharactersRolls.FirstOrDefault(a => a.CharacterSheetInstanceId == selectedCharacterSheetInstance.CharacterSheetInstanceId);
                if (persistentCharacterRolls == null)// a bit paranoia here...eh
                {
                    LoggingManager.LogToFile("1e1d3b42-87a1-4a0f-a9c5-603fd64c9046", $"Can't find the character sheet instance [{selectedCharacterSheetInstance.CharacterSheetInstanceId}] rolls in chat [{chatDbModel.ChatId}].");
                    return false;
                }
            }

            var characterSheetInstancesInScene = charactersInScene?.Select(s => FindCharacterSheetInstanceFromCharacterName(characterSheetInstancesDbModel?.CharacterSheetInstances, s))?.ToArray();
            if (characterSheetInstancesInScene != null)
            {
                characterSheetInstancesInScene = characterSheetInstancesInScene.Where(w => w?.CharacterSheetInstanceId != null).ToArray();
            }

            foreach (SkillCheckQuery query in queries)
            {
                ChatCharacterRoll roll = persistentCharacterRolls.Rolls.FirstOrDefault(f => f.ActionCategory == query.ActionCategory);

                if (roll == null)
                {
                    // Create a new roll
                    roll = new ChatCharacterRoll
                    {
                        ActionCategory = query.ActionCategory,
                        Reasonings = query.Reasonings,
                        NbRemainingInjectionTurns = 1,
                        NbRemainingRollFreeze = 4,
                        CharactersInScene = FilterCharactersInScene(characterSheetInstancesInScene, selectedCharacterSheetInstance.CharacterSheetInstanceId),
                        Value = await GenerateNewRollForCharacterForSkillCheckAsync(selectedCharacterSheetInstance, query.ActionCategory),// Generate a new value using the character sheet (or average if no character sheet)
                    };

                    // Generate counter rolls for characters in scene if required
                    await GenerateCounterRollsForCharactersInSceneAsync(roll, characterSheetInstancesInScene);

                    persistentCharacterRolls.Rolls.Add(roll);
                    continue;
                }

                // Update existing roll
                if (roll.NbRemainingInjectionTurns > 0)
                {
                    // Roll older reasoning, inject new one(s)
                    roll.Reasonings = RemoveOldestXReasonings(roll.Reasonings, query.Reasonings.Count);
                    roll.Reasonings.AddRange(query.Reasonings);
                } else
                {
                    roll.Reasonings.Clear();
                    roll.Reasonings.AddRange(query.Reasonings);
                    roll.NbRemainingInjectionTurns = 1;
                }

                if (roll.NbRemainingRollFreeze <= 0)
                {
                    // That roll is now deemed too old and must be rolled again
                    roll.Value = await GenerateNewRollForCharacterForSkillCheckAsync(selectedCharacterSheetInstance, query.ActionCategory);
                }

                roll.CharactersInScene = FilterCharactersInScene(characterSheetInstancesInScene, selectedCharacterSheetInstance.CharacterSheetInstanceId);

                // Generate counter rolls for characters in scene if required
                await GenerateCounterRollsForCharactersInSceneAsync(roll, characterSheetInstancesInScene);
            }

            // Update the rolls tied to this character in this chat
            if (!await storageService.UpdateChatCharactersRollsAsync(chatCharactersRollsDbModel))
            {
                LoggingManager.LogToFile("53da30a9-3ce0-4bf4-9485-d363e67fb875", $"Couldn't Update the ChatCharactersRolls row for CharacterSheetInstanceId [{selectedCharacterSheetInstance.CharacterSheetInstanceId}] in storage for new character [{characterName}] after all rolls were rolled, added and updated locally.");
                return false;
            }

            return true;// We'll avoid re-generating it infinitely for now. it cost too much... TODO: brainstorm on this behavior
        }

        private async Task GenerateCounterRollsForCharactersInSceneAsync(ChatCharacterRoll roll, CharacterSheetInstance[] characterSheetInstancesInScene)
        {
            foreach (CharacterInScene otherCharacterInScene in roll.CharactersInScene)
            {
                var characterSheetInstance = characterSheetInstancesInScene.FirstOrDefault(f => f.CharacterSheetInstanceId == otherCharacterInScene.CharacterSheetInstanceId);
                if (characterSheetInstance == null)
                    continue;

                switch (roll.ActionCategory)
                {
                    case PathfinderSkills.Deception:
                    {
                        // If the roll is a deception roll, the other characters have a Discernment Attribute check to make
                        var otherCharacterRoll = new CharacterInSceneCounterRoll
                        {
                            Attribute = PathfinderAttributes.Discernment,
                            Value = await GenerateNewRollForCharacterForAttributeCheckAsync(characterSheetInstance, PathfinderAttributes.Discernment),
                        };
                        otherCharacterInScene.CharacterInSceneCounterRoll = otherCharacterRoll;
                        break;
                    }

                    case PathfinderSkills.Charisma:
                    case PathfinderSkills.Intimidation:
                    {
                        var otherCharacterRoll = new CharacterInSceneCounterRoll
                        {
                            Attribute = PathfinderAttributes.Willpower,
                            Value = await GenerateNewRollForCharacterForAttributeCheckAsync(characterSheetInstance, PathfinderAttributes.Willpower),
                        };
                        otherCharacterInScene.CharacterInSceneCounterRoll = otherCharacterRoll;
                        break;
                    }

                    case PathfinderSkills.Thievery:
                    case PathfinderSkills.Stealth:
                    {
                        var otherCharacterRoll = new CharacterInSceneCounterRoll
                        {
                            Attribute = PathfinderAttributes.Perception,
                            Value = await GenerateNewRollForCharacterForAttributeCheckAsync(characterSheetInstance, PathfinderAttributes.Perception),
                        };
                        otherCharacterInScene.CharacterInSceneCounterRoll = otherCharacterRoll;
                        break;
                    }
                }
            }
        }

        private CharacterInScene[] FilterCharactersInScene(CharacterSheetInstance[] input, string characterToIgnore)
        {
            return input?.Select(s => new CharacterInScene { CharacterSheetInstanceId = s.CharacterSheetInstanceId }).Where(w => w.CharacterSheetInstanceId != characterToIgnore).ToArray() ?? [];
        }

        private List<string> RemoveOldestXReasonings(List<string> reasonings, int nbReasoningsToRemove)
        {
            if (reasonings.Count < 5 - nbReasoningsToRemove)
            {
                return reasonings;// There's still space
            }

            var outValue = reasonings.Skip(nbReasoningsToRemove).ToList();
            outValue.AddRange(reasonings);
            return outValue;
        }

        private async Task<int> GetModifierFromSkillAsync(CharacterSheetInstance selectedCharacterSheetInstance, PathfinderSkills skill)
        {
            int value = selectedCharacterSheetInstance?.CharacterSheet?.PathfinderSkillsValues?.FirstOrDefault(f => f.SkillType == skill)?.Value ?? 10;

            // TODO: find a more elegant way of handling this? Is this correctly balanced?
            if (value >= 21) return 6;
            else if (value >= 20) return 5;
            else if (value >= 18) return 4;
            else if (value >= 16) return 3;
            else if (value >= 14) return 2;
            else if (value >= 12) return 1;
            else if (value >= 10) return 0;
            else if (value >= 9) return -1;
            else if (value >= 8) return -2;
            else if (value >= 7) return -3;
            else if (value >= 6) return -4;
            else if (value >= 5) return -5;
            else if (value >= 3) return -6;
            else if (value >= 2) return -7;
            else if (value >= 0) return -8;

            return 0;
        }

        private async Task<int> GetModifierFromAttributeAsync(CharacterSheetInstance selectedCharacterSheetInstance, PathfinderAttributes attribute)
        {
            int value = selectedCharacterSheetInstance?.CharacterSheet?.PathfinderAttributesValues?.FirstOrDefault(f => f.AttributeType == attribute)?.Value ?? 10;

            if (value >= 21) return 7;
            else if (value >= 20) return 5;
            else if (value >= 18) return 4;
            else if (value >= 16) return 3;
            else if (value >= 14) return 2;
            else if (value >= 12) return 1;
            else if (value >= 10) return 0;
            else if (value >= 9) return -1;
            else if (value >= 8) return -2;
            else if (value >= 7) return -3;
            else if (value >= 6) return -4;
            else if (value >= 5) return -5;
            else if (value >= 3) return -6;
            else if (value >= 2) return -7;
            else if (value >= 0) return -8;

            return 0;
        }

        private async Task<int> GenerateNewRollForCharacterForSkillCheckAsync(CharacterSheetInstance selectedCharacterSheetInstance, PathfinderSkills skill)
        {
            var modifier = await GetModifierFromSkillAsync(selectedCharacterSheetInstance, skill);
            await Task.Delay(15);// Random rolls fix to avoid exact same random value
            int randomRoll = new Random(DateTime.Now.Millisecond).Next(1, 21);// [1-20]

            if (randomRoll >= 20)
                return 20;

            if (randomRoll <= 1)
                return 1;

            return Math.Min(19, Math.Max(2, randomRoll + modifier));
        }

        private async Task<int> GenerateNewRollForCharacterForAttributeCheckAsync(CharacterSheetInstance selectedCharacterSheetInstance, PathfinderAttributes attribute)
        {
            var modifier = await GetModifierFromAttributeAsync(selectedCharacterSheetInstance, attribute);
            await Task.Delay(15);// Random rolls fix to avoid exact same random value
            int randomRoll = new Random(DateTime.Now.Millisecond).Next(1, 21);// [1-20]

            if (randomRoll >= 20)
                return 20;

            if (randomRoll <= 1)
                return 1;

            return Math.Min(19, Math.Max(2, randomRoll + modifier));
        }

        public override async Task<bool> ProcessCompletedQueryAsync()
        {
            if (!await base.ProcessCompletedQueryAsync())
            {
                backgroundQueryDbModel.Content = null;
                backgroundQueryDbModel.Status = BackgroundQueryStatus.Pending;// re-queue
                backgroundQueryDbModel.RetryCount++;
                return false;
            }

            // Deserialize the generic content into a valid SkillsChecksInitiator
            try
            {
                LLMApiResponseMessage LLMmessage = messages.LastOrDefault();
                if (!await HandlePathfinderSkillRollsAsync(LLMmessage.Content))
                {
                    // Ignoring for now
                    //backgroundQueryDbModel.Content = null;
                    //backgroundQueryDbModel.Status = BackgroundQueryStatus.Pending;
                    //return;
                }

                backgroundQueryDbModel.EndFocusedGenerationDateTimeUtc = DateTime.UtcNow;
                backgroundQueryDbModel.Status = BackgroundQueryStatus.Completed;
                return true;
            } catch (Exception e)
            {
                LoggingManager.LogToFile("025c0b1d-cfef-4a20-bab7-8bde505060be", $"Couldn't complete backgroundTask [{backgroundQueryDbModel.BackgroundQueryId}]. Task will be set to Pending status for re-generation.", e);
                backgroundQueryDbModel.Content = null;
                backgroundQueryDbModel.Status = BackgroundQueryStatus.Pending;
                backgroundQueryDbModel.RetryCount++;
                return false;
            }
        }
    }
}
