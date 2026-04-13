namespace FadiPhor.Result.Tests;

public class NotFoundErrorTests
{
  [Fact]
  public void NotFoundError_ShouldInheritFromError()
  {
    // Act
    var error = new NotFoundError();

    // Assert
    Assert.IsAssignableFrom<Error>(error);
  }

  [Fact]
  public void NotFoundError_ShouldHaveFixedCode()
  {
    // Act
    var error = new NotFoundError();

    // Assert
    Assert.Equal("not_found", error.Code);
  }

  [Fact]
  public void NotFoundError_ShouldHaveDefaultMessage()
  {
    // Act
    var error = new NotFoundError();

    // Assert
    Assert.Equal("The requested resource was not found.", error.Message);
  }

  [Fact]
  public void NotFoundError_WithCustomMessage_ShouldOverrideDefault()
  {
    // Act
    var error = new NotFoundError("User 42 was not found.");

    // Assert
    Assert.Equal("not_found", error.Code);
    Assert.Equal("User 42 was not found.", error.Message);
  }

  [Fact]
  public void NotFoundError_ShouldConvertImplicitlyToResult()
  {
    // Arrange
    var error = new NotFoundError();

    // Act
    Result<int> result = error;

    // Assert
    Assert.IsType<Failure<int>>(result);
    var failure = (Failure<int>)result;
    Assert.IsType<NotFoundError>(failure.Error);
  }

  [Fact]
  public void NotFoundError_ShouldHaveHttpStatusCode404()
  {
    // Act
    var error = new NotFoundError();

    // Assert
    Assert.Equal(404, error.HttpStatusCode);
  }

  [Fact]
  public void NotFoundError_InResultMatch_ShouldWork()
  {
    // Arrange
    var error = new NotFoundError();
    Result<int> result = error;

    // Act
    var output = result.Match(
      onSuccess: value => "Success",
      onFailure: e => $"Error: {e.Code}");

    // Assert
    Assert.Equal("Error: not_found", output);
  }
}
