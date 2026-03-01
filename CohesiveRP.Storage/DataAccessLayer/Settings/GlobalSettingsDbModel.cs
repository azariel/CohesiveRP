using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using CohesiveRP.Storage.Sqlite;

namespace CohesiveRP.Storage.DataAccessLayer.Settings
{
    /// <summary>
    /// Represents the structure of global settings within the storage.
    /// </summary>
    [Table("GlobalSettings")]
    public class GlobalSettingsDbModel : CohesiveRPSqliteBaseTable
    {
        // ********************************************************************
        //                            Properties
        // ********************************************************************
        [Required]
        [Key]
        public string GlobalSettingsId { get; set; }

        //[MaxLength(1024)]
        public string LLMProviders { get; set; }

        //[Required]
        //[MaxLength(1024)]
        //public string Password { get; set; }// TODO: encrypt

        //[Required]
        //public UserType Type { get; set; } = UserType.User;


        //[Required]
        //[MaxLength(256)]
        //public string UserName { get; set; }
    }
}
