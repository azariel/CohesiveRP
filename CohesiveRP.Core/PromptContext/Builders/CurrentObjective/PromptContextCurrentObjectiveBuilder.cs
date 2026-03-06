using CohesiveRP.Core.Services;
using CohesiveRP.Storage.DataAccessLayer.ChatCompletionPresets.BusinessObjects.Format;
using CohesiveRP.Storage.DataAccessLayer.Chats;

namespace CohesiveRP.Core.PromptContext.Builders.Directive
{
    public class PromptContextCurrentObjectiveBuilder : IPromptContextElementBuilder
    {
        private IStorageService storageService;
        private PromptContextFormatElement promptContextFormatElement;
        private ChatDbModel chatDbModel;

        public PromptContextCurrentObjectiveBuilder(IStorageService storageService, PromptContextFormatElement promptContextFormatElement, ChatDbModel chatDbModel)
        {
            this.storageService = storageService;
            this.promptContextFormatElement = promptContextFormatElement;
            this.chatDbModel = chatDbModel;
        }

        public async Task<string> BuildAsync()
        {
            string currentObjectiveContent = "";//TODO: fetch from Db

            if(string.IsNullOrWhiteSpace(currentObjectiveContent))
            {
                return string.Empty;
            }

            return $"# Current Objective (story progression objective){Environment.NewLine}{promptContextFormatElement?.Options?.Format?.Replace("{{item_description}}", currentObjectiveContent)}";
        }
    }
}
