using CohesiveRP.Common.Serialization;
using CohesiveRP.Core.LLMProviderProcessors.DynamicCharacterCreator.BusinessObjects;
using CohesiveRP.Core.PromptContext.Abstractions;
using CohesiveRP.Core.PromptContext.Utils;
using CohesiveRP.Core.Services;
using CohesiveRP.Storage.DataAccessLayer.AIQueries;
using CohesiveRP.Storage.DataAccessLayer.ChatCompletionPresets.BusinessObjects.Format;
using CohesiveRP.Storage.DataAccessLayer.Chats;

namespace CohesiveRP.Core.PromptContext.Builders.Directive
{
    public class PromptContextCharacterSheetCreationBuilder : IPromptContextElementBuilder
    {
        private IStorageService storageService;
        private PromptContextFormatElement promptContextFormatElement;
        private ChatDbModel chatDbModel;
        private PersonaDbModel personaLinkedToChat;
        private CharacterDbModel[] charactersLinkedToChat;
        private string contextLinkedId;

        public PromptContextCharacterSheetCreationBuilder(IStorageService storageService, PromptContextFormatElement promptContextFormatElement, ChatDbModel chatDbModel, string contextLinkedId, PersonaDbModel personaLinkedToChat, CharacterDbModel[] charactersLinkedToChat)
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
                var links = JsonCommonSerializer.DeserializeFromString<ShareableNewCharacterLinks>(contextLinkedId);
                var character = await storageService.GetCharacterByIdAsync(links.CharacterId);

                if (character == null)
                {
                    return ("", new ShareableContextLink { LinkedBuilder = this });
                }

                return ($"{promptContextFormatElement?.Options?.Format?
                    .Replace("{{character_name}}", character.Name)
                    .Replace("{{character_description}}", character.Description)
                    .InjectMacros(personaLinkedToChat?.Name, charactersLinkedToChat?.FirstOrDefault()?.Name)}{Environment.NewLine}",
                    new ShareableContextLink { LinkedBuilder = this });
            } catch (Exception e)
            {
                return ("", new ShareableContextLink { LinkedBuilder = this });
            }
        }
    }
}
