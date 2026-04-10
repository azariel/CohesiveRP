using System.Text;
using CohesiveRP.Core.PromptContext.Abstractions;
using CohesiveRP.Core.PromptContext.Utils;
using CohesiveRP.Core.Services;
using CohesiveRP.Storage.DataAccessLayer.ChatCompletionPresets.BusinessObjects.Format;
using CohesiveRP.Storage.DataAccessLayer.Chats;

namespace CohesiveRP.Core.PromptContext.Builders.Directive
{
    public class PromptContextDirectCharactersInjectionBuilder : IPromptContextElementBuilder
    {
        private IStorageService storageService;
        private PromptContextFormatElement promptContextFormatElement;
        private ChatDbModel chatDbModel;
        private PersonaDbModel personaLinkedToChat;
        private CharacterDbModel[] charactersLinkedToChat;

        public PromptContextDirectCharactersInjectionBuilder(IStorageService storageService, PromptContextFormatElement promptContextFormatElement, ChatDbModel chatDbModel, PersonaDbModel personaLinkedToChat, CharacterDbModel[] charactersLinkedToChat)
        {
            this.storageService = storageService;
            this.promptContextFormatElement = promptContextFormatElement;
            this.chatDbModel = chatDbModel;
            this.personaLinkedToChat = personaLinkedToChat;
            this.charactersLinkedToChat = charactersLinkedToChat;
        }

        public async Task<(string, IShareableContextLink)> BuildAsync()
        {
            if(chatDbModel?.CharacterIds == null || chatDbModel.CharacterIds.Count <= 0)
            {
                return (null, new ShareableContextLink{ LinkedBuilder = this });
            }

            var persona = await storageService.GetPersonaByIdAsync(chatDbModel.PersonaId);
            var characters = await storageService.GetCharactersAsync();
            string str = "";
            foreach (var characterId in chatDbModel.CharacterIds)
            {
                var character = characters.FirstOrDefault(f => f.CharacterId == characterId);

                if(!string.IsNullOrWhiteSpace(character?.Description))
                {
                    str += promptContextFormatElement?.Options?.Format?
                        .Replace("{{character_name}}", character.Name)
                        .Replace("{{character_description}}", character.Description)
                        .InjectMacros(characterName: character.Name)
                        .Trim()
                        .TrimEnd(Environment.NewLine.ToCharArray());
                }
            }

            return ($"{str.ToString().InjectMacros(personaLinkedToChat?.Name)}", new ShareableContextLink { LinkedBuilder = this });
        }
    }
}
