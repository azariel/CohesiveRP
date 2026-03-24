using System.Text;
using CohesiveRP.Common.Diagnostics;
using CohesiveRP.Core.PromptContext.Abstractions;
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

        public SkillChecksResultsBuilder(IStorageService storageService, PromptContextFormatElement promptContextFormatElement, ChatDbModel chatDbModel)
        {
            this.storageService = storageService;
            this.promptContextFormatElement = promptContextFormatElement;
            this.chatDbModel = chatDbModel;
        }

        private string GeneratePromptInjectionForCharacterRolls(ChatCharacterRoll[] rollsToInject, CharacterSheetInstance currentCharacterSheetInstance, CharacterSheetInstancesDbModel characterSheetsInstances)
        {
            string innerStr = "";

            innerStr += $"<{currentCharacterSheetInstance.CharacterSheet.FirstName}>";
            foreach (ChatCharacterRoll roll in rollsToInject)
            {
                // inject the roll into the prompt
                string value = GeneratePromptInjectionForCharacterRoll(roll, currentCharacterSheetInstance, characterSheetsInstances);
                innerStr += $"{value}{Environment.NewLine}";

                // TODO: this MUST be done in the Processer AFTER completion
                roll.NbRemainingInjectionTurns--;
                roll.NbRemainingRollFreeze--;
            }

            innerStr = innerStr.Trim().TrimEnd(Environment.NewLine.ToCharArray());
            innerStr += $"</{currentCharacterSheetInstance.CharacterSheet.FirstName}>";
            return innerStr.ToString();
        }

        private string GeneratePromptInjectionForCharacterRoll(ChatCharacterRoll roll, CharacterSheetInstance currentCharacterSheetInstance, CharacterSheetInstancesDbModel characterSheetsInstances)
        {
            StringBuilder str = new();
            str.AppendLine($"  <roll>{Environment.NewLine}    {currentCharacterSheetInstance.CharacterSheet.FirstName} has rolled {roll.Value} for skill {roll.ActionCategory}.{Environment.NewLine}  </roll>");
            str.AppendLine($"  <actions>{Environment.NewLine}    -{string.Join($"{Environment.NewLine}-", roll.Reasonings)}{Environment.NewLine}  </actions>");
            str.AppendLine($"  <otherCharacters>{Environment.NewLine}    {FormatActionResult(roll, characterSheetsInstances)}{Environment.NewLine}  </otherCharacters>");
            return str.ToString();
        }

        private string FormatActionResult(ChatCharacterRoll roll, CharacterSheetInstancesDbModel characterSheetsInstances)
        {
            string outputValue = "";

            foreach (CharacterInScene characterInScene in roll.CharactersInScene.Where(w => w.CharacterInSceneCounterRoll != null))
            {
                var characterSheetInstance = characterSheetsInstances.CharacterSheetInstances.FirstOrDefault(f => f.CharacterSheetInstanceId == characterInScene.CharacterSheetInstanceId);

                if (characterSheetInstance == null)
                    continue;

                outputValue += $"- {characterSheetInstance.CharacterSheet.FirstName} has rolled {characterInScene.CharacterInSceneCounterRoll.Value} for attribute {characterInScene.CharacterInSceneCounterRoll.Attribute}.{Environment.NewLine}";
            }

            return outputValue.Trim().TrimEnd(Environment.NewLine.ToCharArray());
        }

        public async Task<(string, IShareableContextLink)> BuildAsync()
        {
            var rollsByCharacters = await storageService.GetChatCharactersRollsByIdAsync(chatDbModel.ChatId);
            if (rollsByCharacters?.ChatCharactersRolls == null || rollsByCharacters.ChatCharactersRolls.Count <= 0)
            {
                return (null, new ShareableContextLink { LinkedBuilder = this, });
            }

            // Get the characterSheetInstance associated with each
            var characterSheetsInstances = await storageService.GetCharacterSheetsInstanceByChatIdAsync(chatDbModel.ChatId);

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
            }

            str.Append($"</pathfinder_characters_rolls>");

            // Update to keep track of NbRemainingInjectionTurns
            await storageService.UpdateChatCharactersRollsAsync(rollsByCharacters);

            if (string.IsNullOrWhiteSpace(str.ToString()))
            {
                return (null, new ShareableContextLink { LinkedBuilder = this, });
            }

            return ($"<pathfinder_module_skills_checks>{Environment.NewLine}Details on characters reactions following recent actions. Consider these  some details in your reply about how other characters react.{Environment.NewLine}{str}{Environment.NewLine}</pathfinder_module_skills_checks>{Environment.NewLine}{Environment.NewLine}",
                new ShareableContextLink
                {
                    LinkedBuilder = this,
                });
        }
    }
}
