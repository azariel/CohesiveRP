using CohesiveRP.Common.BusinessObjects;
using CohesiveRP.Common.Diagnostics;
using CohesiveRP.Common.Serialization;
using CohesiveRP.Common.Utils.Parsers;
using CohesiveRP.Core.LLMProviderManager;
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
using CohesiveRP.Storage.DataAccessLayer.SceneTracker.BusinessObjects;
using CohesiveRP.Storage.DataAccessLayer.SceneTracker.BusinessObjects.Visual;
using CohesiveRP.Storage.DataAccessLayer.Users;
using CohesiveRP.Storage.QueryModels.Chat;
using CohesiveRP.Storage.QueryModels.SceneTracker;

namespace CohesiveRP.Core.LLMProviderProcessors.SceneTracker
{
    public class SceneTrackerLLMQueryProcessor : LLMQueryProcessor
    {
        public SceneTrackerLLMQueryProcessor(
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

        private async Task UpdateMessagesTimeTrackerAsync(SceneTrackerDbModel sceneTracker)
        {
            if (sceneTracker == null)
                return;

            var hotMessagesObj = await storageService.GetAllHotMessagesAsync(sceneTracker.ChatId);
            var hotMessages = hotMessagesObj.Messages.OrderByDescending(m => m.CreatedAtUtc).ToList();
            if (hotMessages.Count <= 0)
                return;

            var indexOfUserMessageToCutChunk = hotMessages.IndexOf(hotMessages.FirstOrDefault(m => m.SourceType == MessageSourceType.User));
            if (hotMessages.First().SourceType == MessageSourceType.User)
            {
                var selection = hotMessages.Skip(1).ToArray();
                indexOfUserMessageToCutChunk = selection.IndexOf(selection.FirstOrDefault(m => m.SourceType == MessageSourceType.User));
            }

            var chunkOfMessagesToUpdate = hotMessages.Take(indexOfUserMessageToCutChunk + 1).ToList();

            // Deserialize the sceneTracker to find out the inRoleplay datetime
            var deserializedSceneTracker = JsonCommonSerializer.DeserializeFromString<BasicInformationSceneTracker>(sceneTracker.Content);
            if(deserializedSceneTracker?.CurrentDateTime == null)
                return;

            // Parse the date
            if(!DateTime.TryParse(deserializedSceneTracker.CurrentDateTime, out DateTime sceneTrackerParsedDate))
                return;

            foreach (var message in chunkOfMessagesToUpdate)
            {
                message.InRoleplayDateTime = sceneTrackerParsedDate;
            }

            var resultUpdate = await storageService.UpdateHotMessagesAsync(hotMessagesObj);

            if(!resultUpdate)
            {
                LoggingManager.LogToFile("5d1768ec-c217-4554-bb2a-8372a09dc431", $"Failed to update messages inRoleplayDateTime with sceneTracker dateTime.");
            }
        }

        public override async Task<bool> ProcessCompletedQueryAsync()
        {
            if (!await base.ProcessCompletedQueryAsync())
            {
                backgroundQueryDbModel.Content = null;
                backgroundQueryDbModel.Status = BackgroundQueryStatus.Pending;// re-queue
                return false;
            }

            // Deserialize the generic content into a valid Scene Tracker
            try
            {
                LLMApiResponseMessage LLMmessage = messages.LastOrDefault();
                IShareableContextLink shareableContextLink = promptContext.ShareableContextLinks.FirstOrDefault(f => f.LinkedBuilder is PromptContextSceneTrackerInstrBuilder);
                if (shareableContextLink == null)
                {
                    LoggingManager.LogToFile("c1a42696-c67e-484f-86b9-e0a41f501221", $"No ShareableContextLink of type [{nameof(PromptContextSceneTrackerInstrBuilder)} found.]");
                    return false;
                }

                // Replace the scene tracker tied to this chat with the new one
                string linkedMessageId = shareableContextLink.Value as string;
                CreateSceneTrackerQueryModel queryModel = new()
                {
                    ChatId = backgroundQueryDbModel.ChatId,
                    LinkMessageId = linkedMessageId,
                    Content = LLMmessage.Content,
                };

                SceneTrackerDbModel updatedMessageInStorage = await storageService.CreateOrUpdateSceneTrackerAsync(queryModel);
                if (updatedMessageInStorage == null)
                {
                    LoggingManager.LogToFile("c352fa3d-7019-4ed1-923a-d4b17db6d7a1", $"Couldn't complete backgroundTask [{backgroundQueryDbModel.BackgroundQueryId}] of Type [{tag}]. Couldn't update storage. Skipping.");
                    backgroundQueryDbModel.Status = BackgroundQueryStatus.Error;
                    return false;
                }

                // Update the time tracker of the messages
                await UpdateMessagesTimeTrackerAsync(updatedMessageInStorage);

                backgroundQueryDbModel.Status = BackgroundQueryStatus.Completed;
                return true;
            } catch (Exception e)
            {
                LoggingManager.LogToFile("be1a2097-a9d4-4242-9a9d-4e60429f59df", $"Couldn't complete backgroundTask [{backgroundQueryDbModel.BackgroundQueryId}]. Task will be set to Pending status for re-generation.", e);
                backgroundQueryDbModel.Content = null;
                backgroundQueryDbModel.Status = BackgroundQueryStatus.Pending;
                return false;
            }
        }
    }
}
