namespace FadiPhor.Result;

/// <summary>
/// Provides extension methods for composing and chaining <see cref="Result{T}"/> operations.
/// </summary>
public static class ResultCompositionExtensions
{
  /// <summary>
  /// Chains a result-producing operation to the current result. If the current result is a failure, 
  /// the chain stops and propagates the error. If successful, executes the next operation.
  /// </summary>
  /// <typeparam name="TIn">The type of the input result's success value.</typeparam>
  /// <typeparam name="TOut">The type of the output result's success value.</typeparam>
  /// <param name="result">The input result to bind.</param>
  /// <param name="next">The function to execute if the input is successful. Receives the success value and returns a new result.</param>
  /// <returns>
  /// If <paramref name="result"/> is a success, returns the result of <paramref name="next"/>.
  /// If <paramref name="result"/> is a failure, returns a failure with the same error (converted to <typeparamref name="TOut"/>).
  /// </returns>
  /// <exception cref="InvalidOperationException">Thrown if the result is neither Success nor Failure (should never occur).</exception>
  /// <remarks>
  /// <para>Bind enables railway-oriented programming where operations are chained and errors 
  /// short-circuit the chain automatically.</para>
  /// <para><strong>Example Usage:</strong></para>
  /// <code>
  /// var result = GetUserId(request)
  ///     .Bind(userId => GetUser(userId))
  ///     .Bind(user => ValidateUser(user))
  ///     .Bind(user => MapToDto(user));
  /// 
  /// // If any step fails, the chain stops and returns that failure
  /// // If all succeed, returns the final success value
  /// </code>
  /// </remarks>
  public static Result<TOut> Bind<TIn, TOut>(
    this Result<TIn> result,
    Func<TIn, Result<TOut>> next)
    where TIn : notnull
    where TOut : notnull
  {
    return result switch
    {
      Success<TIn> s => next(s.Value),
      Failure<TIn> f => new Failure<TOut>(f.Error),
      _ => throw new InvalidOperationException()
    };
  }

  /// <summary>
  /// Chains an asynchronous result-producing operation to the current asynchronous result. 
  /// If the current result is a failure, the chain stops and propagates the error. 
  /// If successful, executes the next asynchronous operation.
  /// </summary>
  /// <typeparam name="TIn">The type of the input result's success value.</typeparam>
  /// <typeparam name="TOut">The type of the output result's success value.</typeparam>
  /// <param name="task">The asynchronous task producing the input result to bind.</param>
  /// <param name="next">The asynchronous function to execute if the input is successful. Receives the success value and returns a task producing a new result.</param>
  /// <returns>
  /// If the awaited <paramref name="task"/> result is a success, returns the result of awaiting <paramref name="next"/>.
  /// If the awaited <paramref name="task"/> result is a failure, returns a failure with the same error (converted to <typeparamref name="TOut"/>).
  /// </returns>
  /// <exception cref="InvalidOperationException">Thrown if the result is neither Success nor Failure (should never occur).</exception>
  /// <remarks>
  /// <para>This asynchronous variant of Bind enables chaining async operations while maintaining 
  /// error propagation semantics.</para>
  /// <para><strong>Example Usage:</strong></para>
  /// <code>
  /// var result = await GetUserIdAsync(request)
  ///     .Bind(userId => GetUserAsync(userId))
  ///     .Bind(user => ValidateUserAsync(user))
  ///     .Bind(user => MapToDtoAsync(user));
  /// 
  /// // Each operation is awaited in sequence
  /// // Errors short-circuit the chain without executing remaining operations
  /// </code>
  /// </remarks>
  public static async Task<Result<TOut>> Bind<TIn, TOut>(
    this Task<Result<TIn>> task,
    Func<TIn, Task<Result<TOut>>> next)
    where TIn : notnull
    where TOut : notnull
  {
    var result = await task;

    return result switch
    {
      Success<TIn> s => await next(s.Value),
      Failure<TIn> f => ResultFactory.Failure<TOut>(f.Error),
      _ => throw new InvalidOperationException()
    };
  }

  /// <summary>
  /// Attempts to retrieve the success value from a result.
  /// </summary>
  /// <typeparam name="T">The type of the success value.</typeparam>
  /// <param name="result">The result to retrieve the value from.</param>
  /// <param name="value">
  /// When this method returns, contains the success value if the result is a success; 
  /// otherwise, the default value for the type.
  /// </param>
  /// <returns>
  /// <c>true</c> if <paramref name="result"/> is a <see cref="Success{T}"/> and <paramref name="value"/> was assigned; 
  /// otherwise, <c>false</c>.
  /// </returns>
  public static bool TryGetValue<T>(
    this Result<T> result,
    out T value)
    where T : notnull
  {
    if (result is Success<T> s)
    {
      value = s.Value;
      return true;
    }

    value = default!;
    return false;
  }

  /// <summary>
  /// Attempts to retrieve the error from a result.
  /// </summary>
  /// <typeparam name="T">The type of the success value.</typeparam>
  /// <param name="result">The result to retrieve the error from.</param>
  /// <param name="error">
  /// When this method returns, contains the error if the result is a failure;
  /// otherwise, <c>null</c>.
  /// </param>
  /// <returns>
  /// <c>true</c> if <paramref name="result"/> is a <see cref="Failure{T}"/> and <paramref name="error"/> was assigned;
  /// otherwise, <c>false</c>.
  /// </returns>
  public static bool TryGetError<T>(
    this Result<T> result,
    out Error error)
    where T : notnull
  {
    if (result is Failure<T> f)
    {
      error = f.Error;
      return true;
    }

    error = null!;
    return false;
  }

  /// <summary>
  /// Extracts the success value from a result, or throws if the result is a failure.
  /// </summary>
  /// <typeparam name="T">The type of the success value.</typeparam>
  /// <param name="result">The result to extract the value from.</param>
  /// <returns>The success value.</returns>
  /// <exception cref="InvalidOperationException">Thrown if the result is not a success.</exception>
  public static T GetValueOrThrow<T>(
    this Result<T> result)
    where T : notnull
  {
    return result switch
    {
      Success<T> s => s.Value,
      _ => throw new InvalidOperationException()
    };
  }

  /// <summary>
  /// Transforms the error of a failed result without affecting success results.
  /// </summary>
  /// <typeparam name="T">The type of the success value.</typeparam>
  /// <param name="result">The result whose error may be transformed.</param>
  /// <param name="map">The function to transform the error. Must not be null.</param>
  /// <returns>
  /// If <paramref name="result"/> is a success, returns it unchanged.
  /// If <paramref name="result"/> is a failure, returns a new failure with the transformed error.
  /// </returns>
  /// <exception cref="ArgumentNullException">Thrown if <paramref name="map"/> is null.</exception>
  public static Result<T> MapError<T>(
    this Result<T> result,
    Func<Error, Error> map)
    where T : notnull
  {
    ArgumentNullException.ThrowIfNull(map);

    return result switch
    {
      Success<T> => result,
      Failure<T> f => new Failure<T>(map(f.Error)),
      _ => throw new InvalidOperationException()
    };
  }

  /// <summary>
  /// Validates a successful result against a predicate. If the predicate fails, converts the success into a failure.
  /// </summary>
  /// <typeparam name="T">The type of the success value.</typeparam>
  /// <param name="result">The result to validate.</param>
  /// <param name="predicate">The validation function to apply to the success value. Must not be null.</param>
  /// <param name="errorFactory">The function to create an error if validation fails. Must not be null.</param>
  /// <returns>
  /// If <paramref name="result"/> is a failure, returns it unchanged.
  /// If <paramref name="result"/> is a success and <paramref name="predicate"/> returns <c>true</c>, returns the success unchanged.
  /// If <paramref name="result"/> is a success and <paramref name="predicate"/> returns <c>false</c>, returns a failure with the error from <paramref name="errorFactory"/>.
  /// </returns>
  /// <exception cref="ArgumentNullException">Thrown if <paramref name="predicate"/> or <paramref name="errorFactory"/> is null.</exception>
  public static Result<T> Ensure<T>(
    this Result<T> result,
    Func<T, bool> predicate,
    Func<Error> errorFactory)
    where T : notnull
  {
    ArgumentNullException.ThrowIfNull(predicate);
    ArgumentNullException.ThrowIfNull(errorFactory);

    return result switch
    {
      Failure<T> => result,
      Success<T> s when predicate(s.Value) => result,
      Success<T> => new Failure<T>(errorFactory()),
      _ => throw new InvalidOperationException()
    };
  }

  /// <summary>
  /// Executes a side-effect action on a successful result without modifying it.
  /// </summary>
  /// <typeparam name="T">The type of the success value.</typeparam>
  /// <param name="result">The result to tap into.</param>
  /// <param name="action">The action to execute on the success value. Must not be null.</param>
  /// <returns>The original <paramref name="result"/> unchanged.</returns>
  /// <exception cref="ArgumentNullException">Thrown if <paramref name="action"/> is null.</exception>
  /// <remarks>
  /// If <paramref name="result"/> is a failure, the action is not executed and the failure is returned unchanged.
  /// Exceptions thrown by <paramref name="action"/> are not caught and will propagate to the caller.
  /// </remarks>
  public static Result<T> Tap<T>(
    this Result<T> result,
    Action<T> action)
    where T : notnull
  {
    ArgumentNullException.ThrowIfNull(action);

    if (result is Success<T> s)
    {
      action(s.Value);
    }

    return result;
  }
}
