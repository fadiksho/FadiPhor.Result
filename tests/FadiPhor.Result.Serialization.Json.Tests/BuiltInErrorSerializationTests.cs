using System.Text.Json;
using FadiPhor.Result.Serialization.Json.Configuration;

namespace FadiPhor.Result.Serialization.Json.Tests;

public class BuiltInErrorSerializationTests
{
  [Theory]
  [MemberData(nameof(BuiltInErrorCases))]
  public void BuiltInError_RoundTrip_ShouldPreserveType(
    Error error, string expectedType, string expectedCode, string expectedMessage)
  {
    // Arrange
    Result<int> result = error;
    var options = CreateSerializerOptions();

    // Act
    var json = JsonSerializer.Serialize(result, options);
    var deserialized = JsonSerializer.Deserialize<Result<int>>(json, options);

    // Assert - verify JSON structure
    Assert.Contains("\"kind\":\"Failure\"", json);
    Assert.Contains($"\"$type\":\"{expectedType}\"", json);
    Assert.Contains($"\"code\":\"{expectedCode}\"", json);
    Assert.Contains($"\"message\":\"{expectedMessage}\"", json);

    // Assert - verify deserialization preserves concrete type
    Assert.NotNull(deserialized);
    Assert.IsType<Failure<int>>(deserialized);
    var failure = (Failure<int>)deserialized;
    Assert.Equal(error.GetType(), failure.Error.GetType());
    Assert.Equal(expectedCode, failure.Error.Code);
    Assert.Equal(expectedMessage, failure.Error.Message);
  }

  [Fact]
  public void BuiltInErrors_WithCustomMessage_ShouldRoundTrip()
  {
    // Arrange
    var error = new NotFoundError("User 42 was not found.");
    Result<string> result = error;
    var options = CreateSerializerOptions();

    // Act
    var json = JsonSerializer.Serialize(result, options);
    var deserialized = JsonSerializer.Deserialize<Result<string>>(json, options);

    // Assert
    Assert.NotNull(deserialized);
    var failure = Assert.IsType<Failure<string>>(deserialized);
    var restored = Assert.IsType<NotFoundError>(failure.Error);
    Assert.Equal("User 42 was not found.", restored.Message);
  }

  [Fact]
  public void BuiltInErrors_ShouldCoexistWithValidationFailure()
  {
    // Arrange
    var options = CreateSerializerOptions();

    Result<int> notFoundResult = new NotFoundError();
    Result<int> validationResult = new ValidationFailure(
      new[] { new ValidationIssue("Email", "Email is required") });

    // Act
    var notFoundJson = JsonSerializer.Serialize(notFoundResult, options);
    var validationJson = JsonSerializer.Serialize(validationResult, options);

    var notFoundDeserialized = JsonSerializer.Deserialize<Result<int>>(notFoundJson, options);
    var validationDeserialized = JsonSerializer.Deserialize<Result<int>>(validationJson, options);

    // Assert
    Assert.IsType<NotFoundError>(((Failure<int>)notFoundDeserialized!).Error);
    Assert.IsType<ValidationFailure>(((Failure<int>)validationDeserialized!).Error);
  }

  public static TheoryData<Error, string, string, string> BuiltInErrorCases => new()
  {
    { new NotFoundError(), "NotFoundError", "not_found", "The requested resource was not found." },
    { new UnauthenticatedError(), "UnauthenticatedError", "unauthenticated", "Authentication is required." },
    { new UnauthorizedError(), "UnauthorizedError", "unauthorized", "You do not have permission to perform this action." },
    { new ConflictError(), "ConflictError", "conflict", "The request conflicts with the current state of the resource." },
    { new UnexpectedError(), "UnexpectedError", "unexpected", "An unexpected error occurred." }
  };

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
