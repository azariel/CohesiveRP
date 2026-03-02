using System.Runtime.CompilerServices;
using CohesiveRP.Common.BusinessObjects;
using CohesiveRP.Common.Diagnostics;
using CohesiveRP.Common.Serialization;
using CohesiveRP.Core.Services;
using CohesiveRP.Storage.DataAccessLayer.AIQueries;
using CohesiveRP.Storage.DataAccessLayer.BackgroundQueries.BusinessObjects;
using CohesiveRP.Storage.DataAccessLayer.Messages;
using CohesiveRP.Storage.DataAccessLayer.Users;
using CohesiveRP.Storage.QueryModels.Message;

namespace CohesiveRP.Core.LLMProviderManager
{
    public class LLMQueryProcessor : ILLMQueryProcessor
    {
        private IMessagesDal messagesDal;

        public LLMQueryProcessor(IMessagesDal messagesDal)
        {
            this.messagesDal = messagesDal;
        }

        /// <summary>
        /// Process the resulting completed query. If it was a 'main', it'll add a new AI message, if it was a sceneTracker, it'll attach the tracker, if it was a summary, it'll attach the summary to an existing message, etc.
        /// </summary>
        public async Task ProcessCompletedQueryAsync(BackgroundQueryDbModel selectedQuery)
        {
            if (selectedQuery == null || selectedQuery.Status != BackgroundQueryStatus.Completed.ToString())
            {
                LoggingManager.LogToFile("12498826-8f44-4f5f-ac9f-51f7de6e08fa", $"Ignoring completed background query [{selectedQuery?.BackgroundQueryId}]. Status was [{selectedQuery?.Status}].");
                return;
            }

            List<string> parsedTags = JsonCommonSerializer.DeserializeFromString<List<string>>(selectedQuery.Tags);
            if (parsedTags.Contains(BackgroundQuerySystemTags.main.ToString()))
            {
                await HandleMainWorkflowAsync(selectedQuery);
            }

            if (parsedTags.Contains(BackgroundQuerySystemTags.sceneTracker.ToString()))
            {
            }

            if (parsedTags.Contains(BackgroundQuerySystemTags.summary.ToString()))
            {
            }
        }

        private async Task HandleMainWorkflowAsync(BackgroundQueryDbModel selectedQuery)
        {
            // Add the message
            CreateMessageQueryModel messageQueryModel = new()
            {
                ChatId = selectedQuery.ChatId,
                SourceType = MessageSourceType.AI,
                MessageContent = selectedQuery.Content,
                CreatedAtUtc = DateTime.UtcNow,
            };

            await messagesDal.CreateMessageAsync(messageQueryModel);
        }
    }
}
