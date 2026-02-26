using MobileConfiguration.DataTransferObjects;
using MobileConfiguration.Factories;
using MobileConfiguration.Models;
using MobileConfiguration.Repository;
using Shared.Results.Web;

namespace MobileConfiguration.Handlers
{
    public static class TransactionMobileConfigurationHandler
    {
        public static async Task<IResult> PostConfiguration(IConfigurationRepository repository,
                                                             Configuration configuration,
                                                             CancellationToken cancellationToken)
        {
            Models.MobileConfiguration configurationModel = Factory.ToMobileConfiguration(configuration);

            var result = await repository.CreateConfiguration(configurationModel, cancellationToken);

            // Attempt to map the result to an appropriate HTTP response.
            // The shared library previously used an extension to convert to IActionResult from controllers.
            // For minimal APIs return 200 on success and 400 on failure.
            try
            {
                // Try to read a `Success` or `IsSuccess` property via dynamic - fallback if not present.
                dynamic dyn = result;
                bool success = false;

                if (HasProperty(dyn, "Success")) success = (bool)dyn.Success;
                else if (HasProperty(dyn, "IsSuccess")) success = (bool)dyn.IsSuccess;
                else if (HasProperty(dyn, "Succeeded")) success = (bool)dyn.Succeeded;

                if (success)
                {
                    return Results.Ok(result);
                }

                return Results.BadRequest(result);
            }
            catch
            {
                // If we cannot inspect the result type, return OK with the result object.
                return Results.Ok(result);
            }
        }

        public static async Task<IResult> GetConfiguration(IConfigurationRepository repository,
                                                            string id,
                                                            CancellationToken cancellationToken)
        {
            Models.MobileConfiguration configurationModel = await repository.GetConfiguration(ConfigurationType.TransactionMobile, id, cancellationToken);

            ConfigurationResponse response = Factory.ToConfigurationResponse(configurationModel);

            return Results.Ok(response);
        }

        public static async Task<IResult> PutConfiguration(IConfigurationRepository repository,
                                                            string id,
                                                            Configuration configuration,
                                                            CancellationToken cancellationToken)
        {
            Models.MobileConfiguration configurationModel = Factory.ToMobileConfiguration(configuration);

            await repository.UpdateConfiguration(ConfigurationType.TransactionMobile, id, configurationModel, cancellationToken);

            return Results.Ok();
        }

        private static bool HasProperty(dynamic obj, string name)
        {
            try
            {
                var dictionary = obj as System.Collections.IDictionary;
                if (dictionary != null)
                {
                    return dictionary.Contains(name);
                }

                // fallback to reflection
                return obj.GetType().GetProperty(name) != null;
            }
            catch
            {
                return false;
            }
        }
    }
}
