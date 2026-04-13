namespace FadiPhor.Result.Tests;

public class UnauthenticatedErrorTests
{
  [Fact]
  public void UnauthenticatedError_ShouldInheritFromError()
  {
    // Act
    var error = new UnauthenticatedError();

    // Assert
    Assert.IsAssignableFrom<Error>(error);
  }

  [Fact]
  public void UnauthenticatedError_ShouldHaveFixedCode()
  {
    // Act
    var error = new UnauthenticatedError();

    // Assert
    Assert.Equal("unauthenticated", error.Code);
  }

  [Fact]
  public void UnauthenticatedError_ShouldHaveDefaultMessage()
  {
    // Act
    var error = new UnauthenticatedError();

    // Assert
    Assert.Equal("Authentication is required.", error.Message);
  }

  [Fact]
  public void UnauthenticatedError_WithCustomMessage_ShouldOverrideDefault()
  {
    // Act
    var error = new UnauthenticatedError("Token has expired.");

    // Assert
    Assert.Equal("unauthenticated", error.Code);
    Assert.Equal("Token has expired.", error.Message);
  }

  [Fact]
  public void UnauthenticatedError_ShouldConvertImplicitlyToResult()
  {
    // Arrange
    var error = new UnauthenticatedError();

    // Act
    Result<int> result = error;

    // Assert
    Assert.IsType<Failure<int>>(result);
    var failure = (Failure<int>)result;
    Assert.IsType<UnauthenticatedError>(failure.Error);
  }

  [Fact]
  public void UnauthenticatedError_ShouldHaveHttpStatusCode401()
  {
    // Act
    var error = new UnauthenticatedError();

    // Assert
    Assert.Equal(401, error.HttpStatusCode);
  }

  [Fact]
  public void UnauthenticatedError_InResultMatch_ShouldWork()
  {
    // Arrange
    var error = new UnauthenticatedError();
    Result<int> result = error;

    // Act
    var output = result.Match(
      onSuccess: value => "Success",
      onFailure: e => $"Error: {e.Code}");

    // Assert
    Assert.Equal("Error: unauthenticated", output);
  }
}
