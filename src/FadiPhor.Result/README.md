# FadiPhor.Result

A foundational `Result<T>` library for robust error handling in .NET applications.

## Overview

`FadiPhor.Result` provides a union-based `Result<T>` type that enforces safe error handling through the type system. It eliminates the need for exception-based error flows in business logic while maintaining type safety and composability.

**Key Features:**

- **Type-safe error handling** - Union-based design with clear Success/Failure states
- **Structurally sound** - No invalid states possible, enforced by the type system
- **Extensible** - Built for future union cases without breaking changes
- **MediatR-friendly** - Designed for CQRS and pipeline architectures
- **Transport-agnostic** - Core library has no serialization dependencies
- **Well-documented** - Comprehensive XML documentation for IntelliSense support

## Installation

```bash
dotnet add package FadiPhor.Result
```

---

## Design Philosophy

### Core Principles

1. **Union-based `Result<T>`** - No non-generic `Result` type. Forces explicit success type declaration.
2. **Non-nullable values** - `T` must be non-nullable (enforced by `notnull` constraint)
3. **Sealed union cases** - Success and Failure are distinct sealed types
4. **Abstract error base** - Errors inherit from `Error` for polymorphic handling
5. **Transport independence** - Core library has no serialization dependencies

### Why Result<T>?

Traditional exception-based error handling has several drawbacks:
- Errors are invisible in method signatures
- Forces defensive null checks and try-catch blocks
- Makes error propagation implicit and hard to trace
- Mixing exceptions with business logic errors creates ambiguity

`Result<T>` makes errors explicit, visible, and composable through the type system.

### Design Decisions

**No non-generic Result:** Every result must have an explicit success type. Use `Result<Unit>` for operations without meaningful return values (commands, side effects).

**Implicit operators:** Enable expressive handler code where return type provides type safety:
```csharp
public Result<User> GetUser(string id) 
{
    var user = repository.Find(id);
    if (user is null)
        return new NotFoundError("user.not_found", "User not found");
    return user; // Implicit conversion to Success<User>
}
```

**Sealed types:** Prevents inheritance and maintains union integrity.

---

## Core Concepts

### Result<T>

An abstract union type representing either:
- **Success<T>** - Contains a non-null value of type T
- **Failure<T>** - Contains a Error describing what went wrong

### Success<T>

A sealed record representing successful execution. Guarantees value is non-null.

### Failure<T>

A sealed record representing failed execution. Contains a Error with details.

### Error

Abstract base record for all error types. Provides:
- **Code** - Machine-readable error identifier (e.g., "user.not_found")
- **Message** - Human-readable error description

Custom errors extend Error:
```csharp
public record ValidationError(string Code, string Message, Dictionary<string, string[]> Errors) 
    : Error(Code, Message);

public record NotFoundError(string Code, string Message, string EntityId) 
    : Error(Code, Message);
```

### Unit

A type representing "no meaningful value". Use `Result<Unit>` for operations that succeed/fail without returning data.

---

## Basic Usage Examples

### Creating Results

```csharp
// Success case
var result = Result.Success(42);

// Failure case
var error = new ValidationError("validation.failed", "Email is required", errors);
var result = Result.Failure<int>(error);

// Using implicit conversions
public Result<User> GetUser(string id)
{
    var user = repository.Find(id);
    if (user is null)
        return new NotFoundError("user.not_found", "User not found", id);
    return user; // Implicitly converts to Success<User>
}
```

### Pattern Matching

```csharp
var message = result.Match(
    onSuccess: value => $"Value: {value}",
    onFailure: error => $"Error: {error.Message}"
);

// In ASP.NET Core controllers
return result.Match(
    onSuccess: user => Ok(user),
    onFailure: error => Problem(
        title: error.Message,
        detail: error.Code,
        statusCode: MapErrorToStatusCode(error)
    )
);
```

---

## Commands with Unit

For operations without meaningful return values (commands, side effects), use `Result<Unit>`:

```csharp
public async Task<Result<Unit>> Handle(DeleteUserCommand request, CancellationToken ct)
{
    var user = await repository.FindAsync(request.UserId, ct);
    if (user is null)
        return new NotFoundError("user.not_found", "User not found", request.UserId);
    
    await repository.DeleteAsync(user, ct);
    return Result.Success(Unit.Value);
}

public async Task<Result<Unit>> Handle(UpdateUserCommand request, CancellationToken ct)
{
    // Validation
    if (string.IsNullOrEmpty(request.Email))
        return new ValidationError("email.required", "Email is required", errors);
    
    // Update logic
    await repository.UpdateAsync(user, ct);
    return Unit.Value; // Implicit conversion to Success<Unit>
}
```

**Key Point:** `Unit.Value` is the sole instance of the Unit type, signaling successful completion without a value.

---

## Implicit Conversions

`Result<T>` provides two implicit operators for expressive code:

### From Value to Success

```csharp
public Result<User> CreateUser(string name)
{
    var user = new User(name);
    return user; // Implicitly converts to Success<User>
}
```

**Behavior:**
- Automatically wraps value in `Success<T>`
- Throws `ArgumentNullException` if value is null
- Conversion depends on method's return type for type safety

### From Error to Failure

```csharp
public Result<User> GetUser(string id)
{
    if (string.IsNullOrEmpty(id))
        return new ValidationError("id.required", "ID is required"); // Implicitly converts to Failure<User>
    
    // ... lookup logic
}
```

**Behavior:**
- Automatically wraps error in `Failure<T>`
- Throws `ArgumentNullException` if error is null
- Return type determines the T in Failure<T>

**Important:** These operators are intentional for expressive handler design. They rely on the method's return type to provide type safety.

---

## Pattern Matching & Bind

### Exhaustive Matching

Match ensures both Success and Failure cases are handled:

```csharp
var output = result.Match(
    onSuccess: value => ProcessValue(value),
    onFailure: error => LogAndDefault(error)
);
```

### Railway-Oriented Programming with Bind

Chain operations that may fail. Errors short-circuit the chain automatically:

```csharp
// Synchronous
var result = GetUserId(request)
    .Bind(userId => GetUser(userId))
    .Bind(user => ValidateUser(user))
    .Bind(user => MapToDto(user));

// If any step fails, the chain stops and returns that failure
// If all succeed, returns the final success value

// Asynchronous
var result = await GetUserIdAsync(request)
    .Bind(userId => GetUserAsync(userId))
    .Bind(user => ValidateUserAsync(user))
    .Bind(user => MapToDtoAsync(user));
```

**Bind Behavior:**
- On Success: Executes the next function with the success value
- On Failure: Propagates the error without executing remaining functions
- Async variant supports Task<Result<T>> chaining

---

## Validation Support

The library provides minimal, infrastructure-agnostic validation support through built-in types that integrate naturally with the existing error model.

### Built-in Validation Types

**ValidationFailure** - An error type for validation failures:
```csharp
public sealed record ValidationFailure(
    IReadOnlyCollection<ValidationIssue> Issues)
    : Error("validation.failed");
```

**ValidationIssue** - Represents a single validation issue:
```csharp
public sealed record ValidationIssue(
    string Identifier,
    string Message,
    ValidationSeverity Severity = ValidationSeverity.Error);
```

**ValidationSeverity** - Severity levels for validation issues:
```csharp
public enum ValidationSeverity
{
    Error = 0,    // Critical validation error
    Warning = 1,  // Non-critical issue
    Info = 2      // Informational guidance
}
```

### Basic Validation Usage

```csharp
public Result<User> CreateUser(CreateUserRequest request)
{
    var issues = new List<ValidationIssue>();

    if (string.IsNullOrEmpty(request.Email))
        issues.Add(new ValidationIssue("Email", "Email is required"));

    if (request.Age < 0)
        issues.Add(new ValidationIssue("Age", "Age must be positive"));

    if (request.Age < 18)
        issues.Add(new ValidationIssue("Age", "User is below recommended age", ValidationSeverity.Warning));

    if (issues.Any(i => i.Severity == ValidationSeverity.Error))
        return new ValidationFailure(issues);

    var user = new User(request.Email, request.Age);
    return user;
}
```

### Integration with FluentValidation

The validation types are framework-agnostic and can map from any validation library:

```csharp
using FluentValidation;

public class CreateUserValidator : AbstractValidator<CreateUserRequest>
{
    public CreateUserValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Age).GreaterThanOrEqualTo(0);
    }
}

public async Task<Result<User>> Handle(CreateUserCommand request, CancellationToken ct)
{
    var validator = new CreateUserValidator();
    var validationResult = await validator.ValidateAsync(request, ct);
    
    if (!validationResult.IsValid)
    {
        var issues = validationResult.Errors
            .Select(e => new ValidationIssue(
                e.PropertyName,
                e.ErrorMessage,
                MapSeverity(e.Severity)))
            .ToList();
        
        return new ValidationFailure(issues);
    }
    
    var user = new User(request.Email, request.Age);
    return user;
}

private static ValidationSeverity MapSeverity(Severity fluentSeverity)
{
    return fluentSeverity switch
    {
        Severity.Error => ValidationSeverity.Error,
        Severity.Warning => ValidationSeverity.Warning,
        Severity.Info => ValidationSeverity.Info,
        _ => ValidationSeverity.Error
    };
}
```

### Handling Validation Failures in APIs

```csharp
[HttpPost]
public async Task<IActionResult> CreateUser(CreateUserCommand command)
{
    var result = await _mediator.Send(command);
    
    return result.Match(
        onSuccess: user => CreatedAtAction(nameof(GetUser), new { id = user.Id }, user),
        onFailure: error => error switch
        {
            ValidationFailure vf => BadRequest(new
            {
                Code = vf.Code,
                Message = vf.Message,
                Issues = vf.Issues.Select(i => new
                {
                    i.Identifier,
                    i.Message,
                    Severity = i.Severity.ToString()
                })
            }),
            _ => Problem(detail: error.Code, title: error.Message)
        }
    );
}
```

### Serialization

`ValidationFailure` works automatically without any configuration:

```csharp
var options = new JsonSerializerOptions()
    .AddResultSerialization();

// ValidationFailure is automatically registered and works out of the box
var json = JsonSerializer.Serialize(result, options);
```

For custom error types, implement `IErrorPolymorphicResolver`:

```csharp
public class MyErrorResolver : IErrorPolymorphicResolver
{
    public void ResolveDerivedType(JsonTypeInfo typeInfo)
    {
        if (typeInfo.Type != typeof(Error))
            return;

        typeInfo.PolymorphismOptions = new JsonPolymorphismOptions
        {
            TypeDiscriminatorPropertyName = "$type",
            DerivedTypes =
            {
                // Only register custom error types
                // Core types like ValidationFailure are already registered
                new JsonDerivedType(typeof(NotFoundError), "NotFoundError"),
                new JsonDerivedType(typeof(UnauthorizedError), "UnauthorizedError")
            }
        };
    }
}

// Usage
var options = new JsonSerializerOptions()
    .AddResultSerialization(new MyErrorResolver());
```

**JSON Output:**
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
        "message": "Age must be positive",
        "severity": 0
      }
    ]
  }
}
```

**Key Points:**
- No external validation framework dependencies
- Works with FluentValidation, DataAnnotations, or any validation library
- Integrates seamlessly with existing Result<T> infrastructure
- Supports multiple validation issues with severity levels
- Fully serializable with polymorphic error handling

---

## MediatR Integration

`Result<T>` integrates seamlessly with MediatR for CQRS patterns:

### Query Handler

```csharp
public class GetUserQueryHandler : IRequestHandler<GetUserQuery, Result<UserDto>>
{
    private readonly IUserRepository _repository;
    private readonly IMapper _mapper;

    public async Task<Result<UserDto>> Handle(GetUserQuery request, CancellationToken ct)
    {
        var user = await _repository.FindAsync(request.UserId, ct);
        
        if (user is null)
            return new NotFoundError("user.not_found", "User not found", request.UserId);
        
        var dto = _mapper.Map<UserDto>(user);
        return dto;
    }
}
```

### Command Handler

```csharp
public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, Result<Guid>>
{
    private readonly IUserRepository _repository;

    public async Task<Result<Guid>> Handle(CreateUserCommand request, CancellationToken ct)
    {
        // Validation
        if (string.IsNullOrEmpty(request.Email))
            return new ValidationError("email.required", "Email is required", validationErrors);
        
        // Business logic
        var user = new User(request.Email, request.Name);
        await _repository.AddAsync(user, ct);
        
        return user.Id;
    }
}
```

### Pipeline Behavior for Logging

```csharp
public class LoggingBehavior<TRequest, TResponse> 
    : IPipelineBehavior<TRequest, Result<TResponse>>
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;

    public async Task<Result<TResponse>> Handle(
        TRequest request,
        RequestHandlerDelegate<Result<TResponse>> next,
        CancellationToken ct)
    {
        _logger.LogInformation("Handling {RequestName}", typeof(TRequest).Name);
        
        var result = await next();
        
        if (result is Failure<TResponse> failure)
            _logger.LogWarning("Request failed: {Code} - {Message}", 
                failure.Error.Code, failure.Error.Message);
        
        return result;
    }
}
```

### API Boundary

```csharp
[HttpGet("{id}")]
public async Task<IActionResult> GetUser(string id)
{
    var result = await _mediator.Send(new GetUserQuery(id));
    
    return result.Match(
        onSuccess: user => Ok(user),
        onFailure: error => error switch
        {
            NotFoundError => NotFound(new { error.Code, error.Message }),
            ValidationError ve => BadRequest(new { ve.Code, ve.Message, ve.Errors }),
            _ => Problem(detail: error.Code, title: error.Message)
        }
    );
}
```

---

## JSON Serialization

System.Text.Json support is provided by a separate project:

**`FadiPhor.Result.Serialization.Json`**

This separation ensures:
- Core library remains transport-agnostic
- No infrastructure dependencies in domain types
- Future extensibility for other serializers (e.g., Newtonsoft.Json, MessagePack)
- Clean architectural boundaries

### Installation

```bash
dotnet add package FadiPhor.Result
dotnet add package FadiPhor.Result.Serialization.Json
```

### Quick Start

```csharp
using FadiPhor.Result;
using FadiPhor.Result.Serialization.Json;
using System.Text.Json;

var options = new JsonSerializerOptions()
    .AddResultSerialization();

// Core errors like ValidationFailure work automatically
var json = JsonSerializer.Serialize(result, options);
var deserialized = JsonSerializer.Deserialize<Result<User>>(json, options);
```

### Custom Error Types

To serialize custom error types, implement `IErrorPolymorphicResolver`:

```csharp
using FadiPhor.Result.Serialization.Json;

public class MyErrorResolver : IErrorPolymorphicResolver
{
    public void ResolveDerivedType(JsonTypeInfo typeInfo)
    {
        if (typeInfo.Type != typeof(Error))
            return;

        typeInfo.PolymorphismOptions = new JsonPolymorphismOptions
        {
            TypeDiscriminatorPropertyName = "$type",
            DerivedTypes =
            {
                new JsonDerivedType(typeof(NotFoundError), "NotFoundError"),
                new JsonDerivedType(typeof(UnauthorizedError), "UnauthorizedError")
            }
        };
    }
}

var options = new JsonSerializerOptions()
    .AddResultSerialization(new MyErrorResolver());
```

**Note:** Core library error types (e.g., `ValidationFailure`) are automatically registered.

For complete documentation, see the [FadiPhor.Result.Serialization.Json README](src/FadiPhor.Result.Serialization.Json/README.md).

---
}
```

### Multiple Resolvers

You can register multiple resolvers for modular error organization:

```csharp
var options = new JsonSerializerOptions()
    .AddResultSerialization(
        new AuthErrorResolver(),
        new DomainErrorResolver()
    );
```

---

## Source Generator Compatibility

`FadiPhor.Result` is compatible with `System.Text.Json` source generators for AOT and trimming support.

### Using with JsonSerializerContext

```csharp
[JsonSerializable(typeof(Result<User>))]
[JsonSerializable(typeof(Result<Unit>))]
public partial class AppJsonContext : JsonSerializerContext
{
}

// Configure options
var options = new JsonSerializerOptions
{
    TypeInfoResolver = AppJsonContext.Default
}.AddResultSerialization(new MyErrorResolver());
```

**Important:** When using source generators:
1. Register all `Result<T>` types you'll serialize in the context
2. Apply `AddResultSerialization` AFTER setting TypeInfoResolver
3. The extension preserves existing resolvers and combines them

### AOT and Trimming

The library is trim-friendly and supports Native AOT compilation when:
- All Result<T> types are registered in JsonSerializerContext
- Error resolvers don't use reflection
- Custom errors are source-generator compatible

---

## Behavioral Guarantees

`FadiPhor.Result` provides strong structural guarantees:

### Type Safety

- `Result<T>` instances are always Success or Failure, never null
- Success values are never null (enforced by `notnull` constraint and validation)
- Failure errors are never null (enforced by constructor validation)
- Pattern matching exhaustively covers all cases

### Immutability

- All types are records (immutable by design)
- Success and Failure are sealed to prevent inheritance
- Error state cannot change after creation

### Error Propagation

- `Bind` automatically propagates errors through operation chains
- Errors maintain their type and data through propagation
- No silent failures or data loss

### Serialization

- Discriminator property `"kind"` is always present
- Deserialization validates structure strictly
- Invalid JSON throws `JsonException` (no silent failures)
- Polymorphic errors require explicit registration

### Implicit Conversions

- Value-to-Success conversion validates non-null
- Error-to-Failure conversion validates non-null
- Conversions depend on method return type (compile-time safe)

---

## Non-Goals

`FadiPhor.Result` intentionally does NOT attempt to solve:

### 1. Multiple Error Accumulation

For validation scenarios requiring multiple errors, use the built-in `ValidationFailure` type:

```csharp
var issues = new List<ValidationIssue>
{
    new("Email", "Email is required"),
    new("Password", "Password must be at least 8 characters"),
    new("Age", "Age is below recommended", ValidationSeverity.Warning)
};
return new ValidationFailure(issues);
```

This provides a minimal, structural solution without adding complexity. For advanced validation workflows with conditional logic and complex rules, consider using a dedicated validation library like FluentValidation and mapping the results to `ValidationFailure`.

### 2. Success with Warnings

No "success with warnings" state. Result is strictly Success or Failure. If you need warnings:
- Include warnings in your success value type
- Use a separate channel (logging, metadata)

### 3. Async/Await Syntax Integration

No custom `async`/`await` operators. Use standard `Task<Result<T>>` patterns. The library provides async Bind for composition.

### 4. Monad Laws Enforcement

While Result supports monadic operations (Bind), it doesn't enforce or test monad laws rigorously. It's a pragmatic tool, not a category theory implementation.

### 5. Exception Replacement Everywhere

Don't use Result for:
- Framework-level errors (null references, index out of range)
- Programming errors (use exceptions)
- Performance-critical tight loops (record allocation overhead)

Use Result for business logic errors that should be handled explicitly.

### 6. LINQ Query Syntax

No custom LINQ support (`from`/`select`/`where`). Use Bind for composition.

### 7. Partial Functions

No built-in support for partial functions or optional results. Use `Result<T?>` if you need optionality, or use a dedicated Option<T> type.

---

## Architecture

```
FadiPhor.Result/
├── Core/
│   ├── Result.cs           - Abstract base type with implicit operators
│   ├── Success.cs          - Success case (sealed)
│   ├── Failure.cs          - Failure case (sealed)
│   ├── Unit.cs             - Unit type for commands
│   └── ResultFactory.cs    - Static factory methods
├── Errors/
│   ├── Error.cs                - Abstract base error record
│   ├── ValidationFailure.cs    - Core validation error type
│   ├── ValidationIssue.cs      - Validation issue record
│   └── ValidationSeverity.cs   - Validation severity enum
├── Extensions/
│   ├── ResultMatchExtensions.cs       - Pattern matching
│   └── ResultCompositionExtensions.cs - Bind operations
└── Serialization/
    ├── IErrorPolymorphicResolver.cs
    ├── DefaultErrorPolymorphicResolver.cs (internal)
    ├── ResultJsonConverterFactory.cs
    ├── ResultJsonConverter.cs
    └── JsonSerializerOptionsExtensions.cs
```

---

## Testing

Run tests:
```bash
dotnet test
```

The test suite validates:
- Core functionality (Success, Failure, Unit)
- Pattern matching (Match extension)
- Synchronous and asynchronous Bind
- Error propagation through chains
- Implicit conversions (value, error, null handling)
- JSON serialization and deserialization
- Polymorphic error handling
- Strict deserialization behavior

All 43 tests ensure the library behaves correctly and maintains its guarantees.

---

## Contributing

Contributions are welcome! Please ensure:
- All public APIs are documented with XML comments
- Tests are added for new functionality
- Existing tests pass
- Code follows existing patterns and conventions

---

## License

MIT