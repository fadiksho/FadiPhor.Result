namespace FadiPhor.Result;

/// <summary>
/// Represents a successful result containing a non-null value.
/// </summary>
/// <typeparam name="T">The type of the success value. Must be non-nullable.</typeparam>
/// <remarks>
/// <para>This is a sealed record that represents the success case of a <see cref="Result{T}"/> union. 
/// It guarantees that a value is present and non-null.</para>
/// <para><strong>Structural Guarantees:</strong></para>
/// <list type="bullet">
/// <item>Value is never null (enforced by constructor validation)</item>
/// <item>Type is sealed to prevent inheritance and maintain union integrity</item>
/// <item>Immutable by design (record semantics)</item>
/// </list>
/// </remarks>
public sealed record Success<T> : Result<T>
  where T : notnull
{
  /// <summary>
  /// Gets the success value.
  /// </summary>
  /// <value>The non-null value contained in this success result.</value>
  public T Value { get; }

  /// <summary>
  /// Initializes a new instance of the <see cref="Success{T}"/> record with the specified value.
  /// </summary>
  /// <param name="value">The success value. Must not be null.</param>
  /// <exception cref="ArgumentNullException">Thrown if value is null.</exception>
  public Success(T value)
  {
    Value = value ?? throw new ArgumentNullException(nameof(value));
  }
}