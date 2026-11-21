using MobileConfiguration.DataTransferObjects;
using MobileConfiguration.Factories;
using MobileConfiguration.Models;
using MobileConfiguration.Repository;
using TransactionProcessing.UnionResults;
using Results = TransactionProcessing.UnionResults.Results;

namespace MobileConfiguration.Handlers
{
    public static class TransactionMobileConfigurationHandler
    {
        public static async Task<Result> PostConfiguration(IConfigurationRepository repository,
                                                             Configuration configuration,
                                                             CancellationToken cancellationToken)
        {
            Models.MobileConfiguration configurationModel = Factory.ToMobileConfiguration(configuration);

            var result = await repository.CreateConfiguration(configurationModel, cancellationToken);

            return result;
        }

        public static async Task<Result<ConfigurationResponse>> GetConfiguration(IConfigurationRepository repository,
                                                            string id,
                                                            CancellationToken cancellationToken)
        {
            Models.MobileConfiguration configurationModel = await repository.GetConfiguration(ConfigurationType.TransactionMobile, id, cancellationToken);

            ConfigurationResponse response = Factory.ToConfigurationResponse(configurationModel);

            return Results.Ok(response);
        }

        public static async Task<Result> PutConfiguration(IConfigurationRepository repository,
                                                            string id,
                                                            Configuration configuration,
                                                            CancellationToken cancellationToken)
        {
            Models.MobileConfiguration configurationModel = Factory.ToMobileConfiguration(configuration);

            await repository.UpdateConfiguration(ConfigurationType.TransactionMobile, id, configurationModel, cancellationToken);

            return Results.Ok();
        }
    }
}
