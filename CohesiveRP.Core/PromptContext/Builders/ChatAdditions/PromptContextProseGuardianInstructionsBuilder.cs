using CohesiveRP.Core.PromptContext.Abstractions;
using CohesiveRP.Core.PromptContext.Utils;
using CohesiveRP.Core.Services;
using CohesiveRP.Storage.DataAccessLayer.ChatCompletionPresets.BusinessObjects.Format;
using CohesiveRP.Storage.DataAccessLayer.Chats;
using CohesiveRP.Storage.DataAccessLayer.Messages;
using CohesiveRP.Storage.DataAccessLayer.Messages.Hot;

namespace CohesiveRP.Core.PromptContext.Builders.Directive
{
    public class PromptContextProseGuardianInstructionsBuilder : IPromptContextElementBuilder
    {
        private const int NB_RECENT_MESSAGES_TO_INCLUDE = 5;

        private IStorageService storageService;
        private PromptContextFormatElement promptContextFormatElement;
        private ChatDbModel chatDbModel;
        private PersonaDbModel personaLinkedToChat;
        private CharacterDbModel[] charactersLinkedToChat;

        public PromptContextProseGuardianInstructionsBuilder(IStorageService storageService, PromptContextFormatElement promptContextFormatElement, ChatDbModel chatDbModel, string linkedMessageId, PersonaDbModel personaLinkedToChat, CharacterDbModel[] charactersLinkedToChat)
        {
            this.storageService = storageService;
            this.promptContextFormatElement = promptContextFormatElement;
            this.chatDbModel = chatDbModel;
            this.personaLinkedToChat = personaLinkedToChat;
            this.charactersLinkedToChat = charactersLinkedToChat;
        }

        public async Task<(string, IShareableContextLink)> BuildAsync()
        {
            var currentValuesFromStorage = await storageService.GetProseGuardiansAsync(s => s.ChatId == chatDbModel.ChatId);
            var currentValueFromStorage = currentValuesFromStorage?.FirstOrDefault();
            if (currentValueFromStorage == null || string.IsNullOrWhiteSpace(currentValueFromStorage?.Content?.Content))
            {
                return (string.Empty, new ShareableContextLink { LinkedBuilder = this });
            }

            string recentMessagesContent = await GetRecentMessagesContentAsync();

            string formatted = promptContextFormatElement?.Options?.Format?
                .InjectMacros(personaLinkedToChat?.Name, charactersLinkedToChat?.FirstOrDefault()?.Name)
                .Replace("{{description}}", currentValueFromStorage.Content?.Content)
                .Replace("{{recent_messages}}", recentMessagesContent);

            return ($"{Environment.NewLine}{formatted}", new ShareableContextLink { LinkedBuilder = this });
        }

        private async Task<string> GetRecentMessagesContentAsync()
        {
            HotMessagesDbModel hotMessagesDbModel = await storageService.GetAllHotMessagesAsync(chatDbModel.ChatId);
            List<IMessageDbModel> orderedMessages = hotMessagesDbModel?.Messages?
                .Cast<IMessageDbModel>()
                .OrderBy(o => o.CreatedAtUtc)
                .ToList() ?? new();

            List<IMessageDbModel> lastMessages = orderedMessages.TakeLast(NB_RECENT_MESSAGES_TO_INCLUDE).ToList();

            string messagesContent = $"<last_messages>{Environment.NewLine}";
            foreach (var message in lastMessages)
            {
                messagesContent += $"{message.SourceType}:{Environment.NewLine}<message>{message.Content}</message>{Environment.NewLine}";
            }

            messagesContent = messagesContent.Trim().TrimEnd(Environment.NewLine.ToCharArray());
            messagesContent += $"{Environment.NewLine}</last_messages>";

            return messagesContent;
        }
    }
}