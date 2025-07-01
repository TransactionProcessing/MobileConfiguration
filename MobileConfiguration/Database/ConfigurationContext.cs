using Microsoft.EntityFrameworkCore;
using MobileConfiguration.Database.Entities;

namespace MobileConfiguration.Database;

public class ConfigurationContext : DbContext
{
    #region Fields

    protected readonly String ConnectionString;
    
    #endregion

    #region Constructors

    public ConfigurationContext(String connectionString) {
        this.ConnectionString = connectionString;
    }

    public ConfigurationContext(DbContextOptions<ConfigurationContext> dbContextOptions) : base(dbContextOptions) {
    }

    #endregion

    public virtual async Task MigrateAsync(CancellationToken cancellationToken) {
        if (this.Database.IsSqlServer()) {
            await this.Database.MigrateAsync(cancellationToken);
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder) {
        modelBuilder.SetupConfigurationTable().SetupLoggingLevelsTable();

        base.OnModelCreating(modelBuilder);
    }
    
    public DbSet<Configuration> Configurations { get; set; }
    
    public DbSet<LoggingLevels> LoggingLevels { get; set; }
}