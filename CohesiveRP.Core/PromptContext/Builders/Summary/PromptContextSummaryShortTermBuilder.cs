using CohesiveRP.Core.Services;
using CohesiveRP.Storage.DataAccessLayer.ChatCompletionPresets.BusinessObjects.Format;
using CohesiveRP.Storage.DataAccessLayer.ChatCompletionPresets.BusinessObjects.Settings;
using CohesiveRP.Storage.DataAccessLayer.Chats;

namespace CohesiveRP.Core.PromptContext.Builders.Directive
{
    public class PromptContextSummaryShortTermBuilder : IPromptContextElementBuilder
    {
        private IStorageService storageService;
        private PromptContextFormatElement promptContextFormatElement;
        private ChatDbModel chatDbModel;
        private PromptContextSettings settings;

        public PromptContextSummaryShortTermBuilder(IStorageService storageService, PromptContextFormatElement promptContextFormatElement, PromptContextSettings settings, ChatDbModel chatDbModel)
        {
            this.storageService = storageService;
            this.promptContextFormatElement = promptContextFormatElement;
            this.chatDbModel = chatDbModel;
            this.settings = settings;
        }

        public async Task<string> BuildAsync()
        {



            //string ShortTermMemoryContent = "";//TODO: fetch from Db

            //if (string.IsNullOrWhiteSpace(ShortTermMemoryContent))
            //{
            //    return string.Empty;
            //}

            //return $"# Short-term memory{Environment.NewLine}{promptContextFormatElement?.Options?.Format?.Replace("{{item_description}}", ShortTermMemoryContent)}";
            return null;
        }
    }
}
