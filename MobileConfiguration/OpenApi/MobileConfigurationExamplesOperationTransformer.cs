using System.Text.Json.Nodes;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace MobileConfiguration.OpenApi;

/// <summary>
/// Adds representative request and response payloads to the generated OpenAPI document.
/// </summary>
internal sealed class MobileConfigurationExamplesOperationTransformer : IOpenApiOperationTransformer
{
    private const string ConfigurationExample = """
        {
          "clientId": "mobile-client",
          "clientSecret": "example-secret",
          "deviceIdentifier": "device-123",
          "enableAutoUpdates": true,
          "hostAddresses": [
            { "serviceType": 2, "uri": "https://transactions.example.test" }
          ],
          "id": "device-123",
          "logLevel": 3
        }
        """;

    private const string ConfigurationResponseExample = """
        {
          "clientId": "mobile-client",
          "clientSecret": "example-secret",
          "deviceIdentifier": "device-123",
          "enableAutoUpdates": true,
          "hostAddresses": [
            { "serviceType": 2, "uri": "https://transactions.example.test" }
          ],
          "id": "device-123",
          "logLevel": 3
        }
        """;

    private const string LogMessagesExample = """
        [
          {
            "entryDateTime": "2026-10-21 09:30:00",
            "logLevel": "Information",
            "message": "Mobile client started"
          }
        ]
        """;

    public Task TransformAsync(OpenApiOperation operation,
                               OpenApiOperationTransformerContext context,
                               CancellationToken cancellationToken)
    {
        string path = context.Description.RelativePath?.Trim('/') ?? string.Empty;
        string? requestExample = (context.Description.HttpMethod, path) switch
        {
            ("POST", "api/transactionmobile/configuration") => ConfigurationExample,
            ("PUT", "api/transactionmobile/configuration/{id}") => ConfigurationExample,
            ("POST", "api/transactionmobile/logging") => LogMessagesExample,
            _ => null
        };

        if (requestExample is not null
            && operation.RequestBody is OpenApiRequestBody requestBody
            && requestBody.Content is not null
            && requestBody.Content.TryGetValue("application/json", out IOpenApiMediaType? requestMediaType)
            && requestMediaType is OpenApiMediaType openApiRequestMediaType)
        {
            openApiRequestMediaType.Example = JsonNode.Parse(requestExample);
        }

        string? successResponseExample = (context.Description.HttpMethod, path) switch
        {
            ("GET", "api/transactionmobile/configuration/{id}") => ConfigurationResponseExample,
            _ => null
        };

        SetResponseExample(operation, "200", successResponseExample);

        return Task.CompletedTask;
    }

    private static void SetResponseExample(OpenApiOperation operation, string statusCode, string? example)
    {
        if (example is not null
            && operation.Responses is not null
            && operation.Responses.TryGetValue(statusCode, out IOpenApiResponse? response)
            && response.Content is not null
            && response.Content.TryGetValue("application/json", out IOpenApiMediaType? mediaType)
            && mediaType is OpenApiMediaType openApiMediaType)
        {
            openApiMediaType.Example = JsonNode.Parse(example);
        }
    }
}
