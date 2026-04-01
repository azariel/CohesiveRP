using System.Text;
using CohesiveRP.Common.BusinessObjects;
using CohesiveRP.Core.PromptContext.Abstractions;
using CohesiveRP.Core.PromptContext.Utils;
using CohesiveRP.Core.Services;
using CohesiveRP.Storage.DataAccessLayer.ChatCompletionPresets.BusinessObjects.Format;
using CohesiveRP.Storage.DataAccessLayer.Chats;
using CohesiveRP.Storage.DataAccessLayer.Messages;
using CohesiveRP.Storage.DataAccessLayer.Messages.Hot;

namespace CohesiveRP.Core.PromptContext.Builders.Directive
{
    public class PromptContextSceneAnalyzerInstrBuilder : IPromptContextElementBuilder
    {
        private IStorageService storageService;
        private PromptContextFormatElement promptContextFormatElement;
        private ChatDbModel chatDbModel;
        private PersonaDbModel personaLinkedToChat;
        private CharacterDbModel[] charactersLinkedToChat;

        public PromptContextSceneAnalyzerInstrBuilder(IStorageService storageService, PromptContextFormatElement promptContextFormatElement, ChatDbModel chatDbModel, PersonaDbModel personaLinkedToChat, CharacterDbModel[] charactersLinkedToChat)
        {
            this.storageService = storageService;
            this.promptContextFormatElement = promptContextFormatElement;
            this.chatDbModel = chatDbModel;
            this.personaLinkedToChat = personaLinkedToChat;
            this.charactersLinkedToChat = charactersLinkedToChat;
        }

        public async Task<(string, IShareableContextLink)> BuildAsync()
        {
            // Get the messages that should be included in our sceneAnalyze generation. Logically, it would be all the messages at the end of the list until we match the latest user message.
            // So, if we're in a group chat and there's 3 messages from 3 characters after the user message, we'll include the last 4 messages
            HotMessagesDbModel hotMessagesDbModel = await storageService.GetAllHotMessagesAsync(chatDbModel.ChatId);

            if (hotMessagesDbModel.Messages.Count <= 0)
            {
                return (null, new ShareableContextLink() { LinkedBuilder = this });
            }

            hotMessagesDbModel.Messages = hotMessagesDbModel.Messages.OrderByDescending(o => o.CreatedAtUtc).ToList();
            List<IMessageDbModel> messagesToIncludeInSceneToAnalyze = new();
            for (int i = 0; i < hotMessagesDbModel.Messages.Count; i++)
            {
                var message = (IMessageDbModel)hotMessagesDbModel.Messages[i];
                messagesToIncludeInSceneToAnalyze.Add(message);
                if (i > 0 && message.SourceType == MessageSourceType.User)
                {
                    break;
                }
            }

            // Put back in order
            messagesToIncludeInSceneToAnalyze = messagesToIncludeInSceneToAnalyze.OrderBy(o => o.CreatedAtUtc).ToList();

            if (messagesToIncludeInSceneToAnalyze.Count <= 0)
            {
                return (null, new ShareableContextLink() { LinkedBuilder = this });
            }

            List<IMessageDbModel> messagesToIncludeInPreviousScene = new();
            for (int i = 0; i < hotMessagesDbModel.Messages.Count; i++)
            {
                var message = (IMessageDbModel)hotMessagesDbModel.Messages[i];
                if (message == null || message.Summarized)
                    break;

                messagesToIncludeInPreviousScene.Add(message);
            }

            // Put back in order
            messagesToIncludeInPreviousScene = messagesToIncludeInPreviousScene.OrderBy(o => o.CreatedAtUtc).ToList();

            // Finally, remove the one intersecting with the messages to include in scene to analyze
            messagesToIncludeInPreviousScene = messagesToIncludeInPreviousScene.Except(messagesToIncludeInSceneToAnalyze).ToList();

            StringBuilder str = new();
            if (messagesToIncludeInPreviousScene != null && messagesToIncludeInPreviousScene.Count > 0)
            {
                str.AppendLine($"<informationOnPreviousSceneForContext>");

                foreach (IMessageDbModel message in messagesToIncludeInPreviousScene)
                {
                    str.AppendLine($"{message.SourceType}:{Environment.NewLine}<message>{message.Content}</message>{Environment.NewLine}");
                }

                str.AppendLine($"</informationOnPreviousSceneForContext>{Environment.NewLine}");
            }

            if (messagesToIncludeInSceneToAnalyze == null || messagesToIncludeInSceneToAnalyze.Count <= 0)
            {
                return (null, new ShareableContextLink() { LinkedBuilder = this });
            }

            str.AppendLine($"<informationOnCurrentScene>");

            foreach (IMessageDbModel message in messagesToIncludeInSceneToAnalyze)
            {
                str.AppendLine($"{message.SourceType}:{Environment.NewLine}<message>{message.Content}</message>{Environment.NewLine}");
            }

            str.AppendLine($"</informationOnCurrentScene>{Environment.NewLine}");

            var persona = await storageService.GetPersonaByIdAsync(chatDbModel.PersonaId);
            return ($"{str.ToString().InjectMacros(personaLinkedToChat?.Name, charactersLinkedToChat?.FirstOrDefault()?.Name)}"
                .Replace(Constants.USER_PLACEHOLDER, persona?.Name ?? "User")
                .Trim(),
                new ShareableContextLink
                {
                    LinkedBuilder = this,
                    Value = messagesToIncludeInSceneToAnalyze.LastOrDefault()?.MessageId
                });
        }
    }
}
