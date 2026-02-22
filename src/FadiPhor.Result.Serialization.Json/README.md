# FadiPhor.Result.Serialization.Json

`System.Text.Json` converters for `Result<T>` with polymorphic `Error` support.

---

## Registration

```csharp
using FadiPhor.Result.Serialization.Json;

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
    "message": "user/42 not found",
    "entityId": "user/42"
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

`AddResultSerialization` automatically registers `ValidationFailure`. You do not need to include it in your resolver.

### Custom errors

Define your error types and implement `IErrorPolymorphicResolver`:

```csharp
public record NotFoundError(string EntityId) : Error("not_found")
{
    public override string? Message => $"{EntityId} not found";
}

public record ConflictError(string Reason) : Error("conflict")
{
    public override string? Message => Reason;
}

public class MyErrorResolver : IErrorPolymorphicResolver
{
    public IEnumerable<JsonDerivedType> GetDerivedTypes()
    {
        yield return new JsonDerivedType(typeof(NotFoundError), nameof(NotFoundError));
        yield return new JsonDerivedType(typeof(ConflictError), nameof(ConflictError));
    }
}
```

Register the resolver:

```csharp
var options = new JsonSerializerOptions()
    .AddResultSerialization(new MyErrorResolver());
```

All derived types from all resolvers — including the built-in `ValidationFailure` — are merged into a single configuration automatically.

---

## Round-Trip Example

```csharp
var options = new JsonSerializerOptions
{
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
}.AddResultSerialization(new MyErrorResolver());

// Serialize
Result<int> result = Result.Success(42);
var json = JsonSerializer.Serialize(result, options);
// {"kind":"Success","value":42}

// Deserialize
var deserialized = JsonSerializer.Deserialize<Result<int>>(json, options);
// deserialized is Success<int> { Value = 42 }

// Failure round-trip
Result<int> failure = new NotFoundError("item/7");
var failureJson = JsonSerializer.Serialize(failure, options);
// {"kind":"Failure","error":{"$type":"NotFoundError","code":"not_found","message":"item/7 not found","entityId":"item/7"}}

var restored = JsonSerializer.Deserialize<Result<int>>(failureJson, options);
// restored is Failure<int> { Error = NotFoundError { EntityId = "item/7" } }
```

---

## Structural Notes

- The JSON shape reflects the current binary union (`Success` / `Failure`). If new union states are added to the core, the converter must be updated to handle them.
- Changes to property names (`kind`, `value`, `error`, `$type`) or structure are contract-breaking.
- Deserialization is strict: missing `kind`, unknown `kind` values, or missing `value`/`error` properties throw `JsonException`.
