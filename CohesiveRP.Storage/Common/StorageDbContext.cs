using CohesiveRP.Storage.DataAccessLayer.Messages.Hot;
using CohesiveRP.Storage.DataAccessLayer.Settings;
using CohesiveRP.Storage.DataAccessLayer.Users;
using CohesiveRP.Storage.Sqlite;
using CohesiveRP.Storage.Users;
using Microsoft.EntityFrameworkCore;

namespace CohesiveRP.Storage.Common
{
    /// <summary>
    /// Main structure of the CohesiveRP database.
    /// </summary>
    public class StorageDbContext : CohesiveRPSqliteDbContext
    {
        // ********************************************************************
        //                            Constructors
        // ********************************************************************
        public StorageDbContext()
        {
            this.ChangeTracker.AutoDetectChangesEnabled = true;

            // We're having too much performance issue with lazy loading... we'll deal with load/include what we need manually instead..
            // this.ChangeTracker.LazyLoadingEnabled = true;
        }

        // ********************************************************************
        //                            Protected
        // ********************************************************************
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);

            //optionsBuilder.UseLazyLoadingProxies();
        }

        public override string GetDataBaseFileName() => "CohesiveRP-Storage.db";

        // ********************************************************************
        //                            Properties
        // ********************************************************************
        public DbSet<UserDbModel> Users { get; set; }

        public DbSet<ChatDbModel> Chats { get; set; }

        // Recent message in a specific chat
        public DbSet<HotMessagesDbModel> HotMessages { get; set; }

        // Old messages in a specific chat (strictly for performance, we're sunsetting old messages in this table to keep things lean and efficient)
        public DbSet<ColdMessagesDbModel> ColdMessages { get; set; }

        public DbSet<GlobalSettingsDbModel> GlobalSettings { get; set; }
    }
}
