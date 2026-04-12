namespace FadiPhor.Result;

/// <summary>
/// Represents an unexpected error that was not anticipated by the application logic.
/// </summary>
/// <param name="Message">An optional diagnostic message. Defaults to a generic unexpected-error message.</param>
/// <remarks>
/// Always uses error code "unexpected". Implicitly converts to Result&lt;T&gt;.
/// Typically used to wrap caught exceptions or other unhandled failure conditions.
/// </remarks>
public sealed record UnexpectedError(string? Message = null)
  : Error(FadiPhorErrorCodes.Unexpected)
{
  /// <summary>
  /// Gets the diagnostic message describing the unexpected error.
  /// </summary>
  public override string? Message { get; } = Message ?? "An unexpected error occurred.";
}
