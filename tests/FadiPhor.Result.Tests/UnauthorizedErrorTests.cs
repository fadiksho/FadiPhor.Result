namespace FadiPhor.Result.Tests;

public class UnauthorizedErrorTests
{
  [Fact]
  public void UnauthorizedError_ShouldInheritFromError()
  {
    // Act
    var error = new UnauthorizedError();

    // Assert
    Assert.IsAssignableFrom<Error>(error);
  }

  [Fact]
  public void UnauthorizedError_ShouldHaveFixedCode()
  {
    // Act
    var error = new UnauthorizedError();

    // Assert
    Assert.Equal("unauthorized", error.Code);
  }

  [Fact]
  public void UnauthorizedError_ShouldHaveDefaultMessage()
  {
    // Act
    var error = new UnauthorizedError();

    // Assert
    Assert.Equal("You do not have permission to perform this action.", error.Message);
  }

  [Fact]
  public void UnauthorizedError_WithCustomMessage_ShouldOverrideDefault()
  {
    // Act
    var error = new UnauthorizedError("Admin role required.");

    // Assert
    Assert.Equal("unauthorized", error.Code);
    Assert.Equal("Admin role required.", error.Message);
  }

  [Fact]
  public void UnauthorizedError_ShouldConvertImplicitlyToResult()
  {
    // Arrange
    var error = new UnauthorizedError();

    // Act
    Result<int> result = error;

    // Assert
    Assert.IsType<Failure<int>>(result);
    var failure = (Failure<int>)result;
    Assert.IsType<UnauthorizedError>(failure.Error);
  }

  [Fact]
  public void UnauthorizedError_InResultMatch_ShouldWork()
  {
    // Arrange
    var error = new UnauthorizedError();
    Result<int> result = error;

    // Act
    var output = result.Match(
      onSuccess: value => "Success",
      onFailure: e => $"Error: {e.Code}");

    // Assert
    Assert.Equal("Error: unauthorized", output);
  }
}
