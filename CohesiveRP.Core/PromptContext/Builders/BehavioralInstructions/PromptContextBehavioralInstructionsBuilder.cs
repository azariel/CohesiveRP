using CohesiveRP.Core.PromptContext.Abstractions;
using CohesiveRP.Core.Services;
using CohesiveRP.Storage.DataAccessLayer.ChatCompletionPresets.BusinessObjects.Format;
using CohesiveRP.Storage.DataAccessLayer.Chats;

namespace CohesiveRP.Core.PromptContext.Builders.Directive
{
    public class PromptContextBehavioralInstructionsBuilder : IPromptContextElementBuilder
    {
        private IStorageService storageService;
        private PromptContextFormatElement promptContextFormatElement;
        private ChatDbModel chatDbModel;

        public PromptContextBehavioralInstructionsBuilder(IStorageService storageService, PromptContextFormatElement promptContextFormatElement, ChatDbModel chatDbModel)
        {
            this.storageService = storageService;
            this.promptContextFormatElement = promptContextFormatElement;
            this.chatDbModel = chatDbModel;
        }

        public async Task<(string, IShareableContextLink)> BuildAsync()
        {
            PersonaDbModel defaultPersona = await storageService.GetPersonaByIdAsync(chatDbModel?.PersonaId);
            string userPersonaName = defaultPersona.Name;

            return ($"# Your Behavioral Instruction{Environment.NewLine}{promptContextFormatElement?.Options?.Format?
                //.Replace("{{item_description}}", behaviorInstructionsContent)
                .Replace(Constants.USER_PLACEHOLDER, userPersonaName)}", new ShareableContextLink{ LinkedBuilder = this });
        }
    }
}
