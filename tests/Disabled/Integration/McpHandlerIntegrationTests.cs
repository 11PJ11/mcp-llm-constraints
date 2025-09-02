using System;
using System.Text.Json;
using System.Threading.Tasks;
using NUnit.Framework;
using ConstraintMcpServer.Presentation.Hosting;
using ConstraintMcpServer.Infrastructure.Communication;
using ConstraintMcpServer.Infrastructure.Configuration;

namespace ConstraintMcpServer.Tests.Integration;

/// <summary>
/// Integration tests for MCP handlers that test the actual handler logic in-process.
/// These tests provide code coverage for Presentation layer handlers.
/// </summary>
[TestFixture]
public sealed class McpHandlerIntegrationTests
{
    private ConstraintCommandRouter? _router;
    private TestResponseBuilder? _responseBuilder;
    private TestClientInfoExtractor? _clientInfoExtractor;
    private TestServerConfiguration? _serverConfiguration;

    [SetUp]
    public void SetUp()
    {
        _responseBuilder = new TestResponseBuilder();
        _clientInfoExtractor = new TestClientInfoExtractor();
        _serverConfiguration = new TestServerConfiguration();
        _router = new ConstraintCommandRouter(_responseBuilder, _clientInfoExtractor, _serverConfiguration);
    }

    [TearDown]
    public void TearDown()
    {
        _router = null;
        _responseBuilder = null;
        _clientInfoExtractor = null;
        _serverConfiguration = null;
    }

    [Test]
    public async Task Initialize_Request_Returns_ServerCapabilities()
    {
        // Arrange
        string initializeRequest = CreateInitializeRequest();

        // Act
        object result = await _router!.DispatchAsync(initializeRequest);

        // Assert
        Assert.That(result, Is.Not.Null);
        var response = result as dynamic;
        Assert.That(response, Is.Not.Null);

        // Verify it's a successful response (not an error)
        string json = JsonSerializer.Serialize(result);
        Assert.That(json, Does.Not.Contain("error"));
        Assert.That(json, Contains.Substring("result"));
    }

    [Test]
    public async Task Help_Request_Returns_ServerInfo()
    {
        // Arrange
        string helpRequest = CreateHelpRequest();

        // Act
        object result = await _router!.DispatchAsync(helpRequest);

        // Assert
        Assert.That(result, Is.Not.Null);
        string json = JsonSerializer.Serialize(result);
        Assert.That(json, Does.Not.Contain("error"));
        Assert.That(json, Contains.Substring("result"));
    }

    [Test]
    public async Task Shutdown_Request_Returns_SuccessfulShutdown()
    {
        // Arrange
        string shutdownRequest = CreateShutdownRequest();

        // Act
        object result = await _router!.DispatchAsync(shutdownRequest);

        // Assert
        Assert.That(result, Is.Not.Null);
        string json = JsonSerializer.Serialize(result);
        Assert.That(json, Does.Not.Contain("error"));
    }

    [Test]
    public async Task ToolCall_Request_Returns_ConstraintInjection()
    {
        // Arrange
        string toolCallRequest = CreateToolCallRequest();

        // Act
        object result = await _router!.DispatchAsync(toolCallRequest);

        // Assert
        Assert.That(result, Is.Not.Null);
        string json = JsonSerializer.Serialize(result);
        Assert.That(json, Does.Not.Contain("error"));
        Assert.That(json, Contains.Substring("result"));
    }

    [Test]
    public async Task Unknown_Method_Returns_MethodNotFoundError()
    {
        // Arrange
        string unknownRequest = CreateUnknownMethodRequest();

        // Act
        object result = await _router!.DispatchAsync(unknownRequest);

        // Assert
        Assert.That(result, Is.Not.Null);
        string json = JsonSerializer.Serialize(result);
        Assert.That(json, Contains.Substring("error"));
        Assert.That(json, Contains.Substring("Method not found"));
    }

    [Test]
    public async Task Malformed_Json_Returns_ParseError()
    {
        // Arrange
        string malformedJson = "{ invalid json }";

        // Act
        object result = await _router!.DispatchAsync(malformedJson);

        // Assert
        Assert.That(result, Is.Not.Null);
        string json = JsonSerializer.Serialize(result);
        Assert.That(json, Contains.Substring("error"));
        Assert.That(json, Contains.Substring("Parse error"));
    }

    [Test]
    public async Task Missing_Method_Returns_ParseError()
    {
        // Arrange
        string requestWithoutMethod = @"{
            ""id"": 1,
            ""params"": {}
        }";

        // Act
        object result = await _router!.DispatchAsync(requestWithoutMethod);

        // Assert
        Assert.That(result, Is.Not.Null);
        string json = JsonSerializer.Serialize(result);
        Assert.That(json, Contains.Substring("error"));
        Assert.That(json, Contains.Substring("Missing method property"));
    }

    #region Helper Methods

    private static string CreateInitializeRequest()
    {
        return @"{
            ""method"": ""initialize"",
            ""id"": 1,
            ""params"": {
                ""protocolVersion"": ""2024-11-05"",
                ""capabilities"": {},
                ""clientInfo"": {
                    ""name"": ""test-client"",
                    ""version"": ""1.0.0""
                }
            }
        }";
    }

    private static string CreateHelpRequest()
    {
        return @"{
            ""method"": ""server.help"",
            ""id"": 2,
            ""params"": {}
        }";
    }

    private static string CreateShutdownRequest()
    {
        return @"{
            ""method"": ""shutdown"",
            ""id"": 3,
            ""params"": {}
        }";
    }

    private static string CreateToolCallRequest()
    {
        return @"{
            ""method"": ""tools/call"",
            ""id"": 4,
            ""params"": {
                ""name"": ""test-tool"",
                ""arguments"": {}
            }
        }";
    }

    private static string CreateUnknownMethodRequest()
    {
        return @"{
            ""method"": ""unknown.method"",
            ""id"": 5,
            ""params"": {}
        }";
    }

    #endregion

    #region Test Doubles

    private sealed class TestResponseBuilder : IConstraintResponseBuilder
    {
        public object CreateSuccessResponse(int id, object result)
        {
            return new
            {
                id = id,
                result = result ?? new { status = "success" }
            };
        }

        public object CreateErrorResponse(int id, int errorCode, string message)
        {
            return new
            {
                id = id,
                error = new
                {
                    code = errorCode,
                    message = message
                }
            };
        }
    }

    private sealed class TestClientInfoExtractor : IClientInfoExtractor
    {
        public ClientInfo ExtractClientInfo(JsonElement initializeParams)
        {
            return new ClientInfo("test-client", "1.0.0");
        }
    }

    private sealed class TestServerConfiguration : IServerConfiguration
    {
        public string ProtocolVersion => "2024-11-05";
        public string ServerVersion => "1.0.0";
        public string ServerName => "Test Constraint MCP Server";
    }

    #endregion
}
