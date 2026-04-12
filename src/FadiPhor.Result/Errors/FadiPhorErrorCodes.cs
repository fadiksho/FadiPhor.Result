namespace FadiPhor.Result;

/// <summary>
/// Contains the error codes used by all built-in <see cref="Error"/> types in the FadiPhor.Result library.
/// </summary>
/// <remarks>
/// Use these constants for programmatic error handling (e.g. matching on <see cref="Error.Code"/>)
/// without relying on magic strings.
/// </remarks>
public static class FadiPhorErrorCodes
{
  /// <summary>
  /// Error code for <see cref="ValidationFailure"/>.
  /// </summary>
  public const string ValidationFailed = "validation.failed";

  /// <summary>
  /// Error code for <see cref="NotFoundError"/>.
  /// </summary>
  public const string NotFound = "not_found";

  /// <summary>
  /// Error code for <see cref="UnauthenticatedError"/>.
  /// </summary>
  public const string Unauthenticated = "unauthenticated";

  /// <summary>
  /// Error code for <see cref="UnauthorizedError"/>.
  /// </summary>
  public const string Unauthorized = "unauthorized";

  /// <summary>
  /// Error code for <see cref="ConflictError"/>.
  /// </summary>
  public const string Conflict = "conflict";

  /// <summary>
  /// Error code for <see cref="UnexpectedError"/>.
  /// </summary>
  public const string Unexpected = "unexpected";
}
