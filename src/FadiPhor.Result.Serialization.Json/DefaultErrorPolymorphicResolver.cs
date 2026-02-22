using System.Text.Json.Serialization.Metadata;

namespace FadiPhor.Result.Serialization.Json;

/// <summary>
/// Internal resolver that automatically registers core library error types for polymorphic serialization.
/// </summary>
/// <remarks>
/// <para>This resolver is applied automatically when calling AddResultSerialization() 
/// and ensures core error types work out of the box without consumer configuration.</para>
/// <para><strong>Registered Core Error Types:</strong></para>
/// <list type="bullet">
/// <item>ValidationFailure</item>
/// </list>
/// </remarks>
internal sealed class DefaultErrorPolymorphicResolver : IErrorPolymorphicResolver
{
  public IEnumerable<JsonDerivedType> GetDerivedTypes()
  {
    yield return new JsonDerivedType(typeof(ValidationFailure), nameof(ValidationFailure));
  }
}
