# Refactoring Implementation Checklists - Level 1-6

## Overview

Detailed implementation checklists for systematic Level 1-6 refactoring integrated with the 10-day Mikado parallel change implementation sequence. Each checklist provides specific baby steps with maximum 10-line change limits.

## Level 1: 🟨 Foundation Refactoring (Days 1-2)

### Day 1: Constant Extraction and Naming

#### Step 1-2: Extract ServerConstants Class
**Time**: 15 minutes | **Lines Changed**: <10 per step

```csharp
// Step 1: Create ServerConstants class
// File: src/ConstraintMcpServer/Domain/Constants/ServerConstants.cs
public static class ServerConstants
{
    public const string Name = "ConstraintMcpServer";
    public const string Version = "0.1.0";
    public const string Description = "Constraint Enforcement MCP Server - keeps LLM coding agents aligned during code generation with composable software-craft constraints";
}

// Step 2: Update first usage in Program.cs
- const string ServerName = "ConstraintMcpServer"; // Remove
+ using ConstraintMcpServer.Domain.Constants; // Add
+ // Usage: ServerConstants.Name instead of ServerName
```

#### Step 3-4: Extract JsonRpcConstants Class
**Time**: 15 minutes | **Lines Changed**: <10 per step

```csharp
// Step 3: Create JsonRpcConstants class
// File: src/ConstraintMcpServer/Infrastructure/Communication/JsonRpcConstants.cs
public static class JsonRpcConstants
{
    public const string Version = "2.0";
    public const string ContentLengthHeader = "Content-Length:";
    public const int DefaultRequestId = 1;
    public const string UnknownMethod = "unknown";
}

// Step 4: Update Program.cs constants
- const string JsonRpcVersion = "2.0"; // Remove
- const string ContentLengthHeader = "Content-Length:"; // Remove
+ using ConstraintMcpServer.Infrastructure.Communication; // Add
```

#### Step 5-6: Extract CommandLineConstants Class
**Time**: 15 minutes | **Lines Changed**: <10 per step

```csharp
// Step 5: Create CommandLineConstants class
// File: src/ConstraintMcpServer/Presentation/CommandLine/CommandLineConstants.cs
public static class CommandLineConstants
{
    public const string HelpShortFlag = "-h";
    public const string HelpLongFlag = "--help";
    public const string VersionShortFlag = "-v";
    public const string VersionLongFlag = "--version";
    public const int ShutdownDelayMs = 500;
}

// Step 6: Update Program.cs command line handling
- const string HelpShortFlag = "-h"; // Remove
- const string HelpLongFlag = "--help"; // Remove
+ using ConstraintMcpServer.Presentation.CommandLine; // Add
```

#### Validation Checkpoint - Day 1
- [ ] All tests pass after each step
- [ ] Build succeeds with zero warnings
- [ ] Constants properly organized by domain concern
- [ ] Using directives correctly added

### Day 2: Domain-Driven Naming and Comment Cleanup

#### Step 1-2: Apply Domain-Driven Method Naming
**Time**: 20 minutes | **Lines Changed**: <10 per step

```csharp
// Step 1: Rename static methods with domain intent
- static async Task DisplayHelpMessage() // Before
+ static async Task DisplayConstraintServerUsage() // After

- static async Task DisplayVersionMessage() // Before
+ static async Task DisplayConstraintServerVersion() // After

// Step 2: Update method calls
- await DisplayHelpMessage(); // Before
+ await DisplayConstraintServerUsage(); // After

- await DisplayVersionMessage(); // Before
+ await DisplayConstraintServerVersion(); // After
```

#### Step 3-4: Apply Business-Focused Naming
**Time**: 20 minutes | **Lines Changed**: <10 per step

```csharp
// Step 3: Rename core analysis method
- static string AnalyzeContextAndActivateConstraints(JsonDocument request, int requestId) // Before
+ static string ProcessConstraintActivationRequest(JsonDocument mcpRequest, int requestId) // After

// Step 4: Rename routing method
- static string RouteMethodCall(string requestMethod, JsonDocument parsedRequest, int requestId) // Before
+ static string ProcessMcpProtocolRequest(string methodName, JsonDocument mcpRequest, int requestId) // After
```

#### Step 5-6: Remove How-Comments, Preserve Why-Comments
**Time**: 15 minutes | **Lines Changed**: <10 per step

```csharp
// Step 5: Remove implementation comments
- // Parse the request JSON to extract method name // Remove - how comment
- // Create JSON response string with proper format // Remove - how comment
+ // Preserve business context for constraint activation decision // Keep - why comment

// Step 6: Add domain-focused documentation
+ /// <summary>
+ /// Processes MCP protocol requests to determine appropriate constraint activation
+ /// based on development context and target file analysis.
+ /// </summary>
+ static string ProcessConstraintActivationRequest(JsonDocument mcpRequest, int requestId)
```

#### Validation Checkpoint - Day 2
- [ ] All method names reflect business intent
- [ ] How-comments removed, why-comments preserved
- [ ] XML documentation added for public methods
- [ ] Domain terminology consistently applied

## Level 2: 🟢 Complexity Reduction (Days 3-4)

### Day 3: Extract Response Creation Services

#### Step 1-4: Extract JsonRpcResponseService (EXPAND Phase)
**Time**: 60 minutes | **Lines Changed**: <10 per step

```csharp
// Step 1: Create IJsonRpcResponseService interface
// File: src/ConstraintMcpServer/Application/Communication/IJsonRpcResponseService.cs
public interface IJsonRpcResponseService
{
    string CreateStandardResponse(int requestId, object result);
    string CreateInitializeResponse(int requestId);
    string CreateToolsListResponse(int requestId);
    string CreateServerHelpResponse(int requestId);
}

// Step 2: Create JsonRpcResponseService implementation
// File: src/ConstraintMcpServer/Application/Communication/JsonRpcResponseService.cs
public class JsonRpcResponseService : IJsonRpcResponseService
{
    public string CreateStandardResponse(int requestId, object result) =>
        $"{{\"jsonrpc\":\"{JsonRpcConstants.Version}\",\"id\":{requestId},\"result\":{result}}}";
}

// Step 3: Implement remaining response methods
public string CreateInitializeResponse(int requestId)
{
    var capabilities = $"{{\"capabilities\":{{\"tools\":{{}},\"resources\":{{}},\"notifications\":{{\"constraints\":true}}}},\"serverInfo\":{{\"name\":\"{ServerConstants.Name}\",\"version\":\"{ServerConstants.Version}\"}}}}";
    return CreateStandardResponse(requestId, capabilities);
}

// Step 4: Update Program.cs to use service (VALIDATE phase)
private static readonly IJsonRpcResponseService _responseService = new JsonRpcResponseService();

static string CreateInitializeResponse(int requestId) =>
    _responseService.CreateInitializeResponse(requestId); // Delegate to new service
```

#### Step 5-8: Extract CommandLineService (EXPAND Phase)
**Time**: 60 minutes | **Lines Changed**: <10 per step

```csharp
// Step 5: Create ICommandLineService interface
// File: src/ConstraintMcpServer/Presentation/CommandLine/ICommandLineService.cs
public interface ICommandLineService
{
    bool ShouldDisplayHelp(string[] args);
    bool ShouldDisplayVersion(string[] args);
    Task DisplayConstraintServerUsageAsync();
    Task DisplayConstraintServerVersionAsync();
}

// Step 6: Create CommandLineService implementation
// File: src/ConstraintMcpServer/Presentation/CommandLine/CommandLineService.cs
public class CommandLineService : ICommandLineService
{
    public bool ShouldDisplayHelp(string[] args) =>
        args.Length > 0 && (args[0] == CommandLineConstants.HelpLongFlag || args[0] == CommandLineConstants.HelpShortFlag);

    public bool ShouldDisplayVersion(string[] args) =>
        args.Length > 0 && (args[0] == CommandLineConstants.VersionLongFlag || args[0] == CommandLineConstants.VersionShortFlag);
}

// Step 7: Implement display methods
public async Task DisplayConstraintServerUsageAsync()
{
    await Console.Out.WriteLineAsync($"{ServerConstants.Name} - {ServerConstants.Description}");
    await Console.Out.WriteLineAsync("Usage: dotnet run [--help|--version]");
    await Console.Out.WriteLineAsync("  --help, -h       Show this help message");
    await Console.Out.WriteLineAsync("  --version, -v    Show version information");
}

// Step 8: Update Program.cs to use service (VALIDATE phase)
private static readonly ICommandLineService _commandLineService = new CommandLineService();

if (_commandLineService.ShouldDisplayHelp(args))
{
    await _commandLineService.DisplayConstraintServerUsageAsync();
    Environment.Exit(0);
}
```

#### Validation Checkpoint - Day 3
- [ ] All response creation extracted to dedicated service
- [ ] Command line handling extracted to dedicated service
- [ ] Both old and new code work simultaneously (VALIDATE phase)
- [ ] All tests pass with new services

### Day 4: Extract Message Processing and Apply Compose Method

#### Step 1-4: Extract MessageProcessingService (EXPAND Phase)
**Time**: 60 minutes | **Lines Changed**: <10 per step

```csharp
// Step 1: Create IMessageProcessingService interface
// File: src/ConstraintMcpServer/Application/Communication/IMessageProcessingService.cs
public interface IMessageProcessingService
{
    Task<McpProtocolMessage> ReadMcpMessageAsync(StreamReader inputReader);
    string ProcessMcpRequest(McpProtocolMessage message);
    Task WriteMcpResponseAsync(StreamWriter outputWriter, string response);
}

// Step 2: Create McpProtocolMessage value object
// File: src/ConstraintMcpServer/Domain/Communication/McpProtocolMessage.cs
public readonly record struct McpProtocolMessage(
    string MethodName,
    JsonDocument RequestDocument,
    int RequestId,
    int ContentLength
);

// Step 3: Implement message reading logic (Compose Method applied)
public async Task<McpProtocolMessage> ReadMcpMessageAsync(StreamReader inputReader)
{
    var contentLength = await ReadContentLengthAsync(inputReader);
    await ReadBlankLineAsync(inputReader);
    var requestContent = await ReadJsonContentAsync(inputReader, contentLength);
    return ParseMcpMessage(requestContent);
}

// Step 4: Implement supporting composed methods
private async Task<int> ReadContentLengthAsync(StreamReader reader)
{
    var line = await reader.ReadLineAsync();
    if (line?.StartsWith(JsonRpcConstants.ContentLengthHeader) == true)
    {
        return int.Parse(line.Substring(JsonRpcConstants.ContentLengthHeader.Length).Trim());
    }
    throw new InvalidOperationException("Expected Content-Length header");
}
```

#### Step 5-8: Apply Compose Method to Complex Logic
**Time**: 60 minutes | **Lines Changed**: <10 per step

```csharp
// Step 5: Compose constraint activation logic
- // Original 50+ line method broken into focused methods
+ public string ProcessConstraintActivationRequest(JsonDocument mcpRequest, int requestId)
+ {
+     var activationContext = ExtractActivationContext(mcpRequest);
+     var constraintDecision = DetermineConstraintActivation(activationContext);
+     return CreateConstraintActivationResponse(constraintDecision, requestId);
+ }

// Step 6: Extract context analysis
private ConstraintActivationContext ExtractActivationContext(JsonDocument request)
{
    if (!request.RootElement.TryGetProperty("params", out var paramsElement))
        return ConstraintActivationContext.NoParams();

    var developerContext = ExtractDeveloperContext(paramsElement);
    var targetFilePath = ExtractTargetFilePath(paramsElement);
    return new ConstraintActivationContext(developerContext, targetFilePath);
}

// Step 7: Extract decision logic
private ConstraintActivationDecision DetermineConstraintActivation(ConstraintActivationContext context)
{
    if (context.IndicatesTddWorkflow()) return ConstraintActivationDecision.ActivateTdd();
    if (context.IndicatesRefactoring()) return ConstraintActivationDecision.ActivateRefactoring();
    return ConstraintActivationDecision.NoActivation();
}

// Step 8: Extract response creation
private string CreateConstraintActivationResponse(ConstraintActivationDecision decision, int requestId)
{
    return decision.HasActivation
        ? _responseService.CreateConstraintActivationResponse(requestId, decision)
        : _responseService.CreateNoActivationResponse(requestId, decision.Reason);
}
```

#### Validation Checkpoint - Day 4
- [ ] Complex methods broken down using Compose Method pattern
- [ ] Message processing extracted to dedicated service
- [ ] Value objects created for domain concepts
- [ ] All tests pass with composed methods

## Level 3: 🟢 Responsibility Organization (Days 5-6)

### Day 5: Presentation Layer Extraction

#### Step 1-4: Extract Presentation Layer Concerns
**Time**: 60 minutes | **Lines Changed**: <10 per step

```csharp
// Step 1: Create McpProtocolHandler (Presentation layer)
// File: src/ConstraintMcpServer/Presentation/Protocol/McpProtocolHandler.cs
public class McpProtocolHandler
{
    private readonly IConstraintService _constraintService;

    public McpProtocolHandler(IConstraintService constraintService)
    {
        _constraintService = constraintService;
    }
}

// Step 2: Implement protocol routing logic
public McpResponse HandleProtocolRequest(McpRequest request)
{
    return request.Method switch
    {
        "initialize" => HandleInitializeRequest(request),
        "tools/call" => HandleConstraintToolCall(request),
        "server.help" => HandleHelpRequest(request),
        _ => HandleUnknownMethod(request)
    };
}

// Step 3: Extract Application layer interface
// File: src/ConstraintMcpServer/Application/Services/IConstraintService.cs
public interface IConstraintService
{
    ConstraintActivationResult ProcessConstraintRequest(ConstraintRequest request);
    ServerInitializationResult ProcessInitializeRequest();
    ServerHelpResult ProcessHelpRequest();
}

// Step 4: Move constraint logic to Application layer (MIGRATE phase)
public class ConstraintService : IConstraintService
{
    public ConstraintActivationResult ProcessConstraintRequest(ConstraintRequest request)
    {
        // Move constraint activation logic from Program.cs here
        var context = ConstraintActivationContext.FromRequest(request);
        return _constraintAnalyzer.AnalyzeAndActivate(context);
    }
}
```

#### Step 5-8: Extract Application Layer Services
**Time**: 60 minutes | **Lines Changed**: <10 per step

```csharp
// Step 5: Create IConstraintAnalyzer (Domain service port)
// File: src/ConstraintMcpServer/Domain/Services/IConstraintAnalyzer.cs
public interface IConstraintAnalyzer
{
    ConstraintActivationResult AnalyzeAndActivate(ConstraintActivationContext context);
    bool IndicatesTddWorkflow(ConstraintActivationContext context);
    bool IndicatesRefactoring(ConstraintActivationContext context);
}

// Step 6: Implement ConstraintAnalyzer (Domain layer)
// File: src/ConstraintMcpServer/Domain/Services/ConstraintAnalyzer.cs
public class ConstraintAnalyzer : IConstraintAnalyzer
{
    public ConstraintActivationResult AnalyzeAndActivate(ConstraintActivationContext context)
    {
        if (IndicatesTddWorkflow(context))
            return ConstraintActivationResult.CreateTddActivation();

        if (IndicatesRefactoring(context))
            return ConstraintActivationResult.CreateRefactoringActivation();

        return ConstraintActivationResult.NoActivation();
    }
}

// Step 7: Extract scheduling service interface
// File: src/ConstraintMcpServer/Application/Services/ISchedulingService.cs
public interface ISchedulingService
{
    bool ShouldInjectConstraints(int interactionNumber);
    SchedulingDecision DetermineInjectionCadence(ConstraintContext context);
}

// Step 8: Apply Single Responsibility Principle validation
// Verify each class has single, well-defined responsibility:
// - McpProtocolHandler: Protocol communication only
// - ConstraintService: Business workflow orchestration only
// - ConstraintAnalyzer: Constraint activation logic only
// - SchedulingService: Injection timing decisions only
```

#### Validation Checkpoint - Day 5
- [ ] Presentation layer properly separated
- [ ] Application layer services extracted
- [ ] Domain services defined with clear interfaces
- [ ] Single Responsibility Principle applied

### Day 6: Domain Layer and Dependency Injection

#### Step 1-4: Move Constraint Logic to Domain Layer
**Time**: 60 minutes | **Lines Changed**: <10 per step

```csharp
// Step 1: Create domain entities
// File: src/ConstraintMcpServer/Domain/Entities/ConstraintPack.cs
public sealed record ConstraintPack(
    string Id,
    string Title,
    double Priority,
    IReadOnlyList<string> Phases,
    IReadOnlyList<string> Reminders
);

// Step 2: Create domain value objects
// File: src/ConstraintMcpServer/Domain/ValueObjects/ConstraintActivationContext.cs
public readonly record struct ConstraintActivationContext(
    string DeveloperContext,
    string TargetFilePath
)
{
    public bool IndicatesTddWorkflow() =>
        DeveloperContext.Contains("test-first") ||
        DeveloperContext.Contains("TDD") ||
        TargetFilePath.Contains(".test.") ||
        TargetFilePath.Contains(".spec.");
}

// Step 3: Create domain result types
// File: src/ConstraintMcpServer/Domain/Results/ConstraintActivationResult.cs
public sealed record ConstraintActivationResult(
    bool HasActivation,
    string ConstraintType,
    int ConstraintCount,
    string Reason,
    IReadOnlyList<string> ActivatedConstraints,
    string Guidance
)
{
    public static ConstraintActivationResult CreateTddActivation() => new(
        HasActivation: true,
        ConstraintType: "tdd",
        ConstraintCount: 1,
        Reason: "context_match",
        ActivatedConstraints: ["tdd.primary"],
        Guidance: "Write test first before implementation. Follow TDD RED-GREEN-REFACTOR cycle."
    );
}

// Step 4: Implement repository pattern for constraints
// File: src/ConstraintMcpServer/Domain/Repositories/IConstraintRepository.cs
public interface IConstraintRepository
{
    Task<IReadOnlyList<ConstraintPack>> GetConstraintPacksAsync();
    Task<ConstraintPack?> GetConstraintByIdAsync(string constraintId);
    Task<IReadOnlyList<ConstraintPack>> GetConstraintsByPhaseAsync(string phase);
}
```

#### Step 5-8: Establish Proper Dependency Direction
**Time**: 60 minutes | **Lines Changed**: <10 per step

```csharp
// Step 5: Create service registration extensions
// File: src/ConstraintMcpServer/Infrastructure/DependencyInjection/ServiceRegistrationExtensions.cs
public static class ServiceRegistrationExtensions
{
    public static IServiceCollection AddConstraintServices(this IServiceCollection services)
    {
        // Domain services
        services.AddScoped<IConstraintAnalyzer, ConstraintAnalyzer>();

        // Application services
        services.AddScoped<IConstraintService, ConstraintService>();
        services.AddScoped<ISchedulingService, SchedulingService>();

        return services;
    }
}

// Step 6: Create infrastructure registrations
public static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
{
    // Infrastructure adapters
    services.AddScoped<IConstraintRepository, YamlConstraintRepository>();
    services.AddScoped<IJsonRpcResponseService, JsonRpcResponseService>();
    services.AddScoped<IMessageProcessingService, MessageProcessingService>();

    return services;
}

// Step 7: Create presentation registrations
public static IServiceCollection AddPresentationServices(this IServiceCollection services)
{
    // Presentation handlers
    services.AddScoped<McpProtocolHandler>();
    services.AddScoped<ICommandLineService, CommandLineService>();

    return services;
}

// Step 8: Update Program.cs to use dependency injection (CONTRACT phase)
var services = new ServiceCollection()
    .AddConstraintServices()
    .AddInfrastructureServices()
    .AddPresentationServices()
    .BuildServiceProvider();

var protocolHandler = services.GetRequiredService<McpProtocolHandler>();
// Remove all static method calls, use injected services
```

#### Validation Checkpoint - Day 6
- [ ] Domain logic properly organized in Domain layer
- [ ] Repository pattern implemented for data access
- [ ] Dependency injection properly configured
- [ ] Dependency direction follows hexagonal architecture (inward)

## Level 4: 🟢 Abstraction Refinement (Days 7-8)

### Day 7: Value Objects and Parameter Objects

#### Step 1-2: Create RequestId Value Object
**Time**: 30 minutes | **Lines Changed**: <10 per step

```csharp
// Step 1: Create RequestId value object
// File: src/ConstraintMcpServer/Domain/ValueObjects/RequestId.cs
public readonly record struct RequestId(int Value)
{
    public static RequestId Default => new(1);
    public static RequestId FromInt32(int value) => new(value);

    public static implicit operator int(RequestId requestId) => requestId.Value;
    public static implicit operator RequestId(int value) => new(value);
}

// Step 2: Update method signatures to use RequestId
- string CreateResponse(int requestId, object result) // Before
+ string CreateResponse(RequestId requestId, object result) // After

- string ProcessMcpRequest(JsonDocument request, int requestId) // Before
+ string ProcessMcpRequest(JsonDocument request, RequestId requestId) // After
```

#### Step 3-4: Create ConstraintContext Parameter Object
**Time**: 30 minutes | **Lines Changed**: <10 per step

```csharp
// Step 3: Create ConstraintContext parameter object
// File: src/ConstraintMcpServer/Domain/ValueObjects/ConstraintContext.cs
public readonly record struct ConstraintContext(
    string DeveloperContext,
    string TargetFilePath,
    RequestId RequestId,
    DateTime RequestTimestamp
)
{
    public static ConstraintContext FromMcpRequest(JsonDocument request, RequestId requestId) =>
        new(
            DeveloperContext: ExtractDeveloperContext(request),
            TargetFilePath: ExtractTargetFilePath(request),
            RequestId: requestId,
            RequestTimestamp: DateTime.UtcNow
        );
}

// Step 4: Replace multiple parameters with ConstraintContext
- ConstraintActivationResult ProcessRequest(string context, string filePath, RequestId id) // Before
+ ConstraintActivationResult ProcessRequest(ConstraintContext context) // After
```

#### Step 5-6: Create McpProtocolMessage Abstractions
**Time**: 30 minutes | **Lines Changed**: <10 per step

```csharp
// Step 5: Create McpRequest abstraction
// File: src/ConstraintMcpServer/Domain/Communication/McpRequest.cs
public sealed record McpRequest(
    string Method,
    RequestId Id,
    JsonDocument Parameters,
    ConstraintContext Context
)
{
    public static McpRequest FromJsonDocument(JsonDocument document) =>
        new(
            Method: ExtractMethod(document),
            Id: ExtractRequestId(document),
            Parameters: ExtractParameters(document),
            Context: ConstraintContext.FromMcpRequest(document, ExtractRequestId(document))
        );
}

// Step 6: Create McpResponse abstraction
// File: src/ConstraintMcpServer/Domain/Communication/McpResponse.cs
public sealed record McpResponse(
    RequestId Id,
    object Result,
    string JsonRpcVersion = "2.0"
)
{
    public string ToJsonString() =>
        $"{{\"jsonrpc\":\"{JsonRpcVersion}\",\"id\":{Id},\"result\":{JsonSerializer.Serialize(Result)}}}";
}
```

#### Validation Checkpoint - Day 7
- [ ] Value objects replace primitive parameters
- [ ] Parameter objects group related data
- [ ] Method signatures simplified and more expressive
- [ ] Strong typing prevents parameter mistakes

### Day 8: Configuration Enhancement and Abstraction

#### Step 1-4: Eliminate Primitive Parameters in Public Interfaces
**Time**: 60 minutes | **Lines Changed**: <10 per step

```csharp
// Step 1: Create MethodName value object
// File: src/ConstraintMcpServer/Domain/ValueObjects/MethodName.cs
public readonly record struct MethodName(string Value)
{
    public static MethodName Initialize => new("initialize");
    public static MethodName ToolsCall => new("tools/call");
    public static MethodName ServerHelp => new("server.help");
    public static MethodName Unknown => new("unknown");

    public bool IsToolsCall => Value == "tools/call";
    public bool IsInitialize => Value == "initialize";
}

// Step 2: Create ContentLength value object
// File: src/ConstraintMcpServer/Domain/ValueObjects/ContentLength.cs
public readonly record struct ContentLength(int Bytes)
{
    public static ContentLength FromHeaderLine(string headerLine)
    {
        var lengthText = headerLine.Substring("Content-Length:".Length).Trim();
        return new ContentLength(int.Parse(lengthText));
    }

    public char[] CreateBuffer() => new char[Bytes];
}

// Step 3: Update interfaces to use value objects
public interface IMessageProcessingService
{
    Task<McpRequest> ReadMcpRequestAsync(StreamReader inputReader);
    McpResponse ProcessMcpRequest(McpRequest request);
    Task WriteMcpResponseAsync(StreamWriter outputWriter, McpResponse response);
}

// Step 4: Remove primitive obsession from domain services
public interface IConstraintAnalyzer
{
    ConstraintActivationResult AnalyzeAndActivate(ConstraintContext context);
    ConstraintDecision DetermineActivation(DeveloperIntent intent, TargetFile targetFile);
}
```

#### Step 5-8: Apply Configuration Validation Through Value Objects
**Time**: 60 minutes | **Lines Changed**: <10 per step

```csharp
// Step 5: Create ConstraintPriority value object
// File: src/ConstraintMcpServer/Domain/ValueObjects/ConstraintPriority.cs
public readonly record struct ConstraintPriority(double Value)
{
    public ConstraintPriority(double value) : this(Math.Clamp(value, 0.0, 1.0))
    {
        if (value < 0.0 || value > 1.0)
            throw new ArgumentOutOfRangeException(nameof(value), "Priority must be between 0.0 and 1.0");
    }

    public static ConstraintPriority High => new(0.9);
    public static ConstraintPriority Medium => new(0.5);
    public static ConstraintPriority Low => new(0.1);
}

// Step 6: Create ConstraintPhase value object
// File: src/ConstraintMcpServer/Domain/ValueObjects/ConstraintPhase.cs
public readonly record struct ConstraintPhase(string Value)
{
    public static ConstraintPhase Kickoff => new("kickoff");
    public static ConstraintPhase Red => new("red");
    public static ConstraintPhase Green => new("green");
    public static ConstraintPhase Refactor => new("refactor");
    public static ConstraintPhase Commit => new("commit");

    private static readonly HashSet<string> ValidPhases = new()
    {
        "kickoff", "red", "green", "refactor", "commit"
    };

    public ConstraintPhase(string value)
    {
        if (!ValidPhases.Contains(value))
            throw new ArgumentException($"Invalid constraint phase: {value}");
        Value = value;
    }
}

// Step 7: Update ConstraintPack with value objects
public sealed record ConstraintPack(
    ConstraintId Id,
    ConstraintTitle Title,
    ConstraintPriority Priority,
    IReadOnlyList<ConstraintPhase> Phases,
    IReadOnlyList<ConstraintReminder> Reminders
);

// Step 8: Create configuration validation service
public interface IConstraintConfigurationValidator
{
    ValidationResult ValidateConstraintPack(YamlConstraintPackDto yamlData);
    ValidationResult ValidateConstraint(YamlConstraintDto constraint, int index);
}
```

#### Validation Checkpoint - Day 8
- [ ] All primitive obsession eliminated from public interfaces
- [ ] Value objects provide validation and domain meaning
- [ ] Configuration enhanced with type-safe validation
- [ ] Domain concepts clearly expressed through types

## Level 5: 🔵 Design Pattern Application (Days 9-10)

### Day 9: Strategy and Factory Patterns

#### Step 1-4: Implement Strategy Pattern for Constraint Selection
**Time**: 60 minutes | **Lines Changed**: <10 per step

```csharp
// Step 1: Create IConstraintSelectionStrategy interface
// File: src/ConstraintMcpServer/Domain/Strategies/IConstraintSelectionStrategy.cs
public interface IConstraintSelectionStrategy
{
    IReadOnlyList<ConstraintPack> SelectConstraints(ConstraintContext context, IReadOnlyList<ConstraintPack> availableConstraints);
    bool CanHandle(ConstraintContext context);
    string StrategyName { get; }
}

// Step 2: Implement TddAwareSelectionStrategy
// File: src/ConstraintMcpServer/Domain/Strategies/TddAwareSelectionStrategy.cs
public class TddAwareSelectionStrategy : IConstraintSelectionStrategy
{
    public string StrategyName => "TDD-Aware";

    public bool CanHandle(ConstraintContext context) =>
        context.DeveloperContext.Contains("test-first") ||
        context.DeveloperContext.Contains("TDD") ||
        context.TargetFilePath.Contains(".test.");

    public IReadOnlyList<ConstraintPack> SelectConstraints(ConstraintContext context, IReadOnlyList<ConstraintPack> availableConstraints) =>
        availableConstraints
            .Where(c => c.Phases.Contains(ConstraintPhase.Red) || c.Id.Value.StartsWith("tdd"))
            .OrderByDescending(c => c.Priority.Value)
            .Take(3)
            .ToList();
}

// Step 3: Implement RefactoringAwareSelectionStrategy
public class RefactoringAwareSelectionStrategy : IConstraintSelectionStrategy
{
    public string StrategyName => "Refactoring-Aware";

    public bool CanHandle(ConstraintContext context) =>
        context.DeveloperContext.Contains("refactoring") ||
        context.DeveloperContext.Contains("improve code quality");

    public IReadOnlyList<ConstraintPack> SelectConstraints(ConstraintContext context, IReadOnlyList<ConstraintPack> availableConstraints) =>
        availableConstraints
            .Where(c => c.Phases.Contains(ConstraintPhase.Refactor) || c.Id.Value.Contains("refactor"))
            .OrderByDescending(c => c.Priority.Value)
            .Take(5)
            .ToList();
}

// Step 4: Create strategy selector service
// File: src/ConstraintMcpServer/Application/Services/ConstraintSelectionService.cs
public class ConstraintSelectionService
{
    private readonly IReadOnlyList<IConstraintSelectionStrategy> _strategies;

    public ConstraintSelectionService(IEnumerable<IConstraintSelectionStrategy> strategies)
    {
        _strategies = strategies.ToList();
    }

    public IReadOnlyList<ConstraintPack> SelectBestConstraints(ConstraintContext context, IReadOnlyList<ConstraintPack> availableConstraints)
    {
        var strategy = _strategies.FirstOrDefault(s => s.CanHandle(context))
                    ?? _strategies.First(); // Default strategy

        return strategy.SelectConstraints(context, availableConstraints);
    }
}
```

#### Step 5-8: Implement Factory Pattern for Response Creation
**Time**: 60 minutes | **Lines Changed**: <10 per step

```csharp
// Step 5: Create IMcpResponseFactory interface
// File: src/ConstraintMcpServer/Application/Factories/IMcpResponseFactory.cs
public interface IMcpResponseFactory
{
    McpResponse CreateInitializeResponse(RequestId requestId);
    McpResponse CreateConstraintActivationResponse(RequestId requestId, ConstraintActivationResult result);
    McpResponse CreateServerHelpResponse(RequestId requestId);
    McpResponse CreateErrorResponse(RequestId requestId, string errorMessage);
}

// Step 6: Implement McpResponseFactory
// File: src/ConstraintMcpServer/Application/Factories/McpResponseFactory.cs
public class McpResponseFactory : IMcpResponseFactory
{
    public McpResponse CreateInitializeResponse(RequestId requestId)
    {
        var capabilities = new
        {
            capabilities = new
            {
                tools = new { },
                resources = new { },
                notifications = new { constraints = true }
            },
            serverInfo = new
            {
                name = ServerConstants.Name,
                version = ServerConstants.Version
            }
        };

        return new McpResponse(requestId, capabilities);
    }

    public McpResponse CreateConstraintActivationResponse(RequestId requestId, ConstraintActivationResult result)
    {
        var responseData = new
        {
            context_analysis = new
            {
                has_activation = result.HasActivation,
                constraint_type = result.ConstraintType,
                constraint_count = result.ConstraintCount,
                reason = result.Reason
            },
            activated_constraints = result.ActivatedConstraints,
            guidance = result.Guidance
        };

        return new McpResponse(requestId, responseData);
    }
}

// Step 7: Create abstract factory for different response types
// File: src/ConstraintMcpServer/Application/Factories/ResponseFactoryProvider.cs
public class ResponseFactoryProvider
{
    private readonly IMcpResponseFactory _mcpResponseFactory;

    public ResponseFactoryProvider(IMcpResponseFactory mcpResponseFactory)
    {
        _mcpResponseFactory = mcpResponseFactory;
    }

    public IMcpResponseFactory GetMcpResponseFactory() => _mcpResponseFactory;
}

// Step 8: Register factories in dependency injection
public static IServiceCollection AddFactoryServices(this IServiceCollection services)
{
    services.AddScoped<IMcpResponseFactory, McpResponseFactory>();
    services.AddScoped<ResponseFactoryProvider>();
    return services;
}
```

#### Validation Checkpoint - Day 9
- [ ] Strategy pattern properly implemented for constraint selection
- [ ] Factory pattern provides type-safe response creation
- [ ] Multiple strategies available and selectable
- [ ] Factories registered in dependency injection

### Day 10: Command Pattern and Final Patterns

#### Step 1-4: Implement Command Pattern for Request Processing
**Time**: 60 minutes | **Lines Changed**: <10 per step

```csharp
// Step 1: Create ICommand interface
// File: src/ConstraintMcpServer/Application/Commands/ICommand.cs
public interface ICommand<TResult>
{
    Task<TResult> ExecuteAsync();
}

// Step 2: Create constraint activation command
// File: src/ConstraintMcpServer/Application/Commands/ActivateConstraintsCommand.cs
public class ActivateConstraintsCommand : ICommand<ConstraintActivationResult>
{
    private readonly ConstraintContext _context;
    private readonly IConstraintAnalyzer _analyzer;
    private readonly ConstraintSelectionService _selectionService;

    public ActivateConstraintsCommand(
        ConstraintContext context,
        IConstraintAnalyzer analyzer,
        ConstraintSelectionService selectionService)
    {
        _context = context;
        _analyzer = analyzer;
        _selectionService = selectionService;
    }

    public async Task<ConstraintActivationResult> ExecuteAsync()
    {
        var availableConstraints = await LoadAvailableConstraintsAsync();
        var selectedConstraints = _selectionService.SelectBestConstraints(_context, availableConstraints);
        return _analyzer.AnalyzeAndActivate(_context, selectedConstraints);
    }
}

// Step 3: Create command dispatcher
// File: src/ConstraintMcpServer/Application/Commands/ICommandDispatcher.cs
public interface ICommandDispatcher
{
    Task<TResult> DispatchAsync<TResult>(ICommand<TResult> command);
}

public class CommandDispatcher : ICommandDispatcher
{
    public async Task<TResult> DispatchAsync<TResult>(ICommand<TResult> command)
    {
        // Add cross-cutting concerns here (logging, validation, etc.)
        return await command.ExecuteAsync();
    }
}

// Step 4: Update service to use commands
public class ConstraintService : IConstraintService
{
    private readonly ICommandDispatcher _commandDispatcher;

    public async Task<ConstraintActivationResult> ProcessConstraintRequestAsync(ConstraintContext context)
    {
        var command = new ActivateConstraintsCommand(context, _analyzer, _selectionService);
        return await _commandDispatcher.DispatchAsync(command);
    }
}
```

#### Step 5-6: Apply Observer Pattern for Notifications
**Time**: 30 minutes | **Lines Changed**: <10 per step

```csharp
// Step 5: Create notification system
// File: src/ConstraintMcpServer/Application/Notifications/IConstraintNotificationService.cs
public interface IConstraintNotificationService
{
    void Subscribe(IConstraintObserver observer);
    void Unsubscribe(IConstraintObserver observer);
    Task NotifyConstraintActivatedAsync(ConstraintActivationEvent activationEvent);
}

public interface IConstraintObserver
{
    Task OnConstraintActivatedAsync(ConstraintActivationEvent activationEvent);
}

// Step 6: Implement notification service
public class ConstraintNotificationService : IConstraintNotificationService
{
    private readonly List<IConstraintObserver> _observers = new();

    public void Subscribe(IConstraintObserver observer) => _observers.Add(observer);
    public void Unsubscribe(IConstraintObserver observer) => _observers.Remove(observer);

    public async Task NotifyConstraintActivatedAsync(ConstraintActivationEvent activationEvent)
    {
        var tasks = _observers.Select(observer => observer.OnConstraintActivatedAsync(activationEvent));
        await Task.WhenAll(tasks);
    }
}
```

#### Validation Checkpoint - Day 10 (Steps 1-6)
- [ ] Command pattern implemented for request processing
- [ ] Observer pattern implemented for notifications
- [ ] Cross-cutting concerns properly handled
- [ ] All patterns work together cohesively

## Level 6: 🔵 SOLID++ Principles Application (Day 10, Final Steps)

### Step 7-10: Final SOLID Validation and Architecture Compliance

#### Step 7: Validate Single Responsibility Principle
**Time**: 20 minutes | **Lines Changed**: Validation only

```csharp
// Verify each class has single, well-defined responsibility:

// ✅ Program.cs - CLI entry point and hosting only
// ✅ McpProtocolHandler - MCP protocol communication only
// ✅ ConstraintService - Constraint workflow orchestration only
// ✅ ConstraintAnalyzer - Constraint analysis logic only
// ✅ MessageProcessingService - Message parsing/formatting only
// ✅ JsonRpcResponseService - JSON-RPC response creation only
// ✅ CommandLineService - Command line argument handling only

// Each class should have exactly one reason to change
```

#### Step 8: Ensure Open/Closed Principle for Extensibility
**Time**: 20 minutes | **Lines Changed**: Validation only

```csharp
// Verify extensibility without modification:

// ✅ IConstraintSelectionStrategy - New strategies can be added without changing existing code
// ✅ IMcpResponseFactory - New response types can be added without changing factory interface
// ✅ ICommand<TResult> - New commands can be added without changing dispatcher
// ✅ IConstraintObserver - New observers can be added without changing notification service

// System should be extensible through composition, not modification
```

#### Step 9: Verify Proper Abstraction Dependencies
**Time**: 15 minutes | **Lines Changed**: Validation only

```csharp
// Validate dependency direction (Dependency Inversion Principle):

// ✅ Presentation layer depends on Application interfaces
// ✅ Application layer depends on Domain interfaces
// ✅ Infrastructure implements Application/Domain interfaces
// ✅ No layer depends on concrete implementations from outer layers

// Dependency flow: Presentation → Application → Domain ← Infrastructure
```

#### Step 10: Final Architecture Compliance Validation
**Time**: 15 minutes | **Lines Changed**: Documentation update

```csharp
// Final validation checklist:

// ✅ Hexagonal architecture properly implemented
// ✅ All 386 tests pass
// ✅ Sub-50ms p95 performance maintained
// ✅ Zero warnings with strict analysis
// ✅ SOLID principles applied throughout
// ✅ Design patterns appropriately used
// ✅ Clean code principles followed
// ✅ Domain-driven design concepts applied

// Update architecture compliance documentation
```

## Quality Metrics Validation

### Expected Improvements After Level 1-6 Refactoring

#### Code Quality Metrics
- **Cyclomatic Complexity**: Reduced from 8-12 to 3-5 per method (60% improvement)
- **Maintainability Index**: Improved by 25% through systematic organization
- **Technical Debt Ratio**: Reduced by 40% through proper abstractions
- **Code Coverage**: Maintained at 100% throughout refactoring process

#### Architecture Quality
- **Coupling Metrics**: 30% reduction in afferent/efferent coupling
- **Cohesion Metrics**: 40% improvement in class cohesion
- **Abstraction Level**: Proper separation of concerns achieved
- **Dependency Direction**: Clean dependency inversion implemented

#### Performance Metrics
- **Latency**: Sub-50ms p95 maintained or improved
- **Memory Usage**: <100MB ceiling preserved
- **Throughput**: Request processing efficiency maintained
- **Resource Usage**: Optimized through proper object lifecycle management

## Summary

This comprehensive refactoring strategy transforms the 229-line procedural Program.cs into a clean, maintainable hexagonal architecture while preserving all quality standards and performance requirements. The systematic Level 1-6 approach ensures progressive improvement with continuous validation at every step.

**Key Achievements**:
- ✅ Complete separation of concerns across architectural layers
- ✅ Proper dependency injection with interface-based design
- ✅ SOLID principles systematically applied
- ✅ Design patterns appropriately implemented
- ✅ Strong typing with value objects eliminating primitive obsession
- ✅ Enterprise-quality code standards achieved
- ✅ Performance and test coverage preserved throughout

The refactoring strategy integrates seamlessly with the Mikado parallel change approach, ensuring minimal risk while achieving maximum architectural improvement.