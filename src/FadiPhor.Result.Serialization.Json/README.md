# FadiPhor.Result.Serialization.Json

`System.Text.Json` serialization for `Result<T>` with polymorphic `Error` support, plus a JSON envelope transport layer for request/response protocols.

---

## Project Structure

```
FadiPhor.Result.Serialization.Json
├── Converters/          Result<T> JSON converters
├── Errors/              Polymorphic Error resolution
├── Configuration/       JsonSerializerOptions setup, FadiPhorJsonOptions, DI extensions
└── Transport/           JsonEnvelope, IJsonEnvelopeSerializer, request type registry
```

| Namespace | Purpose |
|---|---|
| `FadiPhor.Result.Serialization.Json` | DI entry points (`AddResultSerialization`, `AddFadiPhorResultProtocol`) |
| `FadiPhor.Result.Serialization.Json.Configuration` | `AddResultSerialization`, `FadiPhorJsonOptions` |
| `FadiPhor.Result.Serialization.Json.Errors` | `IErrorPolymorphicResolver` |
| `FadiPhor.Result.Serialization.Json.Transport` | `JsonEnvelope`, `IJsonEnvelopeSerializer` |

---

## Registration

### Standalone (no DI)

```csharp
using FadiPhor.Result.Serialization.Json.Configuration;

var options = new JsonSerializerOptions
{
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
};

// No custom errors — core types (ValidationFailure) are registered automatically.
options.AddResultSerialization();

// With custom error types:
options.AddResultSerialization(new MyErrorResolver());

// Multiple resolvers:
options.AddResultSerialization(new DomainErrorResolver(), new AuthErrorResolver());
```

If a `TypeInfoResolver` is already set on the options, `AddResultSerialization` preserves it and combines both.

### DI registration (serialization only)

For applications using DI that need `Result<T>` serialization **without** the full transport protocol (no envelope, no request registry). Ideal for Minimal APIs, Blazor, MAUI, or any app that serializes `Result<T>` over HTTP, SignalR, or other transports directly:

```csharp
using FadiPhor.Result.Serialization.Json;

// Built-in error types only (ValidationFailure, NotFoundError, etc.)
services.AddResultSerialization();

// With custom error resolver auto-discovery from assemblies:
services.AddResultSerialization(
    assemblies: [typeof(MyErrorResolver).Assembly]);
```

This registers:

- **`FadiPhorJsonOptions`** — protocol-owned `JsonSerializerOptions` with Result converters and error polymorphism (does not collide with the consumer's own `JsonSerializerOptions`).
- **Error polymorphic resolvers** — auto-discovers `IErrorPolymorphicResolver` implementations from the scanned assemblies (if provided).

Resolve `FadiPhorJsonOptions` from DI whenever you need to serialize or deserialize `Result<T>`:

```csharp
var options = provider.GetRequiredService<FadiPhorJsonOptions>().SerializerOptions;
var json = JsonSerializer.Serialize(result, options);
```

### DI-based protocol registration

For applications using the full transport protocol (envelope serialization, request type scanning, and polymorphic error resolution):

```csharp
using FadiPhor.Result.Serialization.Json;

services.AddFadiPhorResultProtocol(
    assemblies: modules.SelectMany(m => m.ContractAssemblies),
    requestMarkerType: typeof(IRequest<>));
```

This single call registers:

- **`FadiPhorJsonOptions`** — protocol-owned `JsonSerializerOptions` with Result converters and error polymorphism (does not collide with the consumer's own `JsonSerializerOptions`).
- **`IJsonEnvelopeSerializer`** — symmetric `Serialize` / `Deserialize` for `JsonEnvelope` payloads.
- **Request type registry** — scans assemblies for types implementing the consumer-provided marker interface.
- **Error polymorphic resolvers** — auto-discovers `IErrorPolymorphicResolver` implementations from the scanned assemblies.

The `requestMarkerType` can be any interface — generic (e.g. `typeof(IRequest<>)`) or non-generic (e.g. `typeof(IRequest)`). The library does not depend on MediatR or any specific framework.

---

## JSON Contract

### Success

```json
{
  "kind": "Success",
  "value": { "id": 1, "name": "Alice" }
}
```

Property order is fixed: `kind` first, then `value`.

### Failure (plain error)

```json
{
  "kind": "Failure",
  "error": {
    "$type": "NotFoundError",
    "code": "not_found",
    "message": "User 42 was not found."
  }
}
```

Property order: `kind` first, then `error`. The `$type` discriminator identifies the `Error` subtype.

### Failure (ValidationFailure)

```json
{
  "kind": "Failure",
  "error": {
    "$type": "ValidationFailure",
    "code": "validation.failed",
    "message": "Validation failed.",
    "issues": [
      {
        "identifier": "Email",
        "message": "Email is required",
        "severity": 0
      },
      {
        "identifier": "Age",
        "message": "Must be 18 or older",
        "severity": 0
      }
    ]
  }
}
```

`ValidationFailure` is registered automatically. No resolver needed.

### Success with Unit

```json
{
  "kind": "Success",
  "value": {}
}
```

---

## Polymorphic Error Resolution

Errors are serialized through `System.Text.Json` polymorphism. The discriminator property is `$type`. Each `Error` subtype must be registered with a resolver.

All resolvers are **declarative** — they return the derived types they contribute via `GetDerivedTypes()`. `AddResultSerialization` aggregates every resolver's types into a single `JsonPolymorphismOptions` instance. This means:

- Resolver registration order does not matter.
- Resolvers cannot overwrite each other.
- Adding a new resolver is safe — just implement `GetDerivedTypes()`.

### Default behavior

`AddResultSerialization` automatically registers the following core error types. You do not need to include them in your resolver:

- `ValidationFailure`
- `NotFoundError`
- `UnauthenticatedError`
- `UnauthorizedError`
- `ConflictError`
- `UnexpectedError`

### Custom errors

Define your error types and implement `IErrorPolymorphicResolver`:

```csharp
using FadiPhor.Result.Serialization.Json.Errors;

public record InsufficientFundsError(decimal Required, decimal Available) : Error("insufficient_funds")
{
    public override string? Message => $"Required {Required:C} but only {Available:C} available";
}

public record RateLimitedError(int RetryAfterSeconds) : Error("rate_limited")
{
    public override string? Message => $"Rate limited. Retry after {RetryAfterSeconds}s";
}

public class MyErrorResolver : IErrorPolymorphicResolver
{
    public IEnumerable<JsonDerivedType> GetDerivedTypes()
    {
        yield return new JsonDerivedType(typeof(InsufficientFundsError), nameof(InsufficientFundsError));
        yield return new JsonDerivedType(typeof(RateLimitedError), nameof(RateLimitedError));
    }
}
```

Register the resolver:

```csharp
var options = new JsonSerializerOptions()
    .AddResultSerialization(new MyErrorResolver());
```

All derived types from all resolvers — including the built-in core types (`ValidationFailure`, `NotFoundError`, `UnauthenticatedError`, `UnauthorizedError`, `ConflictError`, `UnexpectedError`) — are merged into a single configuration automatically.

---

## Round-Trip Example

```csharp
var options = new JsonSerializerOptions
{
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
}.AddResultSerialization(new MyErrorResolver());

// Serialize
Result<int> result = ResultFactory.Success(42);
var json = JsonSerializer.Serialize(result, options);
// {"kind":"Success","value":42}

// Deserialize
var deserialized = JsonSerializer.Deserialize<Result<int>>(json, options);
// deserialized is Success<int> { Value = 42 }

// Failure round-trip
Result<int> failure = new NotFoundError("item/7 was not found");
var failureJson = JsonSerializer.Serialize(failure, options);
// {"kind":"Failure","error":{"$type":"NotFoundError","code":"not_found","message":"item/7 was not found"}}

var restored = JsonSerializer.Deserialize<Result<int>>(failureJson, options);
// restored is Failure<int> { Error = NotFoundError { Message = "item/7 was not found" } }
```

---

## Envelope Transport

The transport layer provides symmetric serialization of request objects into `JsonEnvelope` payloads for JSON RPC-style protocols.

### JsonEnvelope

```json
{
  "type": "MyApp.Contracts.CreateUserRequest",
  "body": { "name": "Alice" }
}
```

The `type` property uses the full CLR type name (`Type.FullName`) to ensure uniqueness across namespaces.

### IJsonEnvelopeSerializer

Resolve from DI after calling `AddFadiPhorResultProtocol`:

```csharp
using FadiPhor.Result.Serialization.Json.Transport;

// Client — wrap a request into an envelope
JsonEnvelope envelope = serializer.Serialize(new CreateUserRequest("Alice"));

// Server — unwrap an envelope into the concrete request type
object request = serializer.Deserialize(envelope);
```

### FadiPhorJsonOptions

Access the protocol-owned `JsonSerializerOptions` for manual JSON handling:

```csharp
using FadiPhor.Result.Serialization.Json.Configuration;

var options = provider.GetRequiredService<FadiPhorJsonOptions>().SerializerOptions;
var json = JsonSerializer.Serialize(myObject, options);
```

The library registers its own `JsonSerializerOptions` instance wrapped in `FadiPhorJsonOptions`, so it never collides with options the consumer may register independently.

---

## Structural Notes

- The JSON shape reflects the current binary union (`Success` / `Failure`). If new union states are added to the core, the converter must be updated to handle them.
- Changes to property names (`kind`, `value`, `error`, `$type`) or structure are contract-breaking.
- Deserialization is strict: missing `kind`, unknown `kind` values, or missing `value`/`error` properties throw `JsonException`.
