using System.Text.Json;

namespace FadiPhor.Result.Serialization.Json.Transport;

/// <summary>
/// Represents a JSON RPC envelope containing a type discriminator and a serialized request body.
/// </summary>
/// <remarks>
/// The envelope is the wire-level transport contract. The <see cref="Type"/> property identifies
/// the request type using its full CLR name, and <see cref="Body"/> contains the raw JSON payload
/// to be deserialized into the corresponding .NET type.
/// </remarks>
public sealed class JsonEnvelope
{
  /// <summary>
  /// Gets the full CLR type name identifying the request type.
  /// </summary>
  public required string Type { get; init; }

  /// <summary>
  /// Gets the raw JSON body of the request.
  /// </summary>
  public required JsonElement Body { get; init; }
}
