# MobileConfiguration

Open Scalar at `/scalar` for the interactive, generated API reference. See [the API and DTO reference](docs/api.md) for endpoint routes, request and response shapes, enum values, and examples.

## Development

Install the .NET 11 SDK (version 11.0.100 or later in the 11.0 feature band). The repository pins the SDK in `global.json`.

```sh
dotnet restore MobileConfiguration.sln
dotnet build MobileConfiguration.sln --configuration Release
```

## HTTPS rollout

The service listens on HTTPS port `9200`. Configure the deployment to provide its CA-issued PFX through Kestrel configuration; do not commit the certificate or its password. Set `Kestrel__Certificates__Default__Path` and `Kestrel__Certificates__Default__Password` in the service's deployment secrets. The development certificate is stored at `C:\ProgramData\TransactionProcessing\Certificates\Development\server.pfx`; staging uses `C:\ProgramData\TransactionProcessing\Certificates\Staging\server.pfx`.

The development and staging health monitoring URLs use the service IPs covered by the CA certificate SANs. Callers and probes must trust the private CA and connect using the certificate's matching host name or IP address.
