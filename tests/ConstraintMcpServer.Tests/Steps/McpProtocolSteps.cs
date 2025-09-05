using System;
using System.Text.Json;
using System.Threading.Tasks;

namespace ConstraintMcpServer.Tests.Steps;

/// <summary>
/// Level 3 Refactoring: Extract MCP protocol interaction logic.
/// Focused responsibility for JSON-RPC communication and protocol validation.
/// Follows CUPID properties: Composable, Unix Philosophy, Predictable, Idiomatic, Domain-based.
/// </summary>
internal sealed class McpProtocolSteps
{
    // JSON-RPC protocol constants (extracted from main class)
    private const string JSONRPC_VERSION = "2.0";
    private const string JSONRPC_PROPERTY = "jsonrpc";
    private const string RESULT_PROPERTY = "result";
    private const string ID_PROPERTY = "id";
    private const string CAPABILITIES_PROPERTY = "capabilities";
    private const string SERVER_INFO_PROPERTY = "serverInfo";
    private const string PRODUCT_PROPERTY = "product";
    private const string DESCRIPTION_PROPERTY = "description";
    private const string COMMANDS_PROPERTY = "commands";
    private const string NOTIFICATIONS_PROPERTY = "notifications";
    private const string CONSTRAINTS_PROPERTY = "constraints";
    private const string CONTEXT_ANALYSIS_PROPERTY = "context_analysis";
    private const string HAS_ACTIVATION_PROPERTY = "has_activation";
    private const string CONSTRAINT_COUNT_PROPERTY = "constraint_count";
    private const int MINIMUM_DESCRIPTION_LENGTH = 20;

    private JsonDocument? _lastJsonResponse;

    public void SetLastJsonResponse(JsonDocument? jsonResponse)
    {
        _lastJsonResponse = jsonResponse;
    }

    /// <summary>
    /// Business-focused step: Validate that response contains concise product description
    /// </summary>
    public void ValidateProductDescription()
    {
        if (_lastJsonResponse == null)
        {
            throw new InvalidOperationException("No response received from server");
        }

        JsonElement root = _lastJsonResponse.RootElement;
        JsonElement result = GetRequiredProperty(root, RESULT_PROPERTY);

        // Verify product information is present and meaningful
        string productName = GetRequiredStringProperty(result, PRODUCT_PROPERTY);

        // Verify description is present and meaningful
        string descriptionText = GetRequiredStringProperty(result, DESCRIPTION_PROPERTY);
        if (descriptionText.Length < MINIMUM_DESCRIPTION_LENGTH)
        {
            throw new InvalidOperationException("Description is too brief");
        }
    }

    /// <summary>
    /// Business-focused step: Validate that response contains main commands
    /// </summary>
    public void ValidateMainCommands()
    {
        if (_lastJsonResponse == null)
        {
            throw new InvalidOperationException("No response received from server");
        }

        JsonElement root = _lastJsonResponse.RootElement;
        JsonElement result = GetRequiredProperty(root, RESULT_PROPERTY);

        // Verify commands are present
        JsonElement commands = GetRequiredProperty(result, COMMANDS_PROPERTY);

        if (commands.GetArrayLength() == 0)
        {
            throw new InvalidOperationException("No commands found in response");
        }

        // Commands should be meaningful and contain essential operations
        bool hasEssentialCommands = false;
        foreach (JsonElement command in commands.EnumerateArray())
        {
            string? commandText = command.GetString();
            if (!string.IsNullOrWhiteSpace(commandText) &&
                (commandText.Contains("help", StringComparison.OrdinalIgnoreCase) ||
                 commandText.Contains("constraint", StringComparison.OrdinalIgnoreCase)))
            {
                hasEssentialCommands = true;
                break;
            }
        }

        if (!hasEssentialCommands)
        {
            throw new InvalidOperationException("Commands do not contain essential operations");
        }
    }

    /// <summary>
    /// Business-focused step: Validate MCP capabilities response structure
    /// </summary>
    public void ValidateCapabilitiesResponse()
    {
        if (_lastJsonResponse == null)
        {
            throw new InvalidOperationException("No response received from server");
        }

        JsonElement root = _lastJsonResponse.RootElement;
        JsonElement result = GetRequiredProperty(root, RESULT_PROPERTY);

        // Verify capabilities structure
        JsonElement capabilities = GetRequiredProperty(result, CAPABILITIES_PROPERTY);

        // Verify notifications capability (required for constraint system)
        JsonElement notifications = GetRequiredProperty(capabilities, NOTIFICATIONS_PROPERTY);

        // Verify constraint notifications capability
        if (!notifications.TryGetProperty(CONSTRAINTS_PROPERTY, out JsonElement constraintsNotif) ||
            !constraintsNotif.GetBoolean())
        {
            throw new InvalidOperationException("Server does not support constraint notifications");
        }

        // Verify server info is present
        JsonElement serverInfo = GetRequiredProperty(result, SERVER_INFO_PROPERTY);

        if (!serverInfo.TryGetProperty("name", out _) || !serverInfo.TryGetProperty("version", out _))
        {
            throw new InvalidOperationException("Server info missing name or version");
        }
    }

    /// <summary>
    /// Business-focused step: Validate shutdown confirmation
    /// </summary>
    public void ValidateShutdownConfirmation()
    {
        if (_lastJsonResponse == null)
        {
            throw new InvalidOperationException("No response received from server");
        }

        JsonElement root = _lastJsonResponse.RootElement;

        // Shutdown response should be simple acknowledgment
        if (!root.TryGetProperty(RESULT_PROPERTY, out JsonElement result) ||
            result.ValueKind != JsonValueKind.Object)
        {
            throw new InvalidOperationException("Invalid shutdown response format");
        }
    }

    /// <summary>
    /// Business-focused step: Validate JSON-RPC protocol compliance
    /// </summary>
    public void ValidateProtocolCompliance()
    {
        if (_lastJsonResponse == null)
        {
            throw new InvalidOperationException("No response received from server");
        }

        JsonElement root = _lastJsonResponse.RootElement;

        // Verify JSON-RPC version
        ValidateJsonRpcStructure(root);

        // Verify response ID (should match request ID = 2 for initialize)
        if (!root.TryGetProperty(ID_PROPERTY, out JsonElement id) || id.GetInt32() != 2)
        {
            throw new InvalidOperationException("Response ID does not match expected initialize request ID");
        }
    }

    /// <summary>
    /// Business-focused step: Validate constraint activation responses
    /// </summary>
    public void ValidateConstraintActivation(bool shouldHaveActivation)
    {
        if (_lastJsonResponse == null)
        {
            throw new InvalidOperationException("No response received from server");
        }

        JsonElement root = _lastJsonResponse.RootElement;
        JsonElement result = GetRequiredProperty(root, RESULT_PROPERTY);

        if (result.TryGetProperty(CONTEXT_ANALYSIS_PROPERTY, out JsonElement contextAnalysis))
        {
            if (contextAnalysis.TryGetProperty(HAS_ACTIVATION_PROPERTY, out JsonElement hasActivation))
            {
                bool actualActivation = hasActivation.GetBoolean();
                if (actualActivation != shouldHaveActivation)
                {
                    throw new InvalidOperationException(
                        $"Expected constraint activation: {shouldHaveActivation}, but got: {actualActivation}");
                }
            }
        }
        else if (shouldHaveActivation)
        {
            throw new InvalidOperationException("Expected constraint activation but none found in response");
        }
    }

    /// <summary>
    /// Business-focused step: Validate response contains constraint guidance
    /// </summary>
    public void ValidateConstraintGuidance(string expectedGuidanceType)
    {
        if (_lastJsonResponse == null)
        {
            throw new InvalidOperationException("No response received from server");
        }

        string responseText = _lastJsonResponse.RootElement.GetRawText();

        bool hasExpectedGuidance = expectedGuidanceType.ToLowerInvariant() switch
        {
            "tdd" => responseText.Contains("test") && (responseText.Contains("first") || responseText.Contains("tdd")),
            "clean" => responseText.Contains("clean") || responseText.Contains("refactor") || responseText.Contains("maintain"),
            "none" => !HasConstraintGuidanceMarkers(responseText),
            _ => throw new ArgumentException($"Unknown guidance type: {expectedGuidanceType}")
        };

        if (!hasExpectedGuidance)
        {
            throw new InvalidOperationException($"Response does not contain expected {expectedGuidanceType} guidance");
        }
    }

    // Level 1 Refactoring helpers (moved from main class)
    private static JsonElement GetRequiredProperty(JsonElement element, string propertyName)
    {
        if (!element.TryGetProperty(propertyName, out JsonElement property))
        {
            throw new InvalidOperationException($"Response does not contain a '{propertyName}' property");
        }
        return property;
    }

    private static string GetRequiredStringProperty(JsonElement element, string propertyName)
    {
        var property = GetRequiredProperty(element, propertyName);
        string? value = property.GetString();
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidOperationException($"{propertyName} is missing or empty");
        }
        return value;
    }

    private static void ValidateJsonRpcStructure(JsonElement root)
    {
        if (!root.TryGetProperty(JSONRPC_PROPERTY, out JsonElement jsonrpc) ||
            jsonrpc.GetString() != JSONRPC_VERSION)
        {
            throw new InvalidOperationException($"Invalid JSON-RPC structure or version");
        }
    }

    private static bool HasConstraintGuidanceMarkers(string responseText)
    {
        return responseText.Contains("constraint", StringComparison.OrdinalIgnoreCase) ||
               responseText.Contains("guidance", StringComparison.OrdinalIgnoreCase) ||
               responseText.Contains("reminder", StringComparison.OrdinalIgnoreCase);
    }
}
