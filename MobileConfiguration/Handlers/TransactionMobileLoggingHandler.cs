using Microsoft.AspNetCore.Mvc;
using MobileConfiguration.DataTransferObjects;
using Newtonsoft.Json;
using Shared.Logger;

namespace MobileConfiguration.Handlers
{
    public static class TransactionMobileLoggingHandler
    {
        public static Task<IResult> PostLogging(List<LogMessage> logMessages, CancellationToken cancellationToken)
        {
            Logger.LogInformation(JsonConvert.SerializeObject(logMessages));
            return Task.FromResult(Results.Ok() as IResult);
        }
    }
}
