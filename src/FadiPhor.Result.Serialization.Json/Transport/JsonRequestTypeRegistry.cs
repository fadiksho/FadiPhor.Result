using System.Reflection;

namespace FadiPhor.Result.Serialization.Json.Transport;

/// <summary>
/// Scans assemblies for types implementing a consumer-provided marker interface and builds
/// a lookup from full CLR type name to .NET type for envelope serialization.
/// </summary>
internal sealed class JsonRequestTypeRegistry : IJsonRequestTypeRegistry
{
  private readonly Dictionary<string, Type> _requestTypes;

  /// <param name="assemblies">Assemblies to scan for request types.</param>
  /// <param name="requestMarkerType">
  /// The interface used to identify request types. Can be a non-generic interface
  /// (e.g. <c>typeof(IRequest)</c>) or an open generic interface
  /// (e.g. <c>typeof(IRequest&lt;&gt;)</c>).
  /// </param>
  public JsonRequestTypeRegistry(IEnumerable<Assembly> assemblies, Type requestMarkerType)
  {
    _requestTypes = assemblies
      .SelectMany(a => a.GetExportedTypes())
      .Where(t => !t.IsAbstract && !t.IsInterface && ImplementsMarker(t, requestMarkerType))
      .ToDictionary(t => t.FullName!, t => t);
  }

  public Type GetRequestType(string typeName)
  {
    return _requestTypes.TryGetValue(typeName, out var type)
      ? type
      : throw new InvalidOperationException($"Unknown request type: '{typeName}'");
  }

  public string GetRequestTypeName(Type requestType)
  {
    var fullName = requestType.FullName
      ?? throw new InvalidOperationException($"Request type '{requestType}' has no FullName.");

    return _requestTypes.ContainsKey(fullName)
      ? fullName
      : throw new InvalidOperationException($"Unregistered request type: '{fullName}'");
  }

  private static bool ImplementsMarker(Type type, Type markerType)
  {
    if (markerType.IsGenericTypeDefinition)
    {
      return type
        .GetInterfaces()
        .Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == markerType);
    }

    return markerType.IsAssignableFrom(type);
  }
}
