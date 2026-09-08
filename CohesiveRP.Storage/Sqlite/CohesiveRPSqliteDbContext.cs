using Microsoft.EntityFrameworkCore;

namespace CohesiveRP.Storage.Sqlite
{
    public class CohesiveRPSqliteDbContext : CohesiveRPDbContext
    {
        public CohesiveRPSqliteDbContext()
        {
            Database.EnsureCreated();
            Database.ExecuteSqlRaw("PRAGMA journal_mode=WAL;");
            Database.ExecuteSqlRaw("PRAGMA busy_timeout=5000;");
        }

        // ********************************************************************
        //                            Protected
        // ********************************************************************
        // The following configures EF to create a Sqlite database file in executing directory.
        protected override void OnConfiguring(DbContextOptionsBuilder dbContextOptionsBuilder)
        {
            dbContextOptionsBuilder.UseSqlite(@$"Data Source={GetDataBaseFileName()};Cache=Shared;",
                sqliteOptions => sqliteOptions.CommandTimeout(30));//Foreign Keys = False;
        }

        // ********************************************************************
        //                            Public
        // ********************************************************************
        public virtual string GetDataBaseFileName() => "CohesiveRP.db";
    }
}
