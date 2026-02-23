namespace FadiPhor.Result.Serialization.Json.Transport;

/// <summary>
/// Provides symmetric serialization and deserialization of
/// <see cref="JsonEnvelope"/> payloads using the protocol-owned JSON configuration.
/// </summary>
public interface IJsonEnvelopeSerializer
{
  /// <summary>
  /// Wraps a request object into a <see cref="JsonEnvelope"/> by resolving its
  /// registered type name and serializing its body.
  /// </summary>
  /// <param name="request">The request object to wrap.</param>
  /// <returns>An envelope containing the type discriminator and serialized body.</returns>
  /// <exception cref="ArgumentNullException">Thrown when <paramref name="request"/> is null.</exception>
  /// <exception cref="InvalidOperationException">
  /// Thrown when the request type is not registered or serialization produces an invalid result.
  /// </exception>
  JsonEnvelope Serialize(object request);

  /// <summary>
  /// Deserializes the body of the given envelope into the .NET type identified
  /// by <see cref="JsonEnvelope.Type"/>.
  /// </summary>
  /// <param name="envelope">The envelope to deserialize.</param>
  /// <returns>The deserialized request object.</returns>
  /// <exception cref="ArgumentNullException">Thrown when <paramref name="envelope"/> is null.</exception>
  /// <exception cref="ArgumentException">
  /// Thrown when the envelope type or body is invalid.
  /// </exception>
  /// <exception cref="InvalidOperationException">
  /// Thrown when the type is unknown or deserialization returns null.
  /// </exception>
  object Deserialize(JsonEnvelope envelope);
}
