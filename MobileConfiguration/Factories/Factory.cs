using MobileConfiguration.DataTransferObjects;
using MobileConfiguration.Models;
using Newtonsoft.Json;
using HostAddress = MobileConfiguration.DataTransferObjects.HostAddress;
using LoggingLevel = MobileConfiguration.DataTransferObjects.LoggingLevel;
using ServiceType = MobileConfiguration.DataTransferObjects.ServiceType;

namespace MobileConfiguration.Factories;

public static class Factory {
    public static Models.MobileConfiguration ToMobileConfiguration(Configuration configuration) {
        Models.MobileConfiguration configurationModel = new() {
            ClientId = configuration.ClientId,
            ClientSecret = configuration.ClientSecret,
            ConfigurationType = ConfigurationType.TransactionMobile,
            DeviceIdentifier = configuration.DeviceIdentifier,
            EnableAutoUpdates = configuration.EnableAutoUpdates,
            Id = configuration.Id,
            HostAddresses = new List<Models.HostAddress>()
        };

        foreach (HostAddress configurationHostAddress in configuration.HostAddresses) {
            Models.HostAddress hostAddressModel = new() {
                Uri = configurationHostAddress.Uri,
                ServiceType = configurationHostAddress.ServiceType switch {
                    ServiceType.EstateManagement => Models.ServiceType.EstateManagement,
                    ServiceType.TransactionProcessorAcl => Models.ServiceType.TransactionProcessorAcl,
                    ServiceType.VoucherManagementAcl => Models.ServiceType.VoucherManagementAcl,
                    _ => Models.ServiceType.Security
                }
            };

            configurationModel.HostAddresses.Add(hostAddressModel);
        }

        configurationModel.LogLevel = configuration.LogLevel switch {
            LoggingLevel.Debug => Models.LoggingLevel.Debug,
            LoggingLevel.Error => Models.LoggingLevel.Error,
            LoggingLevel.Fatal => Models.LoggingLevel.Fatal,
            LoggingLevel.Information => Models.LoggingLevel.Information,
            LoggingLevel.Trace => Models.LoggingLevel.Trace,
            LoggingLevel.Warning => Models.LoggingLevel.Warning,
            _ => Models.LoggingLevel.Information
        };

        return configurationModel;
    }

    public static Database.Entities.Configuration ToEntityConfiguration(Models.MobileConfiguration configurationModel) {
        Database.Entities.Configuration configurationEntity = new() {
            Id = configurationModel.Id,
            ConfigType = (Int32)ConfigurationType.TransactionMobile,
            ClientId = configurationModel.ClientId,
            ClientSecret = configurationModel.ClientSecret,
            DeviceIdentifier = configurationModel.DeviceIdentifier,
            EnableAutoUpdates = configurationModel.EnableAutoUpdates,
            LogLevelId = (Int32)configurationModel.LogLevel,
            HostAddresses = JsonConvert.SerializeObject(configurationModel.HostAddresses)
        };

        return configurationEntity;
    }

    public static ConfigurationResponse ToConfigurationResponse(Models.MobileConfiguration configurationModel) {
        ConfigurationResponse response = new ConfigurationResponse {
            HostAddresses = new List<HostAddress>(),
            Id = configurationModel.Id,
            ClientSecret = configurationModel.ClientSecret,
            DeviceIdentifier = configurationModel.DeviceIdentifier,
            EnableAutoUpdates = configurationModel.EnableAutoUpdates,
            ClientId = configurationModel.ClientId,
        };

        foreach (Models.HostAddress configurationHostAddress in configurationModel.HostAddresses) {
            HostAddress hostAddress = new HostAddress { Uri = configurationHostAddress.Uri, };
            hostAddress.ServiceType = configurationHostAddress.ServiceType switch {
                Models.ServiceType.TransactionProcessorAcl => ServiceType.TransactionProcessorAcl,
                _ => ServiceType.Security
            };

            response.HostAddresses.Add(hostAddress);
        }

        response.LogLevel = configurationModel.LogLevel switch {
            Models.LoggingLevel.Debug => LoggingLevel.Debug,
            Models.LoggingLevel.Error => LoggingLevel.Error,
            Models.LoggingLevel.Fatal => LoggingLevel.Fatal,
            Models.LoggingLevel.Information => LoggingLevel.Information,
            Models.LoggingLevel.Trace => LoggingLevel.Trace,
            Models.LoggingLevel.Warning => LoggingLevel.Warning,
            _ => LoggingLevel.Information
        };

        return response;
    }
}