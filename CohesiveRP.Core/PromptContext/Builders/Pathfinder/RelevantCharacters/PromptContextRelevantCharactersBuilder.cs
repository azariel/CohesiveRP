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
            if(str == null || characterSheet == null)
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
                    if(tagName == "pathfinderAttributes" || tagName == "pathfinderSkills")
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

            CharacterSheetInstance[] charactersToInclude = characterSheetInstances.CharacterSheetInstances.Where(w =>
            chatDbModel.CharacterIds.Any(a => w.CharacterId == a) &&
            w.CharacterSheet != null &&
            !string.IsNullOrWhiteSpace(w.CharacterSheet.FirstName)).ToArray();

            if (charactersToInclude.Length <= 0)
            {
                return (null, new ShareableContextLink { LinkedBuilder = this });
            }

            StringBuilder str = new();
            foreach (CharacterSheetInstance characterSheetInstance in charactersToInclude)
            {
                str.AppendLine($"  <{characterSheetInstance.CharacterSheet.FirstName}>");
                AppendCharacterSheetToPromptContext(str, characterSheetInstance.CharacterSheet);
                str.AppendLine($"  </{characterSheetInstance.CharacterSheet.FirstName}>");
            }

            if (!string.IsNullOrWhiteSpace(chatDbModel.PersonaId))
            {
                var personaCharacterSheet = characterSheetInstances.CharacterSheetInstances.FirstOrDefault(f =>
                    f.PersonaId == chatDbModel.PersonaId &&
                    f.CharacterSheet != null &&
                    !string.IsNullOrWhiteSpace(f.CharacterSheet.FirstName));

                str.AppendLine($"  <{personaCharacterSheet.CharacterSheet.FirstName}>");
                AppendCharacterSheetToPromptContext(str, personaCharacterSheet.CharacterSheet);
                str.AppendLine($"  </{personaCharacterSheet.CharacterSheet.FirstName}>");
            }

            return ($"<relevant_characters>{Environment.NewLine}{str.ToString().Trim().TrimEnd(Environment.NewLine.ToCharArray())}{Environment.NewLine}</relevant_characters>{Environment.NewLine}{Environment.NewLine}", new ShareableContextLink { LinkedBuilder = this });
        }
    }
}
