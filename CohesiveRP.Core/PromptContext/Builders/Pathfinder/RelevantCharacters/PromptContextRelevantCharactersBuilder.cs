using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using CohesiveRP.Core.PromptContext.Abstractions;
using CohesiveRP.Core.Services;
using CohesiveRP.Storage.DataAccessLayer.ChatCompletionPresets.BusinessObjects.Format;
using CohesiveRP.Storage.DataAccessLayer.Chats;
using CohesiveRP.Storage.DataAccessLayer.Pathfinder.CharacterSheetInstances.BusinessObjects;
using CohesiveRP.Storage.DataAccessLayer.Pathfinder.CharacterSheets.BusinessObjects;
using CohesiveRP.Storage.DataAccessLayer.Pathfinder.ChatCharactersRolls.BusinessObjects;

namespace CohesiveRP.Core.PromptContext.Builders.Pathfinder.RelevantCharacters
{
    public class PromptContextRelevantCharactersBuilder : IPromptContextElementBuilder
    {
        private IStorageService storageService;
        private PromptContextFormatElement promptContextFormatElement;
        private ChatDbModel chatDbModel;

        public PromptContextRelevantCharactersBuilder(IStorageService storageService, PromptContextFormatElement promptContextFormatElement, ChatDbModel chatDbModel)
        {
            this.storageService = storageService;
            this.promptContextFormatElement = promptContextFormatElement;
            this.chatDbModel = chatDbModel;
        }

        private static string FormatPropertyValue(object value)
        {
            return value switch
            {
                null => null,
                string s => string.IsNullOrWhiteSpace(s) ? null : s.Trim(),
                string[] arr => FormatStringArray(arr),
                DateTime dt => dt.ToString("yyyy-MM-dd"),
                Array arr when arr.Length > 0 => FormatComplexArray(arr),
                Array => null,
                Enum e => e.ToString(),
                _ => value.ToString()
            };
        }

        private static string FormatStringArray(string[] arr)
        {
            string[] nonEmpty = arr?.Where(s => !string.IsNullOrWhiteSpace(s)).ToArray() ?? [];
            return nonEmpty.Length > 0
                ? string.Join(", ", nonEmpty)
                : null;
        }

        private static string FormatComplexArray(Array arr)
        {
            return JsonSerializer.Serialize(arr, new JsonSerializerOptions
            {
                Converters = { new JsonStringEnumConverter() },
                WriteIndented = false
            });
        }

        private void AppendCharacterSheetToPromptContext(StringBuilder str, CharacterSheet characterSheet)
        {
            if (str == null || characterSheet == null)
            {
                return;
            }

            var properties = typeof(CharacterSheet).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (PropertyInfo property in properties)
            {
                try
                {
                    string tagName = property.GetCustomAttribute<JsonPropertyNameAttribute>()?.Name ?? property.Name;

                    // Those particular properties will be handled manually for more control
                    if (tagName == "pathfinderAttributes" || tagName == "pathfinderSkills")
                        continue;

                    object value = property.GetValue(characterSheet);
                    string formattedValue = FormatPropertyValue(value);

                    if (string.IsNullOrWhiteSpace(formattedValue))
                        continue;

                    str.AppendLine($"  <{tagName}>{formattedValue}</{tagName}>");
                } catch (Exception)
                {
                    // ignore
                }
            }

            // Handle the Pathfinder special properties
            str.AppendLine($"    <Attributes>");
            foreach (PathfinderAttribute attribute in characterSheet.PathfinderAttributesValues)
            {
                str.AppendLine($"      <{attribute.AttributeType}>{attribute.Value}</{attribute.AttributeType}>");
            }

            str.AppendLine($"    </Attributes>");
            str.AppendLine($"    <Skills>");
            foreach (PathfinderSkillAttributes skill in characterSheet.PathfinderSkillsValues)
            {
                str.AppendLine($"      <{skill.SkillType}>{skill.Value}</{skill.SkillType}>");
            }
            str.AppendLine($"    </Skills>");
        }

        private bool AreNameEquivalent(string nameToEvaluate, string firstName, string lastName)
        {
            if (string.IsNullOrWhiteSpace(nameToEvaluate))
            {
                return false;
            }

            string inputName = nameToEvaluate.ToLowerInvariant().Trim();
            string inputFirstName = firstName?.ToLowerInvariant().Trim();
            string inputLastName = lastName?.ToLowerInvariant().Trim();

            return inputName == inputFirstName || inputName == inputLastName || inputName == $"{inputFirstName} {inputLastName}";
        }

        public async Task<(string, IShareableContextLink)> BuildAsync()
        {
            if (chatDbModel?.CharacterIds == null || chatDbModel.CharacterIds.Count <= 0)
            {
                return (null, new ShareableContextLink { LinkedBuilder = this });
            }

            var characterSheetInstances = await storageService.GetCharacterSheetsInstanceByChatIdAsync(chatDbModel.ChatId);
            if (characterSheetInstances?.CharacterSheetInstances == null || characterSheetInstances.CharacterSheetInstances.Count <= 0)
            {
                return (null, new ShareableContextLink { LinkedBuilder = this });
            }

            var options = promptContextFormatElement?.Options as PromptContextFormatElementRelevantCharactersOptions;
            CharacterSheetInstance[] charactersToInclude = characterSheetInstances.CharacterSheetInstances.Where(w =>
            chatDbModel.CharacterIds.Any(a => w.CharacterId == a) &&
            w.CharacterSheet != null &&
            !string.IsNullOrWhiteSpace(w.CharacterSheet.FirstName)).ToArray();

            if (charactersToInclude.Length <= 0)
            {
                return (null, new ShareableContextLink { LinkedBuilder = this });
            }

            // refine the characters to include to only include those IN the scene
            var characterRolls = await storageService.GetChatCharactersRollsByIdAsync(chatDbModel.ChatId);

            StringBuilder str = new();
            if (!string.IsNullOrWhiteSpace(chatDbModel.PersonaId))
            {
                var personaCharacterSheet = characterSheetInstances.CharacterSheetInstances.FirstOrDefault(f =>
                    f.PersonaId == chatDbModel.PersonaId &&
                    f.CharacterSheet != null &&
                    !string.IsNullOrWhiteSpace(f.CharacterSheet.FirstName));

                if (personaCharacterSheet != null)
                {
                    str.AppendLine($"  <{personaCharacterSheet.CharacterSheet.FirstName} (player)>");
                    AppendCharacterSheetToPromptContext(str, personaCharacterSheet.CharacterSheet);
                    str.AppendLine($"  </{personaCharacterSheet.CharacterSheet.FirstName} (player)>");
                }
            }

            if (characterRolls?.CharacterNamesInScene != null && characterRolls.CharacterNamesInScene.Count > 0)
            {
                charactersToInclude = charactersToInclude.Where(w => characterRolls.CharacterNamesInScene.Any(a => AreNameEquivalent(a, w.CharacterSheet.FirstName, w.CharacterSheet.LastName))).ToArray();

                List<CharacterSheetInstance> orderedInstances = new();
                foreach (var characterNameInScene in characterRolls.CharacterNamesInScene)
                {
                    var selection = charactersToInclude.FirstOrDefault(f => AreNameEquivalent(characterNameInScene, f.CharacterSheet.FirstName, f.CharacterSheet.LastName));

                    if(selection != null)
                    {
                        orderedInstances.Add(selection);
                    }
                }

                foreach (CharacterSheetInstance characterSheetInstance in orderedInstances.Take(2))// TODO: make the limit configurable
                {
                    str.AppendLine($"  <{GetCharacterFullName(characterSheetInstance.CharacterSheet.FirstName, characterSheetInstance.CharacterSheet.LastName)}>");
                    AppendCharacterSheetToPromptContext(str, characterSheetInstance.CharacterSheet);
                    str.AppendLine($"  </{GetCharacterFullName(characterSheetInstance.CharacterSheet.FirstName, characterSheetInstance.CharacterSheet.LastName)}>");
                }
            }

            List<string> knownCharacters = new();
            var allCharacters = await storageService.GetCharactersAsync();
            foreach (string characterId in chatDbModel.CharacterIds)
            {
                CharacterSheetInstance characterSheet = characterSheetInstances.CharacterSheetInstances.FirstOrDefault(w => characterId == w.CharacterId && w.CharacterSheet != null && !string.IsNullOrWhiteSpace(w.CharacterSheet.FirstName));

                string characterName = "";

                if (characterSheet != null)
                {
                    characterName = GetCharacterFullName(characterSheet?.CharacterSheet?.FirstName, characterSheet?.CharacterSheet?.LastName);
                } else
                {
                    characterName = allCharacters.FirstOrDefault(f => f.CharacterId == characterId)?.Name?.Trim();
                }

                if (!string.IsNullOrWhiteSpace(characterName))
                {
                    knownCharacters.Add(characterName);
                }
            }

            string finalKnownCharacters = "";
            if (knownCharacters.Count > 0 && (options == null || options.IncludeKnownCharacters))
            {
                finalKnownCharacters = $"<known_characters_in_story_context>{string.Join(",", knownCharacters)}</known_characters_in_story_context>";
            }

            return ($"<relevant_characters>{Environment.NewLine}{str.ToString().Trim().TrimEnd(Environment.NewLine.ToCharArray())}{Environment.NewLine}</relevant_characters>{Environment.NewLine}Please note that secretKinks are kinks or fetishes that the character is ashamed or embarassed about and will avoid openly talk about, but that character will react positively when exposed to situations implementing their kinks and secret kinks or fetishes.{Environment.NewLine}{Environment.NewLine}", new ShareableContextLink { LinkedBuilder = this });
        }

        private static string GetCharacterFullName(string firstName, string lastName) => $"{firstName} {lastName}".Trim();
    }
}
