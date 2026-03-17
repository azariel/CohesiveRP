using System.Text;
using CohesiveRP.Core.PromptContext.Abstractions;
using CohesiveRP.Core.Services;
using CohesiveRP.Storage.DataAccessLayer.ChatCompletionPresets.BusinessObjects.Format;
using CohesiveRP.Storage.DataAccessLayer.Chats;

namespace CohesiveRP.Core.PromptContext.Builders.Directive
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

        public async Task<(string, IShareableContextLink)> BuildAsync()
        {
            Dictionary<string, string> loreByQueryContent = [];
            PersonaDbModel linkedPersona = await storageService.GetPersonaByIdAsync(chatDbModel?.PersonaId);
            string userPersonaName = linkedPersona.Name;

            StringBuilder str = new();

            //Start with the player persona
            string value = promptContextFormatElement?.Options?.Format?
                .Replace("{{item_header}}", userPersonaName)
                .Replace("{{item_description}}", linkedPersona.Description)
                .Replace(Constants.USER_PLACEHOLDER, userPersonaName);

            if (value != null)
            {
                str.Append(value);
            }

            // TODO: add characters

            return ($"# Relevant Characters{Environment.NewLine}{str}", new ShareableContextLink { LinkedBuilder = this });
        }
}
}
