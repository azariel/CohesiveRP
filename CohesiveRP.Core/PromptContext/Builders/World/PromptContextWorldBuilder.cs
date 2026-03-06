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

        public PromptContextWorldBuilder(IStorageService storageService, PromptContextFormatElement promptContextFormatElement, ChatDbModel chatDbModel)
        {
            this.storageService = storageService;
            this.promptContextFormatElement = promptContextFormatElement;
            this.chatDbModel = chatDbModel;
        }

        public async Task<string> BuildAsync()
        {
            string worldContent = $"Infer the world from the roleplay context.";//TODO: fetch from Db
            string userPersonaName = "userName";// TODO: fetch from Db

            if (string.IsNullOrWhiteSpace(worldContent))
            {
                worldContent = $"Infer the relevant characters from the roleplay context.{Environment.NewLine}{Environment.NewLine}";
            }

            return $"# World{Environment.NewLine}{promptContextFormatElement?.Options?.Format?
                .Replace("{{item_description}}", worldContent)
                .Replace("{{user}}", userPersonaName)}";
        }
    }
}
