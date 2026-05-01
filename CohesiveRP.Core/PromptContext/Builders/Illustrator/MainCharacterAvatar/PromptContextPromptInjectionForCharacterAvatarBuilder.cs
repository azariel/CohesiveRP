using CohesiveRP.Common.Serialization;
using CohesiveRP.Core.LLMProviderProcessors.DynamicCharacterCreator.BusinessObjects;
using CohesiveRP.Core.PromptContext.Abstractions;
using CohesiveRP.Core.PromptContext.Utils;
using CohesiveRP.Core.Services;
using CohesiveRP.Storage.DataAccessLayer.ChatCompletionPresets.BusinessObjects.Format;
using CohesiveRP.Storage.DataAccessLayer.Chats;

namespace CohesiveRP.Core.PromptContext.Builders.Illustrator.MainCharacterAvatar
{
    public class PromptContextPromptInjectionForCharacterAvatarBuilder : IPromptContextElementBuilder
    {
        private IStorageService storageService;
        private PromptContextFormatElement promptContextFormatElement;
        private ChatDbModel chatDbModel;
        private PersonaDbModel personaLinkedToChat;
        private CharacterDbModel[] charactersLinkedToChat;
        private string contextLinkedId;

        public PromptContextPromptInjectionForCharacterAvatarBuilder(IStorageService storageService, PromptContextFormatElement promptContextFormatElement, ChatDbModel chatDbModel, string contextLinkedId, PersonaDbModel personaLinkedToChat, CharacterDbModel[] charactersLinkedToChat)
        {
            this.storageService = storageService;
            this.promptContextFormatElement = promptContextFormatElement;
            this.chatDbModel = chatDbModel;
            this.personaLinkedToChat = personaLinkedToChat;
            this.charactersLinkedToChat = charactersLinkedToChat;
            this.contextLinkedId = contextLinkedId;
        }

        public async Task<(string, IShareableContextLink)> BuildAsync()
        {
            try
            {
                // Get the interactiveQuery associated with our backgroundQuery
                var character = await storageService.GetCharacterByIdAsync(contextLinkedId);
                if (character == null)
                {
                    return ("", new ShareableContextLink { LinkedBuilder = this });
                }

                var characterSheetInstances = await storageService.GetCharacterSheetsInstanceByChatIdAsync(chatDbModel.ChatId);
                if (characterSheetInstances?.CharacterSheetInstances == null || characterSheetInstances.CharacterSheetInstances.Count <= 0)
                {
                    return ("", new ShareableContextLink { LinkedBuilder = this });
                }

                var characterSheetInstance = characterSheetInstances.CharacterSheetInstances.FirstOrDefault(f => f.CharacterId == character.CharacterId);
                if (characterSheetInstance == null)
                {
                    return ("", new ShareableContextLink { LinkedBuilder = this });
                }

                return ($"{promptContextFormatElement?.Options?.Format?
                    .Replace("{{character_name}}", character.Name)
                    .Replace("{{character_race}}", characterSheetInstance.CharacterSheet.Race)
                    .Replace("{{character_bodyType}}", characterSheetInstance.CharacterSheet.BodyType)
                    .Replace("{{character_height}}", characterSheetInstance.CharacterSheet.Height)
                    .Replace("{{character_eyeColor}}", characterSheetInstance.CharacterSheet.EyeColor)
                    .Replace("{{character_skinColor}}", characterSheetInstance.CharacterSheet.SkinColor)
                    .Replace("{{character_hairColor}}", characterSheetInstance.CharacterSheet.HairColor)
                    .Replace("{{character_hairStyle}}", characterSheetInstance.CharacterSheet.HairStyle)
                    .Replace("{{character_earShape}}", characterSheetInstance.CharacterSheet.EarShape)
                    .Replace("{{character_clothesPreferences}}", characterSheetInstance.CharacterSheet.ClothesPreference)
                    .InjectMacros(personaLinkedToChat?.Name, charactersLinkedToChat?.FirstOrDefault()?.Name)}{Environment.NewLine}",
                    new ShareableContextLink { LinkedBuilder = this });
            } catch (Exception e)
            {
                return ("", new ShareableContextLink { LinkedBuilder = this });
            }
        }
    }
}
