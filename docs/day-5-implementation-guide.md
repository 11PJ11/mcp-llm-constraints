# Day 5-6 Configuration Implementation Guide

## Implementation Overview

This guide provides step-by-step instructions for implementing the configuration infrastructure modernization using Outside-In TDD methodology.

## Prerequisites Validation

### Current System Status Check
```bash
# Verify current system is working
dotnet test  # Should show 399+ tests passing
dotnet build --configuration Release  # Should succeed
./scripts/quality-gates.sh  # Should pass all quality gates
```

### Expected Starting State
- ✅ ModuleLoader and IServiceInstaller pattern working
- ✅ HostingModule registering IConsoleAbstraction and IHostingService
- ✅ McpServerHost using DI container
- ✅ All tests passing (399+)

## Day 5 Implementation (Configuration Service Foundation)

### Phase 1: Create Configuration Service Interfaces (Morning)

#### Step 1.1: Create IConfigurationService Interface

**File**: `src/ConstraintMcpServer/Infrastructure/Configuration/IConfigurationService.cs`

```csharp
using System.Threading.Tasks;

namespace ConstraintMcpServer.Infrastructure.Configuration;

/// <summary>
/// Central configuration service providing unified access to all application configuration.
/// Integrates YAML files, environment variables, and command-line arguments.
/// </summary>
public interface IConfigurationService
{
    /// <summary>
    /// Gets server configuration metadata (version, protocol, name).
    /// </summary>
    IServerConfiguration ServerConfiguration { get; }

    /// <summary>
    /// Gets the loaded constraint pack configuration.
    /// Null if no configuration source was provided or loading failed.
    /// </summary>
    ConstraintPack? ConstraintPack { get; }

    /// <summary>
    /// Gets command-line options parsed from startup arguments.
    /// </summary>
    CommandLineOptions CommandLineOptions { get; }

    /// <summary>
    /// Gets the configuration file path if specified.
    /// </summary>
    string? ConfigurationPath { get; }

    /// <summary>
    /// Gets whether the configuration service has been initialized.
    /// </summary>
    bool IsInitialized { get; }

    /// <summary>
    /// Gets the time when configuration was last loaded.
    /// </summary>
    DateTime LastLoadTime { get; }

    /// <summary>
    /// Initializes configuration from command-line arguments and other sources.
    /// </summary>
    /// <param name="args">Command-line arguments to process.</param>
    /// <exception cref="ConfigurationException">Thrown when configuration loading fails.</exception>
    Task InitializeAsync(string[] args);

    /// <summary>
    /// Validates all loaded configuration and returns validation results.
    /// </summary>
    /// <returns>Configuration validation results with success/failure status.</returns>
    ConfigurationValidationResult ValidateConfiguration();

    /// <summary>
    /// Reloads configuration from the current sources.
    /// Used for hot-reload scenarios (future enhancement).
    /// </summary>
    /// <returns>True if reload succeeded, false otherwise.</returns>
    Task<bool> ReloadAsync();
}
```

#### Step 1.2: Create Configuration Exception Classes

**File**: `src/ConstraintMcpServer/Infrastructure/Configuration/ConfigurationException.cs`

```csharp
using System;
using System.Collections.Generic;

namespace ConstraintMcpServer.Infrastructure.Configuration;

/// <summary>
/// Base exception for all configuration-related errors.
/// </summary>
public class ConfigurationException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ConfigurationException"/> class.
    /// </summary>
    /// <param name="message">The error message.</param>
    public ConfigurationException(string message) : base(message) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="ConfigurationException"/> class.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="innerException">The inner exception.</param>
    public ConfigurationException(string message, Exception innerException) : base(message, innerException) { }
}

/// <summary>
/// Thrown when configuration file cannot be found or accessed.
/// </summary>
public sealed class ConfigurationFileException : ConfigurationException
{
    /// <summary>
    /// Gets the file path that caused the error.
    /// </summary>
    public string FilePath { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ConfigurationFileException"/> class.
    /// </summary>
    /// <param name="filePath">The file path that caused the error.</param>
    /// <param name="message">The error message.</param>
    public ConfigurationFileException(string filePath, string message)
        : base($"Configuration file error at '{filePath}': {message}")
    {
        FilePath = filePath;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ConfigurationFileException"/> class.
    /// </summary>
    /// <param name="filePath">The file path that caused the error.</param>
    /// <param name="message">The error message.</param>
    /// <param name="innerException">The inner exception.</param>
    public ConfigurationFileException(string filePath, string message, Exception innerException)
        : base($"Configuration file error at '{filePath}': {message}", innerException)
    {
        FilePath = filePath;
    }
}

/// <summary>
/// Thrown when configuration validation fails.
/// </summary>
public sealed class ConfigurationValidationException : ConfigurationException
{
    /// <summary>
    /// Gets the validation errors that occurred.
    /// </summary>
    public IReadOnlyList<string> ValidationErrors { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ConfigurationValidationException"/> class.
    /// </summary>
    /// <param name="errors">The validation errors.</param>
    public ConfigurationValidationException(IReadOnlyList<string> errors)
        : base($"Configuration validation failed: {string.Join(", ", errors)}")
    {
        ValidationErrors = errors;
    }
}
```

#### Step 1.3: Create Configuration Validation Result

**File**: `src/ConstraintMcpServer/Infrastructure/Configuration/ConfigurationValidationResult.cs`

```csharp
using System;
using System.Collections.Generic;
using System.Linq;

namespace ConstraintMcpServer.Infrastructure.Configuration;

/// <summary>
/// Result of configuration validation with detailed error and warning information.
/// </summary>
public sealed class ConfigurationValidationResult
{
    /// <summary>
    /// Gets a value indicating whether the configuration is valid.
    /// </summary>
    public bool IsValid { get; }

    /// <summary>
    /// Gets the validation errors that prevent the configuration from being valid.
    /// </summary>
    public IReadOnlyList<ConfigurationError> Errors { get; }

    /// <summary>
    /// Gets the validation warnings that indicate potential issues.
    /// </summary>
    public IReadOnlyList<ConfigurationWarning> Warnings { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ConfigurationValidationResult"/> class.
    /// </summary>
    /// <param name="isValid">Whether the configuration is valid.</param>
    /// <param name="errors">The validation errors.</param>
    /// <param name="warnings">The validation warnings.</param>
    private ConfigurationValidationResult(
        bool isValid,
        IReadOnlyList<ConfigurationError> errors,
        IReadOnlyList<ConfigurationWarning> warnings)
    {
        IsValid = isValid;
        Errors = errors;
        Warnings = warnings;
    }

    /// <summary>
    /// Creates a successful validation result.
    /// </summary>
    /// <param name="warnings">Optional warnings to include.</param>
    /// <returns>A successful validation result.</returns>
    public static ConfigurationValidationResult Success(params ConfigurationWarning[] warnings) =>
        new(true, Array.Empty<ConfigurationError>(), warnings);

    /// <summary>
    /// Creates a failed validation result.
    /// </summary>
    /// <param name="errors">The validation errors.</param>
    /// <returns>A failed validation result.</returns>
    public static ConfigurationValidationResult Failure(params ConfigurationError[] errors) =>
        new(false, errors, Array.Empty<ConfigurationWarning>());

    /// <summary>
    /// Creates a failed validation result with warnings.
    /// </summary>
    /// <param name="errors">The validation errors.</param>
    /// <param name="warnings">The validation warnings.</param>
    /// <returns>A failed validation result.</returns>
    public static ConfigurationValidationResult Failure(
        IEnumerable<ConfigurationError> errors,
        IEnumerable<ConfigurationWarning> warnings) =>
        new(false, errors.ToList(), warnings.ToList());
}

/// <summary>
/// Represents a configuration validation error.
/// </summary>
/// <param name="Message">The error message.</param>
/// <param name="Source">The source of the error (e.g., file path).</param>
/// <param name="LineNumber">The line number where the error occurred (if applicable).</param>
public sealed record ConfigurationError(string Message, string? Source = null, int? LineNumber = null);

/// <summary>
/// Represents a configuration validation warning.
/// </summary>
/// <param name="Message">The warning message.</param>
/// <param name="Source">The source of the warning (e.g., file path).</param>
public sealed record ConfigurationWarning(string Message, string? Source = null);
```

### Phase 2: Create Configuration Service Implementation (Mid-Morning)

#### Step 2.1: Create ConfigurationService Implementation

**File**: `src/ConstraintMcpServer/Infrastructure/Configuration/ConfigurationService.cs`

```csharp
using System;
using System.IO;
using System.Threading.Tasks;

namespace ConstraintMcpServer.Infrastructure.Configuration;

/// <summary>
/// Production implementation of configuration service with multi-source loading.
/// Integrates command-line arguments, environment variables, and configuration files.
/// </summary>
internal sealed class ConfigurationService : IConfigurationService
{
    private readonly IConfigurationLoader _configurationLoader;
    private readonly IConstraintValidator _constraintValidator;
    private readonly ICommandLineParser _commandLineParser;
    private readonly IServerConfiguration _serverConfiguration;

    // Configuration state
    private CommandLineOptions _commandLineOptions;
    private ConstraintPack? _constraintPack;
    private string? _configurationPath;
    private DateTime _lastLoadTime;

    /// <summary>
    /// Initializes a new instance of the <see cref="ConfigurationService"/> class.
    /// </summary>
    /// <param name="configurationLoader">The configuration loader to use.</param>
    /// <param name="constraintValidator">The constraint validator to use.</param>
    /// <param name="commandLineParser">The command-line parser to use.</param>
    /// <param name="serverConfiguration">The server configuration to use.</param>
    public ConfigurationService(
        IConfigurationLoader configurationLoader,
        IConstraintValidator constraintValidator,
        ICommandLineParser commandLineParser,
        IServerConfiguration serverConfiguration)
    {
        _configurationLoader = configurationLoader;
        _constraintValidator = constraintValidator;
        _commandLineParser = commandLineParser;
        _serverConfiguration = serverConfiguration;
        _commandLineOptions = CommandLineOptions.Default();
        _lastLoadTime = DateTime.MinValue;
    }

    /// <inheritdoc />
    public IServerConfiguration ServerConfiguration => _serverConfiguration;

    /// <inheritdoc />
    public ConstraintPack? ConstraintPack => _constraintPack;

    /// <inheritdoc />
    public CommandLineOptions CommandLineOptions => _commandLineOptions;

    /// <inheritdoc />
    public string? ConfigurationPath => _configurationPath;

    /// <inheritdoc />
    public bool IsInitialized { get; private set; }

    /// <inheritdoc />
    public DateTime LastLoadTime => _lastLoadTime;

    /// <inheritdoc />
    public async Task InitializeAsync(string[] args)
    {
        try
        {
            _lastLoadTime = DateTime.UtcNow;

            // Phase 1: Parse command-line arguments (highest priority)
            _commandLineOptions = _commandLineParser.Parse(args);

            // Phase 2: Resolve configuration file path from multiple sources
            _configurationPath = ResolveConfigurationPath();

            // Phase 3: Load constraint pack if configuration path available
            if (!string.IsNullOrEmpty(_configurationPath))
            {
                _constraintPack = await LoadConstraintPackAsync(_configurationPath);
            }

            IsInitialized = true;
        }
        catch (Exception ex) when (!(ex is ConfigurationException))
        {
            throw new ConfigurationException($"Failed to initialize configuration: {ex.Message}", ex);
        }
    }

    /// <inheritdoc />
    public ConfigurationValidationResult ValidateConfiguration()
    {
        if (!IsInitialized)
        {
            return ConfigurationValidationResult.Failure(
                new ConfigurationError("Configuration service has not been initialized"));
        }

        var errors = new List<ConfigurationError>();
        var warnings = new List<ConfigurationWarning>();

        // Validate server configuration
        if (string.IsNullOrEmpty(ServerConfiguration.ServerName))
        {
            errors.Add(new ConfigurationError("Server name is required"));
        }

        if (string.IsNullOrEmpty(ServerConfiguration.ServerVersion))
        {
            errors.Add(new ConfigurationError("Server version is required"));
        }

        // Validate constraint pack if loaded
        if (_constraintPack != null)
        {
            try
            {
                _constraintValidator.Validate(_constraintPack);
            }
            catch (ValidationException ex)
            {
                errors.Add(new ConfigurationError($"Constraint pack validation failed: {ex.Message}"));
            }
        }
        else if (!string.IsNullOrEmpty(_configurationPath))
        {
            warnings.Add(new ConfigurationWarning(
                $"Configuration path specified but no constraint pack loaded: {_configurationPath}"));
        }

        return errors.Count > 0
            ? ConfigurationValidationResult.Failure(errors, warnings)
            : ConfigurationValidationResult.Success(warnings.ToArray());
    }

    /// <inheritdoc />
    public async Task<bool> ReloadAsync()
    {
        if (!IsInitialized || string.IsNullOrEmpty(_configurationPath))
        {
            return false;
        }

        try
        {
            var reloadedPack = await LoadConstraintPackAsync(_configurationPath);
            _constraintPack = reloadedPack;
            _lastLoadTime = DateTime.UtcNow;
            return true;
        }
        catch (Exception)
        {
            // Reload failed, keep existing configuration
            return false;
        }
    }

    private string? ResolveConfigurationPath()
    {
        // Priority 1: Command-line argument
        if (!string.IsNullOrEmpty(_commandLineOptions.ConfigurationPath))
        {
            return ResolveAbsolutePath(_commandLineOptions.ConfigurationPath);
        }

        // Priority 2: Environment variable
        var envPath = Environment.GetEnvironmentVariable("CONSTRAINT_CONFIG_PATH");
        if (!string.IsNullOrEmpty(envPath))
        {
            return ResolveAbsolutePath(envPath);
        }

        // Priority 3: Default configuration files
        var defaultPaths = new[]
        {
            "./config/constraints.yaml",     // Development default
            "/etc/constraint-mcp/constraints.yaml"  // Production default (Linux)
        };

        foreach (var path in defaultPaths)
        {
            if (File.Exists(path))
            {
                return Path.GetFullPath(path);
            }
        }

        return null;
    }

    private async Task<ConstraintPack?> LoadConstraintPackAsync(string configPath)
    {
        if (!File.Exists(configPath))
        {
            throw new ConfigurationFileException(configPath, "Configuration file not found");
        }

        try
        {
            return await _configurationLoader.LoadAsync(configPath);
        }
        catch (Exception ex) when (!(ex is ConfigurationException))
        {
            throw new ConfigurationFileException(configPath, $"Failed to load configuration: {ex.Message}", ex);
        }
    }

    private static string ResolveAbsolutePath(string path)
    {
        return Path.IsPathRooted(path) ? path : Path.GetFullPath(path);
    }
}
```

### Phase 3: Create Configuration Module (Late Morning)

#### Step 3.1: Create ConfigurationModule for DI Registration

**File**: `src/ConstraintMcpServer/Infrastructure/Configuration/ConfigurationModule.cs`

```csharp
using ConstraintMcpServer.Infrastructure.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace ConstraintMcpServer.Infrastructure.Configuration;

/// <summary>
/// Service installer for configuration-related dependencies.
/// Registers configuration services with appropriate lifetimes for DI container.
/// </summary>
public sealed class ConfigurationModule : IServiceInstaller
{
    /// <summary>
    /// Configuration services have priority 20 (after hosting services at 10).
    /// </summary>
    public int Priority => 20;

    /// <summary>
    /// Registers all configuration services with the dependency injection container.
    /// </summary>
    /// <param name="services">The service collection to register services with.</param>
    public void RegisterServices(IServiceCollection services)
    {
        // Server configuration and metadata
        services.AddSingleton<IServerConfiguration, ServerConfiguration>();

        // Command-line argument processing
        services.AddSingleton<ICommandLineParser, CommandLineParser>();

        // YAML constraint pack loading infrastructure
        services.AddSingleton<IConstraintPackReader, YamlConstraintPackReader>();
        services.AddSingleton<IConstraintValidator, ConstraintPackValidator>();
        services.AddSingleton<IConfigurationLoader, ConfigurationLoader>();

        // Central configuration service (main integration point)
        services.AddSingleton<IConfigurationService, ConfigurationService>();
    }
}
```

### Phase 4: Update McpServerHost Integration (Early Afternoon)

#### Step 4.1: Update McpServerHost to Use IConfigurationService

**File**: `src/ConstraintMcpServer/Infrastructure/Hosting/McpServerHost.cs` (Update existing)

**Before updating, read the current file:**

```bash
# Read current implementation to understand what needs to be preserved
```

**Updated implementation** (add IConfigurationService integration):

```csharp
using System;
using System.Linq;
using System.Threading.Tasks;
using ConstraintMcpServer.Infrastructure.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ConstraintMcpServer.Infrastructure.Hosting;

/// <summary>
/// Hosts the MCP server with configuration service integration and dependency injection.
/// Replaces static Program.cs logic with modern service-based approach.
/// </summary>
public sealed class McpServerHost : IHostingService
{
    private const string ServerName = "Constraint MCP Server";
    private const string ProductDescription = "Constraint Enforcement MCP Server - keeps LLM coding agents aligned during code generation with composable software-craft constraints";

    private readonly IConsoleAbstraction _console;
    private readonly IConfigurationService _configurationService;
    private readonly IServiceProvider _serviceProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="McpServerHost"/> class.
    /// </summary>
    /// <param name="console">Console abstraction for I/O operations.</param>
    /// <param name="configurationService">Configuration service for application settings.</param>
    /// <param name="serviceProvider">Service provider for dependency resolution.</param>
    public McpServerHost(
        IConsoleAbstraction console,
        IConfigurationService configurationService,
        IServiceProvider serviceProvider)
    {
        _console = console;
        _configurationService = configurationService;
        _serviceProvider = serviceProvider;
    }

    /// <inheritdoc />
    public async Task<int> RunAsync(string[] args)
    {
        try
        {
            // Phase 1: Initialize configuration from all sources
            await _configurationService.InitializeAsync(args);

            // Phase 2: Handle special commands (help, version)
            if (await HandleSpecialCommandsAsync())
            {
                return 0;
            }

            // Phase 3: Validate configuration
            var validationResult = _configurationService.ValidateConfiguration();
            if (!validationResult.IsValid)
            {
                await DisplayConfigurationErrorsAsync(validationResult);
                return 1;
            }

            // Phase 4: Display configuration warnings if any
            if (validationResult.Warnings.Count > 0)
            {
                await DisplayConfigurationWarningsAsync(validationResult);
            }

            // Phase 5: Start MCP server (future implementation)
            // For now, return success as this is the walking skeleton phase
            await _console.WriteLineAsync("Configuration loaded successfully. MCP server ready.");
            return 0;
        }
        catch (ConfigurationException ex)
        {
            await _console.WriteLineAsync($"Configuration error: {ex.Message}");
            return 1;
        }
        catch (Exception ex)
        {
            await _console.WriteLineAsync($"Server error: {ex.Message}");
            return 1;
        }
    }

    private async Task<bool> HandleSpecialCommandsAsync()
    {
        var options = _configurationService.CommandLineOptions;

        if (options.ShowHelp)
        {
            await DisplayHelpMessageAsync();
            return true;
        }

        if (options.ShowVersion)
        {
            await DisplayVersionMessageAsync();
            return true;
        }

        return false;
    }

    private async Task DisplayHelpMessageAsync()
    {
        var serverConfig = _configurationService.ServerConfiguration;
        await _console.WriteLineAsync($"{ServerName} - {ProductDescription}");
        await _console.WriteLineAsync("Usage: dotnet run [--help|--version] [--config <path>]");
        await _console.WriteLineAsync("  --help, -h       Show this help message");
        await _console.WriteLineAsync("  --version, -v    Show version information");
        await _console.WriteLineAsync("  --config <path>  Load configuration from specified YAML file");
        await _console.WriteLineAsync("");
        await _console.WriteLineAsync($"Server Version: {serverConfig.ServerVersion}");
        await _console.WriteLineAsync($"Protocol Version: {serverConfig.ProtocolVersion}");
    }

    private async Task DisplayVersionMessageAsync()
    {
        var serverConfig = _configurationService.ServerConfiguration;
        await _console.WriteLineAsync($"{ServerName} {serverConfig.ServerVersion}");
        await _console.WriteLineAsync($"{ProductDescription}");
        await _console.WriteLineAsync($"Protocol Version: {serverConfig.ProtocolVersion}");
    }

    private async Task DisplayConfigurationErrorsAsync(ConfigurationValidationResult result)
    {
        await _console.WriteLineAsync("Configuration validation failed:");
        foreach (var error in result.Errors)
        {
            var location = error.Source != null
                ? error.LineNumber.HasValue
                    ? $" at {error.Source}:{error.LineNumber}"
                    : $" in {error.Source}"
                : string.Empty;

            await _console.WriteLineAsync($"  Error{location}: {error.Message}");
        }
    }

    private async Task DisplayConfigurationWarningsAsync(ConfigurationValidationResult result)
    {
        await _console.WriteLineAsync("Configuration warnings:");
        foreach (var warning in result.Warnings)
        {
            var location = warning.Source != null ? $" in {warning.Source}" : string.Empty;
            await _console.WriteLineAsync($"  Warning{location}: {warning.Message}");
        }
        await _console.WriteLineAsync("");
    }
}
```

#### Step 4.2: Update HostingModule to Register New Dependencies

**File**: `src/ConstraintMcpServer/Infrastructure/Hosting/HostingModule.cs` (Update existing)

```csharp
using ConstraintMcpServer.Infrastructure.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace ConstraintMcpServer.Infrastructure.Hosting;

/// <summary>
/// Service installer for hosting-related dependencies.
/// </summary>
public sealed class HostingModule : IServiceInstaller
{
    /// <summary>
    /// Gets the priority for this service installer.
    /// Lower values indicate higher priority (executed first).
    /// </summary>
    public int Priority => 10;

    /// <summary>
    /// Registers hosting services with the dependency injection container.
    /// </summary>
    /// <param name="services">The service collection to register services with.</param>
    public void RegisterServices(IServiceCollection services)
    {
        // Console I/O abstraction
        services.AddTransient<IConsoleAbstraction, ConsoleAbstraction>();

        // Main hosting service
        services.AddTransient<IHostingService, McpServerHost>();
    }
}
```

### Phase 5: Create Unit Tests (Late Afternoon)

#### Step 5.1: Create ConfigurationService Unit Tests

**File**: `tests/ConstraintMcpServer.Tests/Unit/Configuration/ConfigurationServiceShould.cs`

```csharp
using System;
using System.Threading.Tasks;
using ConstraintMcpServer.Infrastructure.Configuration;
using FluentAssertions;
using NSubstitute;
using NUnit.Framework;

namespace ConstraintMcpServer.Tests.Unit.Configuration;

[TestFixture]
public class ConfigurationServiceShould
{
    private IConfigurationLoader _configurationLoader;
    private IConstraintValidator _constraintValidator;
    private ICommandLineParser _commandLineParser;
    private IServerConfiguration _serverConfiguration;
    private ConfigurationService _configurationService;

    [SetUp]
    public void SetUp()
    {
        _configurationLoader = Substitute.For<IConfigurationLoader>();
        _constraintValidator = Substitute.For<IConstraintValidator>();
        _commandLineParser = Substitute.For<ICommandLineParser>();
        _serverConfiguration = Substitute.For<IServerConfiguration>();

        _serverConfiguration.ServerName.Returns("Test Server");
        _serverConfiguration.ServerVersion.Returns("1.0.0");
        _serverConfiguration.ProtocolVersion.Returns("2024-11-05");

        _configurationService = new ConfigurationService(
            _configurationLoader,
            _constraintValidator,
            _commandLineParser,
            _serverConfiguration);
    }

    [Test]
    public async Task InitializeSuccessfully_WhenNoArgumentsProvided()
    {
        // Given: Empty command-line arguments
        var args = Array.Empty<string>();
        var defaultOptions = CommandLineOptions.Default();
        _commandLineParser.Parse(args).Returns(defaultOptions);

        // When: Initializing configuration
        await _configurationService.InitializeAsync(args);

        // Then: Configuration should be initialized with defaults
        _configurationService.IsInitialized.Should().BeTrue();
        _configurationService.CommandLineOptions.Should().Be(defaultOptions);
        _configurationService.ConstraintPack.Should().BeNull();
        _configurationService.ConfigurationPath.Should().BeNull();
        _configurationService.LastLoadTime.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Test]
    public async Task LoadConfigurationFromCommandLineArguments()
    {
        // Given: Command-line arguments with config path
        var args = new[] { "--config", "./test-config.yaml" };
        var options = CommandLineOptions.WithConfiguration("./test-config.yaml");
        var expectedPack = CreateTestConstraintPack();

        _commandLineParser.Parse(args).Returns(options);
        _configurationLoader.LoadAsync(Arg.Any<string>()).Returns(expectedPack);

        // When: Initializing configuration
        await _configurationService.InitializeAsync(args);

        // Then: Configuration should be loaded from specified path
        _configurationService.IsInitialized.Should().BeTrue();
        _configurationService.ConstraintPack.Should().Be(expectedPack);
        _configurationService.ConfigurationPath.Should().NotBeNull();
        _configurationService.CommandLineOptions.ConfigurationPath.Should().Be("./test-config.yaml");
    }

    [Test]
    public async Task HandleConfigurationLoadingError_ThrowsConfigurationException()
    {
        // Given: Command-line arguments with invalid config path
        var args = new[] { "--config", "./invalid-config.yaml" };
        var options = CommandLineOptions.WithConfiguration("./invalid-config.yaml");

        _commandLineParser.Parse(args).Returns(options);
        _configurationLoader.LoadAsync(Arg.Any<string>())
            .ThrowsAsync(new System.IO.FileNotFoundException("File not found"));

        // When/Then: Initializing configuration should throw ConfigurationException
        var exception = await FluentActions
            .Invoking(() => _configurationService.InitializeAsync(args))
            .Should()
            .ThrowAsync<ConfigurationFileException>();

        exception.Which.FilePath.Should().Contain("invalid-config.yaml");
    }

    [Test]
    public void ValidateConfiguration_ReturnsSuccess_WhenConfigurationValid()
    {
        // Given: Initialized configuration service with valid setup
        _configurationService.GetType()
            .GetProperty("IsInitialized", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?.SetValue(_configurationService, true);

        // When: Validating configuration
        var result = _configurationService.ValidateConfiguration();

        // Then: Validation should succeed
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Test]
    public void ValidateConfiguration_ReturnsFailure_WhenNotInitialized()
    {
        // Given: Uninitialized configuration service
        // (default state)

        // When: Validating configuration
        var result = _configurationService.ValidateConfiguration();

        // Then: Validation should fail
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(1);
        result.Errors[0].Message.Should().Contain("not been initialized");
    }

    private static ConstraintPack CreateTestConstraintPack()
    {
        return new ConstraintPack
        {
            Version = "0.2.0",
            Constraints = new List<Constraint>
            {
                new()
                {
                    Id = "test.constraint",
                    Title = "Test Constraint",
                    Priority = 0.8,
                    WorkflowContexts = new[] { "test-context" },
                    Reminders = new[] { "Test reminder" }
                }
            }
        };
    }
}
```

#### Step 5.2: Create ConfigurationModule Unit Tests

**File**: `tests/ConstraintMcpServer.Tests/Unit/Configuration/ConfigurationModuleShould.cs`

```csharp
using ConstraintMcpServer.Infrastructure.Configuration;
using ConstraintMcpServer.Infrastructure.DependencyInjection;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace ConstraintMcpServer.Tests.Unit.Configuration;

[TestFixture]
public class ConfigurationModuleShould
{
    private ConfigurationModule _configurationModule;
    private ServiceCollection _services;

    [SetUp]
    public void SetUp()
    {
        _configurationModule = new ConfigurationModule();
        _services = new ServiceCollection();
    }

    [Test]
    public void HaveCorrectPriority()
    {
        // Given: ConfigurationModule instance
        // When: Getting priority
        var priority = _configurationModule.Priority;

        // Then: Priority should be 20 (after hosting services)
        priority.Should().Be(20);
    }

    [Test]
    public void RegisterAllRequiredServices()
    {
        // Given: Empty service collection
        // When: Registering services
        _configurationModule.RegisterServices(_services);

        // Then: All configuration services should be registered
        var serviceProvider = _services.BuildServiceProvider();

        serviceProvider.GetService<IServerConfiguration>().Should().NotBeNull();
        serviceProvider.GetService<ICommandLineParser>().Should().NotBeNull();
        serviceProvider.GetService<IConstraintPackReader>().Should().NotBeNull();
        serviceProvider.GetService<IConstraintValidator>().Should().NotBeNull();
        serviceProvider.GetService<IConfigurationLoader>().Should().NotBeNull();
        serviceProvider.GetService<IConfigurationService>().Should().NotBeNull();
    }

    [Test]
    public void RegisterConfigurationServiceAsSingleton()
    {
        // Given: Service collection with configuration module
        _configurationModule.RegisterServices(_services);
        var serviceProvider = _services.BuildServiceProvider();

        // When: Resolving IConfigurationService multiple times
        var service1 = serviceProvider.GetRequiredService<IConfigurationService>();
        var service2 = serviceProvider.GetRequiredService<IConfigurationService>();

        // Then: Should return the same instance (singleton)
        service1.Should().BeSameAs(service2);
    }

    [Test]
    public void CreateWorkingConfigurationService()
    {
        // Given: Service collection with all dependencies
        _configurationModule.RegisterServices(_services);
        var serviceProvider = _services.BuildServiceProvider();

        // When: Resolving configuration service
        var configurationService = serviceProvider.GetRequiredService<IConfigurationService>();

        // Then: Service should be properly constructed
        configurationService.Should().NotBeNull();
        configurationService.Should().BeOfType<ConfigurationService>();
        configurationService.IsInitialized.Should().BeFalse(); // Not initialized yet
    }
}
```

### Phase 6: Integration Testing (End of Day 5)

#### Step 6.1: Create Integration Test

**File**: `tests/ConstraintMcpServer.Tests/Integration/Configuration/ConfigurationIntegrationShould.cs`

```csharp
using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using ConstraintMcpServer.Infrastructure.Configuration;
using ConstraintMcpServer.Infrastructure.DependencyInjection;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace ConstraintMcpServer.Tests.Integration.Configuration;

[TestFixture]
public class ConfigurationIntegrationShould
{
    private IServiceProvider _serviceProvider;
    private string _testDataDirectory;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        var services = new ServiceCollection();
        services.RegisterModulesFromAssembly(Assembly.GetExecutingAssembly());
        _serviceProvider = services.BuildServiceProvider();

        _testDataDirectory = Path.Combine(TestContext.CurrentContext.TestDirectory, "TestData");
        Directory.CreateDirectory(_testDataDirectory);
    }

    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        (_serviceProvider as IDisposable)?.Dispose();
    }

    [SetUp]
    public void SetUp()
    {
        // Create test configuration file
        var testConfigContent = @"
version: ""0.2.0""
constraints:
  - id: test.constraint
    title: ""Test Constraint""
    priority: 0.8
    workflow_contexts: [""test-context""]
    reminders:
      - ""Test reminder""
";
        var testConfigPath = Path.Combine(_testDataDirectory, "test-config.yaml");
        File.WriteAllText(testConfigPath, testConfigContent);
    }

    [TearDown]
    public void TearDown()
    {
        // Clean up test files
        if (Directory.Exists(_testDataDirectory))
        {
            Directory.Delete(_testDataDirectory, true);
            Directory.CreateDirectory(_testDataDirectory);
        }
    }

    [Test]
    public async Task LoadConfigurationSuccessfully_WithValidYamlFile()
    {
        // Given: Service provider with all configuration services
        var configurationService = _serviceProvider.GetRequiredService<IConfigurationService>();
        var testConfigPath = Path.Combine(_testDataDirectory, "test-config.yaml");
        var args = new[] { "--config", testConfigPath };

        // When: Initializing configuration with valid YAML file
        await configurationService.InitializeAsync(args);

        // Then: Configuration should be loaded successfully
        configurationService.IsInitialized.Should().BeTrue();
        configurationService.ConstraintPack.Should().NotBeNull();
        configurationService.ConstraintPack!.Version.Should().Be("0.2.0");
        configurationService.ConstraintPack.Constraints.Should().HaveCount(1);
        configurationService.ConfigurationPath.Should().EndWith("test-config.yaml");

        var validation = configurationService.ValidateConfiguration();
        validation.IsValid.Should().BeTrue();
    }

    [Test]
    public async Task HandleMissingConfigurationFile_GracefullyWithHelpfulError()
    {
        // Given: Service provider and non-existent config file
        var configurationService = _serviceProvider.GetRequiredService<IConfigurationService>();
        var missingConfigPath = Path.Combine(_testDataDirectory, "missing-config.yaml");
        var args = new[] { "--config", missingConfigPath };

        // When/Then: Should throw helpful configuration exception
        var exception = await FluentActions
            .Invoking(() => configurationService.InitializeAsync(args))
            .Should()
            .ThrowAsync<ConfigurationFileException>();

        exception.Which.FilePath.Should().Contain("missing-config.yaml");
        exception.Which.Message.Should().Contain("Configuration file not found");
    }

    [Test]
    public async Task UseDefaultConfiguration_WhenNoArgumentsProvided()
    {
        // Given: Service provider and empty arguments
        var configurationService = _serviceProvider.GetRequiredService<IConfigurationService>();
        var args = Array.Empty<string>();

        // When: Initializing with no arguments
        await configurationService.InitializeAsync(args);

        // Then: Should initialize with defaults
        configurationService.IsInitialized.Should().BeTrue();
        configurationService.ConstraintPack.Should().BeNull();
        configurationService.ConfigurationPath.Should().BeNull();
        configurationService.CommandLineOptions.ConfigurationPath.Should().BeNull();
        configurationService.CommandLineOptions.ShowHelp.Should().BeFalse();

        var validation = configurationService.ValidateConfiguration();
        validation.IsValid.Should().BeTrue();
    }
}
```

### Validation and Testing

#### Run Tests to Verify Implementation
```bash
# Build the solution
dotnet build

# Run all tests to ensure no regressions
dotnet test

# Verify quality gates
./scripts/quality-gates.sh
```

#### Expected Results After Day 5
- ✅ All existing tests still passing (399+)
- ✅ New configuration service tests passing
- ✅ ConfigurationModule properly registered in DI container
- ✅ McpServerHost using IConfigurationService
- ✅ Basic configuration loading working

## Day 6 Implementation (Advanced Configuration Features)

### Phase 1: Environment Variable Support (Morning)

[Continue with Day 6 implementation details...]

### Testing Strategy for Each Phase

#### Unit Test Requirements
- Test each service in isolation using mocks
- Focus on business behavior, not implementation details
- Use FluentAssertions for readable test assertions
- Follow AAA pattern (Arrange, Act, Assert)

#### Integration Test Requirements
- Test full configuration loading pipeline
- Use real YAML files and file system operations
- Test error scenarios with actual failure conditions
- Validate service registration and DI container integration

#### Performance Testing
- Configuration loading should complete in <10ms
- Memory usage should not exceed 1MB for typical configs
- No memory leaks during repeated initialization/disposal

## Success Criteria Validation

After completing Day 5 implementation:

### Functional Requirements
- ✅ IConfigurationService loads configuration from command-line arguments
- ✅ Configuration validation provides clear error messages
- ✅ McpServerHost uses DI-injected configuration service
- ✅ All command-line arguments work exactly as before
- ✅ Configuration exceptions provide helpful error details

### Quality Requirements
- ✅ All tests passing (include new configuration tests)
- ✅ Code coverage ≥95% for new configuration services
- ✅ No performance regression (startup time within 10% of baseline)
- ✅ Memory usage within acceptable limits
- ✅ Quality gates passing (formatting, analysis, build)

### Architecture Requirements
- ✅ Hexagonal architecture boundaries maintained
- ✅ DI container integration following established patterns
- ✅ Configuration services properly isolated and testable
- ✅ Clear separation between infrastructure and application concerns

This implementation guide provides a comprehensive, test-driven approach to implementing the configuration infrastructure while maintaining the high quality standards established in the previous modernization phases.