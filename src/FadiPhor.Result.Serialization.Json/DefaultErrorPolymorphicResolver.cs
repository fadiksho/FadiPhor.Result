using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using FadiPhor.Result;

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
  public void ResolveDerivedType(JsonTypeInfo typeInfo)
  {
    if (typeInfo.Type != typeof(Error))
      return;

    // If PolymorphismOptions already exists, merge derived types
    // Otherwise, create new options
    if (typeInfo.PolymorphismOptions != null)
    {
      // Add core types to existing derived types
      var existingTypes = typeInfo.PolymorphismOptions.DerivedTypes.ToList();
      
      // Only add core types if they're not already registered
      if (!existingTypes.Any(dt => dt.DerivedType == typeof(ValidationFailure)))
      {
        existingTypes.Add(new JsonDerivedType(typeof(ValidationFailure), nameof(ValidationFailure)));
      }

      typeInfo.PolymorphismOptions = new JsonPolymorphismOptions
      {
        TypeDiscriminatorPropertyName = typeInfo.PolymorphismOptions.TypeDiscriminatorPropertyName,
        IgnoreUnrecognizedTypeDiscriminators = typeInfo.PolymorphismOptions.IgnoreUnrecognizedTypeDiscriminators,
        UnknownDerivedTypeHandling = typeInfo.PolymorphismOptions.UnknownDerivedTypeHandling,
        DerivedTypes = { }
      };

      foreach (var derivedType in existingTypes)
      {
        typeInfo.PolymorphismOptions.DerivedTypes.Add(derivedType);
      }
    }
    else
    {
      // Create new PolymorphismOptions with core types
      typeInfo.PolymorphismOptions = new JsonPolymorphismOptions
      {
        TypeDiscriminatorPropertyName = "$type",
        IgnoreUnrecognizedTypeDiscriminators = false,
        UnknownDerivedTypeHandling = JsonUnknownDerivedTypeHandling.FailSerialization,
        DerivedTypes =
        {
          new JsonDerivedType(typeof(ValidationFailure), nameof(ValidationFailure))
        }
      };
    }
  }
}
