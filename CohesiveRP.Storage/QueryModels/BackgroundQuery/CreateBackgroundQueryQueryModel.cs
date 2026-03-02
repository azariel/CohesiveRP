using CohesiveRP.Storage.DataAccessLayer.BackgroundQueries.BusinessObjects;

namespace CohesiveRP.Storage.QueryModels.BackgroundQuery
{
    public class CreateBackgroundQueryQueryModel
    {
        public string ChatId { get; set; }

        /// <summary>
        /// Tags tied to the background query. This serves the purpose of knowing on which LLM provider to run the actual query for synchronisation purpose.
        /// </summary>
        public List<string> Tags { get; set; } = new();

        /// <summary>
        /// Tags that this query depend on. If there's ANY background queries with ONE of those tags queued up, those queries will need to be run BEFORE this one runs, even if the linked provider is idle.
        /// </summary>
        public List<string> DependenciesTags { get; set; } = new();

        /// <summary>
        /// Higher priority will run before lower priority.
        /// </summary>
        public BackgroundQueryPriority Priority { get; set; }
    }
}
