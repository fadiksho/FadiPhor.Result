namespace FadiPhor.Result.Serialization.Json.Transport;

/// <summary>
/// Resolves .NET types from full CLR type names used in envelope-based transport.
/// </summary>
internal interface IJsonRequestTypeRegistry
{
  /// <summary>
  /// Gets the .NET type for the given full CLR type name.
  /// </summary>
  /// <param name="typeName">The full CLR type name as it appears in the envelope.</param>
  /// <returns>The resolved .NET type.</returns>
  /// <exception cref="InvalidOperationException">Thrown when the type name is not registered.</exception>
  Type GetRequestType(string typeName);

  /// <summary>
  /// Gets the full CLR type name for the given .NET request type.
  /// </summary>
  /// <param name="requestType">The .NET type of the request.</param>
  /// <returns>The full CLR type name used in envelope transport.</returns>
  /// <exception cref="InvalidOperationException">Thrown when the type is not registered.</exception>
  string GetRequestTypeName(Type requestType);
}
