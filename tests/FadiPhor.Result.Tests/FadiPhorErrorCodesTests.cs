namespace FadiPhor.Result.Tests;

public class FadiPhorErrorCodesTests
{
  [Fact]
  public void ValidationFailed_ShouldHaveExpectedValue()
  {
    Assert.Equal("validation.failed", FadiPhorErrorCodes.ValidationFailed);
  }

  [Fact]
  public void NotFound_ShouldHaveExpectedValue()
  {
    Assert.Equal("not_found", FadiPhorErrorCodes.NotFound);
  }

  [Fact]
  public void Unauthenticated_ShouldHaveExpectedValue()
  {
    Assert.Equal("unauthenticated", FadiPhorErrorCodes.Unauthenticated);
  }

  [Fact]
  public void Unauthorized_ShouldHaveExpectedValue()
  {
    Assert.Equal("unauthorized", FadiPhorErrorCodes.Unauthorized);
  }

  [Fact]
  public void Conflict_ShouldHaveExpectedValue()
  {
    Assert.Equal("conflict", FadiPhorErrorCodes.Conflict);
  }

  [Fact]
  public void Unexpected_ShouldHaveExpectedValue()
  {
    Assert.Equal("unexpected", FadiPhorErrorCodes.Unexpected);
  }

  [Fact]
  public void ValidationFailure_Code_ShouldMatchConstant()
  {
    var error = new ValidationFailure(new[] { new ValidationIssue("X", "msg") });
    Assert.Equal(FadiPhorErrorCodes.ValidationFailed, error.Code);
  }

  [Fact]
  public void NotFoundError_Code_ShouldMatchConstant()
  {
    Assert.Equal(FadiPhorErrorCodes.NotFound, new NotFoundError().Code);
  }

  [Fact]
  public void UnauthenticatedError_Code_ShouldMatchConstant()
  {
    Assert.Equal(FadiPhorErrorCodes.Unauthenticated, new UnauthenticatedError().Code);
  }

  [Fact]
  public void UnauthorizedError_Code_ShouldMatchConstant()
  {
    Assert.Equal(FadiPhorErrorCodes.Unauthorized, new UnauthorizedError().Code);
  }

  [Fact]
  public void ConflictError_Code_ShouldMatchConstant()
  {
    Assert.Equal(FadiPhorErrorCodes.Conflict, new ConflictError().Code);
  }

  [Fact]
  public void UnexpectedError_Code_ShouldMatchConstant()
  {
    Assert.Equal(FadiPhorErrorCodes.Unexpected, new UnexpectedError().Code);
  }
}
