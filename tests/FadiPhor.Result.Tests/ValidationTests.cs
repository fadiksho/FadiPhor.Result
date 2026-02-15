namespace FadiPhor.Result.Tests;

public class ValidationTests
{
  [Fact]
  public void ValidationIssue_ShouldCreateWithDefaultSeverity()
  {
    // Act
    var issue = new ValidationIssue("Email", "Email is required");

    // Assert
    Assert.Equal("Email", issue.Identifier);
    Assert.Equal("Email is required", issue.Message);
    Assert.Equal(ValidationSeverity.Error, issue.Severity);
  }

  [Fact]
  public void ValidationIssue_ShouldCreateWithCustomSeverity()
  {
    // Act
    var issue = new ValidationIssue("Age", "Age is below recommended", ValidationSeverity.Warning);

    // Assert
    Assert.Equal("Age", issue.Identifier);
    Assert.Equal("Age is below recommended", issue.Message);
    Assert.Equal(ValidationSeverity.Warning, issue.Severity);
  }

  [Fact]
  public void ValidationFailure_ShouldInheritFromError()
  {
    // Arrange
    var issues = new[] { new ValidationIssue("Email", "Email is required") };

    // Act
    var failure = new ValidationFailure(issues);

    // Assert
    Assert.IsAssignableFrom<Error>(failure);
  }

  [Fact]
  public void ValidationFailure_ShouldHaveFixedCodeAndMessage()
  {
    // Arrange
    var issues = new[] { new ValidationIssue("Email", "Email is required") };

    // Act
    var failure = new ValidationFailure(issues);

    // Assert
    Assert.Equal("validation.failed", failure.Code);
    Assert.Equal("Validation failed.", failure.Message);
  }

  [Fact]
  public void ValidationFailure_ShouldExposeIssuesAsReadOnly()
  {
    // Arrange
    var issuesList = new List<ValidationIssue>
    {
      new("Email", "Email is required"),
      new("Password", "Password too short")
    };

    // Act
    var failure = new ValidationFailure(issuesList);

    // Assert
    Assert.NotNull(failure.Issues);
    Assert.Equal(2, failure.Issues.Count);
    Assert.IsAssignableFrom<IReadOnlyCollection<ValidationIssue>>(failure.Issues);
  }

  [Fact]
  public void ValidationFailure_WithNullIssues_ShouldThrow()
  {
    // Act & Assert
    Assert.Throws<ArgumentNullException>(() => new ValidationFailure(null!));
  }

  [Fact]
  public void ValidationFailure_ShouldConvertImplicitlyToResult()
  {
    // Arrange
    var issues = new[] { new ValidationIssue("Email", "Email is required") };
    var failure = new ValidationFailure(issues);

    // Act
    Result<int> result = failure;

    // Assert
    Assert.IsType<Failure<int>>(result);
    var failureResult = (Failure<int>)result;
    Assert.IsType<ValidationFailure>(failureResult.Error);
  }

  [Fact]
  public void ValidationFailure_InResultMatch_ShouldWork()
  {
    // Arrange
    var issues = new[] { new ValidationIssue("Email", "Email is required") };
    var failure = new ValidationFailure(issues);
    Result<int> result = failure;

    // Act
    var output = result.Match(
      onSuccess: value => "Success",
      onFailure: error => $"Error: {error.Code}");

    // Assert
    Assert.Equal("Error: validation.failed", output);
  }

  [Fact]
  public void ValidationFailure_WithMultipleIssues_ShouldWorkCorrectly()
  {
    // Arrange
    var issues = new List<ValidationIssue>
    {
      new("Email", "Email is required", ValidationSeverity.Error),
      new("Password", "Password must be at least 8 characters", ValidationSeverity.Error),
      new("Age", "Age is below recommended", ValidationSeverity.Warning),
      new("Username", "Consider a more unique username", ValidationSeverity.Info)
    };

    // Act
    var failure = new ValidationFailure(issues);

    // Assert
    Assert.Equal(4, failure.Issues.Count);
    Assert.Equal("validation.failed", failure.Code);
    Assert.Equal("Validation failed.", failure.Message);
  }
}
