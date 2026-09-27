using CohesiveRP.Common.Diagnostics;
using CohesiveRP.Core.PromptContext.Abstractions;
using CohesiveRP.Core.PromptContext.Utils;
using CohesiveRP.Core.Services;
using CohesiveRP.Storage.DataAccessLayer.ChatCompletionPresets.BusinessObjects.Format;
using CohesiveRP.Storage.DataAccessLayer.Chats;
using CohesiveRP.Storage.DataAccessLayer.Messages;
using CohesiveRP.Storage.DataAccessLayer.Settings;

namespace CohesiveRP.Core.PromptContext.Builders.Directive
{
    public class PromptContextRelevantComputedSummariesOnlyBuilder : IPromptContextElementBuilder
    {
        private IStorageService storageService;
        private PromptContextFormatElement promptContextFormatElement;
        private ChatDbModel chatDbModel;
        private GlobalSettingsDbModel settings;
        private PersonaDbModel personaLinkedToChat;
        private CharacterDbModel[] charactersLinkedToChat;

        public PromptContextRelevantComputedSummariesOnlyBuilder(IStorageService storageService, PromptContextFormatElement promptContextFormatElement, GlobalSettingsDbModel settings, ChatDbModel chatDbModel, PersonaDbModel personaLinkedToChat, CharacterDbModel[] charactersLinkedToChat)
        {
            this.storageService = storageService;
            this.promptContextFormatElement = promptContextFormatElement;
            this.chatDbModel = chatDbModel;
            this.settings = settings;
            this.personaLinkedToChat = personaLinkedToChat;
            this.charactersLinkedToChat = charactersLinkedToChat;
        }

        public async Task<(string, IShareableContextLink)> BuildAsync()
        {
            if (promptContextFormatElement == null || chatDbModel == null)
            {
                LoggingManager.LogToFile("fb791430-ad57-45bd-851d-94ccf7f7b1a6", $"Invalid parameters. ChatId: [{chatDbModel?.ChatId}].");
                return (null, new ShareableContextLink { LinkedBuilder = this });
            }

            SummaryDbModel summaryDbModel = await storageService.GetSummaryAsync(chatDbModel.ChatId);
            if (string.IsNullOrWhiteSpace(summaryDbModel?.RelevantSummaryInformationFromMostRecentStoryContext))
            {
                // We still don't have any summary yet, so we have nothing to add to the prompt atm
                return (null, new ShareableContextLink { LinkedBuilder = this });
            }

            // Inject the computed relevant summaries
            string output = $"<history>{Environment.NewLine}Previous facts, events, speech and actions:{Environment.NewLine}{promptContextFormatElement.Options?.Format?.Replace("{{item_description}}", $"{summaryDbModel.RelevantSummaryInformationFromMostRecentStoryContext}")}{Environment.NewLine}</history>{Environment.NewLine}";
            
            return (output.InjectMacros(personaLinkedToChat?.Name, charactersLinkedToChat?.FirstOrDefault()?.Name),
                    new ShareableContextLink
                    {
                        LinkedBuilder = this,
                        Value = summaryDbModel.ShortTermSummaries.Select(s => s.SummaryEntryId).ToArray()
                    });
        }
    }
}
