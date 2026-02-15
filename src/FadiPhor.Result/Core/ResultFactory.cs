namespace FadiPhor.Result;

/// <summary>
/// Provides factory methods for creating <see cref="Result{T}"/> instances.
/// </summary>
/// <remarks>
/// <para>This static class offers a convenient API for creating Success and Failure results 
/// without requiring explicit type instantiation. Use these methods when implicit conversions 
/// are not applicable or when explicit intent improves code clarity.</para>
/// <para><strong>Example Usage:</strong></para>
/// <code>
/// // Explicit success
/// var result = Result.Success(42);
/// 
/// // Explicit failure
/// var error = new NotFoundError("user.not_found", "User not found");
/// var result = Result.Failure&lt;User&gt;(error);
/// </code>
/// </remarks>
public static class Result
{
  /// <summary>
  /// Creates a success result containing the specified value.
  /// </summary>
  /// <typeparam name="T">The type of the success value. Must be non-nullable.</typeparam>
  /// <param name="value">The value to wrap in a success result.</param>
  /// <returns>A <see cref="Success{T}"/> containing the specified value.</returns>
  /// <exception cref="ArgumentNullException">Thrown if value is null.</exception>
  public static Result<T> Success<T>(T value)
    where T : notnull
    => new Success<T>(value);

  /// <summary>
  /// Creates a failure result containing the specified error.
  /// </summary>
  /// <typeparam name="T">The type that would have been returned on success. Must be non-nullable.</typeparam>
  /// <param name="error">The error describing the failure.</param>
  /// <returns>A <see cref="Failure{T}"/> containing the specified error.</returns>
  /// <exception cref="ArgumentNullException">Thrown if error is null.</exception>
  public static Result<T> Failure<T>(Error error)
    where T : notnull
    => new Failure<T>(error);
}