namespace MobileConfiguration.Database;

using System.Reflection;
using Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shared.Logger;

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
}