using CohesiveRP.Common.BusinessObjects;
using CohesiveRP.Common.Configuration;
using CohesiveRP.Common.Diagnostics;
using CohesiveRP.Core.ComfyUI;
using CohesiveRP.Core.ComfyUI.Client;
using CohesiveRP.Core.Services;
using CohesiveRP.Storage.DataAccessLayer.Chats;
using CohesiveRP.Storage.DataAccessLayer.IllustrationQueries.BusinessObjects;
using CohesiveRP.Storage.DataAccessLayer.InteractiveUserInputQueries;
using CohesiveRP.Storage.DataAccessLayer.Pathfinder.ChatCharactersRolls.BusinessObjects;
using CohesiveRP.Storage.DataAccessLayer.SceneTracker.BusinessObjects;
using CohesiveRP.Storage.DataAccessLayer.Users;
using Microsoft.Extensions.Hosting;

namespace CohesiveRP.Core.WebApi.BackgroundServices.Characters.DynamicCharactersCreator
{
    public class IllustratorMainAvatarsQueriesWorker : BackgroundService
    {
        const int ERROR_DELAY_MS = 30000;
        const int STANDARD_DELAY_MS = 10000;
        private IStorageService storageService;
        private IComfyUiClient comfyUiClient;
        private ComfyUiEndpointConfig config;

        public IllustratorMainAvatarsQueriesWorker(
            IStorageService storageService,
            IComfyUiClient comfyUiClient,
            ComfyUiEndpointConfig config
        )
        {
            this.storageService = storageService;
            this.comfyUiClient = comfyUiClient;
            this.config = config;
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

            bool result = await RunQueryAgainstImageGeneratorAsync(selectedQuery);
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

        private async Task<bool> RunQueryAgainstImageGeneratorAsync(IllustrationQueryDbModel selectedQuery)
        {
            // TODO: Run the query against ComfyUI (or any other image provider, but let's start with ComfyUI) We should probably have a globalConfig around illustrators to define the type, tags, workflows, variables, etc.
            // TODO: check if comfyUI is ready to process the query

            try
            {
                var characterDbModel = await storageService.GetCharacterByIdAsync(selectedQuery.CharacterId);
                if (characterDbModel?.ImageGenerationConfiguration == null)
                    return false;

                CharacterSheet characterSheet = null;
                var characterSheets = await storageService.GetCharacterSheetsInstanceByChatIdAsync(selectedQuery.ChatId);
                var characterSheetInstance = characterSheets?.CharacterSheetInstances?.FirstOrDefault(f => f.CharacterId == selectedQuery.CharacterId);
                if (characterSheetInstance?.CharacterSheet == null)
                {
                    var characterSheetDbModel = await storageService.GetCharacterSheetByCharacterIdAsync(selectedQuery.CharacterId);

                    if (characterSheetDbModel == null)
                        return false;

                    characterSheet = characterSheetDbModel.CharacterSheet;
                } else
                {
                    characterSheet = characterSheetInstance.CharacterSheet;
                }

                string templateJson = await File.ReadAllTextAsync("Workflows/CohesiveRP-MainAvatarGenerator-v1.0.api.json");

                bool allSucceeded = true;
                bool atLeastOneGenerated = false;

                foreach (var outfit in characterDbModel.ImageGenerationConfiguration.IllustrationMapOutfits.Where(o => !string.IsNullOrWhiteSpace(o.IllustratorPromptInjection)))
                {
                    string rawFolder = $"{WebConstants.CharactersAvatarFilePath}\\{characterDbModel.Name.ToLowerInvariant()}\\raws\\{outfit.Outfit.ToString().ToLowerInvariant()}";
                    if (!Directory.Exists(rawFolder))
                    {
                        Directory.CreateDirectory(rawFolder);
                    }

                    // Limit the images by outfit to 3 to avoid generating TB of stuff TODO: make this configurable
                    int nbImagesToGenerate = 3 - Directory.EnumerateFiles(rawFolder, "*.*", SearchOption.TopDirectoryOnly).Count();
                    if (nbImagesToGenerate <= 0)
                        continue;

                    for (int i = 0; i < nbImagesToGenerate; i++)
                    {
                        int seedToUse = new Random().Next(1, int.MaxValue);
                        if (!Enum.TryParse(characterSheet.AgeGroupAppearance, out AgeGroupAppearance ageGroupAppearance))
                        {
                            ageGroupAppearance = AgeGroupAppearance.Adult;
                        }

                        string genderedTags = "";
                        if (new[] { "male", "boy", "man" }.Any(a => a.Equals(characterSheet.Gender.Trim(), StringComparison.OrdinalIgnoreCase)))
                        {
                            switch (ageGroupAppearance)
                            {
                                case AgeGroupAppearance.Infant:
                                    genderedTags = "1boy, boy, infant, children";
                                    break;
                                case AgeGroupAppearance.Toddler:
                                    genderedTags = "1boy, boy, toddler, children";
                                    break;
                                case AgeGroupAppearance.Children:
                                    genderedTags = "1boy, boy, children";
                                    break;
                                case AgeGroupAppearance.Teenager:
                                    genderedTags = "1boy, boy, teenager, children";
                                    break;
                                case AgeGroupAppearance.YoungAdult:
                                    genderedTags = "1man, man, adult, teenager";
                                    break;
                                case AgeGroupAppearance.Elderly:
                                    genderedTags = "1man, man, senior, elderly";
                                    break;
                                case AgeGroupAppearance.Adult:
                                default:
                                    genderedTags = "1man, man, adult";
                                    break;
                            }
                        } else
                        {
                            switch (ageGroupAppearance)
                            {
                                case AgeGroupAppearance.Infant:
                                    genderedTags = "1girl, girl, infant, children";
                                    break;
                                case AgeGroupAppearance.Toddler:
                                    genderedTags = "1girl, girl, toddler, children";
                                    break;
                                case AgeGroupAppearance.Children:
                                    genderedTags = "1girl, girl, children";
                                    break;
                                case AgeGroupAppearance.Teenager:
                                    genderedTags = "1girl, girl, teenager, children";
                                    break;
                                case AgeGroupAppearance.YoungAdult:
                                    genderedTags = "1woman, woman, adult, teenager";
                                    break;
                                case AgeGroupAppearance.Elderly:
                                    genderedTags = "1woman, woman, senior, elderly";
                                    break;
                                case AgeGroupAppearance.Adult:
                                default:
                                    genderedTags = "1woman, woman, adult";
                                    break;
                            }
                        }

                        string workflowJson = new MainAvatarWorkflowInjector(templateJson).Inject(new AvatarGenerationRequest
                        {
                            CharacterId = selectedQuery.CharacterId,
                            PositivePrompt = $"score_9,masterpiece,best quality,amazing quality,score_8_up,score_7_up,{genderedTags},solo,upper body,facing viewer,looking at viewer,standing,straight-on,{Environment.NewLine}{outfit.IllustratorPromptInjection}{Environment.NewLine}neutral expression,good head proportions,black background,simple background,detailed face,beautiful eyes,nsfw,explicit",
                            NegativePromptOverride = "CyberRealistic_Negative-neg,UnrealisticDream,BadDream,(deformed iris,deformed pupils,semi-realistic,cgi,3d,render),text,cropped,out of frame,worst quality,low quality,jpeg artifacts,ugly,duplicate,morbid,username,mutilated,extra fingers,mutated,mutated hands,poorly drawn hands,poorly drawn face,mutation,deformed,blurry,dehydrated,bad anatomy,bad proportions,extra limbs,cloned face,disfigured,gross proportions,large head,malformed limbs,missing arms,missing legs,extra arms,extra legs,fused fingers,too many fingers,long neck,bad quality,pixelated,deformed faces,deformed body,blurry,extra body parts,extra finges,extra arms,pink,worst quality ,bad-hands,(bad art: 1.4),bad eye,bad eyes,deformed eyes,bad face,deformed face,blond hair,score_6,score_5,score_4,(worst quality:1.2),(low quality:1.2),(normal quality:1.2),lowres,bad anatomy,bad hands,signature,watermarks,ugly,imperfect eyes,skewed eyes,unnatural face,unnatural body,error,extra limb,missing limbs,extra digits,fewer digits,score_6,score_5,score_4,simplified,abstract,unrealistic,impressionistic,low resolution,lowres,bad anatomy,bad hands,missing fingers,normal quality,worst detail,illustration,sketch,artificial,poor quality,colorful background,detailed background,gradient background,outdoor,indoors,scenery,landscape,sky,trees,street,censor",
                            HeightOverride = 1216,
                            WidthOverride = 832,
                            Seed = seedToUse
                        });

                        using var cts = new CancellationTokenSource(TimeSpan.FromMinutes(config.ExecutionTimeoutMinutes));

                        await comfyUiClient.ConnectAsync(cts.Token);
                        string promptId = await comfyUiClient.SubmitAsync(workflowJson, cts.Token);
                        ComfyUiOutputFile file = await comfyUiClient.WaitAsync(promptId, cts.Token);
                        byte[] image = await comfyUiClient.DownloadAsync(file, cts.Token);

                        await File.WriteAllBytesAsync($"{rawFolder}\\c_s_{seedToUse}.png", image, cts.Token);
                        atLeastOneGenerated = true;
                    }
                }

                // Set a main Avatar if none were already selected
                if (!File.Exists($"{WebConstants.CharactersAvatarFilePath}\\{characterDbModel.Name.ToLowerInvariant()}\\avatar.png") || atLeastOneGenerated)
                {
                    // Select the oldest file in the raws/clothed folder
                    var clothedFolder = $"{WebConstants.CharactersAvatarFilePath}\\{characterDbModel.Name.ToLowerInvariant()}\\raws\\{ClothingStateOfDress.Clothed.ToString().ToLowerInvariant()}";
                    var oldestFile = Directory.GetFiles(clothedFolder).OrderBy(f => File.GetCreationTimeUtc(f)).FirstOrDefault();
                    if (oldestFile != null)
                    {
                        string outFilePath = $"{WebConstants.CharactersAvatarFilePath}\\{characterDbModel.Name.ToLowerInvariant()}\\avatar.png";
                        if (File.Exists(outFilePath))
                            File.Delete(outFilePath);

                        File.Copy(oldestFile, outFilePath);
                    }
                }

                return allSucceeded;
            } catch (Exception E)
            {
                LoggingManager.LogToFile("b87b81c7-11b9-4571-92ae-80bc5bd27db2", $"Error while running illustrator query [{selectedQuery.IllustrationQueryId}] against image generator.", E);
                return false;
            }
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

            allPendingQueries = allPendingQueries.OrderByDescending(o => o.Status).ThenBy(t => t.CreatedAtUtc).ToArray();

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
