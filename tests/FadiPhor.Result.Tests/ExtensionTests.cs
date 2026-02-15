namespace FadiPhor.Result.Tests;

public class ExtensionTests
{
  // Test helper error classes
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
    public ValidationError(string code, string? message = null) : base(code)
    {
      MessageInternal = message;
    }

    private string? MessageInternal { get; init; }
    public override string? Message => MessageInternal;
  }

  #region IsSuccess and IsFailure Tests

  [Fact]
  public void IsSuccess_IsFailure_AreMutuallyExclusive()
  {
    // Arrange
    var success = Result.Success(42);
    var failure = Result.Failure<int>(new TestError("test.error"));

    // Act & Assert
    Assert.True(success.IsSuccess);
    Assert.False(success.IsFailure);
    Assert.False(failure.IsSuccess);
    Assert.True(failure.IsFailure);
  }

  #endregion

  #region TryGetValue Tests

  [Fact]
  public void TryGetValue_OnSuccess_ShouldReturnTrueAndAssignValue()
  {
    // Arrange
    var result = Result.Success(42);

    // Act
    var success = result.TryGetValue(out var value);

    // Assert
    Assert.True(success);
    Assert.Equal(42, value);
  }

  [Fact]
  public void TryGetValue_OnFailure_ShouldReturnFalseAndAssignDefault()
  {
    // Arrange
    var error = new TestError("test.error", "Test error");
    var result = Result.Failure<int>(error);

    // Act
    var success = result.TryGetValue(out var value);

    // Assert
    Assert.False(success);
    Assert.Equal(0, value); // default(int) is 0
  }

  [Fact]
  public void TryGetValue_WithComplexType_ShouldWork()
  {
    // Arrange
    var person = new { Name = "John", Age = 30 };
    var result = Result.Success(person);

    // Act
    var success = result.TryGetValue(out var value);

    // Assert
    Assert.True(success);
    Assert.Equal(person, value);
  }

  #endregion

  #region MapError Tests

  [Fact]
  public void MapError_OnSuccess_ShouldReturnUnchanged()
  {
    // Arrange
    var result = Result.Success(42);

    // Act
    var mapped = result.MapError(e => new ValidationError("mapped.error"));

    // Assert
    Assert.IsType<Success<int>>(mapped);
    Assert.Equal(42, ((Success<int>)mapped).Value);
    Assert.Same(result, mapped); // Should return the same instance
  }

  [Fact]
  public void MapError_OnFailure_ShouldTransformError()
  {
    // Arrange
    var originalError = new TestError("test.error", "Original error");
    var result = Result.Failure<int>(originalError);

    // Act
    var mapped = result.MapError(e => new ValidationError("mapped.error", "Mapped error"));

    // Assert
    Assert.IsType<Failure<int>>(mapped);
    var failure = (Failure<int>)mapped;
    Assert.IsType<ValidationError>(failure.Error);
    Assert.Equal("mapped.error", failure.Error.Code);
    Assert.Equal("Mapped error", failure.Error.Message);
  }

  [Fact]
  public void MapError_WithNullMap_ShouldThrowArgumentNullException()
  {
    // Arrange
    var result = Result.Success(42);

    // Act & Assert
    Assert.Throws<ArgumentNullException>(() => result.MapError(null!));
  }

  #endregion

  #region Ensure Tests

  [Fact]
  public void Ensure_OnFailure_ShouldReturnUnchanged()
  {
    // Arrange
    var error = new TestError("test.error", "Test error");
    var result = Result.Failure<int>(error);

    // Act
    var ensured = result.Ensure(
      x => x > 0,
      () => new ValidationError("should.not.be.called"));

    // Assert
    Assert.IsType<Failure<int>>(ensured);
    Assert.Same(result, ensured); // Should return the same instance
    Assert.Equal(error, ((Failure<int>)ensured).Error);
  }

  [Fact]
  public void Ensure_OnSuccessWithPredicateTrue_ShouldReturnUnchanged()
  {
    // Arrange
    var result = Result.Success(42);

    // Act
    var ensured = result.Ensure(
      x => x > 0,
      () => new ValidationError("validation.failed"));

    // Assert
    Assert.IsType<Success<int>>(ensured);
    Assert.Same(result, ensured); // Should return the same instance
    Assert.Equal(42, ((Success<int>)ensured).Value);
  }

  [Fact]
  public void Ensure_OnSuccessWithPredicateFalse_ShouldReturnFailure()
  {
    // Arrange
    var result = Result.Success(-5);

    // Act
    var ensured = result.Ensure(
      x => x > 0,
      () => new ValidationError("validation.failed", "Value must be positive"));

    // Assert
    Assert.IsType<Failure<int>>(ensured);
    var failure = (Failure<int>)ensured;
    Assert.Equal("validation.failed", failure.Error.Code);
    Assert.Equal("Value must be positive", failure.Error.Message);
  }

  [Fact]
  public void Ensure_ShouldNotEvaluatePredicateOnFailure()
  {
    // Arrange
    var error = new TestError("test.error", "Test error");
    var result = Result.Failure<int>(error);
    var predicateCalled = false;

    // Act
    var ensured = result.Ensure(
      x => { predicateCalled = true; return true; },
      () => new ValidationError("validation.failed"));

    // Assert
    Assert.False(predicateCalled);
    Assert.Same(result, ensured);
  }

  [Fact]
  public void Ensure_ShouldNotEvaluateErrorFactoryWhenPredicateTrue()
  {
    // Arrange
    var result = Result.Success(42);
    var errorFactoryCalled = false;

    // Act
    var ensured = result.Ensure(
      x => x > 0,
      () => { errorFactoryCalled = true; return new ValidationError("validation.failed"); });

    // Assert
    Assert.False(errorFactoryCalled);
    Assert.Same(result, ensured);
  }

  [Fact]
  public void Ensure_WithNullPredicate_ShouldThrowArgumentNullException()
  {
    // Arrange
    var result = Result.Success(42);

    // Act & Assert
    Assert.Throws<ArgumentNullException>(() =>
      result.Ensure(null!, () => new ValidationError("error")));
  }

  [Fact]
  public void Ensure_WithNullErrorFactory_ShouldThrowArgumentNullException()
  {
    // Arrange
    var result = Result.Success(42);

    // Act & Assert
    Assert.Throws<ArgumentNullException>(() =>
      result.Ensure(x => true, null!));
  }

  [Fact]
  public void Ensure_CanChainMultipleValidations()
  {
    // Arrange
    var result = Result.Success(42);

    // Act
    var validated = result
      .Ensure(x => x > 0, () => new ValidationError("must.be.positive"))
      .Ensure(x => x < 100, () => new ValidationError("must.be.less.than.100"))
      .Ensure(x => x % 2 == 0, () => new ValidationError("must.be.even"));

    // Assert
    Assert.IsType<Success<int>>(validated);
    Assert.Equal(42, ((Success<int>)validated).Value);
  }

  [Fact]
  public void Ensure_ChainStopsAtFirstFailure()
  {
    // Arrange
    var result = Result.Success(42);
    var secondPredicateCalled = false;

    // Act
    var validated = result
      .Ensure(x => x < 10, () => new ValidationError("must.be.less.than.10"))
      .Ensure(x => { secondPredicateCalled = true; return true; },
              () => new ValidationError("should.not.reach"));

    // Assert
    Assert.IsType<Failure<int>>(validated);
    Assert.False(secondPredicateCalled);
    Assert.Equal("must.be.less.than.10", ((Failure<int>)validated).Error.Code);
  }

  #endregion

  #region Tap Tests

  [Fact]
  public void Tap_OnSuccess_ShouldExecuteActionAndReturnUnchanged()
  {
    // Arrange
    var result = Result.Success(42);
    var capturedValue = 0;

    // Act
    var tapped = result.Tap(x => capturedValue = x);

    // Assert
    Assert.Same(result, tapped); // Should return the same instance
    Assert.Equal(42, capturedValue);
  }

  [Fact]
  public void Tap_OnFailure_ShouldNotExecuteActionAndReturnUnchanged()
  {
    // Arrange
    var error = new TestError("test.error", "Test error");
    var result = Result.Failure<int>(error);
    var actionCalled = false;

    // Act
    var tapped = result.Tap(x => actionCalled = true);

    // Assert
    Assert.Same(result, tapped); // Should return the same instance
    Assert.False(actionCalled);
  }

  [Fact]
  public void Tap_WithNullAction_ShouldThrowArgumentNullException()
  {
    // Arrange
    var result = Result.Success(42);

    // Act & Assert
    Assert.Throws<ArgumentNullException>(() => result.Tap(null!));
  }

  [Fact]
  public void Tap_ShouldNotSwallowExceptions()
  {
    // Arrange
    var result = Result.Success(42);

    // Act & Assert
    var exception = Assert.Throws<InvalidOperationException>(() =>
      result.Tap(x => throw new InvalidOperationException("Test exception")));
    Assert.Equal("Test exception", exception.Message);
  }

  [Fact]
  public void Tap_CanBeChainedWithBind()
  {
    // Arrange
    var result = Result.Success(10);
    var log = new List<string>();

    // Act
    var output = result
      .Tap(x => log.Add($"Initial value: {x}"))
      .Bind(x => Result.Success(x * 2))
      .Tap(x => log.Add($"After doubling: {x}"))
      .Bind(x => Result.Success(x + 5))
      .Tap(x => log.Add($"After adding 5: {x}"));

    // Assert
    Assert.IsType<Success<int>>(output);
    Assert.Equal(25, ((Success<int>)output).Value);
    Assert.Equal(3, log.Count);
    Assert.Equal("Initial value: 10", log[0]);
    Assert.Equal("After doubling: 20", log[1]);
    Assert.Equal("After adding 5: 25", log[2]);
  }

  #endregion

  #region Integration Tests

  [Fact]
  public void AllExtensions_CanWorkTogether()
  {
    // Arrange
    var log = new List<string>();

    // Act
    var result = Result.Success(42)
      .Tap(x => log.Add($"Starting with: {x}"))
      .Ensure(x => x > 0, () => new ValidationError("must.be.positive"))
      .Tap(x => log.Add($"Validated: {x}"))
      .Bind(x => Result.Success(x * 2))
      .Tap(x => log.Add($"Doubled: {x}"))
      .MapError(e => new TestError("enriched." + e.Code, $"Enriched: {e.Message}"));

    // Assert
    Assert.True(result.IsSuccess);
    Assert.True(result.TryGetValue(out var value));
    Assert.Equal(84, value);
    Assert.Equal(3, log.Count);
  }

  [Fact]
  public void AllExtensions_WithFailure_ShouldStopChain()
  {
    // Arrange
    var log = new List<string>();

    // Act
    var result = Result.Success(-5)
      .Tap(x => log.Add($"Starting with: {x}"))
      .Ensure(x => x > 0, () => new ValidationError("must.be.positive", "Value must be positive"))
      .Tap(x => log.Add("Should not execute"))
      .Bind(x => Result.Success(x * 2))
      .MapError(e => new TestError("enriched." + e.Code, $"Enriched: {e.Message}"));

    // Assert
    Assert.True(result.IsFailure);
    Assert.False(result.TryGetValue(out _));
    Assert.Single(log);
    Assert.Equal("Starting with: -5", log[0]);
    Assert.IsType<Failure<int>>(result);
    var failure = (Failure<int>)result;
    Assert.Equal("enriched.must.be.positive", failure.Error.Code);
    Assert.Equal("Enriched: Value must be positive", failure.Error.Message);
  }

  #endregion
}
