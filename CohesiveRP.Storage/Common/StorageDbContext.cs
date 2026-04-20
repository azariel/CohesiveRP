using System.Reflection;
using CohesiveRP.Storage.DataAccessLayer.AIQueries;
using CohesiveRP.Storage.DataAccessLayer.Chats;
using CohesiveRP.Storage.DataAccessLayer.InteractiveUserInputQueries;
using CohesiveRP.Storage.DataAccessLayer.Messages;
using CohesiveRP.Storage.DataAccessLayer.Messages.Hot;
using CohesiveRP.Storage.DataAccessLayer.Settings;
using CohesiveRP.Storage.DataAccessLayer.Users;
using CohesiveRP.Storage.JsonConverters;
using CohesiveRP.Storage.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            ApplyJsonValueConverters(modelBuilder);
        }

        /// <summary>
        /// Scans all registered entity types and applies <see cref="JsonValueConverter{T}"/>
        /// to every property decorated with <see cref="JsonValueConverterAttribute"/>.
        /// </summary>
        /// <param name="modelBuilder">The EF Core model builder.</param>
        protected void ApplyJsonValueConverters(ModelBuilder modelBuilder)
        {
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                foreach (var property in entityType.ClrType.GetProperties(BindingFlags.Public | BindingFlags.Instance))
                {
                    if (property.GetCustomAttribute<JsonValueConverterAttribute>() == null)
                    {
                        continue;
                    }

                    var converterType = typeof(JsonValueConverter<>).MakeGenericType(property.PropertyType);
                    var converterInstance = (ValueConverter)Activator.CreateInstance(converterType);

                    modelBuilder
                        .Entity(entityType.ClrType)
                        .Property(property.Name)
                        .HasConversion(converterInstance);
                }
            }
        }

        public override string GetDataBaseFileName() => "CohesiveRP-Storage.db";

        // ********************************************************************
        //                            Properties
        // ********************************************************************
        public DbSet<UserDbModel> Users { get; set; }

        public DbSet<ChatDbModel> Chats { get; set; }

        public DbSet<CharacterDbModel> Characters { get; set; }

        public DbSet<PersonaDbModel> Personas { get; set; }

        // Recent message in a specific chat
        public DbSet<HotMessagesDbModel> HotMessages { get; set; }

        // Old messages in a specific chat (strictly for performance, we're sunsetting old messages in this table to keep things lean and efficient)
        public DbSet<ColdMessagesDbModel> ColdMessages { get; set; }

        public DbSet<GlobalSettingsDbModel> GlobalSettings { get; set; }

        public DbSet<ChatCompletionPresetsDbModel> ChatCompletionPresets { get; set; }

        // Queries that are queued to be processed, processing or recently processed against a background worker
        public DbSet<BackgroundQueryDbModel> BackgroundQueries { get; set; }

        public DbSet<LLMApiQueryDbModel> LLMApiQueries { get; set; }

        public DbSet<SummaryDbModel> Summaries { get; set; }
        
        public DbSet<SceneTrackerDbModel> SceneTrackers { get; set; }

        public DbSet<SceneAnalyzerDbModel> SceneAnalyzers { get; set; }

        public DbSet<LorebookDbModel> Lorebooks { get; set; }

        public DbSet<LorebookInstanceDbModel> LorebookInstances { get; set; }
        
        public DbSet<InteractiveUserInputDbModel> InteractiveUserInputQueries { get; set; }

        // Pathfinder
        public DbSet<CharacterSheetDbModel> CharacterSheets { get; set; }
        public DbSet<CharacterSheetInstancesDbModel> CharacterSheetInstances { get; set; }
        public DbSet<ChatCharactersRollsDbModel> ChatCharactersRolls { get; set; }
        // ---------
    }
}
