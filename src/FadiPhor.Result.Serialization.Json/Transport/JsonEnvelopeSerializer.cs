using System.Text.Json;
using FadiPhor.Result.Serialization.Json.Configuration;

namespace FadiPhor.Result.Serialization.Json.Transport;

/// <summary>
/// Implementation of <see cref="IJsonEnvelopeSerializer"/> that resolves request types
/// through the <see cref="IJsonRequestTypeRegistry"/> and serializes/deserializes using the
/// protocol-owned <see cref="FadiPhorJsonOptions"/>.
/// </summary>
internal sealed class JsonEnvelopeSerializer : IJsonEnvelopeSerializer
{
  private readonly IJsonRequestTypeRegistry _registry;
  private readonly JsonSerializerOptions _options;

  public JsonEnvelopeSerializer(
    IJsonRequestTypeRegistry registry,
    FadiPhorJsonOptions jsonOptions)
  {
    _registry = registry;
    _options = jsonOptions.SerializerOptions;
  }

  public JsonEnvelope Serialize(object request)
  {
    ArgumentNullException.ThrowIfNull(request);

    var typeName = _registry.GetRequestTypeName(request.GetType());
    var body = JsonSerializer.SerializeToElement(request, request.GetType(), _options);

    if (body.ValueKind == JsonValueKind.Undefined)
      throw new InvalidOperationException(
        $"Serialization of '{typeName}' produced an invalid result.");

    return new JsonEnvelope
    {
      Type = typeName,
      Body = body
    };
  }

  public object Deserialize(JsonEnvelope envelope)
  {
    ArgumentNullException.ThrowIfNull(envelope);

    if (string.IsNullOrEmpty(envelope.Type))
      throw new ArgumentException("Envelope type cannot be null or empty.", nameof(envelope));

    if (envelope.Body.ValueKind == JsonValueKind.Undefined)
      throw new ArgumentException("Envelope body cannot be undefined.", nameof(envelope));

    var requestType = _registry.GetRequestType(envelope.Type);

    return envelope.Body.Deserialize(requestType, _options)
      ?? throw new InvalidOperationException(
        $"Deserialization of '{envelope.Type}' returned null.");
  }
}
