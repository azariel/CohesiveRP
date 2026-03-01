using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using CohesiveRP.Storage.Sqlite;

namespace CohesiveRP.Storage.DataAccessLayer.AIQueries
{
    /// <summary>
    /// Represents the structure of queries made to background backend within the storage.
    /// </summary>
    [Table("BackgroundQueries")]
    public class BackgroundQueryDbModel : CohesiveRPSqliteBaseTable
    {
        // ********************************************************************
        //                            Properties
        // ********************************************************************
        [Required]
        [Key]
        public string BackgroundQueryId { get; set; }

        public string Tags { get; set; }

        //[MaxLength(1024)]
        //public string LLMProviders { get; set; }

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
