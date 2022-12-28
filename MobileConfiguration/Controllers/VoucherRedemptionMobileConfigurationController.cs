namespace MobileConfiguration.Controllers;

using DataTransferObjects;
using Microsoft.AspNetCore.Mvc;
using Models;
using Repository;
using HostAddress = DataTransferObjects.HostAddress;
using LoggingLevel = DataTransferObjects.LoggingLevel;
using ServiceType = DataTransferObjects.ServiceType;

[Route("api/[controller]")]
[ApiController]
public class VoucherRedemptionMobileConfigurationController : ControllerBase
{
    private readonly IConfigurationRepository Repository;

    public VoucherRedemptionMobileConfigurationController(IConfigurationRepository repository)
    {
        this.Repository = repository;
    }
        
    [HttpPost]
    public async Task<IActionResult> PostConfiguration([FromBody] Configuration configuration,
                                                       CancellationToken cancellationToken)
    {

        // TODO: Add a factory
        Models.MobileConfiguration configurationModel = new Models.MobileConfiguration
                                                        {
                                                            ClientId = configuration.ClientId,
                                                            ClientSecret = configuration.ClientSecret,
                                                            ConfigurationType = ConfigurationType.VoucherRedemption,
                                                            DeviceIdentifier = configuration.DeviceIdentifier,
                                                            EnableAutoUpdates = configuration.EnableAutoUpdates,
                                                            Id = configuration.Id,
                                                            HostAddresses = new List<Models.HostAddress>()
                                                        };

        foreach (HostAddress configurationHostAddress in configuration.HostAddresses)
        {
            Models.HostAddress hostAddressModel = new Models.HostAddress
                                                  {
                                                      Uri = configurationHostAddress.Uri,
                                                  };
            hostAddressModel.ServiceType = configurationHostAddress.ServiceType switch
            {
                ServiceType.EstateManagement => Models.ServiceType.EstateManagement,
                ServiceType.TransactionProcessorAcl => Models.ServiceType.TransactionProcessorAcl,
                ServiceType.VoucherManagementAcl => Models.ServiceType.VoucherManagementAcl,
                _ => Models.ServiceType.Security
            };

            configurationModel.HostAddresses.Add(hostAddressModel);
        }

        configurationModel.LogLevel = configuration.LogLevel switch
        {
            LoggingLevel.Debug => Models.LoggingLevel.Debug,
            LoggingLevel.Error => Models.LoggingLevel.Error,
            LoggingLevel.Fatal => Models.LoggingLevel.Fatal,
            LoggingLevel.Information => Models.LoggingLevel.Information,
            LoggingLevel.Trace => Models.LoggingLevel.Trace,
            LoggingLevel.Warning => Models.LoggingLevel.Warning,
            _ => Models.LoggingLevel.Information
        };

        await this.Repository.CreateConfiguration(configurationModel, cancellationToken);

        // TODO: return "correct" response...
        return this.Ok();
    }

    [HttpGet]
    [Route("{id}")]
    public async Task<IActionResult> GetConfiguration([FromRoute] String id, CancellationToken cancellationToken)
    {
        Models.MobileConfiguration config = await this.Repository.GetConfiguration(ConfigurationType.VoucherRedemption, id, cancellationToken);

        // TODO: Convert to a DTO...
        return this.Ok(config);
    }

    [HttpPut]
    public async Task<IActionResult> PutConfiguration(String id, Configuration configuration, CancellationToken cancellationToken)
    {
        Models.MobileConfiguration configurationModel = new Models.MobileConfiguration
                                                        {
                                                            ClientId = configuration.ClientId,
                                                            ClientSecret = configuration.ClientSecret,
                                                            ConfigurationType = ConfigurationType.TransactionMobile,
                                                            DeviceIdentifier = configuration.DeviceIdentifier,
                                                            EnableAutoUpdates = configuration.EnableAutoUpdates,
                                                            Id = configuration.Id,
                                                            HostAddresses = new List<Models.HostAddress>()
                                                        };

        foreach (HostAddress configurationHostAddress in configuration.HostAddresses)
        {
            Models.HostAddress hostAddressModel = new Models.HostAddress
                                                  {
                                                      Uri = configurationHostAddress.Uri,
                                                  };
            hostAddressModel.ServiceType = configurationHostAddress.ServiceType switch
            {
                ServiceType.EstateManagement => Models.ServiceType.EstateManagement,
                ServiceType.TransactionProcessorAcl => Models.ServiceType.TransactionProcessorAcl,
                ServiceType.VoucherManagementAcl => Models.ServiceType.VoucherManagementAcl,
                _ => Models.ServiceType.Security
            };

            configurationModel.HostAddresses.Add(hostAddressModel);
        }

        configurationModel.LogLevel = configuration.LogLevel switch
        {
            LoggingLevel.Debug => Models.LoggingLevel.Debug,
            LoggingLevel.Error => Models.LoggingLevel.Error,
            LoggingLevel.Fatal => Models.LoggingLevel.Fatal,
            LoggingLevel.Information => Models.LoggingLevel.Information,
            LoggingLevel.Trace => Models.LoggingLevel.Trace,
            LoggingLevel.Warning => Models.LoggingLevel.Warning,
            _ => Models.LoggingLevel.Information
        };

        await this.Repository.UpdateConfiguration(ConfigurationType.TransactionMobile, id, configurationModel, cancellationToken);

        return this.Ok();
    }
}