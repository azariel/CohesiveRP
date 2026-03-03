using System.Diagnostics;
using CohesiveRP.Common.Diagnostics;
using CohesiveRP.Common.Serialization;
using CohesiveRP.Core.LLMProviderManager;
using CohesiveRP.Core.LLMProviderManager.BusinessObjects;
using CohesiveRP.Storage.DataAccessLayer.AIQueries;
using CohesiveRP.Storage.DataAccessLayer.BackgroundQueries.BusinessObjects;
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
            //this.llmQueryProcessor = llmQueryProcessor;
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

            if(queryProcessor == null)
            {
                LoggingManager.LogToFile("fbf44e69-5367-49fe-86ba-ae306420a961", $"LLMProviderQueryerFactory couldn't generate a valid QueryProcessor in [{nameof(BackgroundQueriesWorker)}].");
                return;
            }

            await queryProcessor.QueueProcessAsync();

            // Start a new thread and execute the task asynchronously
            var cancellationToken = new CancellationTokenSource(new TimeSpan(0, 30, 0)).Token;// 30 minutes max
            _ = Task.Run(async () =>
            {
                var finalStatus = BackgroundQueryStatus.InProgress;
                try
                {
                    while (true)
                    {
                        if (selectedQuery.Status == BackgroundQueryStatus.Completed.ToString() || selectedQuery.Status == BackgroundQueryStatus.Error.ToString())
                        {
                            // process the resulting completed query. If it was a 'main', it'll add a new AI message, if it was a sceneTracker, it'll attach the tracker, if it was a summary, it'll attach the summary to an existing message, etc.
                            await queryProcessor.ProcessCompletedQueryAsync(selectedQuery);
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
            return new List<string>() { "main", "sceneTracker" };
        }

        /// <summary>
        /// Not really locking the row in storage, but 'reserving it'... TODO: investigate this... we're hanlding this at the logical level currently, we need more safeguards (conditional updates are not a thing in sqlite?..)
        /// </summary>
        private async Task<BackgroundQueryDbModel> LockNextPendingQueryIfAnyAsync(string[] tagsAllowedByIdleProviders)
        {
            var allPendingQueries = await backgroundQueriesDal.GetAllPendingQueriesAsync();

            if (allPendingQueries.Length <= 0)
            {
                return null;
            }

            // Filter only those that have no dependencies currently queued up
            List<BackgroundQueryDbModel> validQueries = new();
            foreach (var query in allPendingQueries)
            {
                if (query.DependenciesTags != null)
                {
                    string[] dependentTags = JsonCommonSerializer.DeserializeFromString<string[]>(query.DependenciesTags);

                    if (allPendingQueries.Any(a => dependentTags.Any(an => a.Tags.Contains(an))))
                    {
                        continue;
                    }
                }

                validQueries.Add(query);
            }

            // All queued queries depend on each other (possible, if we're waiting on a background process to add a query to unblock them)
            BackgroundQueryDbModel selectedQuery = validQueries.FirstOrDefault();
            if (validQueries.Count > 1)
            {
                // Filter by the tags allowed by the providers. Only the tags tied to idle providers (or provider with sufficient concurrency) are accepted
                validQueries = validQueries.Where(w => tagsAllowedByIdleProviders.Any(a => w.Tags.Contains(a))).ToList();

                if (validQueries.Count <= 0)
                {
                    return null;
                }

                // Select by priority
                BackgroundQueryDbModel[] priorityQueries = validQueries.Where(w => w.Priority == validQueries.Max(m => m.Priority)).ToArray();
                selectedQuery = priorityQueries.FirstOrDefault();

                if (selectedQuery == null)
                {
                    return null;
                }
            }

            // change status
            selectedQuery.Status = BackgroundQueryStatus.InProgress.ToString();
            if (!await backgroundQueriesDal.UpdateBackgroundQueryAsync(selectedQuery))
            {
                LoggingManager.LogToFile("4be430da-d967-4277-b2c0-86d7ef2380b9", $"Failed to update background status of query [{selectedQuery.BackgroundQueryId}] to [{BackgroundQueryStatus.InProgress}]. Ignoring this query.");
                return null;
            }

            return selectedQuery;
        }
    }
}
