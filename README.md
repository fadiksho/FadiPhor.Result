# FadiPhor.Result
[![Build](https://github.com/fadiksho/FadiPhor.Result/actions/workflows/ci.yml/badge.svg)](https://github.com/fadiksho/FadiPhor.Result/actions/workflows/ci.yml)
[![Publish](https://github.com/fadiksho/FadiPhor.Result/actions/workflows/publish.yml/badge.svg)](https://github.com/fadiksho/FadiPhor.Result/actions/workflows/publish.yml)
[![NuGet](https://img.shields.io/nuget/v/FadiPhor.Result.svg)](https://www.nuget.org/packages/FadiPhor.Result)

This repository contains two packages:

| Package | Purpose |
|---|---|
| [`FadiPhor.Result`](src/FadiPhor.Result/README.md) | Core `Result<T>` type — union of `Success<T>` and `Failure<T>`, composition operators, validation types. No infrastructure dependencies. |
| [`FadiPhor.Result.Serialization.Json`](src/FadiPhor.Result.Serialization.Json/README.md) | `System.Text.Json` converters for `Result<T>` with polymorphic `Error` serialization, plus JSON envelope transport protocol with DI registration. Depends on the core package. |

Both packages share the same semantic version, driven by Git tags.

---

## End-to-End Example

Define a domain error and a method that returns `Result<T>`:

```csharp
public record NotFoundError(string EntityId) : Error("not_found")
{
    public override string? Message => $"{EntityId} not found";
}

public Result<User> GetUser(int id)
{
    var user = repository.Find(id);
    if (user is null)
        return new NotFoundError($"user/{id}");

    return user; // implicit conversion to Success<User>
}
```

Compose and match at the boundary:

```csharp
var result = GetUser(request.Id)
    .Ensure(u => u.IsActive, () => new Error("user.inactive"))
    .Bind(u => MapToDto(u));

return result.Match(
    onSuccess: dto => Ok(dto),
    onFailure: error => error switch
    {
        NotFoundError => NotFound(),
        _ => Problem(detail: error.Code)
    }
);
```

---

## Dependency Direction

```
FadiPhor.Result                       (core — no serialization dependency)
    ▲
    │
FadiPhor.Result.Serialization.Json    (infrastructure adapter)
```

The core package is transport-agnostic. Serialization depends on core, never the reverse.

---

## Union Structure

`Result<T>` is currently a binary union: `Success<T>` or `Failure<T>`. Both are sealed.

The design allows introducing additional union states (e.g. `PartialSuccess<T>`) if needed. Adding a new state requires changes to:

- The core model and all `switch` expressions over `Result<T>`.
- Any infrastructure that depends on the union shape, including the JSON converter.

---

## License

MIT