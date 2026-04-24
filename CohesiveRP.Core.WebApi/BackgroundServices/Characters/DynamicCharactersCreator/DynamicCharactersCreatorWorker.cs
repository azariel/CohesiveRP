using CohesiveRP.Common.Diagnostics;
using CohesiveRP.Core.Services;
using CohesiveRP.Core.WebApi.Workflows.Characters.CharacterSheets.Abstractions;
using CohesiveRP.Storage.DataAccessLayer.BackgroundQueries.BusinessObjects;
using CohesiveRP.Storage.DataAccessLayer.InteractiveUserInputQueries;
using CohesiveRP.Storage.DataAccessLayer.InteractiveUserInputQueries.BusinessObjects;
using CohesiveRP.Storage.DataAccessLayer.LLMApiQueries.BusinessObjects;
using CohesiveRP.Storage.DataAccessLayer.Settings.LLMProviders;
using CohesiveRP.Storage.QueryModels.BackgroundQuery;
using CohesiveRP.Storage.QueryModels.Chat;

namespace CohesiveRP.Core.WebApi.BackgroundServices.Characters.DynamicCharactersCreator
{
    public class DynamicCharactersCreatorWorker : BackgroundService
    {
        const int ERROR_DELAY_MS = 60000;
        const int STANDARD_DELAY_MS = 30000;
        private IStorageService storageService;
        private IRegenerateCharacterSheetWorkflow regenerateCharacterSheetWorkflow;

        public DynamicCharactersCreatorWorker(
            IStorageService storageService,
            IRegenerateCharacterSheetWorkflow regenerateCharacterSheetWorkflow
        )
        {
            this.storageService = storageService;
            this.regenerateCharacterSheetWorkflow = regenerateCharacterSheetWorkflow;
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
                    LoggingManager.LogToFile("79d3e81c-ffd9-46fc-9307-210675e7826b", $"Unhandled error in [{nameof(DynamicCharactersCreatorWorker)}].", e);
                    await Task.Delay(ERROR_DELAY_MS, stoppingToken);
                }
            }
        }

        private async Task ProcessBackgroundQueryAsync(InteractiveUserInputDbModel selectedQuery)
        {
            if (selectedQuery == null)
            {
                return;
            }

            if (selectedQuery.UserChoice == false)
            {
                // The user specified NOT to generate a character from this detected characterName, set it to completed immediately
                selectedQuery.Status = InteractiveUserInputStatus.Completed;
                await storageService.UpdateInteractiveUserInputQueryAsync(selectedQuery);
                return;
            }

            // Register a new backgroundTask to handle it (we're just delegating the whole hard work to an handler)
            CreateBackgroundQueryQueryModel backgroundQuery = new()
            {
                ChatId = selectedQuery.ChatId,
                LinkedId = selectedQuery.InteractiveUserInputQueryId,
                DependenciesTags = Enum.GetValues<BackgroundQuerySystemTags>().Except(new[] { BackgroundQuerySystemTags.shortSummary, BackgroundQuerySystemTags.mediumSummary, BackgroundQuerySystemTags.longSummary, BackgroundQuerySystemTags.extraSummary, BackgroundQuerySystemTags.overflowSummary, BackgroundQuerySystemTags.dynamicCharacterCreation }).Select(s => s.ToString()).ToList(),
                Tags = [BackgroundQuerySystemTags.dynamicCharacterCreation.ToString()],
                Priority = BackgroundQueryPriority.Lowest,
            };

            var dbModel = await storageService.AddBackgroundQueryAsync(backgroundQuery);
            if (dbModel == null)
                return;

            selectedQuery.Status = InteractiveUserInputStatus.WaitingOnBackgroundTask;
            await storageService.UpdateInteractiveUserInputQueryAsync(selectedQuery);
        }

        /// <summary>
        /// Not really locking the row in storage, but 'reserving it'
        /// </summary>
        private async Task<InteractiveUserInputDbModel> LockNextPendingQueryIfAnyAsync()
        {
            var allPendingQueries = await storageService.GetInteractiveUserInputQueriesAsync(g => g.Status == InteractiveUserInputStatus.Pending || g.Status == InteractiveUserInputStatus.Error);
            if (allPendingQueries.Length <= 0)
            {
                return null;
            }

            // Filter only those that can run on an LLM Api that is currently free (idle)
            List<InteractiveUserInputDbModel> validQueries = new();

            var runningQueries = await storageService.GetQueriesOnLLMApisAsync(null);
            runningQueries = runningQueries?.Where(w => w.Status == LLMApiQueryStatus.Running).ToArray();

            // If there's any running queries beside summaries, we'll delay
            if (runningQueries != null && runningQueries.Any(w => w.Tag != ChatCompletionPresetType.Summarize.ToString() && w.Tag != ChatCompletionPresetType.SummariesMerge.ToString()))
            {
                return null;
            }

            var globalSettings = await storageService.GetGlobalSettingsAsync();
            var listOfProviders = globalSettings.LLMProviders?.Where(w => w.Tags.Contains(ChatCompletionPresetType.DynamicCharacterCreation))?.ToArray();
            if (listOfProviders.Length <= 0)
            {
                return null;
            }

            List<LLMProviderConfig> providersToUse = new List<LLMProviderConfig>();
            foreach (var provider in listOfProviders)
            {
                // check for concurrency limitation
                int nbConcurrentQueriesQueuedUpOnThisProvider = runningQueries?.Count(c => c.LLMProviderConfigId == provider.ProviderConfigId) ?? 0;
                if (nbConcurrentQueriesQueuedUpOnThisProvider >= provider.ConcurrencyLimit)
                {
                    continue;
                }

                providersToUse.Add(provider);
            }

            // If there's no available providers, delay and retry later
            if (providersToUse.Count <= 0)
            {
                return null;
            }

            var clashingBackgroundQueries = await storageService.GetPendingOrProcessingBackgroundQueryAsync();
            InteractiveUserInputDbModel selectedQuery = null;
            foreach (InteractiveUserInputDbModel pendingQuery in allPendingQueries)
            {
                // Check if that query is allowed to run, we only want it to run when there's basically nothing going on to avoid delaying chatting
                var sameChatClashingBackgroundQueries = clashingBackgroundQueries.Where(w => w.ChatId == pendingQuery.ChatId).ToArray();
                if (sameChatClashingBackgroundQueries.Length > 0)
                {
                    continue;
                }

                selectedQuery = pendingQuery;
                break;
            }

            if (selectedQuery == null)
            {
                return null;
            }

            // change status
            selectedQuery.Status = InteractiveUserInputStatus.Processing;

            if (!await storageService.UpdateInteractiveUserInputQueryAsync(selectedQuery))
            {
                LoggingManager.LogToFile("0e7a6a0d-b8a4-45e9-b700-c1e827d3c02a", $"Failed to update background status of query [{selectedQuery.InteractiveUserInputQueryId}] to [{InteractiveUserInputStatus.Processing}]. Ignoring this query.");
                return null;
            }

            return selectedQuery;
        }
    }
}
