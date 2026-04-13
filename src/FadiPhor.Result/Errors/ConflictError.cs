namespace FadiPhor.Result;

/// <summary>
/// Represents an error indicating that the request conflicts with the current state of the resource.
/// </summary>
/// <param name="Message">An optional diagnostic message. Defaults to a generic conflict message.</param>
/// <remarks>
/// Always uses error code "conflict". Implicitly converts to Result&lt;T&gt;.
/// </remarks>
public sealed record ConflictError(string? Message = null)
  : Error(FadiPhorErrorCodes.Conflict)
{
  /// <summary>
  /// Gets the diagnostic message describing the conflict.
  /// </summary>
  public override string? Message { get; } = Message ?? "The request conflicts with the current state of the resource.";

  /// <inheritdoc />
  public override int HttpStatusCode => 409;
}
