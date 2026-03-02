using CohesiveRP.Common.Diagnostics;
using CohesiveRP.Core.LLMProviderManager.BusinessObjects;

namespace CohesiveRP.Core.LLMProviderManager
{
    internal static class LLMProviderManager
    {
        private static List<LLMProviderQueryer> queryers = new();


        internal static LLMProviderQuery GetQuery(string queryId)
        {
            return queryers.FirstOrDefault(f => f.GetQuery().Id == queryId)?.GetQuery();
        }

        internal static void RegisterNewQuery(LLMProviderQuery llmProviderQuery)
        {
            queryers.Add(new LLMProviderQueryer(llmProviderQuery));
        }

        internal static void AcknowledgeQuery(string id)
        {
            var query = GetQuery(id);

            if (query.Status != LLMProviderQueryStatus.Completed)
            {
                LoggingManager.LogToFile("56effd44-2bd3-4e56-a0e7-c5a337b4b036", $"Query with id [{id}] was acknowledged to be done, but status was [{query.Status}].");
            }

            queryers.RemoveAll(r => r.GetQuery().Id == id);
        }
    }
}
