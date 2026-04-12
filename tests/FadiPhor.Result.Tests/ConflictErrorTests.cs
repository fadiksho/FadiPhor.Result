namespace FadiPhor.Result.Tests;

public class ConflictErrorTests
{
  [Fact]
  public void ConflictError_ShouldInheritFromError()
  {
    // Act
    var error = new ConflictError();

    // Assert
    Assert.IsAssignableFrom<Error>(error);
  }

  [Fact]
  public void ConflictError_ShouldHaveFixedCode()
  {
    // Act
    var error = new ConflictError();

    // Assert
    Assert.Equal("conflict", error.Code);
  }

  [Fact]
  public void ConflictError_ShouldHaveDefaultMessage()
  {
    // Act
    var error = new ConflictError();

    // Assert
    Assert.Equal("The request conflicts with the current state of the resource.", error.Message);
  }

  [Fact]
  public void ConflictError_WithCustomMessage_ShouldOverrideDefault()
  {
    // Act
    var error = new ConflictError("Duplicate email address.");

    // Assert
    Assert.Equal("conflict", error.Code);
    Assert.Equal("Duplicate email address.", error.Message);
  }

  [Fact]
  public void ConflictError_ShouldConvertImplicitlyToResult()
  {
    // Arrange
    var error = new ConflictError();

    // Act
    Result<int> result = error;

    // Assert
    Assert.IsType<Failure<int>>(result);
    var failure = (Failure<int>)result;
    Assert.IsType<ConflictError>(failure.Error);
  }

  [Fact]
  public void ConflictError_InResultMatch_ShouldWork()
  {
    // Arrange
    var error = new ConflictError();
    Result<int> result = error;

    // Act
    var output = result.Match(
      onSuccess: value => "Success",
      onFailure: e => $"Error: {e.Code}");

    // Assert
    Assert.Equal("Error: conflict", output);
  }
}
