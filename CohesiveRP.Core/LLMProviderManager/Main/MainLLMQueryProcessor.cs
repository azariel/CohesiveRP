using CohesiveRP.Common.BusinessObjects;
using CohesiveRP.Common.Diagnostics;
using CohesiveRP.Core.PromptContext.Abstractions;
using CohesiveRP.Core.PromptContext.Builders;
using CohesiveRP.Storage.DataAccessLayer.AIQueries;
using CohesiveRP.Storage.DataAccessLayer.BackgroundQueries.BusinessObjects;
using CohesiveRP.Storage.DataAccessLayer.Messages;
using CohesiveRP.Storage.QueryModels.Message;

namespace CohesiveRP.Core.LLMProviderManager.Main
{
    public class MainLLMQueryProcessor : ILLMQueryProcessor
    {
        private BackgroundQueryDbModel queryDbModel;
        private IPromptContextBuilderFactory contextBuilderFactory;
        private IPromptContextElementBuilderFactory promptContextElementBuilderFactory;
        private IMessagesDal messagesDal;

        public MainLLMQueryProcessor(BackgroundQueryDbModel queryDbModel, IPromptContextBuilderFactory contextBuilderFactory, IPromptContextElementBuilderFactory promptContextElementBuilderFactory, IMessagesDal messagesDal)
        {
            this.queryDbModel = queryDbModel;
            this.contextBuilderFactory = contextBuilderFactory;
            this.promptContextElementBuilderFactory = promptContextElementBuilderFactory;
            this.messagesDal = messagesDal;
        }

        private async Task ProcessMainQueryAsync()
        {
            var promptContext = await BuildContextAsync();

            // TODO Query the LLM provider here and use the promptContext

            // for debug
            for (var i = 0; i < 100; ++i)
            {
                await Task.Delay(100);
                queryDbModel.Content += Guid.NewGuid().ToString()[0];
            }

            queryDbModel.Content += $"-COMPLETED{Environment.NewLine}{promptContext?.Value}";
            queryDbModel.Status = BackgroundQueryStatus.Completed.ToString();
        }

        private async Task<IPromptContext> BuildContextAsync()
        {
            // Generate a context builder appropriate for our MainQuery
            IPromptContextBuilder contextBuilder = await contextBuilderFactory.GenerateAsync(BackgroundQuerySystemTags.main, promptContextElementBuilderFactory);
            return await contextBuilder.BuildAsync(queryDbModel.ChatId);
        }

        public async Task<bool> QueueProcessAsync()
        {
            if(queryDbModel == null)
            {
                return false;
            }

            _ = Task.Run(async () => await ProcessMainQueryAsync());

            return true;
        }

        public async Task<BackgroundQueryDbModel> GetBackgroundQueryDbModelAsync() => queryDbModel;

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

            // Add the message
            CreateMessageQueryModel messageQueryModel = new()
            {
                ChatId = selectedQuery.ChatId,
                SourceType = MessageSourceType.AI,
                MessageContent = selectedQuery.Content,
                CreatedAtUtc = DateTime.UtcNow,
            };


            var message = await messagesDal.CreateMessageAsync(messageQueryModel);
            selectedQuery.LinkedMessageId = message.MessageId;
        }
    }
}
