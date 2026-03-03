using CohesiveRP.Core.LLMProviderManager.BusinessObjects;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace CohesiveRP.Core.LLMProviderManager
{
    public class LLMProviderQueryer
    {
        private LLMProviderQuery query = null;

        public LLMProviderQueryer(LLMProviderQuery query)
        {
            this.query = query;
            _ = Task.Run(async () => await ProcessQueryAsync());
        }

        public LLMProviderQuery GetQuery() => query;

        private async Task ProcessQueryAsync()
        {
            // Query the LLM provider here and update Db
            for (var i = 0; i < 100; ++i)
            {
                await Task.Delay(100);
                query.Content += Guid.NewGuid().ToString()[0];
            }
            query.Content += "-COMPLETED";
            query.Status = LLMProviderQueryStatus.Completed;
        }
    }
}
