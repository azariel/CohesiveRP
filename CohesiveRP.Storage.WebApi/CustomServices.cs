using System.Text.Json;
using CohesiveRP.Storage.DataAccessLayer.Users;

namespace CohesiveRP.Storage.WebApi
{
    internal static class CustomServices
    {
        internal static void AddCustomServices(this IServiceCollection services)
        {
            // Workflows
            //services.AddSingleton<IGetChatWorkflow, GetChatWorkflow>();

            // DataAccessLayers
            //services.AddSingleton<IUsersDal, UsersDal>();
            //services.AddSingleton<IChatsDal, ChatsDal>();

            //// Default Json options
            //services.AddSingleton(new JsonSerializerOptions()
            //{
            //    AllowTrailingCommas = true,
            //    PropertyNameCaseInsensitive = true,
            //});
        }
    }
}
