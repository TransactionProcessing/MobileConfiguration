namespace MobileConfiguration.Database;

using Microsoft.EntityFrameworkCore;
using Shared.General;

public class ConfigurationMySqlContext : ConfigurationGenericContext
{
    public ConfigurationMySqlContext() : base("MySql", ConfigurationReader.GetConnectionString("ConfigurationDatabase")) {
    }

    public ConfigurationMySqlContext(String connectionString) : base("MySql", connectionString) {
    }

    public ConfigurationMySqlContext(DbContextOptions dbContextOptions) : base(dbContextOptions) {
    }

    protected override void OnConfiguring(DbContextOptionsBuilder options) {
        if (!string.IsNullOrWhiteSpace(this.ConnectionString)) {
            options.UseMySql(this.ConnectionString, ServerVersion.Parse("8.0.27")).AddInterceptors(new MySqlIgnoreDuplicatesOnInsertInterceptor());
        }
    }
}