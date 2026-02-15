# FadiPhor.Result.Serialization.Json

System.Text.Json serialization support for `FadiPhor.Result`.

## Overview

This library provides JSON serialization and deserialization for `Result<T>` types using `System.Text.Json`. It is separated from the core `FadiPhor.Result` library to maintain transport independence and allow the core library to remain free of serialization dependencies.

**Key Features:**

- **Seamless Result<T> serialization** - Custom converters handle Success and Failure cases
- **Polymorphic error support** - Serialize and deserialize custom error types
- **Core errors auto-registered** - ValidationFailure and other core error types work out of the box
- **Extensible** - Register custom error types with IErrorPolymorphicResolver
- **Clean JSON format** - Uses discriminated union pattern with "kind" property

---

## Installation

```bash
dotnet add package FadiPhor.Result
dotnet add package FadiPhor.Result.Serialization.Json
```

---

## Quick Start

### Basic Usage

Core error types like `ValidationFailure` are automatically registered and work without any configuration:

```csharp
using System.Text.Json;
using FadiPhor.Result;
using FadiPhor.Result.Serialization.Json;

// Configure JSON options
var options = new JsonSerializerOptions()
    .AddResultSerialization();

// Serialize a successful result
var successResult = Result.Success(42);
var json = JsonSerializer.Serialize(successResult, options);
// Output: {"kind":"Success","value":42}

// Serialize a validation failure (core error - no resolver needed)
var validationFailure = new ValidationFailure(new[]
{
    new ValidationIssue("Email", "Email is required"),
    new ValidationIssue("Password", "Password must be at least 8 characters")
});
var failureResult = Result.Failure<User>(validationFailure);
var failureJson = JsonSerializer.Serialize(failureResult, options);

// Deserialize
var deserialized = JsonSerializer.Deserialize<Result<int>>(json, options);
```

---

## Custom Error Types

### Registering Custom Errors

To serialize custom error types, implement `IErrorPolymorphicResolver`:

```csharp
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using FadiPhor.Result;
using FadiPhor.Result.Serialization.Json;

// Define custom error types
public record NotFoundError(string Code, string EntityId) : Error(Code)
{
    public override string? Message => $"Entity {EntityId} not found";
}

public record UnauthorizedError(string Code) : Error(Code)
{
    public override string? Message => "Access denied";
}

// Create a resolver for your custom error types
public class MyErrorResolver : IErrorPolymorphicResolver
{
    public void ResolveDerivedType(JsonTypeInfo typeInfo)
    {
        if (typeInfo.Type != typeof(Error))
            return;

        typeInfo.PolymorphismOptions = new JsonPolymorphismOptions
        {
            TypeDiscriminatorPropertyName = "$type",
            IgnoreUnrecognizedTypeDiscriminators = false,
            UnknownDerivedTypeHandling = JsonUnknownDerivedTypeHandling.FailSerialization,
            DerivedTypes =
            {
                new JsonDerivedType(typeof(NotFoundError), "NotFoundError"),
                new JsonDerivedType(typeof(UnauthorizedError), "UnauthorizedError")
            }
        };
    }
}

// Register the resolver
var options = new JsonSerializerOptions()
    .AddResultSerialization(new MyErrorResolver());
```

**Important:** You do NOT need to register core error types like `ValidationFailure` - they are automatically included.

---

## JSON Format

### Success Result

```json
{
  "kind": "Success",
  "value": 42
}
```

### Failure Result

```json
{
  "kind": "Failure",
  "error": {
    "$type": "ValidationFailure",
    "code": "validation.failed",
    "message": "Validation failed.",
    "issues": [
      {
        "propertyName": "Email",
        "errorMessage": "Email is required",
        "severity": 0
      }
    ]
  }
}
```

### Custom Error Result

```json
{
  "kind": "Failure",
  "error": {
    "$type": "NotFoundError",
    "code": "user.not_found",
    "message": "Entity user-123 not found",
    "entityId": "user-123"
  }
}
```

---

## Core Error Types

The following core error types are **automatically registered** and require no configuration:

- **ValidationFailure** - Contains a collection of validation issues

Future core error types will also be registered automatically.

---

## Configuration Options

### Multiple Resolvers

You can register multiple custom error resolvers:

```csharp
var options = new JsonSerializerOptions()
    .AddResultSerialization(
        new MyDomainErrorResolver(),
        new MyApplicationErrorResolver()
    );
```

### Preserving Existing Resolvers

`AddResultSerialization` preserves any existing `TypeInfoResolver` in your options and combines them:

```csharp
var options = new JsonSerializerOptions
{
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    TypeInfoResolver = new DefaultJsonTypeInfoResolver() // Your existing resolver
};

options.AddResultSerialization(new MyErrorResolver());
// Both resolvers are now active
```

---

## Architecture

This library follows clean architecture principles:

- **Core library** (`FadiPhor.Result`) - Domain types, no infrastructure dependencies
- **Serialization library** (this library) - Infrastructure adapter for System.Text.Json
- **Dependency direction** - Serialization depends on Core, never the reverse

This separation allows:
- Core library to remain transport-agnostic
- Future support for other serialization formats (e.g., Newtonsoft.Json, MessagePack)
- Easier testing and maintenance
- Smaller core library footprint

---

## Source Generator Compatibility

The resolver pattern is compatible with System.Text.Json source generators for AOT and trimming scenarios. Use resolvers within a custom `JsonSerializerContext` for optimal performance.

---

## License

This library is part of the FadiPhor.Result project. See the main repository for license information.
