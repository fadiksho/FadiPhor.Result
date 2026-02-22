using System.Text.Json.Serialization.Metadata;

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
  /// Returns the derived types that this resolver contributes for polymorphic Error serialization.
  /// </summary>
  /// <returns>A collection of <see cref="JsonDerivedType"/> instances representing derived error types.</returns>
  IEnumerable<JsonDerivedType> GetDerivedTypes();
}
