namespace FadiPhor.Result;

/// <summary>
/// Represents a failed result containing an error.
/// </summary>
/// <typeparam name="T">The type that would have been returned on success. Must be non-nullable.</typeparam>
/// <remarks>
/// <para>This is a sealed record that represents the failure case of a <see cref="Result{T}"/> union. 
/// It guarantees that an error is present and provides typed error information.</para>
/// <para><strong>Structural Guarantees:</strong></para>
/// <list type="bullet">
/// <item>Error is never null (enforced by constructor validation)</item>
/// <item>Type is sealed to prevent inheritance and maintain union integrity</item>
/// <item>Immutable by design (record semantics)</item>
/// <item>Error can be any subtype of <see cref="Error"/> for polymorphic error modeling</item>
/// </list>
/// </remarks>
public sealed record Failure<T> : Result<T>
  where T : notnull
{
  /// <summary>
  /// Gets the error that caused the failure.
  /// </summary>
  /// <value>The non-null error describing why the operation failed.</value>
  public Error Error { get; }

  /// <summary>
  /// Initializes a new instance of the <see cref="Failure{T}"/> record with the specified error.
  /// </summary>
  /// <param name="error">The error describing the failure. Must not be null.</param>
  /// <exception cref="ArgumentNullException">Thrown if error is null.</exception>
  public Failure(Error error)
  {
    Error = error ?? throw new ArgumentNullException(nameof(error));
  }
}