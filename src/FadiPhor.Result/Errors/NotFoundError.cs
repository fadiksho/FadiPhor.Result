namespace FadiPhor.Result;

/// <summary>
/// Represents an error indicating that the requested resource was not found.
/// </summary>
/// <param name="Message">An optional diagnostic message. Defaults to a generic not-found message.</param>
/// <remarks>
/// Always uses error code "not_found". Implicitly converts to Result&lt;T&gt;.
/// </remarks>
public sealed record NotFoundError(string? Message = null)
  : Error(FadiPhorErrorCodes.NotFound)
{
  /// <summary>
  /// Gets the diagnostic message describing the not-found condition.
  /// </summary>
  public override string? Message { get; } = Message ?? "The requested resource was not found.";
}
