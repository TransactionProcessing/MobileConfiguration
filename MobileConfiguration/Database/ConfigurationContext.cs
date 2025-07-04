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

    public ConfigurationContext(DbContextOptions dbContextOptions) : base(dbContextOptions) {
    }

    #endregion

    public virtual async Task MigrateAsync(CancellationToken cancellationToken) {
        if (this.Database.IsSqlServer()) {
            try {
                await this.Database.MigrateAsync(cancellationToken);
            }
            catch (Exception ex) {
                // Log the exception or handle it as needed
                throw new InvalidOperationException("An error occurred while migrating the database.", ex);
            }
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder) {
        modelBuilder.SetupConfigurationTable().SetupLoggingLevelsTable();

        base.OnModelCreating(modelBuilder);
    }
    
    public DbSet<Configuration> Configurations { get; set; }
    
    public DbSet<LoggingLevels> LoggingLevels { get; set; }
}