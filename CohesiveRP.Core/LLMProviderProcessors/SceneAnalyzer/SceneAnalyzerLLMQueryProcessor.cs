using CohesiveRP.Common.Diagnostics;
using CohesiveRP.Common.Serialization;
using CohesiveRP.Common.Utils.Parsers;
using CohesiveRP.Core.LLMProviderManager;
using CohesiveRP.Core.LLMProviderProcessors.SceneAnalyzer.BusinessObjects;
using CohesiveRP.Core.PromptContext;
using CohesiveRP.Core.PromptContext.Abstractions;
using CohesiveRP.Core.PromptContext.Builders;
using CohesiveRP.Core.PromptContext.Builders.Directive;
using CohesiveRP.Core.Services;
using CohesiveRP.Core.Services.LLMApiProvider;
using CohesiveRP.Core.Services.Summary;
using CohesiveRP.Storage.DataAccessLayer.AIQueries;
using CohesiveRP.Storage.DataAccessLayer.BackgroundQueries.BusinessObjects;
using CohesiveRP.Storage.DataAccessLayer.Chats;
using CohesiveRP.Storage.DataAccessLayer.Messages;
using CohesiveRP.Storage.DataAccessLayer.Pathfinder.CharacterSheetInstances.BusinessObjects;
using CohesiveRP.Storage.QueryModels.Chat;

namespace CohesiveRP.Core.LLMProviderProcessors.SceneAnalyzer
{
    public class SceneAnalyzerLLMQueryProcessor : LLMQueryProcessor
    {
        public SceneAnalyzerLLMQueryProcessor(
            ChatCompletionPresetType completionPresetType,
            BackgroundQuerySystemTags tag,
            BackgroundQueryDbModel backgroundQueryDbModel,
            IPromptContextBuilderFactory contextBuilderFactory,
            IPromptContextElementBuilderFactory promptContextElementBuilderFactory,
            IStorageService storageService,
            IHttpLLMApiProviderService httpLLMApiProviderService,
            ISummaryService summaryService) : base(
                completionPresetType,
                tag,
                backgroundQueryDbModel,
                contextBuilderFactory,
                promptContextElementBuilderFactory,
                storageService,
                httpLLMApiProviderService,
                summaryService)
        { }

        private async Task<string> GetChatAvatarFromSceneAnalysisFilePathAsync(ChatDbModel chatDbModel, SceneAnalyzerDbModel dbModel)
        {
            if (string.IsNullOrWhiteSpace(dbModel?.PlayerAnalyze?.EyesDirection?.LookingAtCharacterName))
            {
                return null;
            }

            if (chatDbModel?.CharacterIds == null || chatDbModel.CharacterIds.Count <= 0)
            {
                return null;
            }

            var characterSheetInstances = await storageService.GetCharacterSheetsInstanceByChatIdAsync(chatDbModel.ChatId);
            if (characterSheetInstances?.CharacterSheetInstances == null || characterSheetInstances.CharacterSheetInstances.Count <= 0)
            {
                return null;
            }

            CharacterSheetInstance[] charactersToConsider = characterSheetInstances.CharacterSheetInstances.Where(w =>
            chatDbModel.CharacterIds.Any(a => w.CharacterId == a) &&
            w.CharacterSheet != null &&
            !string.IsNullOrWhiteSpace(w.CharacterSheet.FirstName)).ToArray();

            if (charactersToConsider.Length <= 0)
            {
                return null;
            }

            string targetCharacterName = dbModel.PlayerAnalyze.EyesDirection.LookingAtCharacterName.ToLowerInvariant().Trim();
            CharacterSheetInstance targetCharacterSheet = charactersToConsider.FirstOrDefault(w =>
                targetCharacterName.Equals(w.CharacterSheet.FirstName, StringComparison.InvariantCultureIgnoreCase) ||
                targetCharacterName.Equals(w.CharacterSheet.LastName, StringComparison.InvariantCultureIgnoreCase) ||
                targetCharacterName == $"{w.CharacterSheet.FirstName.ToLowerInvariant()} {w.CharacterSheet.LastName?.ToLowerInvariant()}");

            if (targetCharacterSheet == null)
            {
                return null;
            }

            // Check if the character has an assets folder in this chat
            var characterDbModel = await storageService.GetCharacterByIdAsync(targetCharacterSheet.CharacterId);
            string characterFolderPath = Path.Combine(WebConstants.CharactersAvatarFilePath, characterDbModel.Name.ToLowerInvariant());
            if (!Directory.Exists(WebConstants.CharactersAvatarFilePath))
            {
                return null;
            }

            // Alright, we got an assets folder for this character
            string finalAvatarSelectionFilePath = null;

            // Let's check if there's an avatar there so that we can default to it
            string avatarFilePath = Path.Combine(characterFolderPath, WebConstants.AvatarFileName);
            if (File.Exists(avatarFilePath))
            {
                finalAvatarSelectionFilePath = avatarFilePath;
            }

            // Start by validating that we have a folder for the outfit the lookedAt character is currently wearing
            var targetCharacter = dbModel.CharactersAnalyze.FirstOrDefault(f =>
            f.Name.Equals(targetCharacterSheet.CharacterSheet.FirstName, StringComparison.InvariantCultureIgnoreCase) ||
            f.Name.Equals(targetCharacterSheet.CharacterSheet.LastName, StringComparison.InvariantCultureIgnoreCase) ||
            f.Name.Equals($"{targetCharacterSheet.CharacterSheet.FirstName} {targetCharacterSheet.CharacterSheet.LastName}", StringComparison.InvariantCultureIgnoreCase));

            if (targetCharacter != null)
            {
                string currentOutfitFolderPath = Path.Combine(characterFolderPath, targetCharacter.StateOfDress);
                if (!Directory.Exists(currentOutfitFolderPath))
                {
                    return finalAvatarSelectionFilePath.Replace(WebConstants.WebAppPublicFolder, "").ToLowerInvariant();
                }

                // Ok, the folder with the right outfit exists. Let's check if there's an avatar matching the right expression there so that we can prioritize it over the default one
                string avatarWithNeutralExpressionFilePath = Path.Combine(currentOutfitFolderPath, WebConstants.AvatarFileName);
                if (File.Exists(avatarWithNeutralExpressionFilePath))
                {
                    finalAvatarSelectionFilePath = avatarWithNeutralExpressionFilePath;
                }

                // Lastly, if there's an avatar matching the current facial expression, let's prioritize it over the neutral one
                string avatarWithCurrentExpressionFilePath = Path.Combine(currentOutfitFolderPath, targetCharacter.FacialExpression?.ToLowerInvariant());
                if (Directory.Exists(avatarWithCurrentExpressionFilePath))
                {
                    // Get a random file within that folder, if any
                    string[] availableAvatarsWithTheRightExpressionAndClothes = Directory.GetFiles(avatarWithCurrentExpressionFilePath, "*.*", SearchOption.AllDirectories);

                    if (availableAvatarsWithTheRightExpressionAndClothes != null && availableAvatarsWithTheRightExpressionAndClothes.Length > 0)
                    {
                        string choosenFile = availableAvatarsWithTheRightExpressionAndClothes[new Random(DateTime.Now.Millisecond).Next(0, availableAvatarsWithTheRightExpressionAndClothes.Length - 1)];
                        finalAvatarSelectionFilePath = choosenFile;
                    }
                }
            }

            return finalAvatarSelectionFilePath.Replace(WebConstants.WebAppPublicFolder, "").ToLowerInvariant();
        }

        private async Task SetChatAvatarAsync(ChatDbModel chat, string messageId, string avatarFilePath)
        {
            if (string.IsNullOrWhiteSpace(avatarFilePath) || chat == null)
            {
                return;
            }

            MessageDbModel message = await storageService.GetSpecificMessageAsync(chat.ChatId, messageId) as MessageDbModel;

            if(message == null)
                return;

            message.AvatarFilePath = avatarFilePath;

            //chat.AvatarFilePath = avatarFilePath;
            await storageService.UpdateHotMessageAsync(chat.ChatId, message);
        }

        public override async Task<bool> ProcessCompletedQueryAsync()
        {
            if (!await base.ProcessCompletedQueryAsync())
            {
                backgroundQueryDbModel.Content = null;
                backgroundQueryDbModel.Status = BackgroundQueryStatus.Pending;// re-queue
                return false;
            }

            // Deserialize the generic content into a valid Scene Analyzer
            try
            {
                LLMApiResponseMessage LLMmessage = messages.LastOrDefault();
                IShareableContextLink shareableContextLink = promptContext.ShareableContextLinks.FirstOrDefault(f => f.LinkedBuilder is PromptContextSceneAnalyzerInstrBuilder);
                if (shareableContextLink == null)
                {
                    LoggingManager.LogToFile("e214f6b2-ffda-4fd8-8b92-d3b0b565d297", $"No ShareableContextLink of type [{nameof(PromptContextSceneAnalyzerInstrBuilder)} found.]");
                    return false;
                }

                // Parse the response as Json
                SceneAnalyzerLLMResponseDto responseDto = null;
                try
                {
                    responseDto = LLMResponseParser.ParseFromApiMessageContent<SceneAnalyzerLLMResponseDto>(backgroundQueryDbModel.Content);
                } catch (Exception ex)
                {
                    LoggingManager.LogToFile("a604227f-e724-4807-92e9-76424d86b9a3", $"The LLM response was malformed [{backgroundQueryDbModel.Content}].");
                    backgroundQueryDbModel.Content = null;
                    backgroundQueryDbModel.Status = BackgroundQueryStatus.Error;// drop
                    return false;
                }

                if (responseDto?.PlayerAnalyze == null || responseDto?.CharactersAnalyze == null || responseDto?.SceneCategory == null)
                {
                    LoggingManager.LogToFile("82ffbc70-cef7-4fc7-9183-c64b9bced657", $"The LLM response was malformed [{backgroundQueryDbModel.Content}].");
                    backgroundQueryDbModel.Content = null;
                    backgroundQueryDbModel.Status = BackgroundQueryStatus.Error;// drop
                    return false;
                }

                string messageId = shareableContextLink.Value as string;

                // Replace the scene analyzer tied to this chat with the new one
                SceneAnalyzerDbModel dbModel = new()
                {
                    ChatId = backgroundQueryDbModel.ChatId,
                    CharactersAnalyze = responseDto.CharactersAnalyze,
                    PlayerAnalyze = responseDto.PlayerAnalyze,
                    SceneCategory = responseDto.SceneCategory,
                    LinkedMessageId = messageId,
                };

                SceneAnalyzerDbModel updatedMessageInStorage = await storageService.CreateOrUpdateSceneAnalyzerAsync(dbModel);
                if (updatedMessageInStorage == null)
                {
                    LoggingManager.LogToFile("1cf1c697-34b3-438c-8e24-547d29867fdb", $"Couldn't complete backgroundTask [{backgroundQueryDbModel.BackgroundQueryId}] of Type [{tag}]. Couldn't update storage. Skipping.");
                    backgroundQueryDbModel.Status = BackgroundQueryStatus.Error;
                    return false;
                }

                // Prebuild the images to show in the UI according to context
                var chat = await storageService.GetChatAsync(dbModel.ChatId);
                var avatarFilePath = await GetChatAvatarFromSceneAnalysisFilePathAsync(chat, dbModel);
                await SetChatAvatarAsync(chat, messageId, avatarFilePath);

                backgroundQueryDbModel.Status = BackgroundQueryStatus.Completed;
            } catch (Exception e)
            {
                LoggingManager.LogToFile("d697e65a-7997-44e0-8fc5-d40355222627", $"Couldn't complete backgroundTask [{backgroundQueryDbModel.BackgroundQueryId}]. Task will be set to Pending status for re-generation.", e);
                backgroundQueryDbModel.Content = null;
                backgroundQueryDbModel.Status = BackgroundQueryStatus.Pending;
            }

            return true;
        }
    }
}
