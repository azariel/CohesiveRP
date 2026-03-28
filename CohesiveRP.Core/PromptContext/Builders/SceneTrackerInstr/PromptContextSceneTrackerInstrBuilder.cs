using System.Text;
using CohesiveRP.Common.BusinessObjects;
using CohesiveRP.Core.CharacterCards.Loaders.CohesiveRPv1.BusinessObjects;
using CohesiveRP.Core.PromptContext.Abstractions;
using CohesiveRP.Core.Services;
using CohesiveRP.Storage.DataAccessLayer.ChatCompletionPresets.BusinessObjects.Format;
using CohesiveRP.Storage.DataAccessLayer.Chats;
using CohesiveRP.Storage.DataAccessLayer.Messages;
using CohesiveRP.Storage.DataAccessLayer.Messages.Hot;
using CohesiveRP.Storage.DataAccessLayer.SceneAnalyzer.BusinessObjects.CharacterAnalyze;

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

            if (hotMessagesDbModel.Messages.Count <= 0)
            {
                return (null, new ShareableContextLink() { LinkedBuilder = this });
            }

            hotMessagesDbModel.Messages = hotMessagesDbModel.Messages.OrderByDescending(o => o.CreatedAtUtc).ToList();
            List<IMessageDbModel> messagesToInclude = new();
            for (int i = 0; i < hotMessagesDbModel.Messages.Count; i++)
            {
                var message = (IMessageDbModel)hotMessagesDbModel.Messages[i];
                messagesToInclude.Add(message);
                if (i > 0 && message.SourceType == MessageSourceType.User)
                {
                    break;
                }
            }

            // Put back in order
            messagesToInclude = messagesToInclude.OrderBy(o => o.CreatedAtUtc).ToList();

            if (messagesToInclude.Count <= 0)
            {
                return (null, new ShareableContextLink() { LinkedBuilder = this });
            }

            string sceneTrackerMessagesContent = "";
            foreach (var message in messagesToInclude)
            {
                sceneTrackerMessagesContent += $"- {message.Content}{Environment.NewLine}";
            }

            // TODO: if it's stale, what should we do? cut it? may lead to inconsistencies... hm
            var lastSceneTracker = await storageService.GetSceneTrackerAsync(chatDbModel.ChatId);

            // Also inject the Scene Analyzer relevant information
            var sceneAnalysis = await storageService.GetSceneAnalyzerAsync(chatDbModel.ChatId);
            StringBuilder sceneAnalysisInjection = new();
            GenerateSceneAnalysisInjection(sceneAnalysis, sceneAnalysisInjection);

            return ($"<last_scene_analysis>{Environment.NewLine}{sceneAnalysisInjection}{Environment.NewLine}</last_scene_analysis>{Environment.NewLine}{Environment.NewLine}<scene_tracker>{Environment.NewLine}Details on the current scene in the story{Environment.NewLine}{promptContextFormatElement?.Options?.Format?
                .Replace("{{messages_after_last_scene_tracker}}", sceneTrackerMessagesContent)
                .Replace("{{last_scene_tracker}}", lastSceneTracker?.Content ?? "Empty. Generate a new scene tracker.")}{Environment.NewLine}</scene_tracker>{Environment.NewLine}{Environment.NewLine}",
                new ShareableContextLink
                {
                    LinkedBuilder = this,
                    Value = messagesToInclude.LastOrDefault()?.MessageId
                });
        }

        private static void GenerateSceneAnalysisInjection(SceneAnalyzerDbModel sceneAnalysis, StringBuilder sceneAnalysisInjection)
        {
            // Themes
            if (!string.IsNullOrWhiteSpace(sceneAnalysis?.SceneCategory?.MainThemes))
            {
                StringBuilder strSceneCategorization = new();
                strSceneCategorization.AppendLine($"<scene_themes>");
                strSceneCategorization.AppendLine($"<main_topics>");
                strSceneCategorization.AppendLine(sceneAnalysis.SceneCategory.MainThemes);
                strSceneCategorization.AppendLine($"</main_topics>");

                if (!string.IsNullOrWhiteSpace(sceneAnalysis.SceneCategory.NestedThemes))
                {
                    strSceneCategorization.AppendLine($"<secondary_topics>");
                    strSceneCategorization.AppendLine(sceneAnalysis.SceneCategory.NestedThemes);
                    strSceneCategorization.AppendLine($"</secondary_topics>");
                }

                strSceneCategorization.AppendLine($"</scene_themes>");
                sceneAnalysisInjection.AppendLine(strSceneCategorization.ToString());
            }

            // Characters (last thought, next possible actions)
            if (sceneAnalysis?.CharactersAnalyze != null && sceneAnalysis.CharactersAnalyze.Length > 0)
            {
                StringBuilder strSceneCharacters = new();
                var characters = sceneAnalysis.CharactersAnalyze.Where(w => !string.IsNullOrWhiteSpace(w.Name)).ToArray();

                if (characters.Length > 0)
                {
                    strSceneCharacters.AppendLine($"<characters>");
                    foreach (CharactersAnalyze character in characters)
                    {
                        strSceneCharacters.AppendLine($"<{character.Name}>");
                        if (!string.IsNullOrWhiteSpace(character.InnerThoughtsOrMonologue))
                            strSceneCharacters.AppendLine($"<lastThought>{character.InnerThoughtsOrMonologue}</lastThought>");

                        if (!string.IsNullOrWhiteSpace(character.Mood))
                            strSceneCharacters.AppendLine($"<mood>{character.Mood}</mood>");

                        if (!string.IsNullOrWhiteSpace(character.FacialExpression))
                            strSceneCharacters.AppendLine($"<facialExpression>{character.FacialExpression}</facialExpression>");

                        if (!string.IsNullOrWhiteSpace(character.BodyPosition))
                            strSceneCharacters.AppendLine($"<bodyPosition>{character.BodyPosition}</bodyPosition>");

                        if (!string.IsNullOrWhiteSpace(character.SemenOnBodyLocation))
                            strSceneCharacters.AppendLine($"<semenOnBodyLocation>{character.SemenOnBodyLocation}</semenOnBodyLocation>");

                        strSceneCharacters.AppendLine($"</{character.Name}>");
                    }

                    strSceneCharacters.AppendLine($"</characters>");
                    sceneAnalysisInjection.AppendLine(strSceneCharacters.ToString());
                }
            }

            // Player
            if (!string.IsNullOrWhiteSpace(sceneAnalysis?.PlayerAnalyze?.Name))
            {
                StringBuilder strScenePlayer = new();
                strScenePlayer.AppendLine($"<player ({sceneAnalysis.PlayerAnalyze.Name})>");

                //if (!string.IsNullOrWhiteSpace(sceneAnalysis.PlayerAnalyze.FacialExpression))
                //    strScenePlayer.AppendLine($"<facialExpression>{sceneAnalysis.PlayerAnalyze.FacialExpression}</facialExpression>");

                if (!string.IsNullOrWhiteSpace(sceneAnalysis.PlayerAnalyze.Mood))
                    strScenePlayer.AppendLine($"<mood>{sceneAnalysis.PlayerAnalyze.Mood}</mood>");

                if (!string.IsNullOrWhiteSpace(sceneAnalysis.PlayerAnalyze.SemenOnBodyLocation))
                    strScenePlayer.AppendLine($"<semenOnBodyLocation>{sceneAnalysis.PlayerAnalyze.SemenOnBodyLocation}</semenOnBodyLocation>");

                //if (!string.IsNullOrWhiteSpace(sceneAnalysis.PlayerAnalyze.EyesDirection?.LookingAtCharacterName))
                //{
                //    strScenePlayer.AppendLine($"<lookingAt>{sceneAnalysis.PlayerAnalyze.EyesDirection.LookingAtCharacterName}</lookingAt>");
                //    strScenePlayer.AppendLine($"<lookingAtBodyPart>{sceneAnalysis.PlayerAnalyze.EyesDirection.BodyPartBeingLookedAt}</lookingAtBodyPart>");
                //}

                //if (!string.IsNullOrWhiteSpace(sceneAnalysis.PlayerAnalyze.BodyPosition))
                //    strScenePlayer.AppendLine($"<bodyPosition>{sceneAnalysis.PlayerAnalyze.BodyPosition}</bodyPosition>");

                strScenePlayer.AppendLine($"</player ({sceneAnalysis.PlayerAnalyze.Name})>");
                sceneAnalysisInjection.AppendLine(strScenePlayer.ToString());
            }
        }
    }
}
