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
  : Error(FadiPhorErrorCodes.ValidationFailed)
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

/// <summary>
/// Represents a single validation issue with an identifier, message, and severity level.
/// </summary>
/// <param name="Identifier">The field or property identifier where the validation issue occurred.</param>
/// <param name="Message">A human-readable diagnostic message describing the validation issue.</param>
/// <param name="Severity">The severity level of the validation issue. Defaults to <see cref="ValidationSeverity.Error"/>.</param>
public sealed record ValidationIssue(
  string Identifier,
  string Message,
  ValidationSeverity Severity = ValidationSeverity.Error);

/// <summary>
/// Defines the severity level of a validation issue.
/// </summary>
/// <remarks>
/// <para>Validation severity indicates the impact level of a validation issue:</para>
/// <list type="bullet">
/// <item><see cref="Error"/> - Critical validation failure that prevents operation completion</item>
/// <item><see cref="Warning"/> - Non-critical issue that should be reviewed but doesn't block execution</item>
/// <item><see cref="Info"/> - Informational message for guidance or suggestions</item>
/// </list>
/// </remarks>
public enum ValidationSeverity
{
  /// <summary>
  /// Critical validation error that prevents operation completion.
  /// </summary>
  Error = 0,

  /// <summary>
  /// Warning that should be reviewed but doesn't block execution.
  /// </summary>
  Warning = 1,

  /// <summary>
  /// Informational message for guidance or suggestions.
  /// </summary>
  Info = 2
}