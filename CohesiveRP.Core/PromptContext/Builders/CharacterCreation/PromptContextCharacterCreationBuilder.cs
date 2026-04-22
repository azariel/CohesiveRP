using CohesiveRP.Core.PromptContext.Abstractions;
using CohesiveRP.Core.PromptContext.Utils;
using CohesiveRP.Core.Services;
using CohesiveRP.Storage.DataAccessLayer.ChatCompletionPresets.BusinessObjects.Format;
using CohesiveRP.Storage.DataAccessLayer.Chats;

namespace CohesiveRP.Core.PromptContext.Builders.Directive
{
    public class PromptContextCharacterCreationBuilder : IPromptContextElementBuilder
    {
        private IStorageService storageService;
        private PromptContextFormatElement promptContextFormatElement;
        private ChatDbModel chatDbModel;
        private PersonaDbModel personaLinkedToChat;
        private CharacterDbModel[] charactersLinkedToChat;
        private string contextLinkedId;

        public PromptContextCharacterCreationBuilder(IStorageService storageService, PromptContextFormatElement promptContextFormatElement, ChatDbModel chatDbModel, string contextLinkedId, PersonaDbModel personaLinkedToChat, CharacterDbModel[] charactersLinkedToChat)
        {
            this.storageService = storageService;
            this.promptContextFormatElement = promptContextFormatElement;
            this.chatDbModel = chatDbModel;
            this.personaLinkedToChat = personaLinkedToChat;
            this.charactersLinkedToChat = charactersLinkedToChat;
            this.contextLinkedId = contextLinkedId;
        }

        public async Task<(string, IShareableContextLink)> BuildAsync()
        {
            // Get the interactiveQuery associated with our backgroundQuery
            var interactiveUserInputQuery = await storageService.GetInteractiveUserInputQueriesAsync(g => g.ChatId == chatDbModel.ChatId && g.InteractiveUserInputQueryId == contextLinkedId);

            if (interactiveUserInputQuery == null || interactiveUserInputQuery.Length <= 0)
            {
                return ("", new ShareableContextLink { LinkedBuilder = this });
            }

            return ($"{promptContextFormatElement?.Options?.Format?
                .Replace("{{character_name}}", interactiveUserInputQuery.First().Metadata?.Trim())
                .InjectMacros(personaLinkedToChat?.Name, charactersLinkedToChat?.FirstOrDefault()?.Name)}{Environment.NewLine}",
                new ShareableContextLink { LinkedBuilder = this });
        }
    }
}
