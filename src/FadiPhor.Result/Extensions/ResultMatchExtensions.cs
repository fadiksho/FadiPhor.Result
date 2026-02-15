namespace FadiPhor.Result;

/// <summary>
/// Provides extension methods for pattern matching on <see cref="Result{T}"/> instances.
/// </summary>
public static class ResultMatchExtensions
{
  /// <summary>
  /// Matches the result and executes the corresponding function based on success or failure.
  /// </summary>
  /// <typeparam name="T">The type of the success value.</typeparam>
  /// <typeparam name="TResult">The type of the value produced by the match.</typeparam>
  /// <param name="result">The result to match against.</param>
  /// <param name="onSuccess">Function to execute if the result is a success. Receives the success value.</param>
  /// <param name="onFailure">Function to execute if the result is a failure. Receives the error.</param>
  /// <returns>The value produced by the executed function.</returns>
  /// <exception cref="InvalidOperationException">Thrown if the result is neither Success nor Failure (should never occur).</exception>
  /// <remarks>
  /// <para>Match provides exhaustive pattern matching for Result types, ensuring both cases are handled.</para>
  /// <para><strong>Example Usage:</strong></para>
  /// <code>
  /// var message = result.Match(
  ///     onSuccess: user => $"Hello, {user.Name}",
  ///     onFailure: error => $"Error: {error.Message}"
  /// );
  /// 
  /// // In ASP.NET Core controllers
  /// return result.Match(
  ///     onSuccess: user => Ok(user),
  ///     onFailure: error => Problem(detail: error.Code, title: error.Message)
  /// );
  /// </code>
  /// </remarks>
  public static TResult Match<T, TResult>(
    this Result<T> result,
    Func<T, TResult> onSuccess,
    Func<Error, TResult> onFailure)
    where T : notnull
  {
    return result switch
    {
      Success<T> s => onSuccess(s.Value),
      Failure<T> f => onFailure(f.Error),
      _ => throw new InvalidOperationException()
    };
  }
}
