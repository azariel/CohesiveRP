using System.Text.Json;
using System.Text.Json.Serialization;
using CohesiveRP.Core.BackgroundServices.BackgroundQueries;
using CohesiveRP.Core.DtoConverters;
using CohesiveRP.Core.DtoConverters.Abstractions;
using CohesiveRP.Core.LLMProviderManager;
using CohesiveRP.Core.PromptContext;
using CohesiveRP.Core.PromptContext.Abstractions;
using CohesiveRP.Core.PromptContext.Builders;
using CohesiveRP.Core.Services;
using CohesiveRP.Core.Services.LLMApiProvider;
using CohesiveRP.Core.Services.Summary;
using CohesiveRP.Core.WebApi.Workflows.Characters.Abstractions;
using CohesiveRP.Core.WebApi.Workflows.Chat;
using CohesiveRP.Core.WebApi.Workflows.Chat.Abstractions;
using CohesiveRP.Core.WebApi.Workflows.Chats;
using CohesiveRP.Core.WebApi.Workflows.Chats.Abstractions;
using CohesiveRP.Core.WebApi.Workflows.Lorebooks.Abstractions;
using CohesiveRP.Core.WebApi.Workflows.Messages;
using CohesiveRP.Core.WebApi.Workflows.Messages.Abstractions;
using CohesiveRP.Core.WebApi.Workflows.Personas.Abstractions;
using CohesiveRP.Core.WebApi.Workflows.SceneTrackers.Abstractions;
using CohesiveRP.Core.WebApi.Workflows.Settings.Abstractions;
using CohesiveRP.Storage.Common;
using CohesiveRP.Storage.DataAccessLayer.AIQueries;
using CohesiveRP.Storage.DataAccessLayer.ChatCompletionPresets;
using CohesiveRP.Storage.DataAccessLayer.Messages;
using CohesiveRP.Storage.DataAccessLayer.SceneTracker;
using CohesiveRP.Storage.DataAccessLayer.Settings;
using CohesiveRP.Storage.DataAccessLayer.Summary.Short;
using CohesiveRP.Storage.DataAccessLayer.Users;

namespace CohesiveRP.Core.WebApi
{
    internal static class CustomServices
    {
        internal static void AddCustomServices(this IServiceCollection services)
        {
            // Workflows.Chats
            services.AddSingleton<IGetAllSelectableChatsWorkflow, GetAllSelectableChatsWorkflow>();
            services.AddSingleton<IGetSpecificChatWorkflow, GetSpecificChatWorkflow>();
            services.AddSingleton<IDeleteSpecificChatWorkflow, DeleteSpecificChatWorkflow>();

            // Workflows.Chat
            services.AddSingleton<IChatAddNewMessageWorkflow, AddNewMessageWorkflow>();
            services.AddSingleton<ICreateNewChatWorkflow, CreateNewChatWorkflow>();
            services.AddSingleton<IGetAllHotMessagesWorkflow, GetAllHotMessagesWorkflow>();
            services.AddSingleton<IGetSpecificMessageByIdWorkflow, GetSpecificMessageByIdWorkflow>();
            services.AddSingleton<IPatchSpecificMessageByIdWorkflow, PatchSpecificMessageByIdWorkflow>();
            services.AddSingleton<IDeleteSpecificMessageByIdWorkflow, DeleteSpecificMessageByIdWorkflow>();

            // Workflows.Characters
            services.AddSingleton<IGetAllCharactersWorkflow, GetAllCharactersWorkflow>();
            services.AddSingleton<IImportNewCharacterWorkflow, ImportNewCharacterWorkflow>();
            services.AddSingleton<IGetCharacterByIdWorkflow, GetCharacterByIdWorkflow>();
            services.AddSingleton<IUpdateCharacterWorkflow, UpdateCharacterWorkflow>();
            services.AddSingleton<IDeleteCharacterWorkflow, DeleteCharacterWorkflow>();

            // Workflows.Personas
            services.AddSingleton<IGetAllPersonasWorkflow, GetAllPersonasWorkflow>();
            services.AddSingleton<IGetPersonaByIdWorkflow, GetPersonaByIdWorkflow>();
            services.AddSingleton<IAddPersonaWorkflow, AddPersonaWorkflow>();
            services.AddSingleton<IUpdatePersonaWorkflow, UpdatePersonaWorkflow>();
            services.AddSingleton<IDeletePersonaWorkflow, DeletePersonaWorkflow>();
            services.AddSingleton<IImportAndReplacePersonaAvatarWorkflow, ImportAndReplacePersonaAvatarWorkflow>();

            // Workflows.Lorebooks
            services.AddSingleton<IGetAllLorebooksWorkflow, GetAllLorebooksWorkflow>();
            services.AddSingleton<IGetLorebookByIdWorkflow, GetLorebookByIdWorkflow>();
            services.AddSingleton<IAddLorebookWorkflow, AddLorebookWorkflow>();
            services.AddSingleton<IUpdateLorebookWorkflow, UpdateLorebookWorkflow>();
            services.AddSingleton<IDeleteLorebookWorkflow, DeleteLorebookWorkflow>();
            services.AddSingleton<IImportAndReplaceLorebookAvatarWorkflow, ImportAndReplaceLorebookAvatarWorkflow>();
            services.AddSingleton<IImportLorebookWorkflow, ImportLorebookWorkflow>();

            // Workflows.SceneTrackers
            services.AddSingleton<IGetSceneTrackerByIdWorkflow, GetSceneTrackerByIdWorkflow>();
            services.AddSingleton<IUpdateSceneTrackerWorkflow, UpdateSceneTrackerWorkflow>();
            services.AddSingleton<IForceRefreshSceneTrackerWorkflow, ForceRefreshSceneTrackerWorkflow>();

            // Workflows.Settings
            services.AddSingleton<IGetGlobalSettingsWorkflow, GetGlobalSettingsWorkflow>();

            // Workflows.BackgroundQueries
            services.AddSingleton<IGetBackgroundQueryWorkflow, GetBackgroundQueryWorkflow>();
            services.AddSingleton<IGetBackgroundQueriesByChatIdWorkflow, GetBackgroundQueriesByChatIdWorkflow>();

            // DtoConverters
            services.AddSingleton<ILorebookDtoConverter, LorebookDtoConverter>();

            // Services
            services.AddSingleton<IStorageService, StorageService>();
            services.AddSingleton<IHttpLLMApiProviderService, HttpLLMApiProviderService>();
            services.AddSingleton<ISummaryService, SummaryService>();

            // Factories
            services.AddSingleton<ILLMProviderQueryerFactory, LLMProviderQueryerFactory>();
            services.AddSingleton<IPromptContextBuilderFactory, PromptContextBuilderFactory>();
            services.AddSingleton<IPromptContextElementBuilderFactory, PromptContextElementBuilderFactory>();
            services.AddSingleton<ILLMApiQueryPayloadBuilderFactory, LLMApiQueryPayloadBuilderFactory>();

            // DataAccessLayers
            services.AddDbContextFactory<StorageDbContext>();
            //services.AddSingleton<IUsersDal, UsersDal>();
            // Those must be injected into StorageService ctor to make sure their CTOR is called upon service startup and thus default values are injected into storage at startup
            services.AddSingleton<IChatsDal, ChatsDal>();
            services.AddSingleton<ICharactersDal, CharactersDal>();
            services.AddSingleton<IPersonasDal, PersonasDal>();
            services.AddSingleton<ILorebooksDal, LorebooksDal>();
            services.AddSingleton<IMessagesDal, MessagesDal>();
            services.AddSingleton<IGlobalSettingsDal, GlobalSettingsDal>();
            services.AddSingleton<IChatCompletionPresetsDal, ChatCompletionPresetsDal>();
            services.AddSingleton<IBackgroundQueriesDal, BackgroundQueriesDal>();
            services.AddSingleton<ILLMApiQueriesDal, LLMApiQueriesDal>();
            services.AddSingleton<ISummaryDal, SummaryDal>();
            services.AddSingleton<ISceneTrackerDal, SceneTrackerDal>();

            // Default Json options
            services.AddSingleton(new JsonSerializerOptions()
            {
                AllowTrailingCommas = true,
                PropertyNameCaseInsensitive = true,
            });

            // Add background services
            services.AddHostedService<BackgroundQueriesWorker>();

            services.AddControllers().AddJsonOptions(option =>
            {
                option.JsonSerializerOptions.AllowTrailingCommas = true;
                option.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()); // Convert enum value to string instead of integer
            });
        }
    }
}
