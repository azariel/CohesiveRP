using CohesiveRP.Storage.WebApi.DataAccessLayer;
using CohesiveRP.Storage.WebApi.Workflows;

namespace CohesiveRP.Storage.WebApi
{
    internal static class CustomServices
    {
        internal static void AddCustomServices(this IServiceCollection services)
        {
            // Workflows
            services.AddSingleton<IUsersWorkflow, UsersWorkflow>();

            // DataAccessLayers
            services.AddSingleton<IStorageDal, StorageDal>();
        }
    }
}
