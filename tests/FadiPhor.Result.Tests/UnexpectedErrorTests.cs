namespace FadiPhor.Result.Tests;

public class UnexpectedErrorTests
{
  [Fact]
  public void UnexpectedError_ShouldInheritFromError()
  {
    // Act
    var error = new UnexpectedError();

    // Assert
    Assert.IsAssignableFrom<Error>(error);
  }

  [Fact]
  public void UnexpectedError_ShouldHaveFixedCode()
  {
    // Act
    var error = new UnexpectedError();

    // Assert
    Assert.Equal("unexpected", error.Code);
  }

  [Fact]
  public void UnexpectedError_ShouldHaveDefaultMessage()
  {
    // Act
    var error = new UnexpectedError();

    // Assert
    Assert.Equal("An unexpected error occurred.", error.Message);
  }

  [Fact]
  public void UnexpectedError_WithCustomMessage_ShouldOverrideDefault()
  {
    // Act
    var error = new UnexpectedError("Database connection lost.");

    // Assert
    Assert.Equal("unexpected", error.Code);
    Assert.Equal("Database connection lost.", error.Message);
  }

  [Fact]
  public void UnexpectedError_ShouldConvertImplicitlyToResult()
  {
    // Arrange
    var error = new UnexpectedError();

    // Act
    Result<int> result = error;

    // Assert
    Assert.IsType<Failure<int>>(result);
    var failure = (Failure<int>)result;
    Assert.IsType<UnexpectedError>(failure.Error);
  }

  [Fact]
  public void UnexpectedError_ShouldHaveHttpStatusCode500()
  {
    // Act
    var error = new UnexpectedError();

    // Assert
    Assert.Equal(500, error.HttpStatusCode);
  }

  [Fact]
  public void UnexpectedError_InResultMatch_ShouldWork()
  {
    // Arrange
    var error = new UnexpectedError();
    Result<int> result = error;

    // Act
    var output = result.Match(
      onSuccess: value => "Success",
      onFailure: e => $"Error: {e.Code}");

    // Assert
    Assert.Equal("Error: unexpected", output);
  }
}
