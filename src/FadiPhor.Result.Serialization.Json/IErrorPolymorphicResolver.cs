using System.Text.Json.Serialization.Metadata;
using FadiPhor.Result;

namespace FadiPhor.Result.Serialization.Json;

/// <summary>
/// Defines a contract for resolving polymorphic derived types of <see cref="Error"/> 
/// during JSON serialization and deserialization.
/// </summary>
/// <remarks>
/// Implement this interface to register custom error types for polymorphic serialization.
/// Core library error types (e.g., ValidationFailure) are automatically registered.
/// </remarks>
public interface IErrorPolymorphicResolver
{
  /// <summary>
  /// Configures polymorphic serialization for the Error type hierarchy.
  /// </summary>
  /// <param name="typeInfo">The type information to configure.</param>
  void ResolveDerivedType(JsonTypeInfo typeInfo);
}
