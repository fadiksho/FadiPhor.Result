namespace FadiPhor.Result.Tests;

public class CoreTests
{
  // Test helper error class
  private record TestError : Error
  {
    public TestError(string code, string? message = null) : base(code)
    {
      MessageInternal = message;
    }

    private string? MessageInternal { get; init; }
    public override string? Message => MessageInternal;
  }

  [Fact]
  public void Success_ShouldHoldValue()
  {
    // Arrange & Act
    var result = Result.Success(42);

    // Assert
    Assert.IsType<Success<int>>(result);
    var success = (Success<int>)result;
    Assert.Equal(42, success.Value);
  }

  [Fact]
  public void Success_WithNullValue_ShouldThrow()
  {
    // Arrange & Act & Assert
    Assert.Throws<ArgumentNullException>(() => new Success<string>(null!));
  }

  [Fact]
  public void Failure_ShouldHoldError()
  {
    // Arrange
    var error = new TestError("test.error", "Test error");

    // Act
    var result = Result.Failure<int>(error);

    // Assert
    Assert.IsType<Failure<int>>(result);
    var failure = (Failure<int>)result;
    Assert.Equal(error, failure.Error);
  }

  [Fact]
  public void Failure_WithNullError_ShouldThrow()
  {
    // Arrange & Act & Assert
    Assert.Throws<ArgumentNullException>(() => new Failure<int>(null!));
  }



  [Fact]
  public void Bind_ChainMultipleOperations()
  {
    // Arrange
    var result = Result.Success(10);

    // Act
    var output = result
      .Bind(x => Result.Success(x + 5))
      .Bind(x => Result.Success(x * 2))
      .Bind(x => Result.Success(x.ToString()));

    // Assert
    Assert.IsType<Success<string>>(output);
    Assert.Equal("30", ((Success<string>)output).Value);
  }

  [Fact]
  public void Bind_ErrorPropagation_ShouldStopChain()
  {
    // Arrange
    var result = Result.Success(10);
    var error = new TestError("test.error", "Test error");

    // Act
    var output = result
      .Bind(x => Result.Success(x + 5))
      .Bind(x => Result.Failure<int>(error))
      .Bind(x => Result.Success(x * 100)); // Should not execute

    // Assert
    Assert.IsType<Failure<int>>(output);
    Assert.Equal(error, ((Failure<int>)output).Error);
  }

  [Fact]
  public async Task AsyncBind_OnSuccess_ShouldExecuteNextFunction()
  {
    // Arrange
    var resultTask = Task.FromResult(Result.Success(42));

    // Act
    var output = await resultTask.Bind(async value =>
    {
      await Task.Delay(1); // Simulate async work
      return Result.Success(value * 2);
    });

    // Assert
    Assert.IsType<Success<int>>(output);
    Assert.Equal(84, ((Success<int>)output).Value);
  }

  [Fact]
  public async Task AsyncBind_OnFailure_ShouldPropagateError()
  {
    // Arrange
    var error = new TestError("test.error", "Test error");
    var resultTask = Task.FromResult(Result.Failure<int>(error));

    // Act
    var output = await resultTask.Bind(async value =>
    {
      await Task.Delay(1);
      return Result.Success(value * 2);
    });

    // Assert
    Assert.IsType<Failure<int>>(output);
    Assert.Equal(error, ((Failure<int>)output).Error);
  }

  [Fact]
  public async Task AsyncBind_ChainMultipleOperations()
  {
    // Arrange
    var resultTask = Task.FromResult(Result.Success(10));

    // Act
    var output = await resultTask
      .Bind(async x =>
      {
        await Task.Delay(1);
        return Result.Success(x + 5);
      })
      .Bind(async x =>
      {
        await Task.Delay(1);
        return Result.Success(x * 2);
      });

    // Assert
    Assert.IsType<Success<int>>(output);
    Assert.Equal(30, ((Success<int>)output).Value);
  }

  [Fact]
  public void ResultWithUnit_Success()
  {
    // Arrange & Act
    var result = Result.Success(Unit.Value);

    // Assert
    Assert.IsType<Success<Unit>>(result);
  }

  [Fact]
  public void ResultWithUnit_Failure()
  {
    // Arrange
    var error = new TestError("test.error", "Test error");

    // Act
    var result = Result.Failure<Unit>(error);

    // Assert
    Assert.IsType<Failure<Unit>>(result);
    Assert.Equal(error, ((Failure<Unit>)result).Error);
  }

  [Fact]
  public void ImplicitConversion_FromValue_ShouldCreateSuccess()
  {
    // Arrange & Act
    Result<int> result = 42;

    // Assert
    Assert.IsType<Success<int>>(result);
    var success = (Success<int>)result;
    Assert.Equal(42, success.Value);
  }

  [Fact]
  public void ImplicitConversion_FromError_ShouldCreateFailure()
  {
    // Arrange
    var error = new TestError("test.error", "Test error");

    // Act
    Result<int> result = error;

    // Assert
    Assert.IsType<Failure<int>>(result);
    var failure = (Failure<int>)result;
    Assert.Equal(error, failure.Error);
  }

  [Fact]
  public void ImplicitConversion_FromNullValue_ShouldThrow()
  {
    // Arrange & Act & Assert
    Assert.Throws<ArgumentNullException>(() =>
    {
      Result<string> result = (string)null!;
    });
  }

  [Fact]
  public void ImplicitConversion_FromNullError_ShouldThrow()
  {
    // Arrange & Act & Assert
    Assert.Throws<ArgumentNullException>(() =>
    {
      Result<int> result = (Error)null!;
    });
  }

  [Fact]
  public void ImplicitConversion_InMethodReturn_Value()
  {
    // Act
    var result = GetUserById(123);

    // Assert
    Assert.IsType<Success<string>>(result);
    Assert.Equal("User123", ((Success<string>)result).Value);

    // Helper method using implicit conversion
    static Result<string> GetUserById(int id)
    {
      return $"User{id}"; // Implicit conversion from string
    }
  }

  [Fact]
  public void ImplicitConversion_InMethodReturn_Error()
  {
    // Act
    var result = GetUserById(0);

    // Assert
    Assert.IsType<Failure<string>>(result);
    Assert.Equal("user.not_found", ((Failure<string>)result).Error.Code);

    // Helper method using implicit conversion
    static Result<string> GetUserById(int id)
    {
      if (id == 0)
        return new TestError("user.not_found", "User not found"); // Implicit conversion from error

      return $"User{id}";
    }
  }
}
