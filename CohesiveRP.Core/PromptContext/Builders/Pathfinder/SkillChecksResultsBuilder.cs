using System.Text;
using CohesiveRP.Common.Diagnostics;
using CohesiveRP.Core.LLMProviderProcessors.Pathfinder.SkillChecksInitiator.BusinessObjects;
using CohesiveRP.Core.PromptContext.Abstractions;
using CohesiveRP.Core.PromptContext.Utils;
using CohesiveRP.Core.Services;
using CohesiveRP.Storage.DataAccessLayer.ChatCompletionPresets.BusinessObjects.Format;
using CohesiveRP.Storage.DataAccessLayer.Chats;
using CohesiveRP.Storage.DataAccessLayer.Pathfinder.CharacterSheetInstances.BusinessObjects;
using CohesiveRP.Storage.DataAccessLayer.Pathfinder.ChatCharactersRolls.BusinessObjects;

namespace CohesiveRP.Core.PromptContext.Builders.Pathfinder
{
    public class SkillChecksResultsBuilder : IPromptContextElementBuilder
    {
        private IStorageService storageService;
        private PromptContextFormatElement promptContextFormatElement;
        private ChatDbModel chatDbModel;
        private PersonaDbModel personaLinkedToChat;
        private CharacterDbModel[] charactersLinkedToChat;

        private HashSet<PathfinderSkills> skillsInPlayInCurrentScene = new();
        private int nbCounterRollsInjected = 0;

        private enum RollPerformance
        {
            Terrible = 0,// 1
            Bad = 1,// 2-9
            Average = 2,// 10-12
            Good = 3,// 12-19
            Excellent = 4// 20
        }

        public SkillChecksResultsBuilder(IStorageService storageService, PromptContextFormatElement promptContextFormatElement, ChatDbModel chatDbModel, PersonaDbModel personaLinkedToChat, CharacterDbModel[] charactersLinkedToChat)
        {
            this.storageService = storageService;
            this.promptContextFormatElement = promptContextFormatElement;
            this.chatDbModel = chatDbModel;
            this.personaLinkedToChat = personaLinkedToChat;
            this.charactersLinkedToChat = charactersLinkedToChat;
        }

        private string GeneratePromptInjectionForCharacterRolls(ChatCharacterRoll[] rollsToInject, CharacterSheetInstance currentCharacterSheetInstance, CharacterSheetInstancesDbModel characterSheetsInstances)
        {
            string innerStr = "";

            innerStr += $"<{currentCharacterSheetInstance.CharacterSheet.FirstName.Trim()}>{Environment.NewLine}";
            foreach (ChatCharacterRoll roll in rollsToInject)
            {
                // inject the roll into the prompt
                string value = GeneratePromptInjectionForCharacterRoll(roll, currentCharacterSheetInstance, characterSheetsInstances);
                innerStr += $"{value}{Environment.NewLine}";
            }

            innerStr = innerStr.Trim().TrimEnd(Environment.NewLine.ToCharArray());
            innerStr += $"{Environment.NewLine}</{currentCharacterSheetInstance.CharacterSheet.FirstName.Trim()}>";
            return innerStr.ToString();
        }

        private string GeneratePromptInjectionForCharacterRoll(ChatCharacterRoll roll, CharacterSheetInstance currentCharacterSheetInstance, CharacterSheetInstancesDbModel characterSheetsInstances)
        {
            string name = currentCharacterSheetInstance.CharacterSheet.FirstName.Trim();
            if (!string.IsNullOrWhiteSpace(currentCharacterSheetInstance.CharacterSheet.LastName))
                name += $" {currentCharacterSheetInstance.CharacterSheet.LastName.Trim()}";

            skillsInPlayInCurrentScene.Add(roll.ActionCategory);

            // Infer, alorithmically, the success or failure of the roll based on the roll value and the counter rolls
            // When a roll is BELOW 10, then it's considered as badly performed. It doesn't mean that it doesn't work, but it's a bit sloppy.
            var rollPerformance = (roll.Value + roll.Bonus) switch
            {
                <= 1 => RollPerformance.Terrible,
                <= 9 => RollPerformance.Bad,
                <= 12 => RollPerformance.Average,
                <= 19 => RollPerformance.Good,
                _ => RollPerformance.Excellent
            };

            string basicDescription = $"{name} is using her {roll.ActionCategory} skill: {string.Join(", ", roll.Guides.Select(s => s.Reasoning))} ";

            if (roll.CharactersInScene == null || roll.CharactersInScene.Length <= 0 || !roll.CharactersInScene.Any(a => a.CharacterInSceneCounterRoll?.Value != null))
            {
                basicDescription += GetSkillActionCategoryResultDefinition(roll.ActionCategory, rollPerformance, name);
                return basicDescription;
            }

            // When there's counter rolls, the roll performance will depend, for the character with a counter roll, on the actual value of the counter roll vs the roll
            StringBuilder str = new();
            str.AppendLine($"{basicDescription}");
            foreach (var characterWithCounterRoll in roll.CharactersInScene)
            {
                if (characterWithCounterRoll.CharacterInSceneCounterRoll?.Value == null)
                    continue;

                ++nbCounterRollsInjected;

                int offsetRollValue = (roll.Value + roll.Bonus) - characterWithCounterRoll.CharacterInSceneCounterRoll.Value;
                str.AppendLine($"{GetSkillActionCategoryResultCounterRollDefinition(roll.ActionCategory, offsetRollValue, name, characterWithCounterRoll.CharacterName)}");

                if (offsetRollValue <= 0)
                {
                    str.AppendLine($"{name}'s {roll.ActionCategory} skill FAIL against {characterWithCounterRoll.CharacterName}.");
                } else
                {
                    str.AppendLine($"{name}'s {roll.ActionCategory} skill SUCCEED against {characterWithCounterRoll.CharacterName}.");
                    //str.AppendLine($"Possible reaction from {characterWithCounterRoll.CharacterName}: {string.Join(", ", roll.Guides.Select(g => g.ReactionFromOtherCharactersWhenSucceedingSkillCheck))}");
                }
            }

            if (roll.CharactersInScene?.Any(a => a.CharacterInSceneCounterRoll?.Value != null) != null)
            {
                str.AppendLine($"Possible outcome (suggestion) against characters that FAIL: {string.Join(", ", roll.Guides.Select(g => g.ReactionFromOtherCharactersWhenSucceedingSkillCheck))}");
                str.AppendLine($"Possible outcome (suggestion) against characters that SUCCESS: {string.Join(", ", roll.Guides.Select(g => g.ReactionFromOtherCharactersWhenFailingSkillCheck))}");
            }

            return str.ToString();
        }

        private object GetSkillActionCategoryResultCounterRollDefinition(PathfinderSkills skillActionCategory, int offsetRollValue, string sourceCharacterName, string otherCharacterName)
        {
            return skillActionCategory switch
            {
                PathfinderSkills.Sex => offsetRollValue switch
                {
                    <= -4 => $"{otherCharacterName} thinks that {sourceCharacterName} performed the sexual act very poorly, with significant mistakes and lack of skill.",
                    <= -1 => $"{otherCharacterName} thinks that {sourceCharacterName} performed the sexual act poorly, with noticeable errors and lack of finesse.",
                    <= 1 => $"{otherCharacterName} thinks that {sourceCharacterName} performed the sexual act adequately, with some minor mistakes but overall acceptable and enjoyable.",
                    <= 2 => $"{otherCharacterName} thinks that {sourceCharacterName} performed the sexual act well, with good technique and skill, enhancing pleasure.",
                    <= 99 => $"{otherCharacterName} thinks that {sourceCharacterName} performed the sexual act exceptionally well, with great skill and finesse. The partner may even experience an orgasm right away due the exceptional performance.",
                    _ => "",
                },

                PathfinderSkills.Acrobatics => offsetRollValue switch
                {
                    <= -4 => $"{otherCharacterName} thinks {sourceCharacterName} stumbled and lost their balance, appearing as a clumsy fool.",
                    <= -1 => $"{otherCharacterName} thinks that {sourceCharacterName} performed the acrobatic movement poorly, with wobbles and wasted motion.",
                    <= 1 => $"{otherCharacterName} thinks that {sourceCharacterName} performed the acrobatic movement adequately, landing steadily enough.",
                    <= 2 => $"{otherCharacterName} thinks that {sourceCharacterName} performed the acrobatic movement with agility and control, cleanly and effectively.",
                    <= 99 => $"{otherCharacterName} thinks that {sourceCharacterName} performed the acrobatic feat with flawless grace, agility, and precision.",
                    _ => "",
                },

                PathfinderSkills.Athletics => offsetRollValue switch
                {
                    <= -4 => $"{otherCharacterName} notices that {sourceCharacterName} fails the physical exertion; they strain and cannot complete the task or they stumble and fall.",
                    <= -1 => $"{otherCharacterName} notices that {sourceCharacterName} forces through the physical task with poor form and obvious struggle. They fail.",
                    <= 1 => $"{otherCharacterName} notices that {sourceCharacterName} completes the physical task with acceptable effort.",
                    <= 2 => $"{otherCharacterName} notices that {sourceCharacterName} performs the physical task with strong form and endurance.",
                    <= 99 => $"{otherCharacterName} notices that {sourceCharacterName} performs the athletic feat with impressive power and stamina.",
                    _ => "",
                },

                PathfinderSkills.Deception => offsetRollValue switch
                {
                    <= -4 => $"{otherCharacterName} notices that {sourceCharacterName} tells an obvious lie.",
                    <= -1 => $"{otherCharacterName} notices {sourceCharacterName}'s lie.",
                    <= 1 => $"{otherCharacterName} does NOT notice that {sourceCharacterName} lied.",
                    <= 2 => $"{otherCharacterName} does NOT notice that {sourceCharacterName} lied. The lie is well-delivered.",
                    <= 99 => $"{otherCharacterName} does NOT notice that {sourceCharacterName} lied. The lie is masterfully delivered, leaving NO room for doubt.",
                    _ => "",
                },

                PathfinderSkills.Charisma => offsetRollValue switch
                {
                    <= -4 => $"{otherCharacterName} thinks that {sourceCharacterName} makes a terrible impression.",
                    <= -1 => $"{otherCharacterName} thinks that {sourceCharacterName} makes a poor impression and struggles to connect. They're not overly bad, but it's unconvincing.",
                    <= 1 => $"{otherCharacterName} thinks that {sourceCharacterName} makes an acceptable impression.",
                    <= 2 => $"{otherCharacterName} thinks that {sourceCharacterName} makes a strong impression and wins others over.",
                    <= 99 => $"{otherCharacterName} thinks that {sourceCharacterName} captivates those present with charm and presence.",
                    _ => "",
                },

                PathfinderSkills.Intimidation => offsetRollValue switch
                {
                    <= -4 => $"{otherCharacterName} thinks that {sourceCharacterName} delivers a laughable threat that undermines their own authority.",
                    <= -1 => $"{otherCharacterName} thinks that {sourceCharacterName} delivers a weak threat that fails to scare.",
                    <= 1 => $"{otherCharacterName} thinks that {sourceCharacterName} delivers a credible threat.",
                    <= 2 => $"{otherCharacterName} thinks that {sourceCharacterName} delivers a convincing threat. {otherCharacterName} may reconsider.",
                    <= 99 => $"{otherCharacterName} thinks that {sourceCharacterName} delivers a terrifying threat that may leave {otherCharacterName} shaken.",
                    _ => "",
                },

                PathfinderSkills.Medicine => offsetRollValue switch
                {
                    <= -4 => $"{otherCharacterName} notices that {sourceCharacterName} provides harmful medical care, causing additional injury or pain.",
                    <= -1 => $"{otherCharacterName} notices that {sourceCharacterName} delivers sloppy treatment that provides little benefit. It may even injure the patient instead.",
                    <= 1 => $"{otherCharacterName} notices that {sourceCharacterName} delivers adequate treatment that stabilizes or helps the patient as expected.",
                    <= 2 => $"{otherCharacterName} notices that {sourceCharacterName} delivers skilled treatment that improves the patient's condition notably.",
                    <= 99 => $"{otherCharacterName} notices that {sourceCharacterName} demonstrates exceptional medical expertise, saving or greatly restoring the patient.",
                    _ => "",
                },

                PathfinderSkills.Performance => offsetRollValue switch
                {
                    <= -4 => $"{otherCharacterName} notices that {sourceCharacterName} delivers a disasterous performance.",
                    <= -1 => $"{otherCharacterName} notices that {sourceCharacterName} delivers a flawed performance that fails to convince.",
                    <= 1 => $"{otherCharacterName} notices that {sourceCharacterName} delivers an acceptable performance that is mildly entertaining/convincing.",
                    <= 2 => $"{otherCharacterName} notices that {sourceCharacterName} delivers a polished performance that is entertaining or/and convincing.",
                    <= 99 => $"{otherCharacterName} notices that {sourceCharacterName} delivers an unforgettable performance that moves the audience deeply, incredibly entertaining and/or convincing.",
                    _ => "",
                },

                PathfinderSkills.Society => offsetRollValue switch
                {
                    <= -4 => $"{otherCharacterName} thinks that {sourceCharacterName} badly misreads social norms.",
                    <= -1 => $"{otherCharacterName} thinks that {sourceCharacterName} struggles with etiquette and misses important social cues.",
                    <= 1 => $"{otherCharacterName} thinks that {sourceCharacterName} navigates the social situation adequately.",
                    <= 2 => $"{otherCharacterName} thinks that {sourceCharacterName} reads the room well and responds appropriately.",
                    <= 99 => $"{otherCharacterName} thinks that {sourceCharacterName} masters the social intricacies and makes an excellent impression.",
                    _ => "",
                },

                PathfinderSkills.Aristocracy => offsetRollValue switch
                {
                    <= -4 => $"{otherCharacterName} thinks that {sourceCharacterName} commits a grave breach of noble etiquette and embarrasses themselves.",
                    <= -1 => $"{otherCharacterName} thinks that {sourceCharacterName} shows poor manners and offends aristocratic sensibilities.",
                    <= 1 => $"{otherCharacterName} thinks that {sourceCharacterName} conducts themselves with adequate noble decorum.",
                    <= 2 => $"{otherCharacterName} thinks that {sourceCharacterName} presents themselves with fine aristocratic grace and tact.",
                    <= 99 => $"{otherCharacterName} thinks that {sourceCharacterName} embodies noble grace and navigates high society flawlessly.",
                    _ => "",
                },

                PathfinderSkills.Stealth => offsetRollValue switch
                {
                    <= -4 => $"{otherCharacterName} notices {sourceCharacterName} as they make noise, knocks something over, which makes their presence obvious.",
                    <= -1 => $"{otherCharacterName} notices {sourceCharacterName} as they move clumsily and draw attention.",
                    <= 1 => $"{otherCharacterName} DOES NOT notice {sourceCharacterName}. They stay hidden or unnoticed well enough to avoid immediate detection.",
                    <= 2 => $"{otherCharacterName} DOES NOT notice {sourceCharacterName}. They move quietly and avoids notice with practiced skill.",
                    <= 99 => $"{otherCharacterName} DOES NOT notice {sourceCharacterName}. They are virtually undetectable, moving like a shadow and go completely unnoticed.",
                    _ => "",
                },

                PathfinderSkills.Thievery => offsetRollValue switch
                {
                    <= -4 => $"{otherCharacterName} notices {sourceCharacterName} as they fumble the job, possibly breaking something or alerting someone.",
                    <= -1 => $"{otherCharacterName} notices {sourceCharacterName} as they struggle, leaving obvious traces or nearly getting caught.",
                    <= 1 => $"{otherCharacterName} DOES NOT notice {sourceCharacterName}. They complete the theft or lockpicking adequately, though not perfectly.",
                    <= 2 => $"{otherCharacterName} DOES NOT notice {sourceCharacterName}. They perform the thievery cleanly and without leaving obvious evidence.",
                    <= 99 => $"{otherCharacterName} DOES NOT notice {sourceCharacterName}. They execute the theft with flawless precision, leaving no trace.",
                    _ => "",
                },

                _ => "",
            };
        }

        private string GetSkillActionCategoryResultDefinition(PathfinderSkills skillActionCategory, RollPerformance rollPerformance, string characterName)
        {
            return skillActionCategory switch
            {
                PathfinderSkills.Sex => rollPerformance switch
                {
                    RollPerformance.Terrible => $"{characterName} performed the sexual act very poorly, with significant mistakes and lack of skill.",
                    RollPerformance.Bad => $"{characterName} performed the sexual act poorly, with noticeable errors and lack of finesse.",
                    RollPerformance.Good => $"{characterName} performed the sexual act well, with good technique and skill, enhancing pleasure.",
                    RollPerformance.Excellent => $"{characterName} performed the sexual act exceptionally well, with great skill and finesse. The partner may even experience an orgasm right away due the exceptional performance.",
                    _ => $"{characterName} performed the sexual act adequately, with some minor mistakes but overall acceptable and enjoyable.",
                },
                PathfinderSkills.Acrobatics => rollPerformance switch
                {
                    RollPerformance.Terrible => $"{characterName} stumbles and loses their balance, appearing as a clumsy fool.",
                    RollPerformance.Bad => $"{characterName} performs the acrobatic movement poorly, with wobbles and wasted motion.",
                    RollPerformance.Good => $"{characterName} executes the acrobatic movement with agility and control, cleanly and effectively.",
                    RollPerformance.Excellent => $"{characterName} performs the acrobatic feat with flawless grace, agility, and precision.",
                    _ => $"{characterName} performs the acrobatic movement adequately, landing steadily enough."
                },

                PathfinderSkills.Athletics => rollPerformance switch
                {
                    RollPerformance.Terrible => $"{characterName} fails the physical exertion; they strain and cannot complete the task or they stumble and fall.",
                    RollPerformance.Bad => $"{characterName} forces through the physical task with poor form and obvious struggle. They fail.",
                    RollPerformance.Good => $"{characterName} performs the physical task with strong form and endurance.",
                    RollPerformance.Excellent => $"{characterName} performs the athletic feat with impressive power and stamina.",
                    _ => $"{characterName} completes the physical task with acceptable effort."
                },

                PathfinderSkills.Deception => rollPerformance switch
                {
                    RollPerformance.Terrible => $"{characterName} tells an obvious lie that is immediately doubted.",
                    RollPerformance.Bad => $"{characterName} attempts deception but it is shaky, with inconsistencies that could be noticed.",
                    RollPerformance.Good => $"{characterName} deceives effectively and is well-delivered with little risk of being discovered.",
                    RollPerformance.Excellent => $"{characterName} masterfully deceives, leaving NO room for doubt.",
                    _ => $"{characterName} deceives believably enough, though not airtight."
                },

                PathfinderSkills.Charisma => rollPerformance switch
                {
                    RollPerformance.Terrible => $"{characterName} makes a terrible impression; others are put off by their poor charisma.",
                    RollPerformance.Bad => $"{characterName} makes a poor impression and struggles to connect. They're not overly bad, but it's unconvincing.",
                    RollPerformance.Good => $"{characterName} makes a strong impression and wins others over.",
                    RollPerformance.Excellent => $"{characterName} captivates those present with charm and presence.",
                    _ => $"{characterName} makes an acceptable impression."
                },

                PathfinderSkills.Intimidation => rollPerformance switch
                {
                    RollPerformance.Terrible => $"{characterName} delivers a laughable threat that undermines their authority.",
                    RollPerformance.Bad => $"{characterName} delivers a weak threat that fails to cow the target(s).",
                    RollPerformance.Good => $"{characterName} delivers a convincing threat that makes the target(s) reconsider.",
                    RollPerformance.Excellent => $"{characterName} delivers a terrifying threat that leaves the target(s) shaken.",
                    _ => $"{characterName} delivers a credible threat that gives the target(s) pause. Depending on the target(s)'s personality, they may react differently."
                },

                PathfinderSkills.Medicine => rollPerformance switch
                {
                    RollPerformance.Terrible => $"{characterName} provides harmful medical care, causing additional injury or pain.",
                    RollPerformance.Bad => $"{characterName} delivers sloppy treatment that provides little benefit. It may even injure the patient instead.",
                    RollPerformance.Good => $"{characterName} delivers skilled treatment that improves the patient's condition notably.",
                    RollPerformance.Excellent => $"{characterName} demonstrates exceptional medical expertise, saving or greatly restoring the patient.",
                    _ => $"{characterName} delivers adequate treatment that stabilizes or helps the patient as expected."
                },

                PathfinderSkills.Performance => rollPerformance switch
                {
                    RollPerformance.Terrible => $"{characterName} delivers a disasterous performance; the audience immediately know that something is amiss.",
                    RollPerformance.Bad => $"{characterName} delivers a flawed performance that fails to convince.",
                    RollPerformance.Good => $"{characterName} delivers a polished performance that is entertaining or/and convincing.",
                    RollPerformance.Excellent => $"{characterName} delivers an unforgettable performance that moves the audience deeply, incredibly entertaining and/or convincing.",
                    _ => $"{characterName} delivers an acceptable performance that is mildly entertaining/convincing."
                },

                PathfinderSkills.Society => rollPerformance switch
                {
                    RollPerformance.Terrible => $"{characterName} badly misreads social norms and commits a faux pas.",
                    RollPerformance.Bad => $"{characterName} struggles with etiquette and misses important social cues.",
                    RollPerformance.Good => $"{characterName} reads the room well and responds appropriately.",
                    RollPerformance.Excellent => $"{characterName} masters the social intricacies and makes an excellent impression.",
                    _ => $"{characterName} navigates the social situation adequately."
                },

                PathfinderSkills.Aristocracy => rollPerformance switch
                {
                    RollPerformance.Terrible => $"{characterName} commits a grave breach of noble etiquette and embarrasses themselves.",
                    RollPerformance.Bad => $"{characterName} shows poor manners and offends aristocratic sensibilities.",
                    RollPerformance.Good => $"{characterName} presents themselves with fine aristocratic grace and tact.",
                    RollPerformance.Excellent => $"{characterName} embodies noble grace and navigates high society flawlessly.",
                    _ => $"{characterName} conducts themselves with adequate noble decorum."
                },

                PathfinderSkills.Stealth => rollPerformance switch
                {
                    RollPerformance.Terrible => $"{characterName} makes noise, knocks something over, which makes their presence obvious.",
                    RollPerformance.Bad => $"{characterName} moves clumsily and draws attention.",
                    RollPerformance.Good => $"{characterName} moves quietly and avoids notice with practiced skill.",
                    RollPerformance.Excellent => $"{characterName} is virtually undetectable, moving like a shadow.",
                    _ => $"{characterName} stays hidden or unnoticed well enough to avoid immediate detection."
                },

                PathfinderSkills.Thievery => rollPerformance switch
                {
                    RollPerformance.Terrible => $"{characterName} fumbles the job, possibly breaking something or alerting someone.",
                    RollPerformance.Bad => $"{characterName} struggles, leaving obvious traces or nearly getting caught.",
                    RollPerformance.Good => $"{characterName} performs the thievery cleanly and without leaving obvious evidence.",
                    RollPerformance.Excellent => $"{characterName} executes the theft with flawless precision, leaving no trace.",
                    _ => $"{characterName} completes the theft or lockpicking adequately, though not perfectly."
                },
                _ => "",
            };
        }

        private string FormatActionResult(ChatCharacterRoll roll, CharacterSheetInstancesDbModel characterSheetsInstances)
        {
            string outputValue = "";

            foreach (CharacterInScene characterInScene in roll.CharactersInScene.Where(w => w.CharacterInSceneCounterRoll != null))
            {
                var characterSheetInstance = characterSheetsInstances.CharacterSheetInstances.FirstOrDefault(f => f.CharacterSheetInstanceId == characterInScene.CharacterSheetInstanceId);
                string characterName = characterSheetInstance?.CharacterSheet?.FirstName;

                if (characterSheetInstance == null)
                {
                    characterName = characterInScene.CharacterName?.Trim();

                    if (string.IsNullOrWhiteSpace(characterName))
                        continue;
                }

                outputValue += $"    - {characterName} has rolled {characterInScene.CharacterInSceneCounterRoll.Value} for attribute {characterInScene.CharacterInSceneCounterRoll.Attribute}.{Environment.NewLine}";
            }

            if (roll.CharactersInScene.Length <= 0)
            {
                outputValue += $"    No one. The roll success or failure depends solely on itself.";
            }

            return outputValue.TrimEnd().TrimEnd(Environment.NewLine.ToCharArray());
        }

        public async Task<(string, IShareableContextLink)> BuildAsync()
        {
            var rollsByCharacters = await storageService.GetChatCharactersRollsByChatIdAsync(chatDbModel.ChatId);
            if (rollsByCharacters?.ChatCharactersRolls == null || rollsByCharacters.ChatCharactersRolls.Count <= 0)
            {
                return (null, new ShareableContextLink { LinkedBuilder = this, });
            }

            // Get the characterSheetInstance associated with each
            var characterSheetsInstances = await storageService.GetCharacterSheetsInstanceByChatIdAsync(chatDbModel.ChatId);

            int nbRollsInjected = 0;
            StringBuilder str = new StringBuilder();

            str.AppendLine($"<pathfinder_characters_rolls>");
            foreach (ChatCharacterRolls rollsSpecificToOneCharacter in rollsByCharacters.ChatCharactersRolls.Where(w => w.Rolls != null))
            {
                var rollsToInject = rollsSpecificToOneCharacter.Rolls.Where(w => w.NbRemainingInjectionTurns > 0).ToArray();

                if (rollsToInject.Length <= 0)
                    continue;

                var currentCharacterSheetInstance = characterSheetsInstances.CharacterSheetInstances.FirstOrDefault(f => f.CharacterSheetInstanceId == rollsSpecificToOneCharacter.CharacterSheetInstanceId);
                if (currentCharacterSheetInstance == null)
                {
                    LoggingManager.LogToFile("4455c9bb-de6d-4cbe-97ec-95115627e5fd", $"ChatCharacterRolls in chat [{chatDbModel.ChatId}] tied to characterSheetInstance [{rollsSpecificToOneCharacter.CharacterSheetInstanceId}] couldn't be found in CharacterSheetInstances storage. Ignoring rolls for this specific characterSheetInstance.");
                    continue;// Ignore
                }

                string value = GeneratePromptInjectionForCharacterRolls(rollsToInject, currentCharacterSheetInstance, characterSheetsInstances);
                str.AppendLine(value);
                ++nbRollsInjected;
            }

            str.AppendLine($"</pathfinder_characters_rolls>");

            // inject the definition for the skills (action category) that are currently being used in the scene, so that we ONLY inject the skills that are relevant CURRENTLY for the LLM
            if (skillsInPlayInCurrentScene.Count > 0)
            {
                str.AppendLine($"</skills_definition>");
                foreach (var skillActionCategory in skillsInPlayInCurrentScene)
                {
                    str.AppendLine($"<{skillActionCategory}>{GetSkillActionCategoryDefinition(skillActionCategory)}</{skillActionCategory}>");
                }

                str.Append($"</skills_definition>");
            }

            if (nbRollsInjected <= 0)
            {
                return ($"",
                new ShareableContextLink
                {
                    LinkedBuilder = this,
                });
            }

            return ($"<pathfinder>{Environment.NewLine}Details on characters reactions following recent actions. Consider these  some details in your reply about how other characters react.{Environment.NewLine}{str.ToString().InjectMacros(personaLinkedToChat?.Name, charactersLinkedToChat?.FirstOrDefault()?.Name)}{Environment.NewLine}</pathfinder>{Environment.NewLine}{Environment.NewLine}",
                new ShareableContextLink
                {
                    LinkedBuilder = this,
                });
        }

        private string GetSkillActionCategoryDefinition(PathfinderSkills skillActionCategory)
        {
            return skillActionCategory switch
            {
                PathfinderSkills.Sex => "Sexual knowledge, sexual acts performance, sexual teasing, sensuality, etc). It indicates how well the action was performed.",
                PathfinderSkills.Acrobatics => "When someone is dodging or doing acrobatic movements or using their agility to critically enhance their movements.",
                PathfinderSkills.Athletics => "Physical strength. Ability to physically restrain someone, overpower them, etc.",
                PathfinderSkills.Deception => "When someone is lying, when they're being misleading, dishonest or insincere.",
                PathfinderSkills.Charisma => "Diplomacy, argumentation, debating and persuasion.",
                PathfinderSkills.Intimidation => "When someone is intimidating someone else by using physical strength, coercion, compulsion, oppression, harassment, threats or by using their influence.",
                PathfinderSkills.Medicine => "When someone is using medicinal knowledge to treat a condition or to get insights. Medical acts are also included in this category.",
                PathfinderSkills.Performance => "When someone is acting, masking their emotions or disguising themselves.",
                PathfinderSkills.Society => "When someone is using their knowledge about politics or how society works. For example when a character was raised outside societey and is surprised about their lack of common knowledge.",
                PathfinderSkills.Aristocracy => "When a character is using manners or etiquette, usually in nobility or aristocratic context. Elitism knowledge.",
                PathfinderSkills.Stealth => "When a character is trying to not be perceived by another. When they're trying to dissimulate or conceal themselves.",
                PathfinderSkills.Thievery => "When a character is stealing or trying to steal, including sleight of hand.",
                _ => throw new Exception($"Skill action category [{skillActionCategory}] is not defined in the GetSkillActionCategoryDefinition method."),
            };
        }
    }
}
