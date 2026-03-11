using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
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
    }
}
