using CohesiveRP.Core.PromptContext.Abstractions;
using CohesiveRP.Core.PromptContext.Utils;
using CohesiveRP.Core.Services;
using CohesiveRP.Storage.DataAccessLayer.ChatCompletionPresets.BusinessObjects.Format;
using CohesiveRP.Storage.DataAccessLayer.Chats;

namespace CohesiveRP.Core.PromptContext.Builders.Directive
{
    public class PromptContextWorldBuilder : IPromptContextElementBuilder
    {
        private IStorageService storageService;
        private PromptContextFormatElement promptContextFormatElement;
        private ChatDbModel chatDbModel;
        private PersonaDbModel personaLinkedToChat;
        private CharacterDbModel[] charactersLinkedToChat;

        public PromptContextWorldBuilder(IStorageService storageService, PromptContextFormatElement promptContextFormatElement, ChatDbModel chatDbModel, PersonaDbModel personaLinkedToChat, CharacterDbModel[] charactersLinkedToChat)
        {
            this.storageService = storageService;
            this.promptContextFormatElement = promptContextFormatElement;
            this.chatDbModel = chatDbModel;
            this.personaLinkedToChat = personaLinkedToChat;
            this.charactersLinkedToChat = charactersLinkedToChat;
        }

        public async Task<(string, IShareableContextLink)> BuildAsync()
        {
            string worldTitle = $"";//TODO: fetch from Db
            string worldContent = $"Infer the world from the roleplay context.";//TODO: fetch from Db

            PersonaDbModel defaultPersona = await storageService.GetPersonaByIdAsync(chatDbModel?.PersonaId);
            string userPersonaName = defaultPersona.Name;

            if (string.IsNullOrWhiteSpace(worldContent))
            {
                worldContent = $"Infer the relevant characters from the roleplay context.{Environment.NewLine}{Environment.NewLine}";
            }

            return ($"<world>{Environment.NewLine}{promptContextFormatElement?.Options?.Format?
                .Replace("{{item_description}}", worldContent)
                .Replace("{{item_header}}", worldTitle)
                .InjectMacros(personaLinkedToChat?.Name, charactersLinkedToChat?.FirstOrDefault()?.Name)}{Environment.NewLine}</world>{Environment.NewLine}{Environment.NewLine}",
                new ShareableContextLink{ LinkedBuilder = this });
        }
    }
}
