namespace FadiPhor.Result;

/// <summary>
/// Abstract base record for all error types used with <see cref="Result{T}"/>.
/// </summary>
/// <param name="Code">A machine-readable error code for categorization and handling.</param>
/// <remarks>
/// Custom errors must inherit from Error. The <see cref="Code"/> property is mandatory and stable for programmatic handling.
/// The <see cref="Message"/> property is optional and intended for diagnostic purposes only.
/// The <see cref="HttpStatusCode"/> property is mandatory and determines the HTTP status code
/// when this error is returned over HTTP.
/// </remarks>
public abstract record Error(string Code)
{
  /// <summary>
  /// Gets an optional human-readable diagnostic message.
  /// Override in derived types to provide contextual diagnostic information.
  /// </summary>
  public virtual string? Message => null;

  /// <summary>
  /// Gets the HTTP status code that this error maps to when returned over HTTP.
  /// </summary>
  /// <remarks>
  /// Every error type must declare its HTTP status code. This enables infrastructure
  /// (middleware, API endpoints) to set the correct HTTP response status without
  /// pattern matching on concrete error types.
  /// </remarks>
  public abstract int HttpStatusCode { get; }
}
