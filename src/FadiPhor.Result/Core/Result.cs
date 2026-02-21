namespace FadiPhor.Result;

/// <summary>
/// Base abstraction for all result types, providing a common interface for middleware and infrastructure.
/// </summary>
/// <remarks>
/// This non-generic base type enables cross-cutting concerns to operate on results without
/// knowing their generic payload type. It does not introduce new state or expose Error/Value properties.
/// Use for middleware, logging, and pipeline behaviors that need to inspect result status uniformly.
/// </remarks>
public abstract record Result
{
  /// <summary>
  /// Gets a value indicating whether this result represents a successful outcome.
  /// </summary>
  /// <value>
  /// <c>true</c> if this is a success result; otherwise, <c>false</c>.
  /// </value>
  public abstract bool IsSuccess { get; }

  /// <summary>
  /// Gets a value indicating whether this result represents a failed outcome.
  /// </summary>
  /// <value>
  /// <c>true</c> if this is a failure result; otherwise, <c>false</c>.
  /// </value>
  public bool IsFailure => !IsSuccess;

  /// <summary>
  /// Returns the error contained in this result, or <c>null</c> if this is a success.
  /// </summary>
  internal abstract Error? GetErrorInternal();

  /// <summary>
  /// Returns a new result with the error transformed by <paramref name="map"/>, preserving the concrete
  /// generic type. Returns the same instance unchanged when this result is a success.
  /// </summary>
  internal abstract Result MapErrorInternal(Func<Error, Error> map);
}

/// <summary>
/// Represents a union-based result type that models either a successful outcome with a value 
/// or a failure with an error.
/// </summary>
/// <typeparam name="T">The type of the success value. Must be non-nullable.</typeparam>
/// <remarks>
/// This type enforces safe error handling through structural guarantees. Only Success or Failure states exist.
/// Use for operations that can fail with typed errors, MediatR handlers, or API boundaries requiring serializable errors.
/// </remarks>
public abstract record Result<T> : Result
  where T : notnull
{
  /// <summary>
  /// Gets a value indicating whether this result represents a successful outcome.
  /// </summary>
  /// <value>
  /// <c>true</c> if this is a <see cref="Success{T}"/>; otherwise, <c>false</c>.
  /// </value>
  public override bool IsSuccess => this is Success<T>;

  internal override Error? GetErrorInternal() =>
    this is Failure<T> f ? f.Error : null;

  internal override Result MapErrorInternal(Func<Error, Error> map) => this switch
  {
    Success<T> => this,
    Failure<T> f => new Failure<T>(map(f.Error)),
    _ => throw new InvalidOperationException()
  };

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
