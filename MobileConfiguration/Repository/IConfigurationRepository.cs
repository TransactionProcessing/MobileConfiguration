using SimpleResults;

namespace MobileConfiguration.Repository;

using Database;
using Database.Entities;
using Microsoft.EntityFrameworkCore;
using Models;
using Newtonsoft.Json;
using Shared.EntityFramework;
using Shared.Exceptions;
using Shared.General;
using Shared.Repositories;
using ApplicationCentreConfiguration = Database.Entities.ApplicationCentreConfiguration;

public interface IConfigurationRepository
{
    Task<MobileConfiguration> GetConfiguration(ConfigurationType configurationType, String id, CancellationToken cancellationToken);
    Task<Result> CreateConfiguration(MobileConfiguration mobileConfiguration, CancellationToken cancellationToken);

    Task UpdateConfiguration(ConfigurationType configurationType,
                             String id,
                             MobileConfiguration mobileConfiguration, CancellationToken cancellationToken);
}

public class ConfigurationRepository : IConfigurationRepository {
    private readonly IDbContextResolver<ConfigurationContext> Resolver;
    private static readonly String ConfigDatabaseName = "ConfigurationDatabase";

    public ConfigurationRepository(IDbContextResolver<ConfigurationContext> resolver) {
        this.Resolver = resolver;
    }

    public async Task<MobileConfiguration> GetConfiguration(ConfigurationType configurationType,
                                                            String id,
                                                            CancellationToken cancellationToken) {
        using ResolvedDbContext<ConfigurationContext>? resolvedContext = this.Resolver.Resolve(ConfigDatabaseName);
        Configuration? configuration = await resolvedContext.Context.Configurations.SingleOrDefaultAsync(c => c.Id == id && c.ConfigType == (Int32)configurationType, cancellationToken: cancellationToken);

        if (configuration == null) {
            throw new NotFoundException($"No config of type [{configurationType}] found for Id [{id}]");
        }

        // TODO: create a factory
        MobileConfiguration configurationModel = new() {
            ClientId = configuration.ClientId,
            ClientSecret = configuration.ClientSecret,
            ConfigurationType = (ConfigurationType)configuration.ConfigType,
            DeviceIdentifier = configuration.DeviceIdentifier,
            EnableAutoUpdates = configuration.EnableAutoUpdates,
            Id = configuration.Id,
            HostAddresses = JsonConvert.DeserializeObject<List<HostAddress>>(configuration.HostAddresses)
        };

        configurationModel.LogLevel = configuration.LogLevelId switch {
            (Int32)LoggingLevel.Debug => Models.LoggingLevel.Debug,
            (Int32)LoggingLevel.Error => Models.LoggingLevel.Error,
            (Int32)LoggingLevel.Fatal => Models.LoggingLevel.Fatal,
            (Int32)LoggingLevel.Information => Models.LoggingLevel.Information,
            (Int32)LoggingLevel.Trace => Models.LoggingLevel.Trace,
            (Int32)LoggingLevel.Warning => Models.LoggingLevel.Warning,
            _ => Models.LoggingLevel.Information
        };

        return configurationModel;
    }

    public async Task<Result> CreateConfiguration(MobileConfiguration mobileConfiguration,
                                                  CancellationToken cancellationToken) {
        Configuration configurationEntity = Factories.Factory.ToEntityConfiguration(mobileConfiguration);
        using ResolvedDbContext<ConfigurationContext>? resolvedContext = this.Resolver.Resolve(ConfigDatabaseName);
        await resolvedContext.Context.Configurations.AddAsync(configurationEntity, cancellationToken);

        try {
            await resolvedContext.Context.SaveChangesAsync(cancellationToken);
            return Result.Success("Configuration created successfully.");
        }
        catch (Exception ex) {
            return Result.Failure($"Failed to create configuration: {ex.Message}");
        }
    }

    public async Task UpdateConfiguration(ConfigurationType configurationType,
                                          String id,
                                          MobileConfiguration mobileConfiguration,
                                          CancellationToken cancellationToken) {
        using ResolvedDbContext<ConfigurationContext>? resolvedContext = this.Resolver.Resolve(ConfigDatabaseName);
        Configuration? configuration = await resolvedContext.Context.Configurations.SingleOrDefaultAsync(c => c.Id == id && c.ConfigType == (Int32)configurationType, cancellationToken: cancellationToken);

        if (configuration == null) {
            // Create a new configuration
            configuration = new Configuration {
                ClientSecret = mobileConfiguration.ClientSecret,
                DeviceIdentifier = mobileConfiguration.DeviceIdentifier,
                LogLevelId = (Int32)mobileConfiguration.LogLevel,
                ClientId = mobileConfiguration.ClientId,
                EnableAutoUpdates = mobileConfiguration.EnableAutoUpdates,
                HostAddresses = JsonConvert.SerializeObject(mobileConfiguration.HostAddresses),
                ConfigType = (Int32)mobileConfiguration.ConfigurationType,
                Id = mobileConfiguration.Id,
            };
            await resolvedContext.Context.Configurations.AddAsync(configuration, cancellationToken);
        }
        else {
            configuration.ClientSecret = mobileConfiguration.ClientSecret;
            configuration.DeviceIdentifier = mobileConfiguration.DeviceIdentifier;
            configuration.LogLevelId = (Int32)mobileConfiguration.LogLevel;
            configuration.ClientId = mobileConfiguration.ClientId;
            configuration.EnableAutoUpdates = mobileConfiguration.EnableAutoUpdates;
            configuration.HostAddresses = JsonConvert.SerializeObject(mobileConfiguration.HostAddresses);
        }

        await resolvedContext.Context.SaveChangesAsync(cancellationToken);
    }
}
