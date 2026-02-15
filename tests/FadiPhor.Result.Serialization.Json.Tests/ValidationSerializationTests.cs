using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using FadiPhor.Result.Serialization.Json;

namespace FadiPhor.Result.Serialization.Json.Tests;

public class ValidationSerializationTests
{
  [Fact]
  public void ValidationFailure_RoundTrip_ShouldPreserveStructure()
  {
    // Arrange
    var issues = new List<ValidationIssue>
    {
      new("Email", "Email is required"),
      new("Password", "Password too short", ValidationSeverity.Error)
    };
    var failure = new ValidationFailure(issues);
    Result<int> result = failure;

    var options = CreateSerializerOptions();

    // Act
    var json = JsonSerializer.Serialize(result, options);
    var deserialized = JsonSerializer.Deserialize<Result<int>>(json, options);

    // Assert - verify JSON structure
    Assert.Contains("\"kind\":\"Failure\"", json);
    Assert.Contains("\"$type\":\"ValidationFailure\"", json);
    Assert.Contains("\"code\":\"validation.failed\"", json);
    Assert.Contains("\"message\":\"Validation failed.\"", json);
    Assert.Contains("\"issues\":", json);
    Assert.Contains("\"identifier\":\"Email\"", json);
    
    // Assert - verify deserialization
    Assert.NotNull(deserialized);
    Assert.IsType<Failure<int>>(deserialized);
    var deserializedFailure = (Failure<int>)deserialized;
    Assert.IsType<ValidationFailure>(deserializedFailure.Error);
    var validationFailure = (ValidationFailure)deserializedFailure.Error;
    Assert.Equal(2, validationFailure.Issues.Count);
  }

  [Fact]
  public void ValidationFailure_WithCustomResolver_ShouldWorkTogether()
  {
    // Arrange - custom error type
    var customError = new CustomTestError("custom.error");
    Result<int> customResult = customError;

    // Arrange - validation failure
    var issues = new[] { new ValidationIssue("Email", "Email is required") };
    var validationFailure = new ValidationFailure(issues);
    Result<string> validationResult = validationFailure;

    var options = new JsonSerializerOptions
    {
      PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
      WriteIndented = false
    }.AddResultSerialization(new CustomErrorResolver());

    // Act
    var customJson = JsonSerializer.Serialize(customResult, options);
    var validationJson = JsonSerializer.Serialize(validationResult, options);

    // Assert - both core and custom errors should serialize
    Assert.Contains("\"$type\":\"CustomTestError\"", customJson);
    Assert.Contains("\"$type\":\"ValidationFailure\"", validationJson);
    
    // Verify deserialization works for both
    var deserializedCustom = JsonSerializer.Deserialize<Result<int>>(customJson, options);
    var deserializedValidation = JsonSerializer.Deserialize<Result<string>>(validationJson, options);
    
    Assert.NotNull(deserializedCustom);
    Assert.NotNull(deserializedValidation);
    Assert.IsType<Failure<int>>(deserializedCustom);
    Assert.IsType<Failure<string>>(deserializedValidation);
  }

  // Test custom error type
  private record CustomTestError(string Code) : Error(Code);

  private class CustomErrorResolver : IErrorPolymorphicResolver
  {
    public void ResolveDerivedType(JsonTypeInfo typeInfo)
    {
      if (typeInfo.Type != typeof(Error))
        return;

      // Custom resolver should work alongside default resolver
      typeInfo.PolymorphismOptions = new JsonPolymorphismOptions
      {
        TypeDiscriminatorPropertyName = "$type",
        IgnoreUnrecognizedTypeDiscriminators = false,
        UnknownDerivedTypeHandling = JsonUnknownDerivedTypeHandling.FailSerialization,
        DerivedTypes =
        {
          new JsonDerivedType(typeof(CustomTestError), nameof(CustomTestError))
        }
      };
    }
  }

  private static JsonSerializerOptions CreateSerializerOptions()
  {
    var options = new JsonSerializerOptions
    {
      PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
      WriteIndented = false
    };

    return options.AddResultSerialization();
  }
}
