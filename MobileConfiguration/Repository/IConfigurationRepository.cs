namespace MobileConfiguration.Repository
{
    using Database;
    using Database.Entities;
    using Models;
    using Newtonsoft.Json;
    using Shared.EntityFramework;
    using Shared.Exceptions;
    using Microsoft.EntityFrameworkCore;
    using ApplicationCentreConfiguration = Database.Entities.ApplicationCentreConfiguration;

    public interface IConfigurationRepository
    {
        Task<MobileConfiguration> GetConfiguration(ConfigurationType configurationType, String id, CancellationToken cancellationToken);
        Task CreateConfiguration(MobileConfiguration mobileConfiguration, CancellationToken cancellationToken);

        Task UpdateConfiguration(ConfigurationType configurationType,
                                 String id,
                                 MobileConfiguration mobileConfiguration, CancellationToken cancellationToken);

        Task CreateAppCentreConfiguration(Models.ApplicationCentreConfiguration mobileConfiguration, CancellationToken cancellationToken);

        Task<ApplicationCentreConfiguration> GetAppCentreConfiguration(String applicationId, CancellationToken cancellationToken);
    }

    public class ConfigurationRepository : IConfigurationRepository
    {
        private readonly Shared.EntityFramework.IDbContextFactory<ConfigurationGenericContext> ContextFactory;

        public ConfigurationRepository(Shared.EntityFramework.IDbContextFactory<ConfigurationGenericContext> contextFactory) {
            this.ContextFactory = contextFactory;
        }
        public async Task<MobileConfiguration> GetConfiguration(ConfigurationType configurationType,
                                                                String id,
                                                                CancellationToken cancellationToken) {
            ConfigurationGenericContext context = await this.ContextFactory.GetContext(Guid.NewGuid(), "ConfigurationDatabase",cancellationToken);

            Configuration? configuration = await context.Configurations.SingleOrDefaultAsync(c => c.Id == id && c.ConfigType == (Int32)configurationType, cancellationToken:cancellationToken);

            if (configuration == null) {
                throw new NotFoundException($"No config of type [{configurationType}] found for Id [{id}]");
            }

            // TODO: create a factory
            MobileConfiguration configurationModel = new MobileConfiguration {
                                                                                 ClientId = configuration.ClientId,
                                                                                 ClientSecret = configuration.ClientSecret,
                                                                                 ConfigurationType = (ConfigurationType)configuration.ConfigType,
                                                                                 DeviceIdentifier = configuration.DeviceIdentifier,
                                                                                 EnableAutoUpdates = configuration.EnableAutoUpdates,
                                                                                 Id = configuration.Id,
                                                                                 HostAddresses =
                                                                                     JsonConvert.DeserializeObject<List<HostAddress>>(configuration.HostAddresses)
                                                                             };

            configurationModel.LogLevel = configuration.LogLevelId switch
            {
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

        public async Task CreateConfiguration(MobileConfiguration mobileConfiguration,
                                              CancellationToken cancellationToken) {
            ConfigurationGenericContext context = await this.ContextFactory.GetContext(Guid.NewGuid(), "ConfigurationDatabase", cancellationToken);

            Configuration configurationEntity = new Configuration {
                                                                      Id = mobileConfiguration.Id,
                                                                      ConfigType = (Int32)ConfigurationType.TransactionMobile,
                                                                      ClientId = mobileConfiguration.ClientId,
                                                                      ClientSecret = mobileConfiguration.ClientSecret,
                                                                      DeviceIdentifier = mobileConfiguration.DeviceIdentifier,
                                                                      EnableAutoUpdates = mobileConfiguration.EnableAutoUpdates,
                                                                      LogLevelId = (Int32)mobileConfiguration.LogLevel,
                                                                      HostAddresses = JsonConvert.SerializeObject(mobileConfiguration.HostAddresses)
                                                                  };

            await context.Configurations.AddAsync(configurationEntity,cancellationToken);
            await context.SaveChangesAsync(cancellationToken);
        }

        public async Task UpdateConfiguration(ConfigurationType configurationType,
                                              String id, 
                                              MobileConfiguration mobileConfiguration,
                                              CancellationToken cancellationToken) {
            ConfigurationGenericContext context = await this.ContextFactory.GetContext(Guid.NewGuid(), "ConfigurationDatabase", cancellationToken);

            Configuration? configuration = await context.Configurations.SingleOrDefaultAsync(c => c.Id == id && c.ConfigType == (Int32)configurationType, cancellationToken: cancellationToken);

            if (configuration == null)
            {
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
                await context.Configurations.AddAsync(configuration, cancellationToken);
            }
            else {
                configuration.ClientSecret = mobileConfiguration.ClientSecret;
                configuration.DeviceIdentifier = mobileConfiguration.DeviceIdentifier;
                configuration.LogLevelId = (Int32)mobileConfiguration.LogLevel;
                configuration.ClientId = mobileConfiguration.ClientId;
                configuration.EnableAutoUpdates = mobileConfiguration.EnableAutoUpdates;
                configuration.HostAddresses = JsonConvert.SerializeObject(mobileConfiguration.HostAddresses);
            }

            await context.SaveChangesAsync(cancellationToken);
        }

        public async Task CreateAppCentreConfiguration(Models.ApplicationCentreConfiguration configuration,
                                                       CancellationToken cancellationToken) {
            ConfigurationGenericContext context = await this.ContextFactory.GetContext(Guid.NewGuid(), "ConfigurationDatabase", cancellationToken);

            ApplicationCentreConfiguration configurationEntity = new ApplicationCentreConfiguration {
                                                                                                         AndroidKey = configuration.AndroidKey,
                                                                                                         ApplicationId = configuration.ApplicationId,
                                                                                                         IosKey = configuration.IosKey,
                                                                                                         MacosKey = configuration.MacosKey,
                                                                                                         WindowsKey = configuration.WindowsKey
                                                                                                     };

            await context.ApplicationCentreConfigurations.AddAsync(configurationEntity, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);
        }

        public async Task<ApplicationCentreConfiguration> GetAppCentreConfiguration(String applicationId,
                                                                                    CancellationToken cancellationToken) {
            ConfigurationGenericContext context = await this.ContextFactory.GetContext(Guid.NewGuid(), "ConfigurationDatabase", cancellationToken);

            Database.Entities.ApplicationCentreConfiguration? configuration = await context.ApplicationCentreConfigurations.SingleOrDefaultAsync(c => c.ApplicationId == applicationId, cancellationToken: cancellationToken);

            if (configuration == null)
            {
                throw new NotFoundException($"No app centre config for application Id [{applicationId}]");
            }

            return new ApplicationCentreConfiguration {
                                                          AndroidKey = configuration.AndroidKey,
                                                          ApplicationId = configuration.ApplicationId,
                                                          IosKey = configuration.IosKey,
                                                          MacosKey = configuration.MacosKey,
                                                          WindowsKey = configuration.WindowsKey,
                                                      };
        }
    }
}
