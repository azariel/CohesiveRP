using System.Text.Json;
using CohesiveRP.Core.WebApi.Services;
using CohesiveRP.Core.WebApi.Workflows.Chat;

namespace CohesiveRP.Core.WebApi
{
    internal static class CustomServices
    {
        internal static void AddCustomServices(this IServiceCollection services)
        {
            // Workflows
            services.AddSingleton<IChatAddNewMessageWorkflow, ChatAddNewMessageWorkflow>();

            // Services
            services.AddSingleton<IStorageService, StorageService>();

            // Default Json options
            services.AddSingleton(new JsonSerializerOptions()
            {
                AllowTrailingCommas = true,
                PropertyNameCaseInsensitive = true,
            });
        }
    }
}
