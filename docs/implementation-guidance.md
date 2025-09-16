# Implementation Guidance - Phase 1 Infrastructure Foundation

## Implementation Strategy Overview

This document provides detailed step-by-step guidance for implementing the Phase 1 infrastructure foundation using the parallel change pattern (EXPAND → VALIDATE → MIGRATE → CONTRACT) while preserving all 386 tests and maintaining sub-50ms p95 latency.

## EXPAND Phase: Create New Infrastructure Alongside Existing

### Step 1: Extract Infrastructure Layer (Days 1-2)

#### 1.1 Create Infrastructure Foundation

**Create Directory Structure:**
```bash
mkdir -p src/ConstraintMcpServer/Infrastructure/Hosting
mkdir -p src/ConstraintMcpServer/Infrastructure/Configuration
mkdir -p src/ConstraintMcpServer/Infrastructure/Logging
mkdir -p src/ConstraintMcpServer/Infrastructure/DependencyInjection
mkdir -p src/ConstraintMcpServer/Infrastructure/Performance
```

**Add Required Packages:**
```xml
<!-- Add to ConstraintMcpServer.csproj -->
<PackageReference Include="Microsoft.Extensions.Hosting" Version="8.0.0" />
<PackageReference Include="Microsoft.Extensions.DependencyInjection" Version="8.0.0" />
<PackageReference Include="Microsoft.Extensions.Configuration" Version="8.0.0" />
<PackageReference Include="Microsoft.Extensions.Logging" Version="8.0.0" />
<PackageReference Include="Serilog.Extensions.Hosting" Version="8.0.0" />
<PackageReference Include="Serilog.Sinks.Console" Version="5.0.1" />
<PackageReference Include="Serilog.Sinks.File" Version="5.0.0" />
<PackageReference Include="Serilog.Formatting.Compact" Version="2.0.0" />
```

#### 1.2 Implement MCP Server Host Abstraction

**Create: `src/ConstraintMcpServer/Infrastructure/Hosting/IMcpServerHost.cs`**
```csharp
using System.Threading;
using System.Threading.Tasks;

namespace ConstraintMcpServer.Infrastructure.Hosting;

/// <summary>
/// Abstraction for MCP server hosting infrastructure.
/// Provides clean separation between hosting concerns and business logic.
/// </summary>
public interface IMcpServerHost
{
    /// <summary>
    /// Starts the MCP server and begins listening for requests.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token for graceful shutdown</param>
    /// <returns>Task representing the async operation</returns>
    Task StartAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Stops the MCP server and releases resources.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token for shutdown timeout</param>
    /// <returns>Task representing the async operation</returns>
    Task StopAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Processes an individual MCP request with constraint enhancement.
    /// </summary>
    /// <param name="requestJson">Raw JSON-RPC request</param>
    /// <param name="cancellationToken">Cancellation token for operation timeout</param>
    /// <returns>Enhanced JSON-RPC response</returns>
    Task<string> ProcessRequestAsync(string requestJson, CancellationToken cancellationToken = default);
}
```

**Create: `src/ConstraintMcpServer/Infrastructure/Hosting/McpServerHost.cs`**
```csharp
using System;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using ConstraintMcpServer.Application.Services;

namespace ConstraintMcpServer.Infrastructure.Hosting;

/// <summary>
/// Production implementation of MCP server hosting.
/// Handles JSON-RPC protocol over stdio with Content-Length headers.
/// </summary>
internal sealed class McpServerHost : IMcpServerHost
{
    private readonly IConstraintService _constraintService;
    private readonly ILogger<McpServerHost> _logger;
    private readonly CancellationTokenSource _cancellationTokenSource;

    private const string ServerName = "ConstraintMcpServer";
    private const string ServerVersion = "0.1.0";
    private const string ContentLengthHeader = "Content-Length:";
    private const string JsonRpcVersion = "2.0";

    public McpServerHost(
        IConstraintService constraintService,
        ILogger<McpServerHost> logger)
    {
        _constraintService = constraintService ?? throw new ArgumentNullException(nameof(constraintService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _cancellationTokenSource = new CancellationTokenSource();
    }

    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting {ServerName} v{Version}", ServerName, ServerVersion);

        try
        {
            using var linkedToken = CancellationTokenSource.CreateLinkedTokenSource(
                cancellationToken, _cancellationTokenSource.Token);

            await RunMessageLoopAsync(linkedToken.Token);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            _logger.LogInformation("MCP server stopped due to cancellation request");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in MCP server main loop");
            throw;
        }
    }

    public async Task StopAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Stopping MCP server...");

        _cancellationTokenSource.Cancel();

        // Allow graceful shutdown period
        await Task.Delay(100, cancellationToken);

        _logger.LogInformation("MCP server stopped");
    }

    public async Task<string> ProcessRequestAsync(string requestJson, CancellationToken cancellationToken = default)
    {
        using var activity = McpServerHostActivitySource.StartActivity("ProcessRequest");
        var startTime = DateTime.UtcNow;

        try
        {
            // Delegate to constraint service for business logic
            var enhancedResponse = await _constraintService.ProcessMcpRequestAsync(requestJson, cancellationToken);

            var duration = DateTime.UtcNow - startTime;
            _logger.LogInformation("Request processed in {Duration}ms", duration.TotalMilliseconds);

            return enhancedResponse;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing MCP request");
            return CreateErrorResponse(ex);
        }
    }

    private async Task RunMessageLoopAsync(CancellationToken cancellationToken)
    {
        var stdin = Console.OpenStandardInput();
        var stdout = Console.OpenStandardOutput();

        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                var request = await ReadJsonRpcMessageAsync(stdin, cancellationToken);
                if (request == null) break; // EOF

                var response = await ProcessRequestAsync(request, cancellationToken);
                await WriteJsonRpcMessageAsync(stdout, response, cancellationToken);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in message loop");
                // Continue processing other messages
            }
        }
    }

    // ... Additional implementation methods for JSON-RPC protocol handling
}
```

#### 1.3 Create Application Service Layer

**Create: `src/ConstraintMcpServer/Application/Services/IConstraintService.cs`**
```csharp
using System.Threading;
using System.Threading.Tasks;

namespace ConstraintMcpServer.Application.Services;

/// <summary>
/// Core business service for constraint management and enhancement.
/// Orchestrates constraint selection, injection, and session management.
/// </summary>
public interface IConstraintService
{
    /// <summary>
    /// Processes an MCP request with constraint enhancement.
    /// </summary>
    /// <param name="requestJson">Raw JSON-RPC request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Enhanced JSON-RPC response with constraints</returns>
    Task<string> ProcessMcpRequestAsync(string requestJson, CancellationToken cancellationToken = default);

    /// <summary>
    /// Analyzes request context and selects appropriate constraints.
    /// </summary>
    /// <param name="context">Request context</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Selected constraints for injection</returns>
    Task<ConstraintSelectionResult> SelectConstraintsAsync(SessionContext context, CancellationToken cancellationToken = default);
}
```

### Step 2: Implement Dependency Injection Infrastructure (Days 3-4)

#### 2.1 Create Service Registration Framework

**Create: `src/ConstraintMcpServer/Infrastructure/DependencyInjection/IServiceInstaller.cs`**
```csharp
using Microsoft.Extensions.DependencyInjection;

namespace ConstraintMcpServer.Infrastructure.DependencyInjection;

/// <summary>
/// Contract for automatic service registration installers.
/// Enables modular service registration with reflection-based discovery.
/// </summary>
public interface IServiceInstaller
{
    /// <summary>
    /// Registers services in the dependency injection container.
    /// </summary>
    /// <param name="services">Service collection to register services in</param>
    void InstallServices(IServiceCollection services);

    /// <summary>
    /// Gets the installation priority (lower values install first).
    /// </summary>
    int Priority => 100;
}
```

**Create: `src/ConstraintMcpServer/Infrastructure/DependencyInjection/DomainServiceInstaller.cs`**
```csharp
using Microsoft.Extensions.DependencyInjection;
using ConstraintMcpServer.Domain.Services;
using ConstraintMcpServer.Domain.Validation;

namespace ConstraintMcpServer.Infrastructure.DependencyInjection;

/// <summary>
/// Registers domain services as singletons (stateless, pure business logic).
/// </summary>
internal sealed class DomainServiceInstaller : IServiceInstaller
{
    public int Priority => 10; // Install domain services first

    public void InstallServices(IServiceCollection services)
    {
        // Domain services - Singleton lifetime (stateless)
        services.AddSingleton<IConstraintSelector, ConstraintSelector>();
        services.AddSingleton<IPriorityCalculator, PriorityCalculator>();
        services.AddSingleton<IPhaseDetector, PhaseDetector>();
        services.AddSingleton<IConstraintValidator, ConstraintValidator>();

        // Value object factories
        services.AddSingleton<IConstraintFactory, ConstraintFactory>();
        services.AddSingleton<ISessionContextFactory, SessionContextFactory>();
    }
}
```

**Create: `src/ConstraintMcpServer/Infrastructure/DependencyInjection/ApplicationServiceInstaller.cs`**
```csharp
using Microsoft.Extensions.DependencyInjection;
using ConstraintMcpServer.Application.Services;
using ConstraintMcpServer.Application.Workflows;

namespace ConstraintMcpServer.Infrastructure.DependencyInjection;

/// <summary>
/// Registers application services as scoped (per-request state management).
/// </summary>
internal sealed class ApplicationServiceInstaller : IServiceInstaller
{
    public int Priority => 20; // Install after domain services

    public void InstallServices(IServiceCollection services)
    {
        // Application services - Scoped lifetime (per-request context)
        services.AddScoped<IConstraintService, ConstraintService>();
        services.AddScoped<IContextAnalyzer, ContextAnalyzer>();
        services.AddScoped<IConstraintRepository, InMemoryConstraintRepository>();
        services.AddScoped<IValidationService, ValidationService>();

        // Business workflows
        services.AddScoped<IConstraintSelectionWorkflow, ConstraintSelectionWorkflow>();
        services.AddScoped<IConstraintInjectionWorkflow, ConstraintInjectionWorkflow>();
    }
}
```

#### 2.2 Create Service Discovery Engine

**Create: `src/ConstraintMcpServer/Infrastructure/DependencyInjection/ServiceCollectionExtensions.cs`**
```csharp
using System;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ConstraintMcpServer.Infrastructure.DependencyInjection;

/// <summary>
/// Extension methods for automatic service registration with reflection-based discovery.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers all services using automatic installer discovery.
    /// Ensures sub-1ms service resolution performance requirement.
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <returns>Service collection for chaining</returns>
    public static IServiceCollection AddConstraintMcpServices(this IServiceCollection services)
    {
        var startTime = DateTime.UtcNow;

        try
        {
            // Discover and register all service installers
            var installers = DiscoverServiceInstallers();

            foreach (var installer in installers.OrderBy(i => i.Priority))
            {
                installer.InstallServices(services);
            }

            // Add performance monitoring
            services.AddSingleton<IPerformanceMetrics, PerformanceMetrics>();

            var registrationTime = DateTime.UtcNow - startTime;

            // Ensure registration performance meets requirements
            if (registrationTime.TotalMilliseconds > 10)
            {
                throw new InvalidOperationException(
                    $"Service registration took {registrationTime.TotalMilliseconds:F2}ms, " +
                    "exceeding 10ms limit. Optimize installer performance.");
            }

            return services;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed to register constraint MCP services", ex);
        }
    }

    private static IServiceInstaller[] DiscoverServiceInstallers()
    {
        var assembly = Assembly.GetExecutingAssembly();

        return assembly.GetTypes()
            .Where(type => type.IsClass && !type.IsAbstract)
            .Where(type => typeof(IServiceInstaller).IsAssignableFrom(type))
            .Where(type => type.GetConstructor(Type.EmptyTypes) != null)
            .Select(type => (IServiceInstaller)Activator.CreateInstance(type)!)
            .ToArray();
    }
}
```

### Step 3: Configuration Management Infrastructure (Days 5-6)

#### 3.1 Create Configuration Abstractions

**Create: `src/ConstraintMcpServer/Infrastructure/Configuration/IConfigurationLoader.cs`**
```csharp
using System.Threading;
using System.Threading.Tasks;
using ConstraintMcpServer.Domain.Entities;

namespace ConstraintMcpServer.Infrastructure.Configuration;

/// <summary>
/// Abstraction for loading and validating constraint pack configurations.
/// </summary>
public interface IConfigurationLoader
{
    /// <summary>
    /// Loads constraint pack from YAML configuration file.
    /// </summary>
    /// <param name="configPath">Path to configuration file</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Validated constraint pack</returns>
    Task<ConstraintPack> LoadConstraintPackAsync(string configPath, CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates configuration against business rules.
    /// </summary>
    /// <param name="constraintPack">Constraint pack to validate</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Validation result with detailed error information</returns>
    Task<ValidationResult> ValidateConfigurationAsync(ConstraintPack constraintPack, CancellationToken cancellationToken = default);
}
```

#### 3.2 Implement Structured Logging

**Create: `src/ConstraintMcpServer/Infrastructure/Logging/StructuredLogger.cs`**
```csharp
using System;
using Microsoft.Extensions.Logging;
using Serilog;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace ConstraintMcpServer.Infrastructure.Logging;

/// <summary>
/// High-performance structured logger with NDJSON output for constraint MCP server.
/// Provides minimal overhead logging with rich structured data.
/// </summary>
internal sealed class StructuredLogger : IStructuredLogger
{
    private readonly ILogger _logger;

    public StructuredLogger(ILogger<StructuredLogger> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public void LogConstraintSelection(string constraintId, double priority, string phase, TimeSpan duration)
    {
        _logger.LogInformation(
            "Constraint {ConstraintId} selected with priority {Priority} for phase {Phase} in {Duration}ms",
            constraintId, priority, phase, duration.TotalMilliseconds);
    }

    public void LogPerformanceMetric(string operation, TimeSpan duration, long memoryUsage)
    {
        _logger.LogInformation(
            "Performance metric: {Operation} completed in {Duration}ms using {MemoryUsage} bytes",
            operation, duration.TotalMilliseconds, memoryUsage);
    }

    public void LogRequestProcessing(string requestId, string operation, TimeSpan duration, int constraintsActivated)
    {
        _logger.LogInformation(
            "Request {RequestId} processed: {Operation} completed in {Duration}ms with {ConstraintsActivated} constraints",
            requestId, operation, duration.TotalMilliseconds, constraintsActivated);
    }

    public void LogError(string operation, Exception exception, string? requestId = null)
    {
        _logger.LogError(exception,
            "Error in {Operation} for request {RequestId}: {ErrorMessage}",
            operation, requestId ?? "unknown", exception.Message);
    }
}
```

## VALIDATE Phase: Ensure New Infrastructure Works

### Step 4: Integration Testing (Days 7-8)

#### 4.1 Create Infrastructure Integration Tests

**Create: `tests/ConstraintMcpServer.Tests/Integration/Infrastructure/McpServerHostIntegrationTests.cs`**
```csharp
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using FluentAssertions;
using ConstraintMcpServer.Infrastructure.Hosting;
using ConstraintMcpServer.Infrastructure.DependencyInjection;

namespace ConstraintMcpServer.Tests.Integration.Infrastructure;

[TestFixture]
public class McpServerHostIntegrationTests
{
    private IHost? _host;
    private IMcpServerHost? _mcpServerHost;

    [SetUp]
    public async Task SetUp()
    {
        _host = Host.CreateDefaultBuilder()
            .ConfigureServices(services =>
            {
                services.AddConstraintMcpServices();
                services.AddLogging(builder => builder.AddConsole());
            })
            .Build();

        await _host.StartAsync();
        _mcpServerHost = _host.Services.GetRequiredService<IMcpServerHost>();
    }

    [TearDown]
    public async Task TearDown()
    {
        if (_host != null)
        {
            await _host.StopAsync();
            _host.Dispose();
        }
    }

    [Test]
    public async Task ProcessRequestAsync_WithValidMcpRequest_ShouldReturnEnhancedResponse()
    {
        // Arrange
        var validMcpRequest = """
            {
                "jsonrpc": "2.0",
                "id": 1,
                "method": "tools/call",
                "params": {
                    "name": "create_file",
                    "arguments": { "filename": "test.cs", "content": "// Test file" }
                }
            }
            """;

        // Act
        var startTime = DateTime.UtcNow;
        var response = await _mcpServerHost!.ProcessRequestAsync(validMcpRequest);
        var processingTime = DateTime.UtcNow - startTime;

        // Assert
        response.Should().NotBeNullOrEmpty();
        response.Should().Contain("jsonrpc");
        response.Should().Contain("2.0");

        // Performance requirement validation
        processingTime.TotalMilliseconds.Should().BeLessThan(50,
            "MCP request processing must meet p95 latency requirement");
    }

    [Test]
    public async Task ServiceResolution_ShouldMeetPerformanceRequirements()
    {
        // Arrange & Act
        var resolutionTimes = new List<TimeSpan>();

        for (int i = 0; i < 100; i++)
        {
            var startTime = DateTime.UtcNow;
            var service = _host!.Services.GetRequiredService<IMcpServerHost>();
            var resolutionTime = DateTime.UtcNow - startTime;
            resolutionTimes.Add(resolutionTime);
        }

        // Assert
        var averageResolutionTime = resolutionTimes.Average(t => t.TotalMilliseconds);
        var p95ResolutionTime = resolutionTimes
            .OrderBy(t => t.TotalMilliseconds)
            .Skip(94)
            .First()
            .TotalMilliseconds;

        averageResolutionTime.Should().BeLessThan(0.5,
            "Average service resolution should be sub-millisecond");
        p95ResolutionTime.Should().BeLessThan(1.0,
            "P95 service resolution must meet <1ms requirement");
    }
}
```

#### 4.2 Performance Validation Tests

**Create: `tests/ConstraintMcpServer.Tests/Performance/InfrastructurePerformanceTests.cs`**
```csharp
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NUnit.Framework;
using FluentAssertions;
using ConstraintMcpServer.Infrastructure.DependencyInjection;

namespace ConstraintMcpServer.Tests.Performance;

[TestFixture]
public class InfrastructurePerformanceTests
{
    [Test]
    public async Task ServiceContainerStartup_ShouldMeetPerformanceRequirements()
    {
        // Arrange
        var stopwatch = Stopwatch.StartNew();

        // Act
        var host = Host.CreateDefaultBuilder()
            .ConfigureServices(services => services.AddConstraintMcpServices())
            .Build();

        await host.StartAsync();

        stopwatch.Stop();

        // Assert
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(200,
            "Container startup must complete within 200ms");

        // Cleanup
        await host.StopAsync();
        host.Dispose();
    }

    [Test]
    public void MemoryUsage_ShouldStayWithinBudget()
    {
        // Arrange
        var initialMemory = GC.GetTotalMemory(true);

        // Act
        var host = Host.CreateDefaultBuilder()
            .ConfigureServices(services => services.AddConstraintMcpServices())
            .Build();

        var afterRegistrationMemory = GC.GetTotalMemory(false);
        var memoryIncrease = afterRegistrationMemory - initialMemory;

        // Assert
        var memoryIncreaseMB = memoryIncrease / 1024.0 / 1024.0;
        memoryIncreaseMB.Should().BeLessThan(10,
            "Service registration memory overhead should be minimal");

        // Cleanup
        host.Dispose();
    }
}
```

## MIGRATE Phase: Switch Program.cs to New Infrastructure

### Step 5: Program.cs Migration (Day 9)

#### 5.1 Update Program.cs to Use New Infrastructure

**Update: `src/ConstraintMcpServer/Program.cs`**
```csharp
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using ConstraintMcpServer.Infrastructure.DependencyInjection;
using ConstraintMcpServer.Infrastructure.Hosting;

// Configure Serilog for structured logging
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console(new Serilog.Formatting.Compact.CompactJsonFormatter())
    .WriteTo.File(
        path: "logs/constraint-mcp-server-.ndjson",
        formatter: new Serilog.Formatting.Compact.CompactJsonFormatter(),
        rollingInterval: RollingInterval.Day)
    .CreateLogger();

try
{
    // Handle command line arguments (preserve existing behavior)
    if (args.Length > 0)
    {
        if (args[0] == "--help" || args[0] == "-h")
        {
            await DisplayHelpMessage();
            return 0;
        }
        else if (args[0] == "--version" || args[0] == "-v")
        {
            await DisplayVersionMessage();
            return 0;
        }
    }

    // Create and configure host with new infrastructure
    var host = Host.CreateDefaultBuilder(args)
        .UseSerilog()
        .ConfigureServices(services =>
        {
            // Register all constraint MCP services automatically
            services.AddConstraintMcpServices();
        })
        .Build();

    // Start the application
    Log.Information("Starting Constraint MCP Server with new infrastructure");

    var mcpServerHost = host.Services.GetRequiredService<IMcpServerHost>();

    using var cancellationTokenSource = new CancellationTokenSource();
    Console.CancelKeyPress += (_, e) =>
    {
        e.Cancel = true;
        cancellationTokenSource.Cancel();
    };

    await mcpServerHost.StartAsync(cancellationTokenSource.Token);

    Log.Information("Constraint MCP Server stopped gracefully");
    return 0;
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
    return 1;
}
finally
{
    Log.CloseAndFlush();
}

static async Task DisplayHelpMessage()
{
    const string helpText = """
        Constraint MCP Server - Keeps LLM coding agents aligned during code generation

        Usage: dotnet run [--help|--version]

        Options:
          --help, -h       Show this help message
          --version, -v    Show version information

        The server implements the Model Context Protocol (MCP) for constraint enforcement.
        It enhances tool calls with software development best practices and methodologies.
        """;

    await Console.Out.WriteLineAsync(helpText);
}

static async Task DisplayVersionMessage()
{
    const string versionText = """
        Constraint MCP Server 0.1.0
        Constraint Enforcement MCP Server - keeps LLM coding agents aligned
        """;

    await Console.Out.WriteLineAsync(versionText);
}
```

### Step 6: Validation and Quality Gates (Day 10)

#### 6.1 Run Complete Test Suite

**Execute Validation Commands:**
```bash
# 1. Build with new infrastructure
dotnet build --configuration Release

# 2. Run all 386 tests to ensure no regression
dotnet test --configuration Release --verbosity normal

# 3. Run performance benchmarks
dotnet run --project tests/ConstraintMcpServer.Performance

# 4. Execute quality gates
./scripts/quality-gates.sh

# 5. Validate MCP protocol compliance
echo '{"jsonrpc":"2.0","id":1,"method":"initialize","params":{}}' | dotnet run
```

#### 6.2 Performance Verification

**Validate Performance Requirements:**
```bash
# Memory usage validation
dotnet run &
PID=$!
sleep 5
ps -o pid,rss $PID
# Should show RSS < 100MB

# Latency validation (requires load testing tool)
# This validates the <50ms p95 requirement
./scripts/load-test-latency.sh

kill $PID
```

## CONTRACT Phase: Remove Old Code

### Step 7: Cleanup Legacy Implementation (Day 11)

#### 7.1 Remove Legacy Code Patterns

**Tasks:**
1. **Remove inline MCP protocol handling** from old Program.cs
2. **Delete temporary compatibility shims** used during migration
3. **Remove old configuration loading code** replaced by infrastructure
4. **Clean up unused using statements** and references

#### 7.2 Final Architecture Validation

**Create Architecture Tests:**
```csharp
[Test]
public void Infrastructure_ShouldNotDependOnApplication()
{
    var infrastructureAssembly = typeof(McpServerHost).Assembly;
    var applicationTypes = Types.InAssembly(infrastructureAssembly)
        .That().ResideInNamespace("ConstraintMcpServer.Infrastructure");

    applicationTypes.Should()
        .NotHaveDependencyOn("ConstraintMcpServer.Application")
        .Because("infrastructure should not depend on application layer");
}

[Test]
public void Application_ShouldNotDependOnInfrastructure()
{
    var applicationTypes = Types.InAssembly(typeof(IConstraintService).Assembly)
        .That().ResideInNamespace("ConstraintMcpServer.Application");

    applicationTypes.Should()
        .NotHaveDependencyOn("ConstraintMcpServer.Infrastructure")
        .Because("application should depend only on abstractions");
}
```

## Quality Assurance Checklist

### Functional Requirements ✅
- [ ] All 386 tests pass without modification
- [ ] MCP protocol compliance maintained
- [ ] Help/version command line arguments work
- [ ] Constraint enhancement functionality preserved
- [ ] Error handling and logging functional

### Performance Requirements ✅
- [ ] P95 request latency ≤ 50ms
- [ ] Memory usage ≤ 100MB RSS
- [ ] Service resolution < 1ms
- [ ] Container startup < 200ms
- [ ] No performance regression in any measured metric

### Architecture Requirements ✅
- [ ] Hexagonal architecture implemented
- [ ] Clear port/adapter boundaries
- [ ] Dependency injection working
- [ ] Infrastructure → Application → Domain dependency flow
- [ ] All services registered correctly

### Code Quality Requirements ✅
- [ ] Zero build warnings
- [ ] Static analysis passes
- [ ] XML documentation for public APIs
- [ ] Consistent code formatting
- [ ] Error handling patterns implemented

### Documentation Requirements ✅
- [ ] Architecture decisions documented
- [ ] Implementation guidance complete
- [ ] Performance characteristics documented
- [ ] API documentation updated
- [ ] Migration process recorded

## Rollback Procedures

### Emergency Rollback Plan

If critical issues are discovered during migration:

```bash
# 1. Stop current server
pkill -f ConstraintMcpServer

# 2. Revert to backup branch
git checkout modernization-backup

# 3. Rebuild and test
dotnet build
./scripts/quality-gates.sh

# 4. Restart with original implementation
dotnet run
```

### Partial Rollback Options

1. **Configuration Issues**: Revert configuration loading while keeping DI
2. **Performance Issues**: Disable performance monitoring temporarily
3. **Service Registration Issues**: Fall back to manual service registration

### Recovery Validation

After any rollback:
```bash
# Validate all functionality works
./scripts/comprehensive-validation.sh

# Confirm performance requirements met
./scripts/performance-validation.sh

# Verify 386 tests still pass
dotnet test --logger trx
```

This implementation guidance provides a comprehensive, step-by-step approach to transforming the monolithic Program.cs into a proper hexagonal architecture while maintaining all quality attributes and preserving the 386-test safety net.