using System.Text.Json;

namespace FadiPhor.Result.Serialization.Json.Configuration;

/// <summary>
/// Exposes the <see cref="JsonSerializerOptions"/> built and owned by the
/// FadiPhor.Result protocol layer.
/// </summary>
/// <remarks>
/// This wrapper prevents collisions with any <see cref="JsonSerializerOptions"/>
/// the consumer may register independently. Resolve this type from DI when you
/// need the protocol-configured options for manual JSON handling.
/// </remarks>
public sealed class FadiPhorJsonOptions
{
  internal FadiPhorJsonOptions(JsonSerializerOptions serializerOptions)
  {
    SerializerOptions = serializerOptions;
  }

  /// <summary>
  /// Gets the <see cref="JsonSerializerOptions"/> configured with Result converters
  /// and polymorphic error resolution.
  /// </summary>
  public JsonSerializerOptions SerializerOptions { get; }
}
