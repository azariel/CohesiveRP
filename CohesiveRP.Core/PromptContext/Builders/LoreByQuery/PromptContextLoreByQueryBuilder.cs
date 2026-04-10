using System.Text;
using CohesiveRP.Core.PromptContext.Abstractions;
using CohesiveRP.Core.PromptContext.Utils;
using CohesiveRP.Core.Services;
using CohesiveRP.Storage.DataAccessLayer.ChatCompletionPresets.BusinessObjects.Format;
using CohesiveRP.Storage.DataAccessLayer.Chats;

namespace CohesiveRP.Core.PromptContext.Builders.Directive
{
    public class PromptContextLoreByQueryBuilder : IPromptContextElementBuilder
    {
        private IStorageService storageService;
        private PromptContextFormatElement promptContextFormatElement;
        private ChatDbModel chatDbModel;
        private PersonaDbModel personaLinkedToChat;
        private CharacterDbModel[] charactersLinkedToChat;

        public PromptContextLoreByQueryBuilder(IStorageService storageService, PromptContextFormatElement promptContextFormatElement, ChatDbModel chatDbModel, PersonaDbModel personaLinkedToChat, CharacterDbModel[] charactersLinkedToChat)
        {
            this.storageService = storageService;
            this.promptContextFormatElement = promptContextFormatElement;
            this.chatDbModel = chatDbModel;
            this.personaLinkedToChat = personaLinkedToChat;
            this.charactersLinkedToChat = charactersLinkedToChat;
        }

        public async Task<(string, IShareableContextLink)> BuildAsync()
        {
            Dictionary<string, string> loreByQueryContent = [];
            PersonaDbModel defaultPersona = await storageService.GetPersonaByIdAsync(chatDbModel?.PersonaId);
            string userPersonaName = defaultPersona.Name;

            //TODO: fetch from Db
            //loreByQueryContent.Add("le chateau de montenac", "c'est un beau château");
            //loreByQueryContent.Add("loreByQueryContentKey2", "loreByQueryContentValue2");
            //loreByQueryContent.Add("loreByQueryContentKey3", "loreByQueryContentValue3");

            StringBuilder str = new();
            foreach (var loreQueryContent in loreByQueryContent)
            {
                string value = promptContextFormatElement?.Options?.Format?
                    .Replace("{{item_header}}", loreQueryContent.Key)
                    .Replace("{{item_description}}", loreQueryContent.Key)
                    .Replace(Constants.USER_PLACEHOLDER, userPersonaName);

                if(value != null)
                {
                    str.Append(value);
                }
            }

            if (loreByQueryContent.Count <= 0)
            {
                str.Append($"Infer the lore from the roleplay context.");
            }

            return ($"<lore>{Environment.NewLine}Relevant lore in recent story context{Environment.NewLine}{str.ToString().InjectMacros(personaLinkedToChat?.Name, charactersLinkedToChat?.FirstOrDefault()?.Name)}{Environment.NewLine}</lore>{Environment.NewLine}{Environment.NewLine}", new ShareableContextLink{ LinkedBuilder = this });
        }
    }
}
