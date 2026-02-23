using System.Reflection;
using System.Text.Json;
using FadiPhor.Result.Serialization.Json.Configuration;
using FadiPhor.Result.Serialization.Json.Errors;
using FadiPhor.Result.Serialization.Json.Transport;
using Microsoft.Extensions.DependencyInjection;

namespace FadiPhor.Result.Serialization.Json;

/// <summary>
/// Provides a unified entry point for registering the FadiPhor.Result JSON RPC protocol
/// infrastructure into a dependency injection container.
/// </summary>
public static class FadiPhorResultJsonExtensions
{
  /// <summary>
  /// Registers all FadiPhor.Result protocol infrastructure including request type registry,
  /// polymorphic error resolvers, JSON serializer options, and envelope serialization.
  /// </summary>
  /// <param name="services">The service collection to register into.</param>
  /// <param name="assemblies">
  /// Assemblies to scan for request type implementations and
  /// <see cref="IErrorPolymorphicResolver"/> implementations.
  /// </param>
  /// <param name="requestMarkerType">
  /// The interface used to identify request types during assembly scanning.
  /// Can be a non-generic interface (e.g. <c>typeof(IRequest)</c>) or an
  /// open generic interface (e.g. <c>typeof(IRequest&lt;&gt;)</c>).
  /// </param>
  /// <returns>The service collection for fluent chaining.</returns>
  /// <remarks>
  /// <para>This single call replaces manual registration of:</para>
  /// <list type="bullet">
  /// <item>Request type registry (scanning for request types matching the marker)</item>
  /// <item>Error polymorphic resolvers</item>
  /// <item><see cref="FadiPhorJsonOptions"/> with Result serialization support</item>
  /// <item>Envelope serializer</item>
  /// </list>
  /// <para>
  /// The library builds and owns its own <see cref="JsonSerializerOptions"/> instance,
  /// exposed through <see cref="FadiPhorJsonOptions"/> so it never collides with
  /// options the consumer may register independently.
  /// </para>
  /// </remarks>
  public static IServiceCollection AddFadiPhorResultProtocol(
    this IServiceCollection services,
    IEnumerable<Assembly> assemblies,
    Type requestMarkerType)
  {
    var assemblyList = assemblies.ToList();

    // 1. Register JsonRequestTypeRegistry (scanning assemblies for the consumer-provided marker)
    var registry = new JsonRequestTypeRegistry(assemblyList, requestMarkerType);
    services.AddSingleton<IJsonRequestTypeRegistry>(registry);

    // 2. Discover all IErrorPolymorphicResolver implementations from assemblies
    var resolvers = assemblyList
      .SelectMany(a => a.GetExportedTypes())
      .Where(t => !t.IsAbstract && !t.IsInterface
        && typeof(IErrorPolymorphicResolver).IsAssignableFrom(t))
      .Select(t => (IErrorPolymorphicResolver)Activator.CreateInstance(t)!)
      .ToList();

    // 3. Build protocol-owned JsonSerializerOptions with Result serialization
    //    DefaultErrorPolymorphicResolver (ValidationFailure) is added automatically by AddResultSerialization
    var options = new JsonSerializerOptions
    {
      PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };
    options.AddResultSerialization(resolvers);

    var jsonOptions = new FadiPhorJsonOptions(options);
    services.AddSingleton(jsonOptions);

    // 4. Register IJsonEnvelopeSerializer
    services.AddSingleton<IJsonEnvelopeSerializer>(
      new JsonEnvelopeSerializer(registry, jsonOptions));

    return services;
  }
}
