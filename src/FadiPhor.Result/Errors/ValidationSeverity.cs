namespace FadiPhor.Result;

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
