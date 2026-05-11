using Microsoft.AspNetCore.Mvc;
using MobileConfiguration.DataTransferObjects;
using Shared.Logger;
using Shared.Serialisation;

namespace MobileConfiguration.Handlers
{
    public static class TransactionMobileLoggingHandler
    {
        public static Task<IResult> PostLogging(List<LogMessage> logMessages, CancellationToken cancellationToken)
        {
            Logger.LogInformation(StringSerialiser.Serialise(logMessages));
            return Task.FromResult(Results.Ok() as IResult);
        }
    }
}
