# FadiPhor.Result
[![Build](https://github.com/fadiksho/FadiPhor.Result/actions/workflows/ci.yml/badge.svg)](https://github.com/fadiksho/FadiPhor.Result/actions/workflows/ci.yml)
[![Publish](https://github.com/fadiksho/FadiPhor.Result/actions/workflows/publish.yml/badge.svg)](https://github.com/fadiksho/FadiPhor.Result/actions/workflows/publish.yml)
[![NuGet](https://img.shields.io/nuget/v/FadiPhor.Result.svg)](https://www.nuget.org/packages/FadiPhor.Result)


A cohesive Result abstraction ecosystem for explicit, type-safe error handling in modern .NET applications.

This repository contains the core `Result<T>` implementation and its official System.Text.Json serialization support.
All packages in this repository share the same version wave and evolve together.

---

## Packages

### 📦 FadiPhor.Result

Foundational `Result<T>` abstraction for modeling success and failure without exception-driven control flow.

- Union-based design (`Success<T>` / `Failure<T>`)
- Explicit error modeling via `Error`
- Railway-oriented composition with `Bind`
- Built-in validation support (`ValidationFailure`)
- Transport-agnostic core

➡ See full documentation:
[Core package documentation](src/FadiPhor.Result/README.md)

Install:

```bash
dotnet add package FadiPhor.Result
```

---

### 📦 FadiPhor.Result.Serialization.Json

System.Text.Json support for `Result<T>`.

- Discriminated union JSON format (`kind`)
- Polymorphic error serialization
- Core error types auto-registered
- Extensible via `IErrorPolymorphicResolver`

➡ See full documentation:
[Serialization package documentation](src/FadiPhor.Result.Serialization.Json/README.md)

Install:

```bash
dotnet add package FadiPhor.Result.Serialization.Json
```

---

## Design Principles

- Explicit success/failure modeling
- No exception-based business flow
- Immutable, structurally sound types
- Clear separation between core and infrastructure
- Unified versioning across packages

---

## Architecture

```
FadiPhor.Result/
├── FadiPhor.Result                 (Core)
└── FadiPhor.Result.Serialization.Json  (Infrastructure adapter)
```

Core has no serialization dependency.
Serialization depends on core — never the reverse.

---

## Versioning

All packages in this repository share the same semantic version.
Releases are driven by Git tags.

---

## License

MIT

---