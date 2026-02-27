using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using CohesiveRP.Storage.Sqlite;

namespace CohesiveRP.Storage.Users
{
    /// <summary>
    /// Represents the structure of a chat within the storage.
    /// </summary>
    [Table("Chats")]
    public class ChatDbModel : CohesiveRPSqliteBaseTable
    {
        // ********************************************************************
        //                            Properties
        // ********************************************************************
        [Required]
        [Key]
        public string ChatId { get; set; }

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
