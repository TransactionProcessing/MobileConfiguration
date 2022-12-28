using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MobileConfiguration.Repository;

namespace MobileConfiguration.Controllers
{
    using MobileConfiguration.DataTransferObjects;
    using Models;
    using Newtonsoft.Json;
    using Shared.Logger;
    using ApplicationCentreConfiguration = Database.Entities.ApplicationCentreConfiguration;
    using DTOHostAddress = DataTransferObjects.HostAddress;
    using ModelHostAddress = Models.HostAddress;
    using DTOLoggingLevel = DataTransferObjects.LoggingLevel;
    using ModelLoggingLevel = Models.LoggingLevel;
    using DTOServiceType = DataTransferObjects.ServiceType;
    using ModelServiceType = Models.ServiceType;

    [Route("api/[controller]")]
    [ApiController]
    public class TransactionMobileConfigurationController : ControllerBase
    {
        private readonly IConfigurationRepository Repository;

        public TransactionMobileConfigurationController(IConfigurationRepository repository) {
            this.Repository = repository;
        }
        [HttpPost]
        public async Task<IActionResult> PostConfiguration([FromBody] Configuration configuration,
                                                           CancellationToken cancellationToken) {

            // TODO: Add a factory
            Models.MobileConfiguration configurationModel = new Models.MobileConfiguration {
                                                                            ClientId = configuration.ClientId,
                                                                            ClientSecret = configuration.ClientSecret,
                                                                            ConfigurationType = ConfigurationType.TransactionMobile,
                                                                            DeviceIdentifier = configuration.DeviceIdentifier,
                                                                            EnableAutoUpdates = configuration.EnableAutoUpdates,
                                                                            Id = configuration.Id,
                                                                            HostAddresses = new List<Models.HostAddress>()
                                                                        };

            foreach (DTOHostAddress configurationHostAddress in configuration.HostAddresses) {
                Models.HostAddress hostAddressModel = new Models.HostAddress {
                                                                                 Uri = configurationHostAddress.Uri,
                };
                hostAddressModel.ServiceType = configurationHostAddress.ServiceType switch
                {
                    DTOServiceType.EstateManagement => ModelServiceType.EstateManagement,
                    DTOServiceType.TransactionProcessorAcl => ModelServiceType.TransactionProcessorAcl,
                    DTOServiceType.VoucherManagementAcl => ModelServiceType.VoucherManagementAcl,
                    _ => ModelServiceType.Security
                };

                configurationModel.HostAddresses.Add(hostAddressModel);
            }

            configurationModel.LogLevel = configuration.LogLevel switch {
                DTOLoggingLevel.Debug => ModelLoggingLevel.Debug,
                DTOLoggingLevel.Error => ModelLoggingLevel.Error,
                DTOLoggingLevel.Fatal => ModelLoggingLevel.Fatal,
                DTOLoggingLevel.Information => ModelLoggingLevel.Information,
                DTOLoggingLevel.Trace => ModelLoggingLevel.Trace,
                DTOLoggingLevel.Warning => ModelLoggingLevel.Warning,
                _ => ModelLoggingLevel.Information
            };

            await this.Repository.CreateConfiguration(configurationModel, cancellationToken);

            // TODO: return "correct" response...
            return this.Ok();
        }

        [HttpGet]
        [Route("{id}")]
        public async Task<IActionResult> GetConfiguration([FromRoute] String id, CancellationToken cancellationToken) {
            Models.MobileConfiguration configurationModel = await this.Repository.GetConfiguration(ConfigurationType.TransactionMobile, id, cancellationToken);
            // TODO: base this on the enum value???
            ApplicationCentreConfiguration appCenterConfiguration = await this.Repository.GetAppCentreConfiguration("transactionMobilePOS", cancellationToken);

            ConfigurationResponse response = new ConfigurationResponse {
                                                                           HostAddresses = new List<DTOHostAddress>(),
                                                                           Id = configurationModel.Id,
                                                                           ClientSecret = configurationModel.ClientSecret,
                                                                           DeviceIdentifier = configurationModel.DeviceIdentifier,
                                                                           EnableAutoUpdates = configurationModel.EnableAutoUpdates,
                                                                           ClientId = configurationModel.ClientId,
                                                                       };

            foreach (ModelHostAddress configurationHostAddress in configurationModel.HostAddresses)
            {
                DTOHostAddress hostAddress = new DTOHostAddress
                {
                                                 Uri = configurationHostAddress.Uri,
                                             };
                hostAddress.ServiceType = configurationHostAddress.ServiceType switch
                {
                    ModelServiceType.EstateManagement => DTOServiceType.EstateManagement,
                    ModelServiceType.TransactionProcessorAcl => DTOServiceType.TransactionProcessorAcl,
                    ModelServiceType.VoucherManagementAcl => DTOServiceType.VoucherManagementAcl,
                    _ => DTOServiceType.Security
                };

                response.HostAddresses.Add(hostAddress);
            }

            response.LogLevel = configurationModel.LogLevel switch
            {
                ModelLoggingLevel.Debug => DTOLoggingLevel.Debug,
                ModelLoggingLevel.Error => DTOLoggingLevel.Error,
                ModelLoggingLevel.Fatal => DTOLoggingLevel.Fatal,
                ModelLoggingLevel.Information => DTOLoggingLevel.Information,
                ModelLoggingLevel.Trace => DTOLoggingLevel.Trace,
                ModelLoggingLevel.Warning => DTOLoggingLevel.Warning,
                _ => DTOLoggingLevel.Information
            };

            response.ApplicationCentreConfiguration = new DataTransferObjects.ApplicationCentreConfiguration() {
                                                                                                                   AndroidKey = appCenterConfiguration.AndroidKey,
                                                                                                                   ApplicationId = appCenterConfiguration.ApplicationId,
                                                                                                                   IosKey = appCenterConfiguration.IosKey,
                                                                                                                   MacosKey = appCenterConfiguration.MacosKey,
                                                                                                                   WindowsKey = appCenterConfiguration.WindowsKey,
                                                                                                               };

            // TODO: Convert to a DTO...
            return this.Ok(response);
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

            foreach (DTOHostAddress configurationHostAddress in configuration.HostAddresses)
            {
                Models.HostAddress hostAddressModel = new Models.HostAddress
                {
                    Uri = configurationHostAddress.Uri,
                };
                hostAddressModel.ServiceType = configurationHostAddress.ServiceType switch
                {
                    DTOServiceType.EstateManagement => ModelServiceType.EstateManagement,
                    DTOServiceType.TransactionProcessorAcl => ModelServiceType.TransactionProcessorAcl,
                    DTOServiceType.VoucherManagementAcl => ModelServiceType.VoucherManagementAcl,
                    _ => ModelServiceType.Security
                };

                configurationModel.HostAddresses.Add(hostAddressModel);
            }

            configurationModel.LogLevel = configuration.LogLevel switch
            {
                DTOLoggingLevel.Debug => ModelLoggingLevel.Debug,
                DTOLoggingLevel.Error => ModelLoggingLevel.Error,
                DTOLoggingLevel.Fatal => ModelLoggingLevel.Fatal,
                DTOLoggingLevel.Information => ModelLoggingLevel.Information,
                DTOLoggingLevel.Trace => ModelLoggingLevel.Trace,
                DTOLoggingLevel.Warning => ModelLoggingLevel.Warning,
                _ => ModelLoggingLevel.Information
            };

            await this.Repository.UpdateConfiguration(ConfigurationType.TransactionMobile, id, configurationModel, cancellationToken);

            return this.Ok();
        }
    }

    [Route("api/[controller]")]
    [ApiController]
    public class TransactionMobileLoggingController : ControllerBase
    {
        private readonly IConfigurationRepository Repository;

        public TransactionMobileLoggingController() {
            
        }

        [HttpPost]
        public async Task<IActionResult> PostConfiguration([FromBody] List<LogMessage> logMessages,
                                                           CancellationToken cancellationToken) {

            Logger.LogInformation(JsonConvert.SerializeObject(logMessages));

            // TODO: return "correct" response...
            return this.Ok();
        }
    }

    public class LogMessage
    {
        public DateTime EntryDateTime { get; set; }
        public String LogLevel { get; set; }
        public String Message { get; set; }
    }
}
