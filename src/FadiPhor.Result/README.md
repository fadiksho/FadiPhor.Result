# FadiPhor.Result

`Result<T>` is an abstract record with exactly two sealed subtypes:

- `Success<T>` — holds a non-null value of type `T`.
- `Failure<T>` — holds an `Error`.

All types are records (immutable). `T` is constrained to `notnull`.

---

## Error Model

`Error` is an abstract record with a required `Code` (string) and an optional `Message`:

```csharp
public abstract record Error(string Code)
{
    public virtual string? Message => null;
}
```

Define domain errors by inheriting from `Error`:

```csharp
public record NotFoundError(string EntityId) : Error("not_found")
{
    public override string? Message => $"{EntityId} not found";
}
```

### ValidationFailure

A built-in `Error` subtype for returning multiple validation issues:

```csharp
var issues = new List<ValidationIssue>
{
    new("Email", "Email is required"),
    new("Age", "Must be 18 or older"),
    new("Name", "Name is unusually short", ValidationSeverity.Warning)
};

return new ValidationFailure(issues);
```

`ValidationFailure` always has `Code = "validation.failed"`. Each `ValidationIssue` carries an `Identifier`, `Message`, and `Severity` (defaults to `ValidationSeverity.Error`).

`ValidationSeverity` values: `Error`, `Warning`, `Info`.

### Unit

Use `Result<Unit>` for operations that succeed or fail without producing a value:

```csharp
public Result<Unit> DeleteUser(int id)
{
    if (!repository.Exists(id))
        return new NotFoundError($"user/{id}");

    repository.Delete(id);
    return Unit.Value; // implicit conversion to Success<Unit>
}
```

---

## Creating Results

### Implicit Conversions

`Result<T>` defines implicit operators from `T` → `Success<T>` and `Error` → `Failure<T>`. This means you can return values or errors directly when the return type is `Result<T>`:

```csharp
public Result<User> GetUser(int id)
{
    var user = repository.Find(id);
    if (user is null)
        return new NotFoundError($"user/{id}"); // implicit → Failure<User>

    return user; // implicit → Success<User>
}
```

Both conversions throw `ArgumentNullException` if given `null`.

### Factory Methods

When implicit conversion is not applicable (e.g. in generic contexts):

```csharp
var success = Result.Success(42);
var failure = Result.Failure<int>(new NotFoundError("item/7"));
```

---

## Composition

### Bind

Chains operations that return `Result<T>`. On failure, the chain short-circuits and propagates the error:

```csharp
var result = GetOrder(orderId)
    .Bind(order => ValidateOrder(order))
    .Bind(order => ChargePayment(order))
    .Bind(order => CreateShipment(order));
```

Async variant — operates on `Task<Result<T>>`:

```csharp
var result = await GetOrderAsync(orderId)
    .Bind(order => ValidateOrderAsync(order))
    .Bind(order => ChargePaymentAsync(order));
```

### Ensure

Validates a success value against a predicate. Returns the original result if the predicate passes, or converts it to a failure:

```csharp
var result = GetUser(id)
    .Ensure(
        user => user.IsActive,
        () => new Error("user.inactive"));
```

If the result is already a failure, `Ensure` passes it through unchanged.

### MapError

Transforms the error in a failure without affecting successes:

```csharp
var result = GetUser(id)
    .MapError(error => new WrappedError(error.Code));
```

### Tap

Executes a side effect on a success value without changing the result. Failures pass through unchanged:

```csharp
var result = GetUser(id)
    .Tap(user => logger.LogInformation("Found user {Id}", user.Id))
    .Bind(user => MapToDto(user));
```

### TryGetValue

Extracts the success value using a try-pattern:

```csharp
if (result.TryGetValue(out var user))
{
    // use user
}
```

### Match

Exhaustively handles both cases and produces a value:

```csharp
return result.Match(
    onSuccess: user => Ok(user),
    onFailure: error => error switch
    {
        NotFoundError => NotFound(),
        ValidationFailure vf => BadRequest(new { vf.Issues }),
        _ => Problem(detail: error.Code)
    }
);
```

---

## Realistic Chain Example

```csharp
public async Task<Result<OrderConfirmation>> PlaceOrder(PlaceOrderRequest request)
{
    return await ValidateRequest(request)
        .Bind(req => GetCustomerAsync(req.CustomerId))
        .Bind(customer => CreateOrderAsync(customer, request.Items))
        .Bind(order => ChargePaymentAsync(order))
        .Bind(order => Task.FromResult(
            Result.Success(new OrderConfirmation(order.Id, order.Total))));
}
```

If `ValidateRequest` returns a `ValidationFailure`, none of the subsequent steps execute. If `ChargePaymentAsync` fails, the error propagates and `OrderConfirmation` is never created.

---

## Union Structure

`Result<T>` is a binary union. `Success<T>` and `Failure<T>` are both sealed. The `IsSuccess` and `IsFailure` properties are available but `Match` / `Bind` are the primary consumption patterns.

Introducing additional union states (e.g. `PartialSuccess<T>`) would require:

- A new sealed subtype of `Result<T>`.
- Updating every `switch` expression over `Result<T>` (the `_ => throw` arm in `Match`, `Bind`, `Ensure`, `MapError`, `Tap`).
- Updating the JSON converter if serialization is used.

This is by design — the sealed structure makes the union exhaustive and any extension is a deliberate, auditable change.