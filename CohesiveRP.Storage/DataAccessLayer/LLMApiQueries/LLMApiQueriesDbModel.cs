using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using CohesiveRP.Storage.DataAccessLayer.LLMApiQueries.BusinessObjects;
using CohesiveRP.Storage.Sqlite;

namespace CohesiveRP.Storage.DataAccessLayer.AIQueries
{
    /// <summary>
    /// Represents the structure of queries made to LLM Apis.
    /// </summary>
    [Table("LLMApiQueries")]
    public class LLMApiQueryDbModel : CohesiveRPSqliteBaseTable
    {
        // ********************************************************************
        //                            Properties
        // ********************************************************************
        [Required]
        [MaxLength(32)]
        [Key]
        public string LLMApiQueryId { get; set; }

        [Required]
        [MaxLength(32)]
        public string LLMProviderConfigId { get; set; }

        [Required]
        [MaxLength(128)]
        public string Tag { get; set; }

        [Required]
        [MaxLength(64)]
        public LLMApiQueryStatus Status { get; set; }
    }
}
