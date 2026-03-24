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
            var lastSceneTracker = await storageService.GetSceneTrackerAsync(chatDbModel.ChatId);

            if (string.IsNullOrWhiteSpace(lastSceneTracker?.Content))
            {
                return (null, new ShareableContextLink { LinkedBuilder = this, });
            }

            return ($"<scene_tracker>{Environment.NewLine}Details on current scene{Environment.NewLine}{promptContextFormatElement?.Options?.Format?
                .Replace("{{item_description}}", lastSceneTracker.Content)}{Environment.NewLine}</scene_tracker>{Environment.NewLine}{Environment.NewLine}",
                new ShareableContextLink
                {
                    LinkedBuilder = this,
                });
        }
    }
}
