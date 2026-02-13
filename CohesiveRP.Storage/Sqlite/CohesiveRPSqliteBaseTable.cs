using System.ComponentModel.DataAnnotations;

namespace CohesiveRP.Storage.Sqlite
{
    /// <summary>
    /// All table using Sqlite structure must inherit from this class
    /// </summary>
    public class CohesiveRPSqliteBaseTable
    {
        // ********************************************************************
        //                            Properties
        // ********************************************************************
        [Required]
        public DateTime? InsertDateTimeUtc { get; set; } = null;
    }
}
