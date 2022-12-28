namespace MobileConfiguration.Database;

using System.Reflection;
using Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shared.Logger;

public class ConfigurationGenericContext : DbContext
{
    #region Fields

    protected readonly String ConnectionString;

    protected readonly String DatabaseEngine;

    protected static List<String> TablesToIgnoreDuplicates = new List<String>();

    #endregion

    #region Constructors

    protected ConfigurationGenericContext(String databaseEngine,
                                          String connectionString) {
        this.DatabaseEngine = databaseEngine;
        this.ConnectionString = connectionString;
    }

    public ConfigurationGenericContext(DbContextOptions dbContextOptions) : base(dbContextOptions) {
    }

    #endregion

    public virtual async Task MigrateAsync(CancellationToken cancellationToken) {
        if (this.Database.IsSqlServer() || this.Database.IsMySql()) {
            await this.Database.MigrateAsync(cancellationToken);
            await this.SetIgnoreDuplicates(cancellationToken);
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder) {
        modelBuilder.SetupConfigurationTable().SetupLoggingLevelsTable().SetupApplicationCentreConfigurationTable();

        base.OnModelCreating(modelBuilder);
    }

    public static Boolean IsDuplicateInsertsIgnored(String tableName) =>
        ConfigurationGenericContext.TablesToIgnoreDuplicates.Contains(tableName.Trim(), StringComparer.InvariantCultureIgnoreCase);

    private async Task SeedStandingData(CancellationToken cancellationToken)
    {
        String executingAssemblyLocation = Assembly.GetExecutingAssembly().Location;
        String executingAssemblyFolder = Path.GetDirectoryName(executingAssemblyLocation);

        String scriptsFolder = $@"{executingAssemblyFolder}/SeedingScripts";

        String[] sqlFiles = Directory.GetFiles(scriptsFolder, "*.sql");
        foreach (String sqlFile in sqlFiles.OrderBy(x => x))
        {
            Logger.LogDebug($"About to create View [{sqlFile}]");
            String sql = System.IO.File.ReadAllText(sqlFile);

            // Check here is we need to replace a Database Name
            if (sql.Contains("{DatabaseName}"))
            {
                sql = sql.Replace("{DatabaseName}", this.Database.GetDbConnection().Database);
            }

            // Create the new view using the original sql from file
            await this.Database.ExecuteSqlRawAsync(sql, cancellationToken);

            Logger.LogDebug($"Run Seeding Script [{sqlFile}] successfully.");
        }
    }

    protected virtual async Task SetIgnoreDuplicates(CancellationToken cancellationToken) {
        ConfigurationGenericContext.TablesToIgnoreDuplicates = new List<String> {

                                                                                };
    }

    public DbSet<Configuration> Configurations { get; set; }

    public DbSet<ApplicationCentreConfiguration> ApplicationCentreConfigurations { get; set; }

    public DbSet<LoggingLevels> LoggingLevels { get; set; }
}

public static class Extensions
{
    public static PropertyBuilder DecimalPrecision(this PropertyBuilder propertyBuilder,
                                                   Int32 precision,
                                                   Int32 scale) {
        return propertyBuilder.HasColumnType($"decimal({precision},{scale})");
    }

    public static ModelBuilder SetupConfigurationTable(this ModelBuilder modelBuilder) {
        modelBuilder.Entity<Configuration>().HasKey(r => new {
                                                                 r.Id,
                                                                 r.ConfigType
                                                             });

        // TODO: FK on LogLevel

        return modelBuilder;
    }

    public static ModelBuilder SetupLoggingLevelsTable(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<LoggingLevels>().HasKey(r => new {
                                                                 r.LogLevelId
                                                             });
        return modelBuilder;
    }

    public static ModelBuilder SetupApplicationCentreConfigurationTable(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ApplicationCentreConfiguration>().HasKey(r => new {
                                                                 r.ApplicationId
                                                             });
        return modelBuilder;
    }
}