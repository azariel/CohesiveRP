using CohesiveRP.Core.PromptContext.Abstractions;
using CohesiveRP.Core.Services;
using CohesiveRP.Storage.DataAccessLayer.ChatCompletionPresets.BusinessObjects.Format;
using CohesiveRP.Storage.DataAccessLayer.Chats;
using CohesiveRP.Storage.DataAccessLayer.Messages;
using CohesiveRP.Storage.DataAccessLayer.Messages.Hot;

namespace CohesiveRP.Core.PromptContext.Builders.Directive
{
    public class PromptContextSceneTrackerInstrBuilder : IPromptContextElementBuilder
    {
        private IStorageService storageService;
        private PromptContextFormatElement promptContextFormatElement;
        private ChatDbModel chatDbModel;

        public PromptContextSceneTrackerInstrBuilder(IStorageService storageService, PromptContextFormatElement promptContextFormatElement, ChatDbModel chatDbModel)
        {
            this.storageService = storageService;
            this.promptContextFormatElement = promptContextFormatElement;
            this.chatDbModel = chatDbModel;
        }

        public async Task<(string, IShareableContextLink)> BuildAsync()
        {
            // Get the messages that should be included in our new sceneTracker generation. Logically, it would be all the messages at the end of the list until we match the latest user message.
            // So, if we're in a group chat and there's 3 messages from 3 characters after the user message, we'll include the last 4 messages
            HotMessagesDbModel hotMessagesDbModel = await storageService.GetAllHotMessagesAsync(chatDbModel.ChatId);

            if(hotMessagesDbModel.Messages.Count <= 0)
            {
                return (null, new ShareableContextLink(){ LinkedBuilder = this });
            }

            hotMessagesDbModel.Messages.Reverse();
            List<IMessageDbModel> messagesToInclude = new();
            foreach (IMessageDbModel message in hotMessagesDbModel.Messages)
            {
                messagesToInclude.Add(message);
                if(message.SourceType == Common.BusinessObjects.MessageSourceType.User)
                {
                    break;
                }
            }

            // Put back in order
            messagesToInclude = messagesToInclude.OrderBy(o=>o.CreatedAtUtc).ToList();

            if(messagesToInclude.Count <= 0)
            {
                return (null, new ShareableContextLink(){ LinkedBuilder = this });
            }

            string sceneTrackerMessagesContent = "";
            foreach (var message in messagesToInclude)
            {
                sceneTrackerMessagesContent += $"- {message.Content}{Environment.NewLine}";
            }

            var lastSceneTracker = await storageService.GetSceneTrackerAsync(chatDbModel.ChatId);
            //if(lastSceneTracker != null && GetNbMessagesFromUserSinceMessageId(lastSceneTracker.LinkMessageId) <= 1)
            //{
                // TODO: if it's stale, what should we do? cut it? may lead to inconsistencies... hm
            //}

            return ($"<scene_tracker>{Environment.NewLine}Details on the current scene in the story{Environment.NewLine}{promptContextFormatElement?.Options?.Format?
                .Replace("{{messages_after_last_scene_tracker}}", sceneTrackerMessagesContent)
                .Replace("{{last_scene_tracker}}", lastSceneTracker?.Content ?? "Empty. Generate a new scene tracker.")}{Environment.NewLine}</scene_tracker>{Environment.NewLine}{Environment.NewLine}",
                new ShareableContextLink
                {
                    LinkedBuilder = this,
                    Value = messagesToInclude.LastOrDefault()?.MessageId
                });
        }
    }
}
