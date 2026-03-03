using Microsoft.EntityFrameworkCore;

namespace CohesiveRP.Storage.Sqlite
{
    public class CohesiveRPSqliteDbContext : CohesiveRPDbContext
    {
        // ********************************************************************
        //                            Protected
        // ********************************************************************
        // The following configures EF to create a Sqlite database file in executing directory.
        protected override void OnConfiguring(DbContextOptionsBuilder dbContextOptionsBuilder)
        {
            dbContextOptionsBuilder.UseSqlite(@$"Data Source={GetDataBaseFileName()};");//Foreign Keys = False;
        }

        // ********************************************************************
        //                            Public
        // ********************************************************************
        public virtual string GetDataBaseFileName() => "CohesiveRP.db";
    }
}
