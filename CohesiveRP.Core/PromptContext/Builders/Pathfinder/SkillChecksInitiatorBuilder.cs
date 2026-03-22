using CohesiveRP.Core.PromptContext.Abstractions;
using CohesiveRP.Core.Services;
using CohesiveRP.Storage.DataAccessLayer.ChatCompletionPresets.BusinessObjects.Format;
using CohesiveRP.Storage.DataAccessLayer.Chats;
using CohesiveRP.Storage.DataAccessLayer.Messages;

namespace CohesiveRP.Core.PromptContext.Builders.Pathfinder
{
    public class SkillChecksInitiatorBuilder : IPromptContextElementBuilder
    {
        private IStorageService storageService;
        private PromptContextFormatElement promptContextFormatElement;
        private ChatDbModel chatDbModel;

        public SkillChecksInitiatorBuilder(IStorageService storageService, PromptContextFormatElement promptContextFormatElement, ChatDbModel chatDbModel)
        {
            this.storageService = storageService;
            this.promptContextFormatElement = promptContextFormatElement;
            this.chatDbModel = chatDbModel;
        }

        public async Task<(string, IShareableContextLink)> BuildAsync()
        {
            var hotMessagesObj = await storageService.GetAllHotMessagesAsync(chatDbModel.ChatId);

            if (hotMessagesObj?.Messages == null || hotMessagesObj.Messages.Count < 2)
            {
                return (null, new ShareableContextLink { LinkedBuilder = this, });
            }

            MessageDbModel[] hotMessages = hotMessagesObj.Messages.OrderByDescending(o => o.CreatedAtUtc).ToArray();
            var hotMessagesBesideLastOne = hotMessages.Skip(1).ToArray();
            int nbMessagesGeneralContext = hotMessages.IndexOf(hotMessagesBesideLastOne.FirstOrDefault(f => f.SourceType == Common.BusinessObjects.MessageSourceType.User));
            int nbMessagesRequest = Math.Max(1, nbMessagesGeneralContext);
            nbMessagesGeneralContext = Math.Max(5, nbMessagesGeneralContext);

            MessageDbModel[] LastXMessagesforGeneralContext = [.. hotMessagesBesideLastOne.Take(nbMessagesGeneralContext)];
            string contextOnScene = string.Join($"{Environment.NewLine}", LastXMessagesforGeneralContext.OrderBy(o => o.CreatedAtUtc).Select(s => $"<message>{s.Content}</message>"));

            MessageDbModel[] LastXMessagesforRequest = [..hotMessages.Take(nbMessagesRequest)];
            string lastMessage = string.Join($"{Environment.NewLine}", LastXMessagesforRequest.OrderBy(o => o.CreatedAtUtc).Select(s => $"<message>{s.Content}</message>"));
            return ($"# Story Scene{Environment.NewLine}{promptContextFormatElement?.Options?.Format?
                .Replace("{{messages_for_context_on_scene}}", contextOnScene)
                .Replace("{{scene_to_categorize}}", lastMessage)}",
                new ShareableContextLink
                {
                    LinkedBuilder = this,
                });
        }
    }
}
