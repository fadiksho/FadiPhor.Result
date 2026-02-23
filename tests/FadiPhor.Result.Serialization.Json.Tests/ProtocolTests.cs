using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using FadiPhor.Result.Serialization.Json.Configuration;
using FadiPhor.Result.Serialization.Json.Errors;
using FadiPhor.Result.Serialization.Json.Transport;
using Microsoft.Extensions.DependencyInjection;

namespace FadiPhor.Result.Serialization.Json.Tests;

// Consumer-provided marker interface (simulates MediatR IRequest<T>)
public interface ITestRequest<TResponse>;

public class ProtocolTests
{
  // Test request types implementing the consumer marker
  public record CreateUserRequest(string Name) : ITestRequest<Result<int>>;

  public record GetUserRequest(int Id) : ITestRequest<Result<string>>;

  // Non-request type (should NOT be registered)
  public record NotARequest(string Value);

  // Test error type and resolver for protocol integration
  private record ProtocolTestError(string Code) : Error(Code);

  public class ProtocolTestErrorResolver : IErrorPolymorphicResolver
  {
    public IEnumerable<JsonDerivedType> GetDerivedTypes()
    {
      yield return new JsonDerivedType(typeof(ProtocolTestError), nameof(ProtocolTestError));
    }
  }

  #region JsonRequestTypeRegistry Tests

  [Fact]
  public void Registry_ShouldResolveRegisteredTypes()
  {
    // Arrange
    var registry = new JsonRequestTypeRegistry(
      [typeof(CreateUserRequest).Assembly], typeof(ITestRequest<>));

    // Act
    var type = registry.GetRequestType(typeof(CreateUserRequest).FullName!);

    // Assert
    Assert.Equal(typeof(CreateUserRequest), type);
  }

  [Fact]
  public void Registry_ShouldThrowForUnknownType()
  {
    // Arrange
    var registry = new JsonRequestTypeRegistry(
      [typeof(CreateUserRequest).Assembly], typeof(ITestRequest<>));

    // Act & Assert
    var ex = Assert.Throws<InvalidOperationException>(
      () => registry.GetRequestType("NonExistentRequest"));
    Assert.Contains("Unknown request type", ex.Message);
  }

  [Fact]
  public void Registry_GetRequestTypeName_ShouldReturnFullName()
  {
    // Arrange
    var registry = new JsonRequestTypeRegistry(
      [typeof(CreateUserRequest).Assembly], typeof(ITestRequest<>));

    // Act
    var name = registry.GetRequestTypeName(typeof(CreateUserRequest));

    // Assert
    Assert.Equal(typeof(CreateUserRequest).FullName, name);
  }

  [Fact]
  public void Registry_GetRequestTypeName_UnregisteredType_ShouldThrow()
  {
    // Arrange
    var registry = new JsonRequestTypeRegistry(
      [typeof(CreateUserRequest).Assembly], typeof(ITestRequest<>));

    // Act & Assert
    Assert.Throws<InvalidOperationException>(
      () => registry.GetRequestTypeName(typeof(NotARequest)));
  }

  #endregion

  #region JsonEnvelopeSerializer Deserialize Tests

  [Fact]
  public void Deserialize_ShouldDeserializeValidEnvelope()
  {
    // Arrange
    var registry = new JsonRequestTypeRegistry(
      [typeof(CreateUserRequest).Assembly], typeof(ITestRequest<>));
    var options = new JsonSerializerOptions
    {
      PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };
    options.AddResultSerialization();

    var jsonOptions = new FadiPhorJsonOptions(options);
    var serializer = new JsonEnvelopeSerializer(registry, jsonOptions);
    var body = JsonSerializer.SerializeToElement(
      new CreateUserRequest("Alice"), options);
    var envelope = new JsonEnvelope
    {
      Type = typeof(CreateUserRequest).FullName!,
      Body = body
    };

    // Act
    var result = serializer.Deserialize(envelope);

    // Assert
    Assert.IsType<CreateUserRequest>(result);
    var request = (CreateUserRequest)result;
    Assert.Equal("Alice", request.Name);
  }

  [Fact]
  public void Deserialize_NullMessage_ShouldThrow()
  {
    // Arrange
    var registry = new JsonRequestTypeRegistry(
      [typeof(CreateUserRequest).Assembly], typeof(ITestRequest<>));
    var options = new FadiPhorJsonOptions(new JsonSerializerOptions());
    var serializer = new JsonEnvelopeSerializer(registry, options);

    // Act & Assert
    Assert.Throws<ArgumentNullException>(() => serializer.Deserialize(null!));
  }

  [Fact]
  public void Deserialize_EmptyType_ShouldThrow()
  {
    // Arrange
    var registry = new JsonRequestTypeRegistry(
      [typeof(CreateUserRequest).Assembly], typeof(ITestRequest<>));
    var options = new FadiPhorJsonOptions(new JsonSerializerOptions());
    var serializer = new JsonEnvelopeSerializer(registry, options);
    var envelope = new JsonEnvelope
    {
      Type = "",
      Body = JsonSerializer.SerializeToElement(new { })
    };

    // Act & Assert
    Assert.Throws<ArgumentException>(() => serializer.Deserialize(envelope));
  }

  [Fact]
  public void Deserialize_UnknownType_ShouldThrow()
  {
    // Arrange
    var registry = new JsonRequestTypeRegistry(
      [typeof(CreateUserRequest).Assembly], typeof(ITestRequest<>));
    var options = new FadiPhorJsonOptions(new JsonSerializerOptions());
    var serializer = new JsonEnvelopeSerializer(registry, options);
    var envelope = new JsonEnvelope
    {
      Type = "UnknownRequest",
      Body = JsonSerializer.SerializeToElement(new { })
    };

    // Act & Assert
    Assert.Throws<InvalidOperationException>(() => serializer.Deserialize(envelope));
  }

  [Fact]
  public void Deserialize_UndefinedBody_ShouldThrow()
  {
    // Arrange
    var registry = new JsonRequestTypeRegistry(
      [typeof(CreateUserRequest).Assembly], typeof(ITestRequest<>));
    var options = new FadiPhorJsonOptions(new JsonSerializerOptions());
    var serializer = new JsonEnvelopeSerializer(registry, options);
    var envelope = new JsonEnvelope
    {
      Type = typeof(CreateUserRequest).FullName!,
      Body = default
    };

    // Act & Assert
    Assert.Throws<ArgumentException>(() => serializer.Deserialize(envelope));
  }

  #endregion

  #region JsonEnvelopeSerializer Serialize Tests

  [Fact]
  public void Serialize_ShouldCreateValidEnvelope()
  {
    // Arrange
    var registry = new JsonRequestTypeRegistry(
      [typeof(CreateUserRequest).Assembly], typeof(ITestRequest<>));
    var options = new JsonSerializerOptions
    {
      PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };
    options.AddResultSerialization();

    var serializer = new JsonEnvelopeSerializer(registry, new FadiPhorJsonOptions(options));
    var request = new CreateUserRequest("Alice");

    // Act
    var envelope = serializer.Serialize(request);

    // Assert
    Assert.Equal(typeof(CreateUserRequest).FullName, envelope.Type);
    Assert.NotEqual(JsonValueKind.Undefined, envelope.Body.ValueKind);
    Assert.Equal("Alice", envelope.Body.GetProperty("name").GetString());
  }

  [Fact]
  public void Serialize_NullRequest_ShouldThrow()
  {
    // Arrange
    var registry = new JsonRequestTypeRegistry(
      [typeof(CreateUserRequest).Assembly], typeof(ITestRequest<>));
    var serializer = new JsonEnvelopeSerializer(
      registry, new FadiPhorJsonOptions(new JsonSerializerOptions()));

    // Act & Assert
    Assert.Throws<ArgumentNullException>(() => serializer.Serialize(null!));
  }

  [Fact]
  public void Serialize_UnregisteredType_ShouldThrow()
  {
    // Arrange
    var registry = new JsonRequestTypeRegistry(
      [typeof(CreateUserRequest).Assembly], typeof(ITestRequest<>));
    var serializer = new JsonEnvelopeSerializer(
      registry, new FadiPhorJsonOptions(new JsonSerializerOptions()));

    // Act & Assert
    Assert.Throws<InvalidOperationException>(
      () => serializer.Serialize(new NotARequest("test")));
  }

  [Fact]
  public void Serialize_Deserialize_RoundTrip()
  {
    // Arrange
    var registry = new JsonRequestTypeRegistry(
      [typeof(CreateUserRequest).Assembly], typeof(ITestRequest<>));
    var options = new JsonSerializerOptions
    {
      PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };
    options.AddResultSerialization();

    var serializer = new JsonEnvelopeSerializer(registry, new FadiPhorJsonOptions(options));
    var original = new CreateUserRequest("RoundTrip");

    // Act
    var envelope = serializer.Serialize(original);
    var restored = serializer.Deserialize(envelope);

    // Assert
    var request = Assert.IsType<CreateUserRequest>(restored);
    Assert.Equal("RoundTrip", request.Name);
  }

  #endregion

  #region AddFadiPhorResultProtocol Integration Tests

  [Fact]
  public void AddFadiPhorResultProtocol_RegistersAllServices()
  {
    // Arrange
    var services = new ServiceCollection();

    // Act
    services.AddFadiPhorResultProtocol(
      [typeof(CreateUserRequest).Assembly], typeof(ITestRequest<>));
    var provider = services.BuildServiceProvider();

    // Assert
    var jsonOptions = provider.GetRequiredService<FadiPhorJsonOptions>();
    Assert.NotNull(jsonOptions);
    Assert.Equal(JsonNamingPolicy.CamelCase, jsonOptions.SerializerOptions.PropertyNamingPolicy);

    var serializer = provider.GetRequiredService<IJsonEnvelopeSerializer>();
    Assert.NotNull(serializer);
  }

  [Fact]
  public void AddFadiPhorResultProtocol_OptionsSerializeResult()
  {
    // Arrange
    var services = new ServiceCollection();
    services.AddFadiPhorResultProtocol(
      [typeof(CreateUserRequest).Assembly], typeof(ITestRequest<>));
    var provider = services.BuildServiceProvider();

    var options = provider.GetRequiredService<FadiPhorJsonOptions>().SerializerOptions;
    var result = ResultFactory.Success(42);

    // Act
    var json = JsonSerializer.Serialize(result, options);
    var deserialized = JsonSerializer.Deserialize<Result<int>>(json, options);

    // Assert
    Assert.Contains("\"kind\":\"Success\"", json);
    Assert.Contains("\"value\":42", json);
    Assert.NotNull(deserialized);
    Assert.IsType<Success<int>>(deserialized);
  }

  [Fact]
  public void AddFadiPhorResultProtocol_ValidationFailureRegisteredAutomatically()
  {
    // Arrange
    var services = new ServiceCollection();
    services.AddFadiPhorResultProtocol(
      [typeof(CreateUserRequest).Assembly], typeof(ITestRequest<>));
    var provider = services.BuildServiceProvider();

    var options = provider.GetRequiredService<FadiPhorJsonOptions>().SerializerOptions;
    var failure = new ValidationFailure([new ValidationIssue("Email", "Required")]);
    Result<int> result = failure;

    // Act
    var json = JsonSerializer.Serialize(result, options);
    var deserialized = JsonSerializer.Deserialize<Result<int>>(json, options);

    // Assert
    Assert.Contains("\"$type\":\"ValidationFailure\"", json);
    Assert.NotNull(deserialized);
    var f = Assert.IsType<Failure<int>>(deserialized);
    Assert.IsType<ValidationFailure>(f.Error);
  }

  [Fact]
  public void AddFadiPhorResultProtocol_SerializerDeserializesEnvelope()
  {
    // Arrange
    var services = new ServiceCollection();
    services.AddFadiPhorResultProtocol(
      [typeof(CreateUserRequest).Assembly], typeof(ITestRequest<>));
    var provider = services.BuildServiceProvider();

    var options = provider.GetRequiredService<FadiPhorJsonOptions>().SerializerOptions;
    var serializer = provider.GetRequiredService<IJsonEnvelopeSerializer>();

    var body = JsonSerializer.SerializeToElement(
      new CreateUserRequest("Bob"), options);
    var envelope = new JsonEnvelope
    {
      Type = typeof(CreateUserRequest).FullName!,
      Body = body
    };

    // Act
    var result = serializer.Deserialize(envelope);

    // Assert
    var request = Assert.IsType<CreateUserRequest>(result);
    Assert.Equal("Bob", request.Name);
  }

  #endregion
}
