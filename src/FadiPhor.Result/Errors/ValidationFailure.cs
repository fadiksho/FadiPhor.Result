namespace FadiPhor.Result;

/// <summary>
/// Represents a validation failure error containing one or more validation issues.
/// </summary>
/// <param name="Issues">The collection of validation issues that caused the failure.</param>
/// <remarks>
/// Always uses error code "validation.failed". Implicitly converts to Result&lt;T&gt;.
/// </remarks>
public sealed record ValidationFailure(
  IReadOnlyCollection<ValidationIssue> Issues)
  : Error("validation.failed")
{
  /// <summary>
  /// Gets the read-only collection of validation issues.
  /// </summary>
  /// <exception cref="ArgumentNullException">Thrown if Issues is null during construction.</exception>
  public IReadOnlyCollection<ValidationIssue> Issues { get; } = Issues ?? throw new ArgumentNullException(nameof(Issues));

  /// <summary>
  /// Gets a default diagnostic message indicating validation failure.
  /// </summary>
  public override string? Message => "Validation failed.";
}
