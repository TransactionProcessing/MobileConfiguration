using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MobileConfiguration.DataTransferObjects;
using MobileConfiguration.Models;
using MobileConfiguration.Repository;

namespace MobileConfiguration.Controllers
{
    using ApplicationCentreConfiguration = DataTransferObjects.ApplicationCentreConfiguration;

    [Route("api/[controller]")]
    [ApiController]
    public class ApplicationCentreConfigurationController : ControllerBase
    {
        private readonly IConfigurationRepository Repository;

        public ApplicationCentreConfigurationController(IConfigurationRepository repository)
        {
            this.Repository = repository;
        }

        [HttpPost]
        public async Task<IActionResult> PostConfiguration([FromBody] ApplicationCentreConfiguration configuration,
                                                           CancellationToken cancellationToken) {

            Models.ApplicationCentreConfiguration configurationModel = new Models.ApplicationCentreConfiguration {
                                                                           AndroidKey = configuration.AndroidKey,
                                                                           ApplicationId = configuration.ApplicationId,
                                                                           IosKey = configuration.IosKey,
                                                                           MacosKey = configuration.MacosKey,
                                                                           WindowsKey = configuration.WindowsKey,
                                                                       };

            await this.Repository.CreateAppCentreConfiguration(configurationModel, cancellationToken);

            return this.Ok();
        }

        [HttpGet]
        [Route("{applicationid}")]
        public async Task<IActionResult> GetConfiguration([FromRoute] String applicationid, CancellationToken cancellationToken)
        {
            var config = await this.Repository.GetAppCentreConfiguration(applicationid, cancellationToken);

            // TODO: Convert to a DTO...
            return this.Ok(config);
        }
    }
}
