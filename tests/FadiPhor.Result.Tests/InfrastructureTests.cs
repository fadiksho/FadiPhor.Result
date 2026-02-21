namespace FadiPhor.Result.Tests;

public class InfrastructureTests
{
  private record NotFoundError(string Id) : Error("not_found")
  {
    public override string? Message => $"{Id} not found";
  }

  [Fact]
  public void NonGenericResult_CanCheckIsSuccess()
  {
    // Arrange
    Result result = ResultFactory.Success(42);

    // Act & Assert
    Assert.True(result.IsSuccess);
    Assert.False(result.IsFailure);
  }

  [Fact]
  public void NonGenericResult_CanCheckIsFailure()
  {
    // Arrange
    Result result = ResultFactory.Failure<int>(new NotFoundError("user/123"));

    // Act & Assert
    Assert.False(result.IsSuccess);
    Assert.True(result.IsFailure);
  }

  [Fact]
  public void NonGenericResult_CanBeUsedInInfrastructurePattern()
  {
    // Arrange
    Result response = ResultFactory.Failure<string>(new NotFoundError("item/456"));
    bool loggedWarning = false;

    // Act - Simulating middleware/infrastructure code
    if (response is Result result && result.IsFailure)
    {
      loggedWarning = true; // logger.LogWarning("Operation failed.");
    }

    // Assert
    Assert.True(loggedWarning);
  }

  [Fact]
  public void NonGenericResult_SuccessDoesNotTriggerInfrastructurePattern()
  {
    // Arrange
    Result response = ResultFactory.Success("data");
    bool loggedWarning = false;

    // Act - Simulating middleware/infrastructure code
    if (response is Result result && result.IsFailure)
    {
      loggedWarning = true;
    }

    // Assert
    Assert.False(loggedWarning);
  }

  // TryGetError (non-generic)

  [Fact]
  public void TryGetError_OnFailure_ReturnsTrueAndAssignsError()
  {
    // Arrange
    var error = new NotFoundError("user/1");
    Result result = ResultFactory.Failure<int>(error);

    // Act
    var extracted = result.TryGetError(out var outError);

    // Assert
    Assert.True(extracted);
    Assert.Equal(error, outError);
  }

  [Fact]
  public void TryGetError_OnSuccess_ReturnsFalseAndAssignsNull()
  {
    // Arrange
    Result result = ResultFactory.Success(42);

    // Act
    var extracted = result.TryGetError(out var outError);

    // Assert
    Assert.False(extracted);
    Assert.Null(outError);
  }

  [Fact]
  public void TryGetError_UsedInInfrastructurePattern_LogsErrorCode()
  {
    // Arrange
    Result response = ResultFactory.Failure<string>(new NotFoundError("item/99"));
    string? loggedCode = null;

    // Act - Simulating middleware logging
    if (response.TryGetError(out var error))
    {
      loggedCode = error.Code;
    }

    // Assert
    Assert.Equal("not_found", loggedCode);
  }

  // MapError (non-generic)

  [Fact]
  public void MapError_OnFailure_ReturnsNewFailureWithMappedError()
  {
    // Arrange
    Result result = ResultFactory.Failure<int>(new NotFoundError("user/1"));

    // Act
    var mapped = result.MapError(e => new NotFoundError($"wrapped.{e.Code}"));

    // Assert
    Assert.True(mapped.IsFailure);
    mapped.TryGetError(out var mappedError);
    Assert.Equal("not_found", mappedError.Code);
    Assert.IsType<Failure<int>>(mapped);
  }

  [Fact]
  public void MapError_OnSuccess_ReturnsSameInstance()
  {
    // Arrange
    Result result = ResultFactory.Success(42);

    // Act
    var mapped = result.MapError(e => new NotFoundError("should.not.run"));

    // Assert
    Assert.True(mapped.IsSuccess);
    Assert.Same(result, mapped);
  }

  [Fact]
  public void MapError_PreservesGenericType()
  {
    // Arrange
    Result result = ResultFactory.Failure<string>(new NotFoundError("item/1"));

    // Act
    var mapped = result.MapError(e => new NotFoundError($"enriched.{e.Code}"));

    // Assert
    Assert.IsType<Failure<string>>(mapped);
  }

  [Fact]
  public void MapError_WithNullMap_Throws()
  {
    // Arrange
    Result result = ResultFactory.Failure<int>(new NotFoundError("x"));

    // Act & Assert
    Assert.Throws<ArgumentNullException>(() => result.MapError(null!));
  }

  [Fact]
  public void MapError_UsedInMiddlewarePattern_WrapsError()
  {
    // Arrange
    Result response = ResultFactory.Failure<string>(new NotFoundError("entity/5"));

    // Act - Simulating middleware error normalization
    response = response.MapError(e => new NotFoundError($"service.{e.Code}"));

    // Assert
    Assert.True(response.TryGetError(out var error));
    Assert.Equal("not_found", error.Code);
  }
}
