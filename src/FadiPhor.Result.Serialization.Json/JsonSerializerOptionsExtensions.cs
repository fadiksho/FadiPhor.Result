using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace FadiPhor.Result.Serialization.Json;

/// <summary>
/// Provides extension methods for configuring System.Text.Json to serialize <see cref="Result{T}"/> types.
/// </summary>
public static class JsonSerializerOptionsExtensions
{
  /// <summary>
  /// Configures the JSON serializer options to support <see cref="Result{T}"/> serialization 
  /// with polymorphic error support.
  /// </summary>
  /// <param name="options">The serializer options to configure.</param>
  /// <param name="resolvers">A collection of custom error type resolvers for polymorphic Error serialization.</param>
  /// <returns>The configured <see cref="JsonSerializerOptions"/> instance for fluent chaining.</returns>
  /// <remarks>
  /// Core library error types (e.g., ValidationFailure) are automatically registered.
  /// Custom error types require a resolver implementing <see cref="IErrorPolymorphicResolver"/>.
  /// </remarks>
  public static JsonSerializerOptions AddResultSerialization(
    this JsonSerializerOptions options,
    IEnumerable<IErrorPolymorphicResolver> resolvers)
  {
    // Add the Result converter factory
    options.Converters.Add(new ResultJsonConverterFactory());

    // Collect all resolvers: custom resolvers + default resolver
    var allResolvers = resolvers.Append(new DefaultErrorPolymorphicResolver()).ToList();

    // Build type info resolver with centralized polymorphism configuration
    var resultResolver = new DefaultJsonTypeInfoResolver();

    resultResolver.Modifiers.Add(typeInfo =>
    {
      if (typeInfo.Type != typeof(Error))
        return;

      var opts = typeInfo.PolymorphismOptions ??= new JsonPolymorphismOptions
      {
        TypeDiscriminatorPropertyName = "$type",
        IgnoreUnrecognizedTypeDiscriminators = false,
        UnknownDerivedTypeHandling = JsonUnknownDerivedTypeHandling.FailSerialization,
      };

      foreach (var resolver in allResolvers)
        foreach (var derived in resolver.GetDerivedTypes())
          opts.DerivedTypes.Add(derived);
    });

    // Preserve existing resolver if present, otherwise use the new one
    if (options.TypeInfoResolver != null)
    {
      // Combine existing resolver with the result resolver
      // Consumer resolver runs first to maintain expected behavior
      options.TypeInfoResolver = JsonTypeInfoResolver.Combine(options.TypeInfoResolver, resultResolver);
    }
    else
    {
      options.TypeInfoResolver = resultResolver;
    }

    return options;
  }

  /// <summary>
  /// Configures the JSON serializer options to support <see cref="Result{T}"/> serialization 
  /// with polymorphic error support.
  /// </summary>
  /// <param name="options">The serializer options to configure.</param>
  /// <param name="resolvers">Custom error type resolvers for polymorphic Error serialization.</param>
  /// <returns>The configured <see cref="JsonSerializerOptions"/> instance for fluent chaining.</returns>
  public static JsonSerializerOptions AddResultSerialization(
    this JsonSerializerOptions options,
    params IErrorPolymorphicResolver[] resolvers)
  {
    return AddResultSerialization(options, (IEnumerable<IErrorPolymorphicResolver>)resolvers);
  }
}
