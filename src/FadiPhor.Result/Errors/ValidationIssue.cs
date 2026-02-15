namespace FadiPhor.Result;

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
