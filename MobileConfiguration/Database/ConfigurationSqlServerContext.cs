namespace MobileConfiguration.Database;

using Microsoft.EntityFrameworkCore;
using Shared.General;

public class ConfigurationSqlServerContext : ConfigurationGenericContext
{
    public ConfigurationSqlServerContext() : base("SqlServer", ConfigurationReader.GetConnectionString("ConfigurationDatabase")) {
    }

    public ConfigurationSqlServerContext(String connectionString) : base("SqlServer", connectionString) {
    }

    public ConfigurationSqlServerContext(DbContextOptions dbContextOptions) : base(dbContextOptions) {
    }

    protected override void OnConfiguring(DbContextOptionsBuilder options) {
        if (!string.IsNullOrWhiteSpace(this.ConnectionString)) {
            options.UseSqlServer(this.ConnectionString);
        }
    }
}