namespace FadiPhor.Result;

/// <summary>
/// Represents a union-based result type that models either a successful outcome with a value 
/// or a failure with an error.
/// </summary>
/// <typeparam name="T">The type of the success value. Must be non-nullable.</typeparam>
/// <remarks>
/// This type enforces safe error handling through structural guarantees. Only Success or Failure states exist.
/// Use for operations that can fail with typed errors, MediatR handlers, or API boundaries requiring serializable errors.
/// </remarks>
public abstract record Result<T>
  where T : notnull
{
  /// <summary>
  /// Gets a value indicating whether this result represents a successful outcome.
  /// </summary>
  /// <value>
  /// <c>true</c> if this is a <see cref="Success{T}"/>; otherwise, <c>false</c>.
  /// </value>
  public bool IsSuccess => this is Success<T>;

  /// <summary>
  /// Gets a value indicating whether this result represents a failed outcome.
  /// </summary>
  /// <value>
  /// <c>true</c> if this is a <see cref="Failure{T}"/>; otherwise, <c>false</c>.
  /// </value>
  public bool IsFailure => this is Failure<T>;

  /// <summary>
  /// Implicitly converts a value to a Success result.
  /// </summary>
  /// <param name="value">The success value to wrap. Must not be null.</param>
  /// <returns>A <see cref="Success{T}"/> containing the value.</returns>
  /// <exception cref="ArgumentNullException">Thrown if value is null.</exception>
  public static implicit operator Result<T>(T value)
  {
    if (value is null)
      throw new ArgumentNullException(nameof(value));

    return new Success<T>(value);
  }

  /// <summary>
  /// Implicitly converts an error to a Failure result.
  /// </summary>
  /// <param name="error">The error to wrap. Must not be null.</param>
  /// <returns>A <see cref="Failure{T}"/> containing the error.</returns>
  /// <exception cref="ArgumentNullException">Thrown if error is null.</exception>
  public static implicit operator Result<T>(Error error)
  {
    ArgumentNullException.ThrowIfNull(error);

    return new Failure<T>(error);
  }
}
