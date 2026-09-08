using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using CohesiveRP.Storage.JsonConverters;
using CohesiveRP.Storage.Sqlite;

namespace CohesiveRP.Storage.DataAccessLayer.ChatAdditions.Reflection
{
    /// <summary>
    /// Represents the structure of Reflections in db. Reflections are custom Thinking so that the main model can focus on generating the actual reply instead of pure thinking, which usually is more about logic than creativity.
    /// </summary>
    [Table("Reflections")]
    public class ReflectionDbModel : CohesiveRPSqliteBaseTable
    {
        [Required]
        [MaxLength(32)]
        [Key]// Partition key AND FK
        public string ReflectionId { get; set; }

        [Required]
        [MaxLength(32)]
        public string ChatId { get; set; }
        
        [JsonValueConverter]
        public string Content { get; set; }
    }
}
