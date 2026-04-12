namespace FadiPhor.Result;

/// <summary>
/// Represents an error indicating that the caller is not authenticated (HTTP 401).
/// </summary>
/// <param name="Message">An optional diagnostic message. Defaults to a generic authentication-required message.</param>
/// <remarks>
/// Always uses error code "unauthenticated". Implicitly converts to Result&lt;T&gt;.
/// </remarks>
public sealed record UnauthenticatedError(string? Message = null)
  : Error(FadiPhorErrorCodes.Unauthenticated)
{
  /// <summary>
  /// Gets the diagnostic message describing the authentication failure.
  /// </summary>
  public override string? Message { get; } = Message ?? "Authentication is required.";
}
