using Microsoft.AspNetCore.Mvc;
using MobileConfiguration.Repository;
using Shared.Results.Web;

namespace MobileConfiguration.Controllers;
using DataTransferObjects;
using Models;

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

        MobileConfiguration configurationModel = Factories.Factory.ToMobileConfiguration(configuration);

        var result = await this.Repository.CreateConfiguration(configurationModel, cancellationToken);

        return result.ToActionResultX();
    }

    [HttpGet]
    [Route("{id}")]
    public async Task<IActionResult> GetConfiguration([FromRoute] String id, CancellationToken cancellationToken) {
        MobileConfiguration configurationModel = await this.Repository.GetConfiguration(ConfigurationType.TransactionMobile, id, cancellationToken);

        ConfigurationResponse response = Factories.Factory.ToConfigurationResponse(configurationModel);
        
        return this.Ok(response);
    }

    [HttpPut]
    public async Task<IActionResult> PutConfiguration(String id, Configuration configuration, CancellationToken cancellationToken) {
        MobileConfiguration configurationModel = Factories.Factory.ToMobileConfiguration(configuration);
        
        await this.Repository.UpdateConfiguration(ConfigurationType.TransactionMobile, id, configurationModel, cancellationToken);

        return this.Ok();
    }
}
