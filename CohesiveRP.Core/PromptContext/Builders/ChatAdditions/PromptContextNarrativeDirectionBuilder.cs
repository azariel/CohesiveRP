using CohesiveRP.Core.PromptContext.Abstractions;
using CohesiveRP.Core.PromptContext.Utils;
using CohesiveRP.Core.Services;
using CohesiveRP.Storage.DataAccessLayer.ChatCompletionPresets.BusinessObjects.Format;
using CohesiveRP.Storage.DataAccessLayer.Chats;

namespace CohesiveRP.Core.PromptContext.Builders.Directive
{
    public class PromptContextNarrativeDirectionBuilder : IPromptContextElementBuilder
    {
        private IStorageService storageService;
        private PromptContextFormatElement promptContextFormatElement;
        private ChatDbModel chatDbModel;
        private PersonaDbModel personaLinkedToChat;
        private CharacterDbModel[] charactersLinkedToChat;

        public PromptContextNarrativeDirectionBuilder(IStorageService storageService, PromptContextFormatElement promptContextFormatElement, ChatDbModel chatDbModel, string linkedMessageId, PersonaDbModel personaLinkedToChat, CharacterDbModel[] charactersLinkedToChat)
        {
            this.storageService = storageService;
            this.promptContextFormatElement = promptContextFormatElement;
            this.chatDbModel = chatDbModel;
            this.personaLinkedToChat = personaLinkedToChat;
            this.charactersLinkedToChat = charactersLinkedToChat;
        }

        public async Task<(string, IShareableContextLink)> BuildAsync()
        {
            var currentValuesFromStorage = await storageService.GetNarrativeDirectionsAsync(s=> s.ChatId == chatDbModel.ChatId);
            var currentValueFromStorage = currentValuesFromStorage?.FirstOrDefault();
            if(currentValueFromStorage == null || string.IsNullOrWhiteSpace(currentValueFromStorage?.Content?.Content))
            {
                return (string.Empty, new ShareableContextLink{ LinkedBuilder = this });
            }

            return ($"{Environment.NewLine}{promptContextFormatElement?.Options?.Format?.InjectMacros(personaLinkedToChat?.Name, charactersLinkedToChat?.FirstOrDefault()?.Name).Replace("{{description}}", currentValueFromStorage.Content?.Content)}", new ShareableContextLink{ LinkedBuilder = this });
        }
    }
}
