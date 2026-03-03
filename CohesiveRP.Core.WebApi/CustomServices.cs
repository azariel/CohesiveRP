using System.Text.Json;
using System.Text.Json.Serialization;
using CohesiveRP.Core.BackgroundServices.BackgroundQueries;
using CohesiveRP.Core.LLMProviderManager;
using CohesiveRP.Core.Services;
using CohesiveRP.Core.WebApi.Workflows.Chat;
using CohesiveRP.Core.WebApi.Workflows.Chat.Abstractions;
using CohesiveRP.Core.WebApi.Workflows.Settings.Abstractions;
using CohesiveRP.Storage.Common;
using CohesiveRP.Storage.DataAccessLayer.AIQueries;
using CohesiveRP.Storage.DataAccessLayer.Messages;
using CohesiveRP.Storage.DataAccessLayer.Users;

namespace CohesiveRP.Core.WebApi
{
    internal static class CustomServices
    {
        internal static void AddCustomServices(this IServiceCollection services)
        {
            // Workflows.Chats
            services.AddSingleton<IGetAllSelectableChatsWorkflow, GetAllSelectableChatsWorkflow>();

            // Workflows.Chat
            services.AddSingleton<IChatAddNewMessageWorkflow, AddNewMessageWorkflow>();
            services.AddSingleton<ICreateNewChatWorkflow, CreateNewChatWorkflow>();
            services.AddSingleton<IGetAllHotMessagesWorkflow, GetAllHotMessagesWorkflow>();
            services.AddSingleton<IGetSpecificMessageByIdWorkflow, GetSpecificMessageByIdWorkflow>();

            // Workflows.Settings
            services.AddSingleton<IGetGlobalSettingsWorkflow, GetGlobalSettingsWorkflow>();

            // Workflows.BackgroundQueries
            services.AddSingleton<IGetBackgroundQueryWorkflow, GetBackgroundQueryWorkflow>();

            // Services
            services.AddSingleton<IStorageService, StorageService>();

            // Processors
            services.AddSingleton<ILLMQueryProcessor, LLMQueryProcessor>();

            // DataAccessLayers
            services.AddDbContextFactory<StorageDbContext>();
            services.AddSingleton<IUsersDal, UsersDal>();
            services.AddSingleton<IChatsDal, ChatsDal>();
            services.AddSingleton<IMessagesDal, MessagesDal>();
            services.AddSingleton<IGlobalSettingsDal, GlobalSettingsDal>();
            services.AddSingleton<IBackgroundQueriesDal, BackgroundQueriesDal>();

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
