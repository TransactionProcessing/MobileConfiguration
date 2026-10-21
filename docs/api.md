# Mobile Configuration API

This document describes the HTTP endpoints and JSON data transfer objects exposed by the service. The service listens on port `9200` by default. The interactive Scalar reference is available at `/scalar`; it is generated from the OpenAPI metadata on the endpoints and DTO properties.

## Conventions

- Request and response bodies use JSON. Property names are camel case (for example, `clientId`).
- Enum values are represented as JSON integers, using the numeric values shown below.
- The API startup configures authorization middleware but does not declare an authentication scheme or endpoint-specific authorization policy. Any protection supplied by hosting infrastructure is outside this contract.
- Configuration responses include `clientSecret`. Treat them as sensitive and protect the API and its traffic accordingly.
- The endpoint behavior below reflects the handlers and repository implementation. Error response bodies are produced by shared exception handling and are not specified here.

## Endpoints

### Create a configuration

`POST /api/transactionmobile/configuration`

Creates a Transaction Mobile configuration.

**Request body:** [`Configuration`](#configuration)

**Responses:**

- `200 OK` with the repository result when the create operation succeeds.
- `400 Bad Request` with the repository result when persistence reports failure.

Example request:

```json
{
  "clientId": "mobile-client",
  "clientSecret": "replace-with-secret",
  "deviceIdentifier": "device-123",
  "enableAutoUpdates": true,
  "hostAddresses": [
    { "serviceType": 2, "uri": "https://transactions.example.com" }
  ],
  "id": "device-123",
  "logLevel": 3
}
```

Example successful response (`200 OK`):

```json
{
  "status": 0,
  "isSuccess": true,
  "isFailed": false,
  "message": "Configuration created successfully.",
  "errors": []
}
```

Example persistence failure (`400 Bad Request`):

```json
{
  "status": 6,
  "isSuccess": false,
  "isFailed": true,
  "message": "Failed to create configuration: persistence failed.",
  "errors": []
}
```

### Get a configuration

`GET /api/transactionmobile/configuration/{id}`

Returns the Transaction Mobile configuration whose ID matches the path value.

**Path parameters:**

| Name | Type | Description |
| --- | --- | --- |
| `id` | string | Configuration identifier. |

**Responses:**

- `200 OK` with a [`ConfigurationResponse`](#configurationresponse) body.
- If no matching configuration exists, the repository throws a not-found exception; the exact HTTP status and body depend on the shared exception handler.

Example response:

```json
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
```

### Create or update a configuration

`PUT /api/transactionmobile/configuration/{id}`

Updates the configuration matching `id`, or creates one if it does not exist. The configuration type is Transaction Mobile.

**Path parameters:**

| Name | Type | Description |
| --- | --- | --- |
| `id` | string | Identifier of the configuration to update or create. |

**Request body:** [`Configuration`](#configuration). The body `id` is used when a new record is created; for an existing record, the path `id` selects the record and the body ID is not used to change its key.

**Responses:**

- `200 OK` with an empty body after persistence completes.
- If saving fails or no database is available, the shared exception handler determines the error response.

### Submit mobile log messages

`POST /api/transactionmobile/logging`

Writes the submitted log-message list to the service logger. This endpoint does not persist the entries through the configuration database.

**Request body:** array of [`LogMessage`](#logmessage) objects.

Example request:

```json
[
  {
    "entryDateTime": "2026-10-21 09:30:00",
    "logLevel": "Information",
    "message": "Mobile client started"
  }
]
```

**Responses:** `200 OK` with an empty body.

### Health checks

- `GET /health` returns health-check details in the service's shared response format.
- `GET /healthui` returns the Health Checks UI response format.

## DTOs

### Configuration

| Property | JSON type | Description |
| --- | --- | --- |
| `clientId` | string | Client identifier. |
| `clientSecret` | string | Client secret. Sensitive value. |
| `deviceIdentifier` | string | Device identifier associated with the configuration. |
| `enableAutoUpdates` | boolean | Whether automatic updates are enabled. |
| `hostAddresses` | array of [`HostAddress`](#hostaddress) | Service endpoints available to the mobile client. |
| `id` | string | Configuration identifier. |
| `logLevel` | integer | [`LoggingLevel`](#logginglevel) value. |

### ConfigurationResponse

The response DTO returned by the GET endpoint. It has the same properties and types as [`Configuration`](#configuration), including `clientSecret`.

### HostAddress

| Property | JSON type | Description |
| --- | --- | --- |
| `serviceType` | integer | [`ServiceType`](#servicetype) value. |
| `uri` | string | URI for the service. |

### LogMessage

| Property | JSON type | Description |
| --- | --- | --- |
| `entryDateTime` | string (date-time) | Timestamp associated with the log entry. |
| `logLevel` | string | Caller-provided level label. |
| `message` | string | Log message text. |

## Enums

### LoggingLevel

| Name | Value |
| --- | ---: |
| `Fatal` | 0 |
| `Error` | 1 |
| `Warning` | 2 |
| `Information` | 3 |
| `Debug` | 4 |
| `Trace` | 5 |

### ServiceType

| Name | Value |
| --- | ---: |
| `EstateManagement` | 0 |
| `Security` | 1 |
| `TransactionProcessorAcl` | 2 |
| `VoucherManagementAcl` | 3 |

## Known contract limitations

- DTO properties are not annotated with validation rules, so this document does not promise field-level validation or required-field error details.
- `PUT` updates the values on the existing record but does not change its identifier.
- The GET response mapping currently preserves `TransactionProcessorAcl` host types; other stored host types map to `Security` in the response.
- The create endpoint returns the shared repository result object. The OpenAPI schema and example describe its current serialized shape.
