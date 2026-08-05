using System.Text;
using CohesiveRP.Common.Serialization;
using CohesiveRP.Core.LLMProviderProcessors.ChatAdditions.BusinessObjects.NarrativeArchitecture;
using CohesiveRP.Core.PromptContext.Abstractions;
using CohesiveRP.Core.PromptContext.Utils;
using CohesiveRP.Core.Services;
using CohesiveRP.Storage.DataAccessLayer.ChatCompletionPresets.BusinessObjects.Format;
using CohesiveRP.Storage.DataAccessLayer.Chats;

namespace CohesiveRP.Core.PromptContext.Builders.Directive
{
    public class PromptContextNarrativeArchitectureBuilder : IPromptContextElementBuilder
    {
        private IStorageService storageService;
        private PromptContextFormatElement promptContextFormatElement;
        private ChatDbModel chatDbModel;
        private PersonaDbModel personaLinkedToChat;
        private CharacterDbModel[] charactersLinkedToChat;

        public PromptContextNarrativeArchitectureBuilder(IStorageService storageService, PromptContextFormatElement promptContextFormatElement, ChatDbModel chatDbModel, string linkedMessageId, PersonaDbModel personaLinkedToChat, CharacterDbModel[] charactersLinkedToChat)
        {
            this.storageService = storageService;
            this.promptContextFormatElement = promptContextFormatElement;
            this.chatDbModel = chatDbModel;
            this.personaLinkedToChat = personaLinkedToChat;
            this.charactersLinkedToChat = charactersLinkedToChat;
        }

        public async Task<(string, IShareableContextLink)> BuildAsync()
        {
            var currentValuesFromStorage = await storageService.GetNarrativeArchitecturesAsync(s => s.ChatId == chatDbModel.ChatId);
            var currentValueFromStorage = currentValuesFromStorage?.FirstOrDefault();
            if (currentValueFromStorage == null || string.IsNullOrWhiteSpace(currentValueFromStorage?.Content?.Content))
            {
                return (string.Empty, new ShareableContextLink { LinkedBuilder = this });
            }

            NarrativeArchitectureResult narrativeArchitecture = null;

            try
            {
                narrativeArchitecture = JsonCommonSerializer.DeserializeFromString<NarrativeArchitectureResult>(currentValueFromStorage.Content.Content);
            } catch (Exception)
            {
                // Ignore
            }

            if (narrativeArchitecture == null)
            {
                return (string.Empty, new ShareableContextLink { LinkedBuilder = this });
            }

            StringBuilder str = new();
            if (narrativeArchitecture.OverarchingArc != null)
            {
                if (!string.IsNullOrWhiteSpace(narrativeArchitecture.OverarchingArc.Description))
                {
                    str.AppendLine($"<overarchingArc>{narrativeArchitecture.OverarchingArc.Description}</overarchingArc>");
                }

                if (!string.IsNullOrWhiteSpace(narrativeArchitecture.OverarchingArc.ProtagonistArc))
                {
                    str.AppendLine($"<protagonistArc>{narrativeArchitecture.OverarchingArc.ProtagonistArc}</protagonistArc>");
                }
            }

            if (narrativeArchitecture.SceneDirections != null && narrativeArchitecture.SceneDirections.Count > 0)
            {
                foreach (var sceneDirection in narrativeArchitecture.SceneDirections)
                {
                    if (!string.IsNullOrWhiteSpace(sceneDirection.Direction))
                    {
                        str.AppendLine($"<sceneDirection>{sceneDirection.Direction}</sceneDirection>");
                    }
                }
            }

            if (!string.IsNullOrWhiteSpace(narrativeArchitecture.Pacing))
            {
                str.AppendLine($"<pacing>{narrativeArchitecture.Pacing}</pacing>");
            }

            return ($"{Environment.NewLine}{promptContextFormatElement?.Options?.Format?.InjectMacros(personaLinkedToChat?.Name, charactersLinkedToChat?.FirstOrDefault()?.Name).Replace("{{description}}", str.ToString())}", new ShareableContextLink { LinkedBuilder = this });
        }
    }
}
