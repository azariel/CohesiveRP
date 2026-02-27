using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using CohesiveRP.Storage.Sqlite;

namespace CohesiveRP.Storage.DataAccessLayer.Users
{
    /// <summary>
    /// Represents the structure of a user within the storage.
    /// </summary>
    [Table("Users")]
    public class UserDbModel : CohesiveRPSqliteBaseTable
    {
        // ********************************************************************
        //                            Nested
        // ********************************************************************
        public enum UserType// TODO: replace by a system of groups ?
        {
            User,
            Admin
        }

        // ********************************************************************
        //                            Properties
        // ********************************************************************
        [Required]
        [MaxLength(1024)]
        public string Password { get; set; }// TODO: encrypt

        [Required]
        public UserType Type { get; set; } = UserType.User;

        [Required]
        [Key]
        public string UserId { get; set; }

        [Required]
        [MaxLength(256)]
        public string UserName { get; set; }
    }
}
