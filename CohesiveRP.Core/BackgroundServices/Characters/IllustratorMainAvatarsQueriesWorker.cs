using System.Text.RegularExpressions;
using CohesiveRP.Common.BusinessObjects;
using CohesiveRP.Common.Configuration;
using CohesiveRP.Common.Diagnostics;
using CohesiveRP.Core.ComfyUI;
using CohesiveRP.Core.ComfyUI.Client;
using CohesiveRP.Core.Services;
using CohesiveRP.Core.Utils.Characters;
using CohesiveRP.Storage.DataAccessLayer.Characters.BusinessObjects;
using CohesiveRP.Storage.DataAccessLayer.Chats;
using CohesiveRP.Storage.DataAccessLayer.IllustrationQueries.BusinessObjects;
using CohesiveRP.Storage.DataAccessLayer.InteractiveUserInputQueries;
using CohesiveRP.Storage.DataAccessLayer.Pathfinder.ChatCharactersRolls.BusinessObjects;
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
                    var (lockedQuery, originalStatus) = await LockNextPendingQueryIfAnyAsync();

                    if (lockedQuery == null)
                    {
                        await Task.Delay(STANDARD_DELAY_MS, stoppingToken);
                        continue;
                    }

                    using var healthCts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
                    bool comfyAvailable = await comfyUiClient.IsAvailableAsync(healthCts.Token);

                    if (!comfyAvailable)
                    {
                        LoggingManager.LogToFile("bd492c4c-e2af-4846-a6cd-fe51c0da84d9", $"ComfyUI is unreachable. Reverting query [{lockedQuery.IllustrationQueryId}] to [{IllustratorQueryStatus.Pending}] and pausing for 10 minutes.");

                        lockedQuery.Status = originalStatus;
                        if (!await storageService.UpdateIllustrationQueryAsync(lockedQuery))
                        {
                            LoggingManager.LogToFile("fd8f62ff-99b4-4238-9355-1b2b56a6d816", $"Failed to revert query [{lockedQuery.IllustrationQueryId}] to Pending while ComfyUI was down. It will remain in Processing.");
                        }

                        await Task.Delay(TimeSpan.FromMinutes(10), stoppingToken);
                        continue;
                    }

                    await ProcessBackgroundQueryAsync(lockedQuery, originalStatus);

                } catch (Exception e)
                {
                    LoggingManager.LogToFile("a9b2d533-5517-4f98-9e79-70ac6eda76b2", $"Unhandled error in [{nameof(IllustratorMainAvatarsQueriesWorker)}].", e);
                    await Task.Delay(ERROR_DELAY_MS, stoppingToken);
                }
            }
        }

        private async Task ProcessBackgroundQueryAsync(IllustrationQueryDbModel selectedQuery, IllustratorQueryStatus originalStatus)
        {
            if (selectedQuery == null)
            {
                return;
            }

            bool result = await RunQueryAgainstImageGeneratorAsync(selectedQuery, originalStatus);
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

            selectedQuery.Status = originalStatus == IllustratorQueryStatus.PartialCompletion ? IllustratorQueryStatus.Completed : IllustratorQueryStatus.PartialCompletion;
            if (!await storageService.UpdateIllustrationQueryAsync(selectedQuery))
            {
                LoggingManager.LogToFile("ec859574-29a5-4f18-84ae-8f1bb7024ec1", $"Failed to update background status of illustrator query [{selectedQuery.IllustrationQueryId}] to [{IllustratorQueryStatus.Completed}].");
                return;
            }

            return;
        }

        private async Task<bool> RunQueryAgainstImageGeneratorAsync(IllustrationQueryDbModel selectedQuery, IllustratorQueryStatus originalStatus)
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

                bool allSucceeded = true;
                bool atLeastOneGenerated = false;
                (bool flowControl, bool value) = await GenerateAvatarsForEachOutfitsForEachExpressions(selectedQuery, characterDbModel, characterSheet, 1);
                if (!flowControl)
                    return value;

                atLeastOneGenerated = value;
                if (originalStatus == IllustratorQueryStatus.PartialCompletion)
                {
                    (bool flowControl2, bool value2) = await GenerateAvatarsForEachOutfitsForEachExpressions(selectedQuery, characterDbModel, characterSheet, -1);
                    if (!flowControl2)
                        return value2;

                    atLeastOneGenerated = value || value2;
                }

                CharacterAvatarsUtils.RefreshDefaultAvatars($"{WebConstants.CharactersAvatarFilePath}\\{characterDbModel.Name.ToLowerInvariant()}", atLeastOneGenerated);
                return allSucceeded;
            } catch (Exception E)
            {
                LoggingManager.LogToFile("b87b81c7-11b9-4571-92ae-80bc5bd27db2", $"Error while running illustrator query [{selectedQuery.IllustrationQueryId}] against image generator.", E);
                return false;
            }
        }

        private async Task<(bool flowControl, bool value)> GenerateAvatarsForEachOutfitsForEachExpressions(IllustrationQueryDbModel selectedQuery, CharacterDbModel characterDbModel, CharacterSheet characterSheet, int limitNbAvatarByExpressionToThisNumber)
        {
            bool atLeastOneGenerated = false;
            foreach (IllustrationMapOutfit outfit in characterDbModel.ImageGenerationConfiguration.IllustrationMapOutfits.Where(o => !string.IsNullOrWhiteSpace(o.IllustratorPromptInjection)))
            {
                if (selectedQuery.Outfit != null && selectedQuery.Outfit != outfit.Outfit)
                {
                    continue;
                }

                if (selectedQuery.Expressions == null || selectedQuery.Expressions.Count <= 0)
                {
                    if (!File.Exists(CoreConstants.WORKFLOW_GENERATE_SOURCE_AVATAR))
                    {
                        LoggingManager.LogToFile("e12a864c-fc5e-4a2f-ae3b-d80bdc920cc3", $"Workflow file [{CoreConstants.WORKFLOW_GENERATE_SOURCE_AVATAR}] not found for illustrator query [{selectedQuery.IllustrationQueryId}].");
                        await Task.Delay(ERROR_DELAY_MS);
                        return (flowControl: false, value: false);
                    }

                    string templateJson = await File.ReadAllTextAsync(CoreConstants.WORKFLOW_GENERATE_SOURCE_AVATAR);

                    (bool flowControl, atLeastOneGenerated) = await GenerateSourceAvatars(selectedQuery, characterDbModel, characterSheet, templateJson, outfit);
                    if (!flowControl)
                        continue;
                } else
                {
                    if (!File.Exists(CoreConstants.WORKFLOW_GENERATE_EXPRESSION_AVATAR))
                    {
                        LoggingManager.LogToFile("b9b5f443-e594-448a-8434-a889216e7c59", $"Workflow file [{CoreConstants.WORKFLOW_GENERATE_EXPRESSION_AVATAR}] not found for illustrator query [{selectedQuery.IllustrationQueryId}].");
                        await Task.Delay(ERROR_DELAY_MS);
                        return (flowControl: false, value: false);
                    }

                    string templateJson = await File.ReadAllTextAsync(CoreConstants.WORKFLOW_GENERATE_EXPRESSION_AVATAR);

                    (bool flowControl, atLeastOneGenerated) = await GenerateExpressionAvatars(selectedQuery, characterDbModel, characterSheet, templateJson, outfit, limitNbAvatarByExpressionToThisNumber);
                    if (!flowControl)
                        continue;
                }
            }

            return (flowControl: true, value: default);
        }

        private async Task<(bool flowControl, bool value)> GenerateExpressionAvatars(IllustrationQueryDbModel selectedQuery, CharacterDbModel characterDbModel, CharacterSheet characterSheet, string templateJson, IllustrationMapOutfit outfit, int limitNbAvatarByExpressionToThisNumber)
        {
            bool atLeastOneGenerated = false;
            string outfitName = outfit.Outfit.ToString().ToLowerInvariant();
            string rawFolder = $"{WebConstants.CharactersAvatarFilePath}\\{characterDbModel.Name.ToLowerInvariant()}\\raws\\{outfitName}";
            if (!Directory.Exists(rawFolder))
            {
                Directory.CreateDirectory(rawFolder);
            }

            foreach (var expressionRawValue in selectedQuery.Expressions)
            {
                string expression = expressionRawValue.ToString().ToLowerInvariant();
                string expressionOutFolder = $"{WebConstants.CharactersAvatarFilePath}\\{characterDbModel.Name.ToLowerInvariant()}\\expressions\\{outfitName}\\{expression}";
                if (!Directory.Exists(expressionOutFolder))
                {
                    Directory.CreateDirectory(expressionOutFolder);
                }

                // So, the idea is to generate expressions from source avatars
                int nbImagesToGenerate = Directory.EnumerateFiles(rawFolder, "*.*", SearchOption.TopDirectoryOnly).Count();
                if (nbImagesToGenerate <= 0)
                    return (flowControl: false, value: atLeastOneGenerated);

                var sourceAvatarFilePaths = Directory.EnumerateFiles(rawFolder, "*.*", SearchOption.TopDirectoryOnly).ToArray();
                if (limitNbAvatarByExpressionToThisNumber >= 0 && sourceAvatarFilePaths.Length > limitNbAvatarByExpressionToThisNumber)
                {
                    sourceAvatarFilePaths = sourceAvatarFilePaths.Take(limitNbAvatarByExpressionToThisNumber).ToArray();
                }

                foreach (string sourceAvatarFilePath in sourceAvatarFilePaths)
                {
                    try
                    {
                        // The seed comes from the raw image
                        int seedToUse = -1;

                        Match match = Regex.Match(Path.GetFileNameWithoutExtension(sourceAvatarFilePath), @"s_(\d+)");
                        if (match.Success)
                        {
                            if (!int.TryParse(match.Groups[1].Value, out seedToUse))
                            {
                                seedToUse = -1;
                            }
                        }

                        if (seedToUse < 0)
                        {
                            continue;
                        }

                        // If a file with this SEED already exists on disk, skip
                        string[] outFiles = Directory.EnumerateFiles(expressionOutFolder, "*").ToArray();
                        string seedStr = seedToUse.ToString();
                        if (outFiles.Any(a => a.Contains(seedStr)))
                        {
                            continue;
                        }

                        if (!Enum.TryParse(characterSheet.AgeGroupAppearance, out AgeGroupAppearance ageGroupAppearance))
                        {
                            ageGroupAppearance = AgeGroupAppearance.Adult;
                        }

                        string genderedTags = "";
                        if (new[] { "male", "boy", "man" }.Any(a => a.Equals(characterSheet.Gender.Trim(), StringComparison.OrdinalIgnoreCase)))
                        {
                            switch (ageGroupAppearance)
                            {
                                case AgeGroupAppearance.Infant: genderedTags = "1boy, boy, infant, children"; break;
                                case AgeGroupAppearance.Toddler: genderedTags = "1boy, boy, toddler, children"; break;
                                case AgeGroupAppearance.Children: genderedTags = "1boy, boy, children"; break;
                                case AgeGroupAppearance.Teenager: genderedTags = "1boy, boy, teenager, children"; break;
                                case AgeGroupAppearance.YoungAdult: genderedTags = "1man, man, adult, teenager"; break;
                                case AgeGroupAppearance.Elderly: genderedTags = "1man, man, senior, elderly"; break;
                                case AgeGroupAppearance.Adult:
                                default: genderedTags = "1man, man, adult"; break;
                            }
                        } else
                        {
                            switch (ageGroupAppearance)
                            {
                                case AgeGroupAppearance.Infant: genderedTags = "1girl, girl, infant, children"; break;
                                case AgeGroupAppearance.Toddler: genderedTags = "1girl, girl, toddler, children"; break;
                                case AgeGroupAppearance.Children: genderedTags = "1girl, girl, children"; break;
                                case AgeGroupAppearance.Teenager: genderedTags = "1girl, girl, teenager, children"; break;
                                case AgeGroupAppearance.YoungAdult: genderedTags = "1woman, woman, adult, teenager"; break;
                                case AgeGroupAppearance.Elderly: genderedTags = "1woman, woman, senior, elderly"; break;
                                case AgeGroupAppearance.Adult:
                                default: genderedTags = "1woman, woman, adult"; break;
                            }
                        }

                        string positiveExpressionTags = expression switch
                        {
                            "admiration" => "wide eyes, sparkling eyes, open mouth, in awe, admiration, starry eyes, ",
                            "amusement" => "smirk, light smile, half-closed eyes, amused, raised eyebrow, ",
                            "anger" => "angry, furrowed brow, gritted teeth, (smile:0.0), (happy:0.0), scowl, frown, glaring, clenched teeth, angry eyes, turned down mouth, ",
                            "annoyance" => "annoyed, frown, tsurime, closed eyes, irritated, vein, pout, arms crossed, turned down mouth, ",
                            "arousal" => "seductive smile, half-closed eyes, bedroom eyes, flushed, parted lips, lidded eyes, blush, ",
                            "arrogant" => "smug, smirk, half-closed eyes, arrogant, looking down, condescending, chin up, ",
                            "bored" => "bored, half-closed eyes, deadpan, flat expression, slouch, resting on hand, yawning, turned down mouth, ",
                            "confusion" => "confused, tilted head, raised eyebrow, puzzled, question mark, confusion, sweat drop, ",
                            "crying" => "crying, (smile:0.0), (happy:0.0), tears, teary eyes, grimace, sobbing, tears streaming, wailing, watery eyes, grimacing, ",
                            "curiosity" => "curious, head tilt, wide eyes, raised eyebrow, inquisitive, leaning forward, attentive, ",
                            "disappointment" => "disappointed, frown, downcast eyes, (smile:0.0), (happy:0.0), dejected, slumped, sigh, sad eyes, sulking, turned down mouth, ",
                            "disapproval" => "disapproval, frown, raised eyebrow, crossed arms, stern look, tsurime, cold stare, turned down mouth, ",
                            "disgust" => "disgusted, scrunched nose, (smile:0.0), (happy:0.0), grimace, scrunched nose, revolted, nausea, frown, wrinkled nose, ",
                            "embarrassment" => "embarrassed, blush, flustered, covering face, looking away, shy, looking away, ",
                            "excitement" => "excited, wide eyes, big smile, open mouth, sparkling eyes, energetic, arms raised, ",
                            "fear" => "scared, (smile:0.0), (happy:0.0), wide eyes, open mouth, trembling, sweat drop, shaking, pale, frightened, flinching, grimace, fear, nausea, ",
                            "gratitude" => "grateful, warm smile, teary eyes, relieved, hands clasped, bowing, soft eyes, thankful, ",
                            "grief" => "grief, (smile:0.0), (happy:0.0), crying, tears streaming, covering face, trembling, despair, hunched over, sobbing, devastated, turned down mouth, grimace, ",
                            "jealousy" => "jealous, glare, frown, pout, sidelong glance, envious, gritted teeth, slit pupils, turned down mouth, pout, ",
                            "joy" => "joyful, big smile, laughing, sparkling eyes, rosy cheeks, open mouth, gleeful, ",
                            "laughing" => "laughing, open mouth, eyes closed, mouth open, teeth, head thrown back, giggling, smiling, ",
                            "nervousness" => "nervous, sweat drop, awkward smile, fidgeting, stiff smile, shaking, biting lip, ",
                            "neutral" => "neutral, ",
                            "pride" => "proud, confident smile, chin up, arms crossed, satisfied, pleased, smug grin, eyes closed, ",
                            "realization" => "wide eyes, open mouth, gasp, shocked expression, eyebrows raised, sudden surprise, ",
                            "relief" => "relieved, exhale, closed eyes, soft smile, relaxed, sigh, tired but happy, at ease, ",
                            "remorse" => "remorseful, downcast eyes, frown, guilt, head down, (smile:0.0), (happy:0.0), ashamed, turned down mouth, shame, looking away, ",
                            "sadness" => "sad, teary eyes, (smile:0.0), (happy:0.0), frown, downcast eyes, melancholy, pouty lips, dejected, watery eyes, turned down mouth, ",
                            "serious" => "serious, stern, expressionless, closed mouth, focused, firm gaze, furrowed brow, cold eyes, ",
                            "shy" => "shy, blush, looking away, fidgeting, small smile, embarrassed, covering mouth, timid, ",
                            "surprised" => "surprised, wide eyes, open mouth, eyebrows raised, shocked, startled, gasp, ",
                            "sleepy" => "sleepy, half-closed eyes, yawning, tired, drooping eyelids, nodding off, drowsy, sluggish, ",
                            "worried" => "worried, furrowed brow, anxious, (smile:0.0), (happy:0.0), biting lip, uneasy, tense, troubled, sweat drop, nervous, turned down mouth, afraid, fear, scared, ",
                            _ => expression
                        };

                        string negativeExpressionTags = expression switch
                        {
                            "anger" => "smile, smiling, grin, cheerful, happy expression, pleasant expression",
                            "annoyance" => "smile, smiling, grin, cheerful, happy expression, pleasant expression",
                            "bored" => "smile, smiling, grin, cheerful, happy expression, pleasant expression",
                            "crying" => "smile, smiling, grin, cheerful, happy expression, pleasant expression",
                            "disappointment" => "smile, smiling, grin, cheerful, happy expression, pleasant expression",
                            "disapproval" => "smile, smiling, grin, cheerful, happy expression, pleasant expression",
                            "disgust" => "smile, smiling, grin, cheerful, happy expression, pleasant expression",
                            "fear" => "smile, smiling, grin, cheerful, happy expression, pleasant expression",
                            "grief" => "smile, smiling, grin, cheerful, happy expression, pleasant expression",
                            "jealousy" => "smile, smiling, grin, cheerful, happy expression, pleasant expression",
                            "remorse" => "smile, smiling, grin, cheerful, happy expression, pleasant expression",
                            "sadness" => "smile, smiling, grin, cheerful, happy expression, pleasant expression",
                            "serious" => "smile, smiling, grin, cheerful, happy expression, pleasant expression",
                            "worried" => "smile, smiling, grin, cheerful, happy expression, pleasant expression",
                            _ => expression
                        };

                        using var cts = new CancellationTokenSource(TimeSpan.FromMinutes(config.ExecutionTimeoutMinutes));

                        // Upload the source avatar into ComfyUI's input/ directory so LoadImage can resolve it
                        byte[] sourceAvatarBytes = await File.ReadAllBytesAsync(sourceAvatarFilePath, cts.Token);
                        string comfySourceFileName = await comfyUiClient.UploadImageAsync(
                            sourceAvatarBytes,
                            Path.GetFileName(sourceAvatarFilePath),
                            cts.Token
                        );

                        string workflowJson = new ExpressionWorkflowInjector(templateJson).Inject(new AvatarGenerationRequest
                        {
                            CharacterId = selectedQuery.CharacterId,
                            PositivePrompt = $"score_9,masterpiece,best quality,amazing quality,score_8_up,score_7_up,{genderedTags},solo,facing viewer,{(outfit.Outfit == Storage.DataAccessLayer.SceneTracker.BusinessObjects.ClothingStateOfDress.Naked ? "full body" : "portrait")},looking at viewer,standing,straight-on,{Environment.NewLine}{outfit.IllustratorPromptInjection}{Environment.NewLine}{positiveExpressionTags},good head proportions,black background,simple background,detailed face,beautiful eyes",
                            NegativePromptOverride = $"CyberRealistic_Negative-neg,UnrealisticDream,BadDream,(deformed iris,deformed pupils,semi-realistic,cgi,3d,render),text,cropped,out of frame,worst quality,low quality,jpeg artifacts,ugly,duplicate,morbid,username,mutilated,extra fingers,mutated,mutated hands,poorly drawn hands,poorly drawn face,mutation,deformed,blurry,dehydrated,bad anatomy,bad proportions,extra limbs,cloned face,disfigured,gross proportions,large head,malformed limbs,missing arms,{negativeExpressionTags},missing legs,extra arms,extra legs,fused fingers,too many fingers,long neck,bad quality,pixelated,deformed faces,deformed body,blurry,extra body parts,extra finges,extra arms,pink,worst quality ,bad-hands,(bad art: 1.4),bad eye,bad eyes,deformed eyes,bad face,deformed face,blond hair,score_6,score_5,score_4,(worst quality:1.2),(low quality:1.2),(normal quality:1.2),lowres,bad anatomy,bad hands,signature,watermarks,ugly,imperfect eyes,skewed eyes,unnatural face,unnatural body,error,extra limb,missing limbs,extra digits,fewer digits,score_6,score_5,score_4,simplified,abstract,unrealistic,impressionistic,low resolution,lowres,bad anatomy,bad hands,missing fingers,normal quality,worst detail,illustration,sketch,artificial,poor quality,colorful background,detailed background,gradient background,outdoor,indoors,scenery,landscape,sky,trees,street,censor",
                            HeightOverride = 1216,
                            WidthOverride = 832,
                            Seed = seedToUse,
                            Expression = expression,
                            SourceAvatarFileName = comfySourceFileName,
                        });

                        await comfyUiClient.ConnectAsync(cts.Token);
                        string promptId = await comfyUiClient.SubmitAsync(workflowJson, cts.Token);
                        ComfyUiOutputFile file = await comfyUiClient.WaitAsync(promptId, ExpressionWorkflowInjector.NodeSaveImage, cts.Token);
                        byte[] image = await comfyUiClient.DownloadAsync(file, cts.Token);

                        await File.WriteAllBytesAsync($"{expressionOutFolder}\\c_s_{seedToUse}_e_{expression}.png", image, cts.Token);
                        atLeastOneGenerated = true;
                    } catch (Exception ex)
                    {
                        // cancel current process
                        return (flowControl: true, value: atLeastOneGenerated); ;
                    }
                }
            }

            return (flowControl: true, value: atLeastOneGenerated);
        }

        private async Task<(bool flowControl, bool value)> GenerateSourceAvatars(IllustrationQueryDbModel selectedQuery, CharacterDbModel characterDbModel, CharacterSheet characterSheet, string templateJson, IllustrationMapOutfit outfit)
        {
            bool atLeastOneGenerated = false;
            string rawFolder = $"{WebConstants.CharactersAvatarFilePath}\\{characterDbModel.Name.ToLowerInvariant()}\\raws\\{outfit.Outfit.ToString().ToLowerInvariant()}";
            if (!Directory.Exists(rawFolder))
            {
                Directory.CreateDirectory(rawFolder);
            }

            // Limit the images by outfit to 5 to avoid generating TB of stuff TODO: make this configurable
            int nbImagesToGenerate = 5 - Directory.EnumerateFiles(rawFolder, "*.*", SearchOption.TopDirectoryOnly).Count();
            if (nbImagesToGenerate <= 0)
                return (flowControl: false, value: atLeastOneGenerated);

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
                    PositivePrompt = $"score_9,masterpiece,best quality,amazing quality,score_8_up,score_7_up,{genderedTags},solo,facing viewer,{(outfit.Outfit == Storage.DataAccessLayer.SceneTracker.BusinessObjects.ClothingStateOfDress.Naked ? "full body" : "portrait")},looking at viewer,standing,straight-on,{Environment.NewLine}{outfit.IllustratorPromptInjection}{Environment.NewLine}neutral expression,good head proportions,black background,simple background,detailed face,beautiful eyes",
                    NegativePromptOverride = "CyberRealistic_Negative-neg,UnrealisticDream,BadDream,(deformed iris,deformed pupils,semi-realistic,cgi,3d,render),text,cropped,out of frame,worst quality,low quality,jpeg artifacts,ugly,duplicate,morbid,username,mutilated,extra fingers,mutated,mutated hands,poorly drawn hands,poorly drawn face,mutation,deformed,blurry,dehydrated,bad anatomy,bad proportions,extra limbs,cloned face,disfigured,gross proportions,large head,malformed limbs,missing arms,missing legs,extra arms,extra legs,fused fingers,too many fingers,long neck,bad quality,pixelated,deformed faces,deformed body,blurry,extra body parts,extra finges,extra arms,pink,worst quality,bad-hands,(bad art: 1.4),bad eye,bad eyes,deformed eyes,bad face,deformed face,blond hair,score_6,score_5,score_4,(worst quality:1.2),(low quality:1.2),(normal quality:1.2),lowres,bad anatomy,bad hands,signature,watermarks,ugly,imperfect eyes,skewed eyes,unnatural face,unnatural body,error,extra limb,missing limbs,extra digits,fewer digits,score_6,score_5,score_4,simplified,abstract,unrealistic,impressionistic,low resolution,lowres,bad anatomy,bad hands,missing fingers,normal quality,worst detail,illustration,sketch,artificial,poor quality,colorful background,detailed background,gradient background,outdoor,scenery,landscape,sky,trees,street,censor",
                    HeightOverride = 1216,
                    WidthOverride = 832,
                    Seed = seedToUse
                });

                using var cts = new CancellationTokenSource(TimeSpan.FromMinutes(config.ExecutionTimeoutMinutes));

                await comfyUiClient.ConnectAsync(cts.Token);
                string promptId = await comfyUiClient.SubmitAsync(workflowJson, cts.Token);
                ComfyUiOutputFile file = await comfyUiClient.WaitAsync(promptId, MainAvatarWorkflowInjector.NodeSaveImage, cts.Token);
                byte[] image = await comfyUiClient.DownloadAsync(file, cts.Token);

                await File.WriteAllBytesAsync($"{rawFolder}\\c_s_{seedToUse}.png", image, cts.Token);
                atLeastOneGenerated = true;
            }

            return (flowControl: true, value: atLeastOneGenerated);
        }

        /// <summary>
        /// Not really locking the row in storage, but 'reserving it'
        /// </summary>
        private async Task<(IllustrationQueryDbModel, IllustratorQueryStatus)> LockNextPendingQueryIfAnyAsync()
        {
            var allPendingQueries = await storageService.GetIllustrationQueriesAsync(g => g.Status == IllustratorQueryStatus.Pending || g.Status == IllustratorQueryStatus.Error);
            if (allPendingQueries.Length <= 0)
            {
                allPendingQueries = await storageService.GetIllustrationQueriesAsync(g => g.Status == IllustratorQueryStatus.Pending || g.Status == IllustratorQueryStatus.Error || g.Status == IllustratorQueryStatus.PartialCompletion);
                if (allPendingQueries.Length <= 0)
                {
                    return (null, default);
                }
            }

            allPendingQueries = allPendingQueries.OrderBy(o => o.Status).ThenBy(t => t.Expressions?.Count ?? 0).ThenBy(t => t.CreatedAtUtc).ToArray();

            // Filter only those that can run on an Image generator provider and then select the first one (highest priority + oldest one)
            // TODO
            var selectedQuery = allPendingQueries.First();
            var originalStatus = selectedQuery.Status;

            // change status
            selectedQuery.Status = IllustratorQueryStatus.Processing;

            if (!await storageService.UpdateIllustrationQueryAsync(selectedQuery))
            {
                LoggingManager.LogToFile("394a5c3a-6f07-497f-bcca-637a10f0c88f", $"Failed to update background status of illustrator query [{selectedQuery.IllustrationQueryId}] to [{IllustratorQueryStatus.Processing}]. Ignoring this query.");
                return (null, default);
            }

            return (selectedQuery, originalStatus);
        }
    }
}
