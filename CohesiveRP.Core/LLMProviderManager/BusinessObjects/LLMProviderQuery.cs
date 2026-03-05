using CohesiveRP.Storage.DataAccessLayer.BackgroundQueries.BusinessObjects;

namespace CohesiveRP.Core.LLMProviderManager.BusinessObjects
{
    public class LLMProviderQuery
    {
        public string Id { get; set; }
        public LLMProviderQueryStatus Status { get; set; }
        public string Content { get; set; } = string.Empty;
        public BackgroundQuerySystemTags RunningType { get; set; }
    }
}
