using CohesiveRP.Common.Diagnostics;
using CohesiveRP.Core.Services;
using CohesiveRP.Storage.DataAccessLayer.IllustrationQueries.BusinessObjects;
using CohesiveRP.Storage.DataAccessLayer.InteractiveUserInputQueries;
using CohesiveRP.Storage.DataAccessLayer.InteractiveUserInputQueries.BusinessObjects;
using Microsoft.Extensions.Hosting;

namespace CohesiveRP.Core.WebApi.BackgroundServices.Characters.DynamicCharactersCreator
{
    public class IllustratorMainAvatarsQueriesWorker : BackgroundService
    {
        const int ERROR_DELAY_MS = 30000;
        const int STANDARD_DELAY_MS = 10000;
        private IStorageService storageService;

        public IllustratorMainAvatarsQueriesWorker(
            IStorageService storageService
        )
        {
            this.storageService = storageService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (true)
            {
                try
                {
                    // Check if there's any available query to process
                    var lockedQuery = await LockNextPendingQueryIfAnyAsync();

                    if (lockedQuery == null)
                    {
                        await Task.Delay(STANDARD_DELAY_MS, stoppingToken);
                        continue;
                    }

                    await ProcessBackgroundQueryAsync(lockedQuery);

                } catch (Exception e)
                {
                    LoggingManager.LogToFile("a9b2d533-5517-4f98-9e79-70ac6eda76b2", $"Unhandled error in [{nameof(IllustratorMainAvatarsQueriesWorker)}].", e);
                    await Task.Delay(ERROR_DELAY_MS, stoppingToken);
                }
            }
        }

        private async Task ProcessBackgroundQueryAsync(IllustrationQueryDbModel selectedQuery)
        {
            if (selectedQuery == null)
            {
                return;
            }

            bool result = RunQueryAgainstImageGeneratorAsync(selectedQuery);
            if (!result)
            {
                selectedQuery.Status = IllustratorQueryStatus.Error;
                if (!await storageService.UpdateIllustrationQueryAsync(selectedQuery))
                {
                    LoggingManager.LogToFile("331daf74-9c13-4375-a039-0f48df3e9967", $"Failed to update background status of illustrator query [{selectedQuery.IllustrationQueryId}] to [{IllustratorQueryStatus.Error}].");
                    return;
                }

                return;
            }

            selectedQuery.Status = IllustratorQueryStatus.Completed;
            if (!await storageService.UpdateIllustrationQueryAsync(selectedQuery))
            {
                LoggingManager.LogToFile("ec859574-29a5-4f18-84ae-8f1bb7024ec1", $"Failed to update background status of illustrator query [{selectedQuery.IllustrationQueryId}] to [{IllustratorQueryStatus.Completed}].");
                return;
            }

            return;
        }

        private bool RunQueryAgainstImageGeneratorAsync(IllustrationQueryDbModel selectedQuery)
        {
            // TODO: Run the query against ComfyUI (or any other image provider, but let's start with ComfyUI) We should probably have a globalConfig around illustrators to define the type, tags, workflows, variables, etc.
            // TODO: check if comfyUI is ready to process the query
            return true;
        }

        /// <summary>
        /// Not really locking the row in storage, but 'reserving it'
        /// </summary>
        private async Task<IllustrationQueryDbModel> LockNextPendingQueryIfAnyAsync()
        {
            var allPendingQueries = await storageService.GetIllustrationQueriesAsync(g => g.Status == IllustratorQueryStatus.Pending || g.Status == IllustratorQueryStatus.Error);
            if (allPendingQueries.Length <= 0)
            {
                return null;
            }

            // Filter only those that can run on an Image generator provider and then select the first one (highest priority + oldest one)
            // TODO
            var selectedQuery = allPendingQueries.First();

            // change status
            selectedQuery.Status = IllustratorQueryStatus.Processing;

            if (!await storageService.UpdateIllustrationQueryAsync(selectedQuery))
            {
                LoggingManager.LogToFile("394a5c3a-6f07-497f-bcca-637a10f0c88f", $"Failed to update background status of illustrator query [{selectedQuery.IllustrationQueryId}] to [{IllustratorQueryStatus.Processing}]. Ignoring this query.");
                return null;
            }

            return selectedQuery;
        }
    }
}
