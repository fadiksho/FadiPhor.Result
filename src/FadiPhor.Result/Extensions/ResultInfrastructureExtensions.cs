namespace FadiPhor.Result;

/// <summary>
/// Provides non-generic extension methods on <see cref="Result"/> for use in middleware, 
/// pipeline behaviors, and other cross-cutting infrastructure.
/// </summary>
/// <remarks>
/// These extensions operate on the non-generic <see cref="Result"/> base type and are intended
/// strictly for infrastructure concerns: logging, error normalization, and pipeline inspection.
/// They do not expose the success value and do not weaken the union model.
/// </remarks>
public static class ResultInfrastructureExtensions
{
  /// <summary>
  /// Attempts to retrieve the error from a result without requiring knowledge of the generic success type.
  /// </summary>
  /// <param name="result">The result to inspect.</param>
  /// <param name="error">
  /// When this method returns, contains the error if the result is a failure;
  /// otherwise, <c>null</c>.
  /// </param>
  /// <returns>
  /// <c>true</c> if <paramref name="result"/> is a failure and <paramref name="error"/> was assigned;
  /// otherwise, <c>false</c>.
  /// </returns>
  /// <remarks>
  /// <para>Designed for infrastructure that receives an untyped <see cref="Result"/> reference and needs
  /// to inspect the error without knowing the generic payload type.</para>
  /// <para><strong>Example Usage:</strong></para>
  /// <code>
  /// if (response is Result result &amp;&amp;
  ///     result.TryGetError(out var error))
  /// {
  ///     logger.LogWarning("Failure: {Code}", error.Code);
  /// }
  /// </code>
  /// </remarks>
  public static bool TryGetError(
    this Result result,
    out Error error)
  {
    var e = result.GetErrorInternal();

    if (e is not null)
    {
      error = e;
      return true;
    }

    error = null!;
    return false;
  }

  /// <summary>
  /// Transforms the error of a failed result without requiring knowledge of the generic success type.
  /// Success results are returned unchanged. The concrete generic result type is preserved.
  /// </summary>
  /// <param name="result">The result whose error may be transformed.</param>
  /// <param name="map">The function to transform the error. Must not be null.</param>
  /// <returns>
  /// If <paramref name="result"/> is a success, returns it unchanged.
  /// If <paramref name="result"/> is a failure, returns a new failure with the transformed error, 
  /// preserving the original generic result type.
  /// </returns>
  /// <exception cref="ArgumentNullException">Thrown if <paramref name="map"/> is null.</exception>
  /// <exception cref="InvalidOperationException">Thrown if the result is in an unexpected state (should never occur).</exception>
  /// <remarks>
  /// <para>Designed for middleware that needs to wrap or normalize errors globally without inspecting 
  /// the generic success type.</para>
  /// <para><strong>Example Usage:</strong></para>
  /// <code>
  /// if (response is Result result)
  /// {
  ///     response = result.MapError(error =>
  ///         new WrappedError($"wrapped.{error.Code}"));
  /// }
  /// </code>
  /// </remarks>
  public static Result MapError(
    this Result result,
    Func<Error, Error> map)
  {
    ArgumentNullException.ThrowIfNull(map);

    return result.MapErrorInternal(map);
  }
}
