using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using CohesiveRP.Storage.JsonConverters;
using CohesiveRP.Storage.Sqlite;

namespace CohesiveRP.Storage.DataAccessLayer.Chats
{
    /// <summary>
    /// Represents the structure of a character within the storage.
    /// </summary>
    [Table("Characters")]
    public class CharacterDbModel : CohesiveRPSqliteBaseTable
    {
        // ********************************************************************
        //                            Properties
        // ********************************************************************
        [Required]
        [MaxLength(32)]
        [Key]
        public string CharacterId { get; set; }

        [MaxLength(256)]
        public string Name { get; set; }

        [MaxLength(256)]
        public string Creator { get; set; }

        [MaxLength(1024)]
        public string CreatorNotes { get; set; }

        [MaxLength(16384)]
        public string Description { get; set; }

        [JsonValueConverter]
        public List<string> Tags { get; set; }

        [MaxLength(8196)]
        public string FirstMessage { get; set; }

        [JsonValueConverter]
        public List<string> AlternateGreetings { get; set; }

        public DateTime LastActivityAtUtc { get; set; }
    }
}
