using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace FadiPhor.Result.Serialization.Json.Tests;

public class SerializationTests
{
  // Test error classes
  private record TestError : Error
  {
    public TestError(string code, string? message = null) : base(code)
    {
      MessageInternal = message;
    }

    private string? MessageInternal { get; init; }
    public override string? Message => MessageInternal;
  }

  private record ValidationError : Error
  {
    public ValidationError(string code, string[] fields, string? message = null) : base(code)
    {
      Fields = fields;
      MessageInternal = message;
    }

    public string[] Fields { get; init; }
    private string? MessageInternal { get; init; }
    public override string? Message => MessageInternal;
  }

  // Test data class for complex object serialization
  private record User(int Id, string Name);

  // Test resolver for polymorphic error serialization
  private class TestErrorResolver : IErrorPolymorphicResolver
  {
    public void ResolveDerivedType(JsonTypeInfo typeInfo)
    {
      if (typeInfo.Type != typeof(Error))
        return;

      typeInfo.PolymorphismOptions = new JsonPolymorphismOptions
      {
        TypeDiscriminatorPropertyName = "$type",
        IgnoreUnrecognizedTypeDiscriminators = false,
        UnknownDerivedTypeHandling = JsonUnknownDerivedTypeHandling.FailSerialization,
        DerivedTypes =
        {
          new JsonDerivedType(typeof(TestError), nameof(TestError)),
          new JsonDerivedType(typeof(ValidationError), nameof(ValidationError))
        }
      };
    }
  }

  private static JsonSerializerOptions CreateOptions()
  {
    var options = new JsonSerializerOptions
    {
      PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
      WriteIndented = false
    };

    return options.AddResultSerialization(new TestErrorResolver());
  }

  [Fact]
  public void RoundTrip_Success_ShouldPreserveValue()
  {
    // Arrange
    var original = Result.Success("Hello, World!");
    var options = CreateOptions();

    // Act
    var json = JsonSerializer.Serialize(original, options);
    var deserialized = JsonSerializer.Deserialize<Result<string>>(json, options);

    // Assert - verify JSON structure
    Assert.Contains("\"kind\":\"Success\"", json);
    Assert.Contains("\"value\":\"Hello, World!\"", json);

    // Assert - verify deserialization
    Assert.NotNull(deserialized);
    Assert.IsType<Success<string>>(deserialized);
    Assert.Equal("Hello, World!", ((Success<string>)deserialized).Value);
  }

  [Fact]
  public void RoundTrip_Failure_ShouldPreserveError()
  {
    // Arrange
    var error = new TestError("validation.failed", "Validation failed");
    var original = Result.Failure<string>(error);
    var options = CreateOptions();

    // Act
    var json = JsonSerializer.Serialize(original, options);
    var deserialized = JsonSerializer.Deserialize<Result<string>>(json, options);

    // Assert - verify JSON structure
    Assert.Contains("\"kind\":\"Failure\"", json);
    Assert.Contains("\"error\":", json);
    Assert.Contains("\"$type\":\"TestError\"", json);
    Assert.Contains("\"code\":\"validation.failed\"", json);

    // Assert - verify deserialization
    Assert.NotNull(deserialized);
    Assert.IsType<Failure<string>>(deserialized);
    var failure = (Failure<string>)deserialized;
    Assert.Equal("validation.failed", failure.Error.Code);
    Assert.Equal("Validation failed", failure.Error.Message);
  }

  [Fact]
  public void Serialize_PolymorphicError_ShouldIncludeTypeDiscriminator()
  {
    // Arrange
    var error = new ValidationError("validation.failed", ["email", "password"], "Validation failed");
    var result = Result.Failure<int>(error);
    var options = CreateOptions();

    // Act
    var json = JsonSerializer.Serialize(result, options);

    // Assert
    Assert.Contains("\"$type\":\"ValidationError\"", json);
    Assert.Contains("\"fields\":[\"email\",\"password\"]", json);
  }

  [Fact]
  public void Deserialize_PolymorphicError_ShouldRestoreCorrectType()
  {
    // Arrange
    var json = "{\"kind\":\"Failure\",\"error\":{\"$type\":\"ValidationError\",\"code\":\"validation.failed\",\"message\":\"Validation failed\",\"fields\":[\"email\",\"password\"]}}";
    var options = CreateOptions();

    // Act
    var result = JsonSerializer.Deserialize<Result<int>>(json, options);

    // Assert
    Assert.NotNull(result);
    Assert.IsType<Failure<int>>(result);
    var failure = (Failure<int>)result;
    Assert.IsType<ValidationError>(failure.Error);
    var validationError = (ValidationError)failure.Error;
    Assert.Equal(["email", "password"], validationError.Fields);
  }

  [Fact]
  public void Deserialize_MissingKind_ShouldThrow()
  {
    // Arrange
    var json = "{\"value\":42}";
    var options = CreateOptions();

    // Act & Assert
    Assert.Throws<JsonException>(() =>
      JsonSerializer.Deserialize<Result<int>>(json, options));
  }

  [Fact]
  public void Deserialize_UnknownKind_ShouldThrow()
  {
    // Arrange
    var json = "{\"kind\":\"Unknown\",\"value\":42}";
    var options = CreateOptions();

    // Act & Assert
    Assert.Throws<JsonException>(() =>
      JsonSerializer.Deserialize<Result<int>>(json, options));
  }

  [Fact]
  public void Deserialize_SuccessWithoutValue_ShouldThrow()
  {
    // Arrange
    var json = "{\"kind\":\"Success\"}";
    var options = CreateOptions();

    // Act & Assert
    Assert.Throws<JsonException>(() =>
      JsonSerializer.Deserialize<Result<int>>(json, options));
  }

  [Fact]
  public void Deserialize_FailureWithoutError_ShouldThrow()
  {
    // Arrange
    var json = "{\"kind\":\"Failure\"}";
    var options = CreateOptions();

    // Act & Assert
    Assert.Throws<JsonException>(() =>
      JsonSerializer.Deserialize<Result<int>>(json, options));
  }

  [Fact]
  public void Serialize_ComplexObject_Success()
  {
    // Arrange
    var user = new User(123, "John Doe");
    var result = Result.Success(user);
    var options = CreateOptions();

    // Act
    var json = JsonSerializer.Serialize(result, options);
    var deserialized = JsonSerializer.Deserialize<Result<User>>(json, options);

    // Assert
    Assert.Contains("\"kind\":\"Success\"", json);
    Assert.Contains("\"id\":123", json);
    Assert.Contains("\"name\":\"John Doe\"", json);

    Assert.NotNull(deserialized);
    Assert.IsType<Success<User>>(deserialized);
    var success = (Success<User>)deserialized;
    Assert.Equal(123, success.Value.Id);
    Assert.Equal("John Doe", success.Value.Name);
  }

  [Fact]
  public void RoundTrip_ResultWithUnit_ShouldPreserve()
  {
    // Arrange
    var original = Result.Success(Unit.Value);
    var options = CreateOptions();

    // Act
    var json = JsonSerializer.Serialize(original, options);
    var deserialized = JsonSerializer.Deserialize<Result<Unit>>(json, options);

    // Assert - verify JSON structure
    Assert.Contains("\"kind\":\"Success\"", json);
    Assert.Contains("\"value\":{}", json);

    // Assert - verify deserialization
    Assert.NotNull(deserialized);
    Assert.IsType<Success<Unit>>(deserialized);
  }

  [Fact]
  public void AddResultSerialization_PreservesExistingTypeInfoResolver()
  {
    // Arrange
    var options = new JsonSerializerOptions
    {
      PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
      TypeInfoResolver = new DefaultJsonTypeInfoResolver() // Pre-existing resolver
    };
    var existingResolver = options.TypeInfoResolver;

    // Act
    options.AddResultSerialization(new TestErrorResolver());

    // Assert
    Assert.NotNull(options.TypeInfoResolver);
    Assert.NotSame(existingResolver, options.TypeInfoResolver); // Should be combined, not replaced

    // Verify serialization still works
    var result = Result.Success(42);
    var json = JsonSerializer.Serialize(result, options);
    Assert.Contains("\"kind\":\"Success\"", json);
    Assert.Contains("\"value\":42", json);

    var deserialized = JsonSerializer.Deserialize<Result<int>>(json, options);
    Assert.NotNull(deserialized);
    Assert.IsType<Success<int>>(deserialized);
    Assert.Equal(42, ((Success<int>)deserialized).Value);
  }

  [Fact]
  public void AddResultSerialization_WorksWithNullTypeInfoResolver()
  {
    // Arrange
    var options = new JsonSerializerOptions
    {
      PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
      TypeInfoResolver = null // No pre-existing resolver
    };

    // Act
    options.AddResultSerialization(new TestErrorResolver());

    // Assert
    Assert.NotNull(options.TypeInfoResolver);

    // Verify serialization works
    var result = Result.Success(42);
    var json = JsonSerializer.Serialize(result, options);
    Assert.Contains("\"kind\":\"Success\"", json);
    Assert.Contains("\"value\":42", json);

    var deserialized = JsonSerializer.Deserialize<Result<int>>(json, options);
    Assert.NotNull(deserialized);
    Assert.IsType<Success<int>>(deserialized);
    Assert.Equal(42, ((Success<int>)deserialized).Value);
  }
}
