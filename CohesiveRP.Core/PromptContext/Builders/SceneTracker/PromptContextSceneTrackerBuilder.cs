using CohesiveRP.Core.PromptContext.Abstractions;
using CohesiveRP.Core.Services;
using CohesiveRP.Storage.DataAccessLayer.ChatCompletionPresets.BusinessObjects.Format;
using CohesiveRP.Storage.DataAccessLayer.Chats;

namespace CohesiveRP.Core.PromptContext.Builders.Directive
{
    public class PromptContextSceneTrackerBuilder : IPromptContextElementBuilder
    {
        private IStorageService storageService;
        private PromptContextFormatElement promptContextFormatElement;
        private ChatDbModel chatDbModel;

        public PromptContextSceneTrackerBuilder(IStorageService storageService, PromptContextFormatElement promptContextFormatElement, ChatDbModel chatDbModel)
        {
            this.storageService = storageService;
            this.promptContextFormatElement = promptContextFormatElement;
            this.chatDbModel = chatDbModel;
        }

        public async Task<(string, IShareableContextLink)> BuildAsync()
        {
            string sceneTrackerContent = "";//TODO: fetch from Db

            if(string.IsNullOrWhiteSpace(sceneTrackerContent))
            {
                return (string.Empty, new ShareableContextLink { LinkedBuilder = this });
            }

            return ($"# Scene Tracker (details on current scene){Environment.NewLine}{promptContextFormatElement?.Options?.Format?.Replace("{{item_description}}", sceneTrackerContent)}", new ShareableContextLink { LinkedBuilder = this });
        }
    }
}
