using Microsoft.AspNetCore.Mvc;
using MobileConfiguration.DataTransferObjects;
using TransactionProcessing.Shared.Logger;
using TransactionProcessing.Shared.Serialisation;

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
