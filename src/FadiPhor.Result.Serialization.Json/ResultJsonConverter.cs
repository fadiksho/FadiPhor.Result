using System.Text.Json;
using System.Text.Json.Serialization;
using FadiPhor.Result;

namespace FadiPhor.Result.Serialization.Json;

internal sealed class ResultJsonConverter<T> : JsonConverter<Result<T>>
  where T : notnull
{
  public override Result<T>? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
  {
    if (reader.TokenType != JsonTokenType.StartObject)
      throw new JsonException("Expected start of object");

    reader.Read();

    if (reader.TokenType != JsonTokenType.PropertyName)
      throw new JsonException("Expected property name");

    var propertyName = reader.GetString();
    if (propertyName != "kind")
      throw new JsonException("Expected 'kind' property");

    reader.Read();

    if (reader.TokenType != JsonTokenType.String)
      throw new JsonException("Expected string value for 'kind'");

    var kind = reader.GetString();

    reader.Read();

    if (reader.TokenType != JsonTokenType.PropertyName)
      throw new JsonException("Expected property name");

    var dataPropertyName = reader.GetString();

    return kind switch
    {
      "Success" => ReadSuccess(ref reader, dataPropertyName, options),
      "Failure" => ReadFailure(ref reader, dataPropertyName, options),
      _ => throw new JsonException($"Unknown kind: {kind}")
    };
  }

  public override void Write(Utf8JsonWriter writer, Result<T> value, JsonSerializerOptions options)
  {
    writer.WriteStartObject();

    switch (value)
    {
      case Success<T> s:
        writer.WriteString("kind", "Success");
        writer.WritePropertyName("value");
        JsonSerializer.Serialize(writer, s.Value, options);
        break;

      case Failure<T> f:
        writer.WriteString("kind", "Failure");
        writer.WritePropertyName("error");
        JsonSerializer.Serialize<Error>(writer, f.Error, options);
        break;

      default:
        throw new InvalidOperationException("Unknown Result type");
    }

    writer.WriteEndObject();
  }

  private static Result<T> ReadSuccess(ref Utf8JsonReader reader, string? propertyName, JsonSerializerOptions options)
  {
    if (propertyName != "value")
      throw new JsonException("Expected 'value' property for Success");

    reader.Read();

    var value = JsonSerializer.Deserialize<T>(ref reader, options)
      ?? throw new JsonException("Value cannot be null");

    reader.Read();

    if (reader.TokenType != JsonTokenType.EndObject)
      throw new JsonException("Expected end of object");

    return new Success<T>(value);
  }

  private static Result<T> ReadFailure(ref Utf8JsonReader reader, string? propertyName, JsonSerializerOptions options)
  {
    if (propertyName != "error")
      throw new JsonException("Expected 'error' property for Failure");

    reader.Read();

    var error = JsonSerializer.Deserialize<Error>(ref reader, options)
      ?? throw new JsonException("Error cannot be null");

    reader.Read();

    if (reader.TokenType != JsonTokenType.EndObject)
      throw new JsonException("Expected end of object");

    return new Failure<T>(error);
  }
}
