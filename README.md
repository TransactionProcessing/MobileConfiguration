# MobileConfiguration

Open Scalar at `/scalar` for the interactive, generated API reference. See [the API and DTO reference](docs/api.md) for endpoint routes, request and response shapes, enum values, and examples.

## Development

Install the .NET 11 SDK (version 11.0.100 or later in the 11.0 feature band). The repository pins the SDK in `global.json`.

```sh
dotnet restore MobileConfiguration.sln
dotnet build MobileConfiguration.sln --configuration Release
```
