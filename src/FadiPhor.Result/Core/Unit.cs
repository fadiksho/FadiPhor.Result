namespace FadiPhor.Result;

/// <summary>
/// Represents the absence of a meaningful value. Used as a return type for operations 
/// that perform side effects but have no natural result value.
/// </summary>
/// <remarks>
/// <para><strong>Purpose:</strong></para>
/// <para>Unit is used in place of void when a generic type parameter is required. 
/// It signals that an operation succeeds or fails without producing a meaningful value.</para>
/// <para><strong>When to Use:</strong></para>
/// <list type="bullet">
/// <item>Command handlers that perform updates/deletes (e.g., <c>Result&lt;Unit&gt;</c>)</item>
/// <item>Operations with side effects that don't return data</item>
/// <item>MediatR commands where only success/failure matters</item>
/// </list>
/// <para><strong>Example Usage:</strong></para>
/// <code>
/// public async Task&lt;Result&lt;Unit&gt;&gt; Handle(DeleteUserCommand request)
/// {
///     await repository.DeleteAsync(request.UserId);
///     return Result.Success(Unit.Value);
/// }
/// </code>
/// </remarks>
public readonly struct Unit
{
  /// <summary>
  /// Gets the singleton Unit value.
  /// </summary>
  /// <value>The sole instance of the Unit type.</value>
  /// <remarks>
  /// Since Unit carries no information, a single static instance is sufficient for all uses.
  /// </remarks>
  public static readonly Unit Value = new();
}