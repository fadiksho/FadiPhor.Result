using System.Text.Json;
using System.Text.Json.Serialization;

namespace FadiPhor.Result.Serialization.Json;

/// <summary>
/// Factory for creating JSON converters for <see cref="Result{T}"/> types.
/// </summary>
/// <remarks>
/// Automatically registered when using AddResultSerialization. Results are serialized with a "kind" discriminator 
/// property to distinguish Success from Failure.
/// </remarks>
public sealed class ResultJsonConverterFactory : JsonConverterFactory
{
  /// <summary>
  /// Determines whether this factory can create a converter for the specified type.
  /// </summary>
  /// <param name="typeToConvert">The type to check.</param>
  /// <returns>True if the type is a generic Result type; otherwise, false.</returns>
  public override bool CanConvert(Type typeToConvert)
  {
    if (!typeToConvert.IsGenericType)
      return false;

    var genericType = typeToConvert.GetGenericTypeDefinition();

    return genericType == typeof(Result<>) ||
           genericType == typeof(Success<>) ||
           genericType == typeof(Failure<>);
  }

  /// <summary>
  /// Creates a JSON converter for the specified Result type.
  /// </summary>
  /// <param name="typeToConvert">The Result type to create a converter for.</param>
  /// <param name="options">The serializer options being used.</param>
  /// <returns>A converter instance for the specified Result type.</returns>
  public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options)
  {
    var valueType = typeToConvert.GetGenericArguments()[0];
    var converterType = typeof(ResultJsonConverter<>).MakeGenericType(valueType);
    return (JsonConverter?)Activator.CreateInstance(converterType);
  }
}
