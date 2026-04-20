using CohesiveRP.Common.BusinessObjects;
using CohesiveRP.Common.Diagnostics;
using CohesiveRP.Common.Utils.Parsers;
using CohesiveRP.Core.PromptContext.Abstractions;
using CohesiveRP.Core.PromptContext.Builders;
using CohesiveRP.Core.PromptContext.Builders.Directive;
using CohesiveRP.Core.PromptContext.Builders.LoreByKeywords.BusinessObjects;
using CohesiveRP.Core.Services;
using CohesiveRP.Core.Services.LLMApiProvider;
using CohesiveRP.Core.Services.Summary;
using CohesiveRP.Storage.DataAccessLayer.AIQueries;
using CohesiveRP.Storage.DataAccessLayer.BackgroundQueries.BusinessObjects;
using CohesiveRP.Storage.DataAccessLayer.Chats;
using CohesiveRP.Storage.DataAccessLayer.Lorebooks.BusinessObjects;
using CohesiveRP.Storage.DataAccessLayer.Messages;
using CohesiveRP.Storage.DataAccessLayer.Messages.BusinessObjects;
using CohesiveRP.Storage.DataAccessLayer.Pathfinder.CharacterSheetInstances.BusinessObjects;
using CohesiveRP.Storage.DataAccessLayer.SceneTracker.BusinessObjects;
using CohesiveRP.Storage.DataAccessLayer.SceneTracker.BusinessObjects.Visual;
using CohesiveRP.Storage.DataAccessLayer.Settings;
using CohesiveRP.Storage.QueryModels.BackgroundQuery;
using CohesiveRP.Storage.QueryModels.Chat;
using CohesiveRP.Storage.QueryModels.Message;

namespace CohesiveRP.Core.LLMProviderManager.Main
{
    public class MainLLMQueryProcessor : LLMQueryProcessor
    {
        public MainLLMQueryProcessor(
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
        {
        }

        //private string[] GetAvatarsFilePathFromCharactersInScene(VisualSceneTracker sceneTracker, CharacterSheetInstancesDbModel characterSheetInstances)
        //{
        //    List<string> characterAvatars = new();

        //    foreach (VisualCharacterAnalysis characterAnalysis in sceneTracker.CharactersAnalysis.Where(w => !string.IsNullOrWhiteSpace(w.Name)))
        //    {
        //        string targetCharacterName = characterAnalysis.Name.ToLowerInvariant().Trim();
        //        CharacterSheetInstance targetCharacterSheet = characterSheetInstances.CharacterSheetInstances.FirstOrDefault(w =>
        //            targetCharacterName.Equals(w.CharacterSheet.FirstName, StringComparison.InvariantCultureIgnoreCase) ||
        //            targetCharacterName.Equals(w.CharacterSheet.LastName, StringComparison.InvariantCultureIgnoreCase) ||
        //            targetCharacterName == $"{w.CharacterSheet.FirstName.ToLowerInvariant()} {w.CharacterSheet.LastName?.ToLowerInvariant()}");

        //        if (targetCharacterSheet == null)
        //        {
        //            continue;
        //        }


        //    }

        //    return characterAvatars.ToArray();
        //}

        private async Task<List<CharacterAvatarDefinition>> GetAvatarsFromSceneAnalysisFilePathAsync(ChatDbModel chatDbModel, SceneTrackerDbModel dbModel)
        {
            List<CharacterAvatarDefinition> finalAvatarSelection = new();

            if (chatDbModel == null || dbModel == null)
            {
                return finalAvatarSelection;
            }

            var sceneTracker = LLMResponseParser.ParseFromApiMessageContent<VisualSceneTracker>(dbModel.Content);
            if (sceneTracker?.CharactersAnalysis == null || sceneTracker.CharactersAnalysis.Length <= 0)
            {
                return finalAvatarSelection;
            }

            var characterSheetInstances = await storageService.GetCharacterSheetsInstanceByChatIdAsync(chatDbModel.ChatId);
            if (characterSheetInstances?.CharacterSheetInstances == null || characterSheetInstances.CharacterSheetInstances.Count <= 0)
            {
                return finalAvatarSelection;
            }

            CharacterSheetInstance[] charactersToConsider = characterSheetInstances.CharacterSheetInstances.Where(w =>
            chatDbModel.CharacterIds.Any(a => w.CharacterId == a) &&
            w.CharacterSheet != null &&
            !string.IsNullOrWhiteSpace(w.CharacterSheet.FirstName)).ToArray();

            if (charactersToConsider.Length <= 0)
            {
                return finalAvatarSelection;
            }

            List<VisualCharacterAnalysis> characterTargetsToHandle = [];

            // Then we add the rest of the characters in scene that we want to handle
            if (sceneTracker?.CharactersAnalysis != null && sceneTracker.CharactersAnalysis.Length > 0)
            {
                foreach (VisualCharacterAnalysis character in sceneTracker.CharactersAnalysis.Where(w => !string.IsNullOrWhiteSpace(w.Name)))
                {
                    character.Name = character.Name.ToLowerInvariant().Trim();
                    characterTargetsToHandle.Add(character);
                }
            }

            // When the player is looking at a character, we want to prioritize showing the avatar of that character, so we add it at the beginning of the list of character to handle
            if (!string.IsNullOrWhiteSpace(sceneTracker.PlayerAnalysis?.EyesDirection?.LookingAtCharacterName))
            {
                string targetName = sceneTracker.PlayerAnalysis.EyesDirection.LookingAtCharacterName.ToLowerInvariant().Trim();
                if (characterTargetsToHandle.Any(a => a.Name == targetName))
                {
                    var value = characterTargetsToHandle.First(a => a.Name == targetName);
                    characterTargetsToHandle.Remove(value);
                    characterTargetsToHandle.Insert(0, value);
                } else
                {
                    // TODO: else what exactly?... we could set the default avatar, but we don't have clothes status or facial expression..
                }
            }

            // Now that we have the names of the characters we want to handle in order of priority, we loop through them and try to find an avatar for each of them
            foreach (VisualCharacterAnalysis characterTarget in characterTargetsToHandle)
            {
                CharacterAvatarDefinition avatar = await GetAvatarFromSceneAnalysisCharacterTargetAsync(characterTarget, charactersToConsider);

                if (avatar != null && !finalAvatarSelection.Contains(avatar))
                    finalAvatarSelection.Add(avatar);
            }

            return finalAvatarSelection;
        }

        private async Task<CharacterAvatarDefinition> GetAvatarFromSceneAnalysisCharacterTargetAsync(VisualCharacterAnalysis targetCharacter, CharacterSheetInstance[] charactersToConsider)
        {
            CharacterAvatarDefinition avatar = null;

            if (targetCharacter == null || string.IsNullOrWhiteSpace(targetCharacter.Name))
            {
                return avatar;
            }

            CharacterSheetInstance targetCharacterSheet = charactersToConsider.FirstOrDefault(w =>
                targetCharacter.Name.Equals(w.CharacterSheet.FirstName, StringComparison.InvariantCultureIgnoreCase) ||
                targetCharacter.Name.Equals(w.CharacterSheet.LastName, StringComparison.InvariantCultureIgnoreCase) ||
                targetCharacter.Name == $"{w.CharacterSheet.FirstName.ToLowerInvariant()} {w.CharacterSheet.LastName?.ToLowerInvariant()}");

            if (targetCharacterSheet == null)
            {
                return avatar;
            }

            // Check if the character has an assets folder in this chat
            var characterDbModel = await storageService.GetCharacterByIdAsync(targetCharacterSheet.CharacterId);
            string characterFolderPath = Path.Combine(WebConstants.CharactersAvatarFilePath, characterDbModel.Name.ToLowerInvariant());
            if (!Directory.Exists(WebConstants.CharactersAvatarFilePath))
            {
                return avatar;
            }

            // Will default to "...characterName/avatar.png"
            avatar = new()
            {
                Id = characterDbModel.CharacterId,
                Name = characterDbModel.Name,
                FilePath = Path.Combine(characterFolderPath, WebConstants.AvatarFileName)?.Replace(WebConstants.WebAppPublicFolder, "").ToLowerInvariant(),
            };

            string outfit = targetCharacter.ClothingStateOfDress.ToLowerInvariant();
            string currentOutfitFolderPath = Path.Combine(characterFolderPath, outfit);
            if (!Directory.Exists(currentOutfitFolderPath) || Directory.EnumerateFiles(currentOutfitFolderPath, "*", SearchOption.AllDirectories).ToArray().Length <= 0)
            {
                // Try to default back to 'clothed'
                outfit = ClothingStateOfDress.Clothed.ToString().ToLowerInvariant();
                currentOutfitFolderPath = Path.Combine(characterFolderPath, outfit);

                if (!Directory.Exists(currentOutfitFolderPath) || Directory.EnumerateFiles(currentOutfitFolderPath, "*", SearchOption.AllDirectories).ToArray().Length <= 0)
                {
                    // give up, simply return the default avatar
                    return avatar;
                }
            }

            // Ok, the folder with the outfit exists. Let's check if there's an avatar matching the right expression there so that we can prioritize it over the default (neutral) one
            avatar.Outfit = outfit;

            if (File.Exists(Path.Combine(currentOutfitFolderPath, WebConstants.AvatarFileName)))
                avatar.FilePath = Path.Combine(currentOutfitFolderPath, WebConstants.AvatarFileName)?.Replace(WebConstants.WebAppPublicFolder, "").ToLowerInvariant();

            // use 'neutral' folder and default to an image within that folder by default, so that if the right expression isn't found or doesn't have any image, we'll default to neutral expression
            string neutralAvatarsFolderPath = Path.Combine(currentOutfitFolderPath, MappedFacialExpression.Neutral.ToString().ToLowerInvariant());
            if (Directory.Exists(neutralAvatarsFolderPath) && Directory.EnumerateFiles(neutralAvatarsFolderPath).Count() > 0)
            {
                avatar.Expression = MappedFacialExpression.Neutral.ToString().ToLowerInvariant();

                if (File.Exists(Path.Combine(neutralAvatarsFolderPath, WebConstants.AvatarFileName)))
                    avatar.FilePath = Path.Combine(neutralAvatarsFolderPath, WebConstants.AvatarFileName)?.Replace(WebConstants.WebAppPublicFolder, "").ToLowerInvariant();
            }

            // TODO: check semen folder here as it's more important that the facial expression
            // TODO: check for body position

            // Lastly, if there's an avatar matching the current facial expression, let's prioritize it over the neutral one
            string expressionAvatarFolderPath = Path.Combine(currentOutfitFolderPath, targetCharacter.FacialExpression?.ToLowerInvariant());
            if (Directory.Exists(expressionAvatarFolderPath) && Directory.EnumerateFiles(expressionAvatarFolderPath).Count() > 0)
            {
                avatar.Expression = targetCharacter.FacialExpression.ToLowerInvariant();

                // Get a random file within that folder, if any
                string[] availableAvatarsWithTheRightExpressionAndClothes = Directory.GetFiles(expressionAvatarFolderPath, "*.*", SearchOption.AllDirectories);

                if (availableAvatarsWithTheRightExpressionAndClothes != null && availableAvatarsWithTheRightExpressionAndClothes.Length > 0)
                {
                    string choosenFile = availableAvatarsWithTheRightExpressionAndClothes[new Random(DateTime.Now.Millisecond).Next(0, availableAvatarsWithTheRightExpressionAndClothes.Length - 1)];

                    if (File.Exists(choosenFile))
                        avatar.FilePath = choosenFile?.Replace(WebConstants.WebAppPublicFolder, "").ToLowerInvariant();
                }
            }

            return avatar;
        }

        private async Task<CharacterAvatarDefinition> GetPersonaAvatarFromSceneAnalysisFilePathAsync(ChatDbModel chatDbModel, SceneTrackerDbModel dbModel)
        {
            CharacterAvatarDefinition avatar = null;

            if (chatDbModel == null || dbModel == null)
            {
                return avatar;
            }

            var sceneTracker = LLMResponseParser.ParseFromApiMessageContent<VisualSceneTracker>(dbModel.Content);
            if (string.IsNullOrWhiteSpace(sceneTracker?.PlayerAnalysis?.Name))
            {
                return avatar;
            }

            if (string.IsNullOrWhiteSpace(chatDbModel?.PersonaId))
            {
                return avatar;
            }

            var personaDbModel = await storageService.GetPersonaByIdAsync(chatDbModel.PersonaId);
            string personaFolderPath = Path.Combine(WebConstants.PersonasAvatarFilePath, personaDbModel.PersonaId.ToLowerInvariant());
            if (!Directory.Exists(WebConstants.PersonasAvatarFilePath))
            {
                return avatar;
            }

            // Alright, we got an assets folder for this persona
            avatar = new()
            {
                Id = personaDbModel.PersonaId,
                Name = personaDbModel.Name,
                FilePath = Path.Combine(personaFolderPath, WebConstants.AvatarFileName)?.Replace(WebConstants.WebAppPublicFolder, "").ToLowerInvariant(),
            };

            string outfit = sceneTracker.PlayerAnalysis.ClothingStateOfDress.ToLowerInvariant();
            string currentOutfitFolderPath = Path.Combine(personaFolderPath, outfit);
            if (!Directory.Exists(currentOutfitFolderPath) || Directory.EnumerateFiles(currentOutfitFolderPath, "*", SearchOption.AllDirectories).ToArray().Length <= 0)
            {
                // Try to default back to 'clothed'
                outfit = ClothingStateOfDress.Clothed.ToString().ToLowerInvariant();
                currentOutfitFolderPath = Path.Combine(personaFolderPath, outfit);

                if (!Directory.Exists(currentOutfitFolderPath) || Directory.EnumerateFiles(currentOutfitFolderPath, "*", SearchOption.AllDirectories).ToArray().Length <= 0)
                {
                    // give up
                    return avatar;
                }
            }

            // Ok, the folder with the right outfit exists. Let's check if there's an avatar matching the right expression there so that we can prioritize it over the default one
            avatar.Outfit = outfit;

            if (File.Exists(Path.Combine(currentOutfitFolderPath, WebConstants.AvatarFileName)))
                avatar.FilePath = Path.Combine(currentOutfitFolderPath, WebConstants.AvatarFileName)?.Replace(WebConstants.WebAppPublicFolder, "").ToLowerInvariant();

            // use 'neutral' folder and default to avatar.png if not found
            string avatarWithNeutralExpressionFilePath = Path.Combine(currentOutfitFolderPath, WebConstants.AvatarFileName);
            if (File.Exists(avatarWithNeutralExpressionFilePath))
            {
                avatar.Expression = MappedFacialExpression.Neutral.ToString().ToLowerInvariant();

                if (File.Exists(Path.Combine(currentOutfitFolderPath, WebConstants.AvatarFileName)))
                    avatar.FilePath = avatarWithNeutralExpressionFilePath?.Replace(WebConstants.WebAppPublicFolder, "").ToLowerInvariant();
            }

            // TODO: check semen folder here as it's more important that the facial expression
            // TODO: check for body position

            // Lastly, if there's an avatar matching the current facial expression, let's prioritize it over the neutral one
            string expressionAvatarFolderPath = Path.Combine(currentOutfitFolderPath, sceneTracker.PlayerAnalysis.FacialExpression?.ToLowerInvariant());
            if (Directory.Exists(expressionAvatarFolderPath) && Directory.EnumerateFiles(expressionAvatarFolderPath).Count() > 0)
            {
                avatar.Expression = sceneTracker.PlayerAnalysis.FacialExpression.ToLowerInvariant();

                // Get a random file within that folder, if any
                string[] availableAvatarsWithTheRightExpressionAndClothes = Directory.GetFiles(expressionAvatarFolderPath, "*.*", SearchOption.AllDirectories);

                if (availableAvatarsWithTheRightExpressionAndClothes != null && availableAvatarsWithTheRightExpressionAndClothes.Length > 0)
                {
                    string choosenFile = availableAvatarsWithTheRightExpressionAndClothes[new Random(DateTime.Now.Millisecond).Next(0, availableAvatarsWithTheRightExpressionAndClothes.Length - 1)];

                    if(File.Exists(choosenFile))
                    avatar.FilePath = choosenFile?.Replace(WebConstants.WebAppPublicFolder, "").ToLowerInvariant();
                }
            }

            return avatar;
        }

        private async Task SetMessageAvatarAsync(ChatDbModel chat, string messageId, List<CharacterAvatarDefinition> avatarDefinitions)
        {
            if (avatarDefinitions == null || avatarDefinitions.Count <= 0 || chat == null || string.IsNullOrWhiteSpace(messageId))
            {
                return;
            }

            MessageDbModel message = await storageService.GetSpecificMessageAsync(chat.ChatId, messageId) as MessageDbModel;

            if (message == null)
                return;

            message.CharacterAvatars = avatarDefinitions.ToArray();

            //chat.AvatarFilePath = avatarFilePath;
            await storageService.UpdateHotMessageAsync(chat.ChatId, message);
        }

        private async Task HandleLorebookInstancesCreationIfRequired(ChatDbModel chat, LorebookInstanceDbModel[] lorebookInstances)
        {
            var lorebookInstancesToCreate = chat.LorebookIds.Where(w => !lorebookInstances.Any(a => a.LorebookId == w)).ToArray();
            if (lorebookInstancesToCreate.Length > 0)
            {
                foreach (var lorebookInstance in lorebookInstancesToCreate)
                {
                    await storageService.AddLorebookInstanceAsync(new LorebookInstanceDbModel
                    {
                        ChatId = chat.ChatId,
                        LorebookId = lorebookInstance,
                        Entries = [],
                    });
                }
            }
        }

        private async Task HandleExistingLorebookInstanceEntries(LorebookInstanceDbModel[] lorebookInstances)
        {
            List<LorebookStateEntry> entriesToRemove = new();
            foreach (var lorebookInstance in lorebookInstances)
            {
                foreach (var lorebookInstanceEntry in lorebookInstance.Entries)
                {
                    if (lorebookInstanceEntry.RemainingStickeyAmount > 0)
                        lorebookInstanceEntry.RemainingStickeyAmount--;

                    if (lorebookInstanceEntry.RemainingCooldownAmount > 0)
                        lorebookInstanceEntry.RemainingCooldownAmount--;

                    if (lorebookInstanceEntry.RemainingStickeyAmount <= 0 && lorebookInstanceEntry.RemainingCooldownAmount <= 0)
                    {
                        entriesToRemove.Add(lorebookInstanceEntry);
                    }
                }

                // Remove the entries that are not sticky nor in cooldown
                lorebookInstance.Entries.RemoveAll(entriesToRemove.Contains);

                await storageService.UpdateLorebookInstanceAsync(lorebookInstance);
            }
        }

        private async Task HandleNewLorebookInstanceEntries(TrackedLoreEntitesShareableContext trackedLoreEntities)
        {
            if (trackedLoreEntities?.Entries == null || trackedLoreEntities.Entries.Count <= 0)
            {
                return;
            }

            // For each entries that triggered and are either sticky-able or cooldown-able, we want to track them
            var groupedByLorebookId = trackedLoreEntities.Entries.GroupBy(g => g.LorebookId);
            foreach (IGrouping<string, LoreEntryToTrack> group in groupedByLorebookId)
            {
                var currentLorebookInstances = await storageService.GetLorebookInstancesAsync(f => f.LorebookId == group.Key);
                if (currentLorebookInstances == null || currentLorebookInstances.Length != 1)
                {
                    LoggingManager.LogToFile("d96f483e-f214-4b4b-81ab-810b96d7cf61", $"Found [{currentLorebookInstances?.Length}] lorebookInstances matching lorebookId [{group.Key}]. Ignoring adding tracked lore entries using this specific lorebook.");
                    continue;
                }

                List<LorebookStateEntry> newTrackedEntries = new();
                foreach (LoreEntryToTrack entryToTrack in group)
                {
                    // Avoid duplicate on the exact same messageId for the exact same entryId
                    if (currentLorebookInstances[0].Entries.Any(a => a.LinkedMessageId == entryToTrack.LinkedMessageId && a.LorebookEntryId == entryToTrack.EntryId))
                        continue;

                    newTrackedEntries.Add(new LorebookStateEntry
                    {
                        LorebookEntryId = entryToTrack.EntryId,
                        LinkedMessageId = entryToTrack.LinkedMessageId,
                        RemainingStickeyAmount = entryToTrack.Sticky,
                        RemainingCooldownAmount = entryToTrack.Cooldown,
                    });
                }

                currentLorebookInstances[0].Entries.AddRange(newTrackedEntries);
                await storageService.UpdateLorebookInstanceAsync(currentLorebookInstances[0]);
            }
        }

        private async Task HandleStickyAndCooldownAsync(ChatDbModel chat, TrackedLoreEntitesShareableContext trackedLoreEntities)
        {
            var lorebookInstances = await storageService.GetLorebookInstancesAsync(g => g.ChatId == backgroundQueryDbModel.ChatId);

            // Create the lorebook instances if required
            await HandleLorebookInstancesCreationIfRequired(chat, lorebookInstances);

            // Decrement lifecycle of existing tracked entries for this chat and remove them when the tracking is unrequired
            await HandleExistingLorebookInstanceEntries(lorebookInstances);

            // Track new entities
            await HandleNewLorebookInstanceEntries(trackedLoreEntities);
        }

        /// <summary>
        /// Process the resulting completed query. If it was a 'main', it'll add a new AI message, if it was a sceneTracker, it'll attach the tracker, if it was a summary, it'll attach the summary to an existing message, etc.
        /// </summary>
        public override async Task<bool> ProcessCompletedQueryAsync()
        {
            if (!await base.ProcessCompletedQueryAsync())
            {
                backgroundQueryDbModel.Content = null;
                backgroundQueryDbModel.Status = BackgroundQueryStatus.Error;
                return false;
            }

            // Deserialize the generic content into a list of messages
            try
            {
                var chat = await storageService.GetChatAsync(backgroundQueryDbModel.ChatId);

                LLMApiResponseMessage message = messages.LastOrDefault();

                if (string.IsNullOrWhiteSpace(message.Content))
                {
                    backgroundQueryDbModel.Content = null;
                    backgroundQueryDbModel.Status = BackgroundQueryStatus.Pending;
                    backgroundQueryDbModel.RetryCount++;
                    return false;
                }

                backgroundQueryDbModel.EndFocusedGenerationDateTimeUtc = DateTime.UtcNow;

                // Add the AI reply message to the end of the chat
                CreateMessageQueryModel messageQueryModel = new()
                {
                    ChatId = backgroundQueryDbModel.ChatId,
                    SourceType = MessageSourceType.AI,
                    CharacterId = chat.CharacterIds.FirstOrDefault(),
                    CharacterAvatars = null,
                    Summarized = false,// New message, so it's not summarized yet
                    InRoleplayDateTime = null,// At this point, we just generated the message, we don't know the inRoleplay datetime yet, we need the input of the sceneTracker for that
                    MessageContent = ChatMessageParserUtils.ParseMessage(message.Content),
                    ThinkingContent = ChatMessageParserUtils.ParseThinking(message.Content),
                    CreatedAtUtc = DateTime.UtcNow,
                    StartGenerationDateTimeUtc = backgroundQueryDbModel.CreatedAtUtc,
                    StartFocusedGenerationDateTimeUtc = backgroundQueryDbModel.StartFocusedGenerationDateTimeUtc,
                    EndFocusedGenerationDateTimeUtc = backgroundQueryDbModel.EndFocusedGenerationDateTimeUtc,
                };

                IMessageDbModel newMessageInStorage = await storageService.AddMessageAsync(messageQueryModel);
                if (newMessageInStorage == null)
                {
                    LoggingManager.LogToFile("15b7b071-b3bb-4d36-9321-4353dd747797", $"Error. The message creation in storage failed. Couldn't complete backgroundTask [{backgroundQueryDbModel.BackgroundQueryId}] of Type [{tag}]. Task will be set to Pending status for re-generation.");
                    backgroundQueryDbModel.Content = null;
                    backgroundQueryDbModel.Status = BackgroundQueryStatus.Error;
                    return false; ;
                }

                backgroundQueryDbModel.LinkedId = newMessageInStorage.MessageId;

                // Iterate the lorebook sticky and cooldown
                var shareableContextLink = promptContext.ShareableContextLinks.FirstOrDefault(f => f.LinkedBuilder is PromptContextLoreByKeywordsBuilder)?.Value as TrackedLoreEntitesShareableContext;
                if (shareableContextLink != null)
                {
                    await HandleStickyAndCooldownAsync(chat, shareableContextLink);
                }

                // Right before setting the query processor to completed state, we'll launch the background workers
                if (contextBuilder == null)
                {
                    await BuildContextAsync(backgroundQueryDbModel);
                }

                GlobalSettingsDbModel globalSettings = await storageService.GetGlobalSettingsAsync();
                var hotMessages = await storageService.GetAllHotMessagesAsync(chat.ChatId);
                var allMessages = hotMessages.Messages.OrderByDescending(o => o.CreatedAtUtc).ToArray();
                var lastPlayerMessage = allMessages.FirstOrDefault(f => f.SourceType == MessageSourceType.User);

                // Scene Analyzer
                //await QueueSceneAnalyzeAsync(chat);

                // Prebuild the images to show in the UI according to context
                var sceneTracker = await storageService.GetSceneTrackerAsync(backgroundQueryDbModel.ChatId);

                var characterAvatars = await GetAvatarsFromSceneAnalysisFilePathAsync(chat, sceneTracker);
                await SetMessageAvatarAsync(chat, newMessageInStorage.MessageId, characterAvatars);

                var personaAvatars = await GetPersonaAvatarFromSceneAnalysisFilePathAsync(chat, sceneTracker);
                await SetMessageAvatarAsync(chat, lastPlayerMessage?.MessageId, [personaAvatars]);

                // Summary
                _ = summaryService.EvaluateSummaryAsync(backgroundQueryDbModel.ChatId, globalSettings);

                backgroundQueryDbModel.Status = BackgroundQueryStatus.Completed;
                return true;
            } catch (Exception e)
            {
                LoggingManager.LogToFile("3323ca32-a0b4-414f-a0a7-eedea88c4099", $"Couldn't complete backgroundTask [{backgroundQueryDbModel.BackgroundQueryId}]. Task will be set to Pending status for re-generation.", e);
                backgroundQueryDbModel.Content = null;
                backgroundQueryDbModel.Status = BackgroundQueryStatus.Error;
                return false;
            }
        }

        private async Task<bool> QueueSceneAnalyzeAsync(ChatDbModel chat)
        {
            var backgroundQueryModel = new CreateBackgroundQueryQueryModel
            {
                ChatId = chat.ChatId,
                Priority = BackgroundQueryPriority.Highest,// User is waiting!
                DependenciesTags = [],// No dependencies at all
                Tags = [BackgroundQuerySystemTags.sceneAnalyze.ToString()],
            };

            if (await storageService.AddBackgroundQueryAsync(backgroundQueryModel) == null)
                return false;

            return true;
        }
    }
}
