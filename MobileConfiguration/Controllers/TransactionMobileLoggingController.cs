using Microsoft.AspNetCore.Mvc;
using MobileConfiguration.DataTransferObjects;
using MobileConfiguration.Repository;
using Newtonsoft.Json;
using Shared.Logger;

namespace MobileConfiguration.Controllers;

[Route("api/[controller]")]
[ApiController]
public class TransactionMobileLoggingController : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> PostConfiguration([FromBody] List<LogMessage> logMessages,
                                                       CancellationToken cancellationToken) {

        Logger.LogInformation(JsonConvert.SerializeObject(logMessages));

        return this.Ok();
    }
}