namespace FadiPhor.Result;

/// <summary>
/// Represents an error indicating that the caller lacks permission to perform the action (HTTP 403).
/// </summary>
/// <param name="Message">An optional diagnostic message. Defaults to a generic permission-denied message.</param>
/// <remarks>
/// Always uses error code "unauthorized". Implicitly converts to Result&lt;T&gt;.
/// </remarks>
public sealed record UnauthorizedError(string? Message = null)
  : Error(FadiPhorErrorCodes.Unauthorized)
{
  /// <summary>
  /// Gets the diagnostic message describing the authorization failure.
  /// </summary>
  public override string? Message { get; } = Message ?? "You do not have permission to perform this action.";
}
