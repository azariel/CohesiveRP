using System.Text.Json;
using CohesiveRP.Core.Services;
using CohesiveRP.Core.WebApi.Workflows.Chat;
using CohesiveRP.Core.WebApi.Workflows.Chat.Abstractions;
using CohesiveRP.Core.WebApi.Workflows.Settings.Abstractions;
using CohesiveRP.Storage.DataAccessLayer.Messages;
using CohesiveRP.Storage.DataAccessLayer.Settings;
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

            // Workflows.Settings
            services.AddSingleton<IGetGlobalSettingsWorkflow, GetGlobalSettingsWorkflow>();

            // Services
            services.AddSingleton<IStorageService, StorageService>();

            // DataAccessLayers
            services.AddSingleton<IUsersDal, UsersDal>();
            services.AddSingleton<IChatsDal, ChatsDal>();
            services.AddSingleton<IMessagesDal, MessagesDal>();
            services.AddSingleton<IGlobalSettingsDal, GlobalSettingsDal>();

            // Default Json options
            services.AddSingleton(new JsonSerializerOptions()
            {
                AllowTrailingCommas = true,
                PropertyNameCaseInsensitive = true,
            });
        }
    }
}
