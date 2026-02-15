namespace FadiPhor.Result;

/// <summary>
/// Abstract base record for all error types used with <see cref="Result{T}"/>.
/// </summary>
/// <param name="Code">A machine-readable error code for categorization and handling.</param>
/// <remarks>
/// Custom errors must inherit from Error. The <see cref="Code"/> property is mandatory and stable for programmatic handling.
/// The <see cref="Message"/> property is optional and intended for diagnostic purposes only.
/// </remarks>
public abstract record Error(string Code)
{
  /// <summary>
  /// Gets an optional human-readable diagnostic message.
  /// Override in derived types to provide contextual diagnostic information.
  /// </summary>
  public virtual string? Message => null;
}
