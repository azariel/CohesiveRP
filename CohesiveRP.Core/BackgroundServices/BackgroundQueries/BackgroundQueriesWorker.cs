using CohesiveRP.Common.Diagnostics;
using CohesiveRP.Core.LLMProviderManager;
using CohesiveRP.Storage.DataAccessLayer.AIQueries;
using CohesiveRP.Storage.DataAccessLayer.BackgroundQueries.BusinessObjects;
using CohesiveRP.Storage.QueryModels.Chat;
using Microsoft.Extensions.Hosting;

namespace CohesiveRP.Core.BackgroundServices.BackgroundQueries
{
    public class BackgroundQueriesWorker : BackgroundService
    {
        const int ERROR_DELAY_MS = 5000;
        const int STANDARD_DELAY_MS = 1000;
        private IBackgroundQueriesDal backgroundQueriesDal;
        private ILLMProviderQueryerFactory llmProviderQueryerFactory;

        public BackgroundQueriesWorker(IBackgroundQueriesDal backgroundQueriesDal, ILLMProviderQueryerFactory llmProviderQueryerFactory)
        {
            this.backgroundQueriesDal = backgroundQueriesDal;
            this.llmProviderQueryerFactory = llmProviderQueryerFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (true)
            {
                try
                {
                    // Check if there's any available background query to process
                    var allowedTags = await GetTagsFromIdleProvidersAsync();
                    var lockedQuery = await LockNextPendingQueryIfAnyAsync(allowedTags.ToArray());

                    if (lockedQuery == null)
                    {
                        await Task.Delay(STANDARD_DELAY_MS, stoppingToken);
                        continue;
                    }

                    await ProcessBackgroundQueryAsync(lockedQuery);

                } catch (Exception e)
                {
                    LoggingManager.LogToFile("0bb1723d-3790-44a3-8065-8c9356d75941", $"Unhandled error in [{nameof(BackgroundQueriesWorker)}].", e);
                    await Task.Delay(ERROR_DELAY_MS, stoppingToken);
                }
            }
        }

        private async Task ProcessBackgroundQueryAsync(BackgroundQueryDbModel selectedQuery)
        {
            if (selectedQuery == null)
            {
                return;
            }

            // Register the task in the main thread to avoid desyncs
            string queryId = Guid.NewGuid().ToString();
            var queryProcessor = llmProviderQueryerFactory.Generate(selectedQuery);

            if (queryProcessor == null)
            {
                LoggingManager.LogToFile("fbf44e69-5367-49fe-86ba-ae306420a961", $"LLMProviderQueryerFactory couldn't generate a valid QueryProcessor in [{nameof(BackgroundQueriesWorker)}].");
                return;
            }

            if(!await queryProcessor.QueueProcessAsync())
            {
                LoggingManager.LogToFile("6bcabdd1-63e4-44dc-bebe-09481441edab", $"LLMProviderQueryerFactory failed to queue the process of the query [{selectedQuery.BackgroundQueryId}].");
                return;
            }

            // Start a new thread and execute the task asynchronously
            var cancellationToken = new CancellationTokenSource(new TimeSpan(0, 30, 0)).Token;// 30 minutes max
            _ = Task.Run(async () =>
            {
                var finalStatus = BackgroundQueryStatus.InProgress;
                try
                {
                    while (true)
                    {
                        if (selectedQuery.Status == BackgroundQueryStatus.Completed || selectedQuery.Status == BackgroundQueryStatus.Error || selectedQuery.Status == BackgroundQueryStatus.ProcessingFinalInstruction)
                        {
                            if(selectedQuery.Status == BackgroundQueryStatus.Error)
                            {
                                // Nothing to do, simly save the state and drop the query altogether
                                break;
                            }

                            // process the resulting completed query. If it was a 'main', it'll add a new AI message, if it was a sceneTracker, it'll attach the tracker, if it was a summary, it'll attach the summary to an existing message, etc.
                            await queryProcessor.ProcessCompletedQueryAsync();

                            // TODO: if query was set to completed status, set a TTL for auto-delete

                            break;
                        }

                        if (selectedQuery.Status == BackgroundQueryStatus.Pending)
                        {
                            // BackgroundQuery task was re-queued. Drop current after a timeout.
                            await Task.Delay(ERROR_DELAY_MS);
                            break;
                        }

                        await backgroundQueriesDal.UpdateBackgroundQueryAsync(selectedQuery);
                        await Task.Delay(500);
                        cancellationToken.ThrowIfCancellationRequested();
                    }
                } finally
                {
                    if (!await backgroundQueriesDal.UpdateBackgroundQueryAsync(selectedQuery))
                    {
                        LoggingManager.LogToFile("bde82901-e86d-4481-ae76-05c0de13bfb8", $"Failed to update background status of query [{selectedQuery.BackgroundQueryId}] to [{finalStatus}]. Ignoring this query.");
                    }
                }
            });
        }

        /// <summary>
        /// Get the tags from the idle providers or the providers with concurrency space (ex: if a provider can execute 3 queries simulteanously and we're at 2/3).
        /// </summary>
        /// <returns>Tags tied to idle providers (or provider with sufficient concurrency)</returns>
        private async Task<List<string>> GetTagsFromIdleProvidersAsync()
        {
            // FOR DEBUG
            return Enum.GetValues<ChatCompletionPresetType>().Select(s => s.ToString().ToLowerInvariant()).ToList();
        }

        /// <summary>
        /// Not really locking the row in storage, but 'reserving it'... TODO: investigate this... we're hanlding this at the logical level currently, we need more safeguards (conditional updates are not a thing in sqlite?..)
        /// </summary>
        private async Task<BackgroundQueryDbModel> LockNextPendingQueryIfAnyAsync(string[] tagsAllowedByIdleProviders)
        {
            //var allPendingQueries = await backgroundQueriesDal.GetAllPendingQueriesAsync();
            var allProcessingQueries = await backgroundQueriesDal.GetPendingOrProcessingBackgroundQueryAsync();

            if (allProcessingQueries.Length <= 0)
            {
                return null;
            }

            // Filter only those that have no dependencies currently queued up
            //List<BackgroundQueryDbModel> inProgressQueries = allProcessingQueries.Where(w => w.Status == BackgroundQueryStatus.InProgress || w.Status == BackgroundQueryStatus.ProcessingFinalInstruction).ToList();
            List<BackgroundQueryDbModel> validQueries = new();
            foreach (var query in allProcessingQueries)
            {
                if (query.DependenciesTags != null)
                {
                    if (query.DependenciesTags != null && allProcessingQueries.Any(a => query.DependenciesTags.Any(an => a.Tags.Contains(an))))
                    {
                        continue;
                    }
                }

                if (query.Status == BackgroundQueryStatus.Pending || query.Status == BackgroundQueryStatus.ProcessedWaitingForFinalInstruction)
                    validQueries.Add(query);
            }

            // All queued queries depend on each other (possible, if we're waiting on a background process to add a query to unblock them)
            BackgroundQueryDbModel selectedQuery = validQueries.FirstOrDefault();
            if (validQueries.Count > 1)
            {
                // Filter by the tags allowed by the providers. Only the tags tied to idle providers (or provider with sufficient concurrency) are accepted
                validQueries = validQueries.Where(w => tagsAllowedByIdleProviders.Any(a => w.Tags.Select(s=>s.ToLowerInvariant()).Contains(a))).ToList();

                if (validQueries.Count <= 0)
                {
                    return null;
                }

                // Select by priority
                BackgroundQueryDbModel[] priorityQueries = validQueries.Where(w => w.Priority == validQueries.Max(m => m.Priority)).ToArray();
                selectedQuery = priorityQueries?.OrderBy(o=>o.CreatedAtUtc).FirstOrDefault();

                if (selectedQuery == null)
                {
                    return null;
                }
            }

            if (validQueries.Count <= 0)
            {
                return null;
            }

            // change status
            if (selectedQuery.Status == BackgroundQueryStatus.Pending)
            {
                selectedQuery.Status = BackgroundQueryStatus.InProgress;
            }

            if (selectedQuery.Status == BackgroundQueryStatus.ProcessedWaitingForFinalInstruction)
            {
                selectedQuery.Status = BackgroundQueryStatus.ProcessingFinalInstruction;
            }

            if (!await backgroundQueriesDal.UpdateBackgroundQueryAsync(selectedQuery))
            {
                LoggingManager.LogToFile("4be430da-d967-4277-b2c0-86d7ef2380b9", $"Failed to update background status of query [{selectedQuery.BackgroundQueryId}] to [{BackgroundQueryStatus.InProgress}]. Ignoring this query.");
                return null;
            }

            return selectedQuery;
        }
    }
}
