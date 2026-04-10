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
    public class PromptContextSummaryMediumTermBuilder : IPromptContextElementBuilder
    {
        private IStorageService storageService;
        private PromptContextFormatElement promptContextFormatElement;
        private ChatDbModel chatDbModel;
        private GlobalSettingsDbModel settings;
        private PersonaDbModel personaLinkedToChat;
        private CharacterDbModel[] charactersLinkedToChat;

        public PromptContextSummaryMediumTermBuilder(IStorageService storageService, PromptContextFormatElement promptContextFormatElement, GlobalSettingsDbModel settings, ChatDbModel chatDbModel, PersonaDbModel personaLinkedToChat, CharacterDbModel[] charactersLinkedToChat)
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
                LoggingManager.LogToFile("524573ce-fbf3-4b35-96de-f94ee0d000d7", $"Invalid parameters. ChatId: [{chatDbModel?.ChatId}].");
                return (null, new ShareableContextLink { LinkedBuilder = this });
            }

            SummaryDbModel summaryDbModel = await storageService.GetSummaryAsync(chatDbModel.ChatId);
            if (summaryDbModel?.MediumTermSummaries == null)
            {
                // We still don't have any summary yet, so we have nothing to add to the prompt atm
                return (null, new ShareableContextLink { LinkedBuilder = this });
            }

            // Inject that short term summary
            string output = $"<summary_medium_term>{Environment.NewLine}Previous facts, events, speech and actions (Medium-Term){Environment.NewLine}";
            foreach (ISummaryEntryDbModel summaryElement in summaryDbModel.MediumTermSummaries)
            {
                // TODO: add a notion of time?
                string value = $"{promptContextFormatElement.Options?.Format?.Replace("{{item_description}}", $"{summaryElement.Content}")}";
                output += value;
            }

            output += $"{Environment.NewLine}</summary_medium_term>{Environment.NewLine}";
            return (output.InjectMacros(personaLinkedToChat?.Name, charactersLinkedToChat?.FirstOrDefault()?.Name),
                    new ShareableContextLink
                    {
                        LinkedBuilder = this,
                        Value = summaryDbModel.MediumTermSummaries.Select(s => s.SummaryEntryId).ToArray()
                    });
        }
    }
}
