# Day 5-6 Configuration Architecture Design

## System Architecture Overview

### Current vs Target Architecture

#### Current Architecture (Static)
```
Program.cs (Static)
├── Hardcoded JSON-RPC handling
├── Embedded configuration constants
├── Static method routing
└── Direct console I/O

Configuration Classes (Isolated)
├── ServerConfiguration (unused in DI)
├── ConfigurationLoader (not integrated)
├── CommandLineOptions (not integrated)
└── YAML loading (standalone)
```

#### Target Architecture (Service-Based)
```
Program.cs (Minimal Entry Point)
└── ModuleLoader.ConfigureServices()
    ├── HostingModule (Priority 10)
    │   ├── IConsoleAbstraction → ConsoleAbstraction
    │   └── IHostingService → McpServerHost
    ├── ConfigurationModule (Priority 20) ← NEW
    │   ├── IConfigurationService → ConfigurationService
    │   ├── IConfigurationLoader → ConfigurationLoader
    │   ├── IServerConfiguration → ServerConfiguration
    │   └── ICommandLineParser → CommandLineParser
    └── McpServerHost
        ├── Dependencies: IConsoleAbstraction, IConfigurationService
        └── Delegates to: IMcpServer (from serviceProvider)
```

## Component Architecture

### 1. Core Configuration Components

#### IConfigurationService (Central Hub)
```csharp
namespace ConstraintMcpServer.Infrastructure.Configuration;

/// <summary>
/// Central configuration service providing unified access to all application configuration.
/// Acts as the single source of truth for configuration state.
/// </summary>
public interface IConfigurationService
{
    // Configuration State
    IServerConfiguration ServerConfiguration { get; }
    ConstraintPack? ConstraintPack { get; }
    CommandLineOptions CommandLineOptions { get; }
    string? ConfigurationPath { get; }

    // Lifecycle Management
    Task InitializeAsync(string[] args);
    ConfigurationValidationResult ValidateConfiguration();
    Task<bool> ReloadAsync(); // Future hot-reload support

    // Status Information
    bool IsInitialized { get; }
    DateTime LastLoadTime { get; }
    ConfigurationSource ActiveSources { get; }
}

[Flags]
public enum ConfigurationSource
{
    None = 0,
    CommandLine = 1,
    EnvironmentVariables = 2,
    ConfigurationFile = 4,
    Defaults = 8
}
```

#### ConfigurationService Implementation Strategy
```csharp
namespace ConstraintMcpServer.Infrastructure.Configuration;

/// <summary>
/// Production implementation with multi-source configuration loading.
/// </summary>
internal sealed class ConfigurationService : IConfigurationService
{
    // Dependencies (injected)
    private readonly IConfigurationLoader _configurationLoader;
    private readonly IConstraintValidator _constraintValidator;
    private readonly ICommandLineParser _commandLineParser;
    private readonly IServerConfiguration _serverConfiguration;

    // State (loaded during initialization)
    private CommandLineOptions _commandLineOptions = CommandLineOptions.Default();
    private ConstraintPack? _constraintPack;
    private ConfigurationSource _activeSources = ConfigurationSource.Defaults;
    private DateTime _lastLoadTime = DateTime.MinValue;

    // Initialization flow
    public async Task InitializeAsync(string[] args)
    {
        _lastLoadTime = DateTime.UtcNow;

        // 1. Parse command-line arguments (highest priority)
        _commandLineOptions = _commandLineParser.Parse(args);
        _activeSources |= ConfigurationSource.CommandLine;

        // 2. Load from environment variables
        await LoadFromEnvironmentVariables();

        // 3. Load configuration file if specified
        await LoadConfigurationFile();

        // 4. Apply defaults for missing configuration
        ApplyDefaults();

        IsInitialized = true;
    }

    private async Task LoadFromEnvironmentVariables()
    {
        var envConfigPath = Environment.GetEnvironmentVariable("CONSTRAINT_CONFIG_PATH");
        if (!string.IsNullOrEmpty(envConfigPath))
        {
            // Environment variable overrides command-line if not specified
            if (string.IsNullOrEmpty(_commandLineOptions.ConfigurationPath))
            {
                _commandLineOptions = _commandLineOptions with { ConfigurationPath = envConfigPath };
            }
            _activeSources |= ConfigurationSource.EnvironmentVariables;
        }
    }

    private async Task LoadConfigurationFile()
    {
        var configPath = _commandLineOptions.ConfigurationPath;
        if (!string.IsNullOrEmpty(configPath))
        {
            try
            {
                _constraintPack = await _configurationLoader.LoadAsync(configPath);
                _activeSources |= ConfigurationSource.ConfigurationFile;
            }
            catch (Exception ex)
            {
                throw new ConfigurationException($"Failed to load configuration from '{configPath}': {ex.Message}", ex);
            }
        }
    }
}
```

### 2. Configuration Module (DI Integration)

#### ConfigurationModule Implementation
```csharp
namespace ConstraintMcpServer.Infrastructure.Configuration;

/// <summary>
/// Service installer for configuration infrastructure.
/// Priority 20 ensures configuration loads after hosting but before application services.
/// </summary>
public sealed class ConfigurationModule : IServiceInstaller
{
    public int Priority => 20;

    public void RegisterServices(IServiceCollection services)
    {
        // Server metadata and versioning
        services.AddSingleton<IServerConfiguration, ServerConfiguration>();

        // Command-line processing
        services.AddSingleton<ICommandLineParser, CommandLineParser>();

        // YAML constraint pack loading
        services.AddSingleton<IConstraintPackReader, YamlConstraintPackReader>();
        services.AddSingleton<IConstraintValidator, ConstraintPackValidator>();
        services.AddSingleton<IConfigurationLoader, ConfigurationLoader>();

        // Configuration validation
        services.AddSingleton<IConfigurationValidator, ConfigurationValidator>();

        // Central configuration service (core integration point)
        services.AddSingleton<IConfigurationService, ConfigurationService>();
    }
}
```

### 3. Enhanced McpServerHost Integration

#### Updated McpServerHost with Configuration
```csharp
namespace ConstraintMcpServer.Infrastructure.Hosting;

/// <summary>
/// Modern hosting service with configuration and dependency injection integration.
/// Replaces static Program.cs logic with service-based approach.
/// </summary>
public sealed class McpServerHost : IHostingService
{
    private readonly IConsoleAbstraction _console;
    private readonly IConfigurationService _configurationService;
    private readonly IServiceProvider _serviceProvider;

    public McpServerHost(
        IConsoleAbstraction console,
        IConfigurationService configurationService,
        IServiceProvider serviceProvider)
    {
        _console = console;
        _configurationService = configurationService;
        _serviceProvider = serviceProvider;
    }

    public async Task<int> RunAsync(string[] args)
    {
        try
        {
            // Phase 1: Initialize configuration from all sources
            await _configurationService.InitializeAsync(args);

            // Phase 2: Handle special commands (help, version)
            if (await HandleSpecialCommands())
            {
                return 0;
            }

            // Phase 3: Validate configuration
            var validationResult = _configurationService.ValidateConfiguration();
            if (!validationResult.IsValid)
            {
                await DisplayConfigurationErrors(validationResult);
                return 1;
            }

            // Phase 4: Start MCP server with configured services
            var mcpServer = _serviceProvider.GetRequiredService<IMcpServer>();
            return await mcpServer.RunAsync();
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

    private async Task<bool> HandleSpecialCommands()
    {
        var options = _configurationService.CommandLineOptions;

        if (options.ShowHelp)
        {
            await DisplayHelpMessage();
            return true;
        }

        if (options.ShowVersion)
        {
            await DisplayVersionMessage();
            return true;
        }

        return false;
    }
}
```

## Configuration Loading Strategy

### Multi-Source Configuration Priority

#### Priority Order (Highest to Lowest)
```
1. Command-Line Arguments    (--config /path/to/file.yaml)
   ├── --help, -h            (Show help and exit)
   ├── --version, -v         (Show version and exit)
   └── --config <path>       (Configuration file path)

2. Environment Variables     (Process environment)
   ├── CONSTRAINT_CONFIG_PATH   (Override config file path)
   ├── DOTNET_ENVIRONMENT       (Development/Production mode)
   └── CONSTRAINT_LOG_LEVEL     (Future: Log level override)

3. Configuration Files       (YAML constraint packs)
   ├── Specified path           (From command-line or environment)
   ├── ./config/constraints.yaml (Default development)
   └── /etc/constraint-mcp/constraints.yaml (Default production)

4. Built-in Defaults         (Fallback values)
   ├── Empty constraint pack    (No constraints loaded)
   ├── Default server metadata  (Version, protocol info)
   └── Standard command options (Help, version flags)
```

#### Configuration Resolution Algorithm
```csharp
public async Task InitializeAsync(string[] args)
{
    // Step 1: Parse command-line arguments (immediate)
    var commandLineOptions = _commandLineParser.Parse(args);

    // Step 2: Check for special commands (exit early if needed)
    if (commandLineOptions.ShowHelp || commandLineOptions.ShowVersion)
    {
        return; // Early exit, no further loading needed
    }

    // Step 3: Resolve configuration file path (priority order)
    var configPath = ResolveConfigurationPath(commandLineOptions);

    // Step 4: Load constraint pack if path available
    ConstraintPack? constraintPack = null;
    if (!string.IsNullOrEmpty(configPath))
    {
        constraintPack = await _configurationLoader.LoadAsync(configPath);
    }

    // Step 5: Store resolved configuration
    UpdateConfigurationState(commandLineOptions, constraintPack, configPath);
}

private string? ResolveConfigurationPath(CommandLineOptions options)
{
    // Priority 1: Command-line argument
    if (!string.IsNullOrEmpty(options.ConfigurationPath))
    {
        return ResolveAbsolutePath(options.ConfigurationPath);
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
        "./config/constraints.yaml",           // Development default
        "/etc/constraint-mcp/constraints.yaml" // Production default (Linux)
    };

    return defaultPaths.FirstOrDefault(File.Exists);
}
```

## Error Handling & Validation

### Configuration Exception Hierarchy
```csharp
namespace ConstraintMcpServer.Infrastructure.Configuration;

/// <summary>
/// Base exception for all configuration-related errors.
/// </summary>
public class ConfigurationException : Exception
{
    public ConfigurationException(string message) : base(message) { }
    public ConfigurationException(string message, Exception innerException) : base(message, innerException) { }
}

/// <summary>
/// Thrown when configuration file cannot be found or accessed.
/// </summary>
public class ConfigurationFileException : ConfigurationException
{
    public string FilePath { get; }

    public ConfigurationFileException(string filePath, string message)
        : base($"Configuration file error at '{filePath}': {message}")
    {
        FilePath = filePath;
    }
}

/// <summary>
/// Thrown when configuration validation fails.
/// </summary>
public class ConfigurationValidationException : ConfigurationException
{
    public IReadOnlyList<string> ValidationErrors { get; }

    public ConfigurationValidationException(IReadOnlyList<string> errors)
        : base($"Configuration validation failed: {string.Join(", ", errors)}")
    {
        ValidationErrors = errors;
    }
}
```

### Validation Result Pattern
```csharp
namespace ConstraintMcpServer.Infrastructure.Configuration;

/// <summary>
/// Result of configuration validation with detailed error information.
/// </summary>
public sealed class ConfigurationValidationResult
{
    public bool IsValid { get; }
    public IReadOnlyList<ConfigurationError> Errors { get; }
    public IReadOnlyList<ConfigurationWarning> Warnings { get; }

    public static ConfigurationValidationResult Success() =>
        new(true, Array.Empty<ConfigurationError>(), Array.Empty<ConfigurationWarning>());

    public static ConfigurationValidationResult Failure(params ConfigurationError[] errors) =>
        new(false, errors, Array.Empty<ConfigurationWarning>());
}

public sealed record ConfigurationError(string Message, string? Source = null, int? LineNumber = null);
public sealed record ConfigurationWarning(string Message, string? Source = null);
```

## Service Integration Patterns

### Dependency Graph
```
IConfigurationService (Root)
├── IServerConfiguration (Server metadata)
├── ICommandLineParser (Argument processing)
├── IConfigurationLoader (File loading)
│   ├── IConstraintPackReader (YAML parsing)
│   └── IConstraintValidator (Validation logic)
└── IConfigurationValidator (Overall validation)

McpServerHost (Consumer)
├── IConsoleAbstraction (Console I/O)
├── IConfigurationService (Configuration access)
└── IServiceProvider (Service resolution)
```

### Service Lifetime Management
```csharp
// Singleton Services (Stateless, Configuration)
services.AddSingleton<IServerConfiguration, ServerConfiguration>();
services.AddSingleton<ICommandLineParser, CommandLineParser>();
services.AddSingleton<IConstraintPackReader, YamlConstraintPackReader>();
services.AddSingleton<IConstraintValidator, ConstraintPackValidator>();
services.AddSingleton<IConfigurationLoader, ConfigurationLoader>();
services.AddSingleton<IConfigurationService, ConfigurationService>();

// Transient Services (Stateful, Per-Request)
services.AddTransient<IConsoleAbstraction, ConsoleAbstraction>();
services.AddTransient<IHostingService, McpServerHost>();
```

## Performance Optimization

### Configuration Loading Performance

#### Caching Strategy
```csharp
/// <summary>
/// Cached configuration loader to avoid repeated YAML parsing.
/// </summary>
internal sealed class CachedConfigurationLoader : IConfigurationLoader
{
    private readonly IConfigurationLoader _innerLoader;
    private readonly IMemoryCache _cache;
    private readonly FileSystemWatcher? _fileWatcher;

    // Cache configuration based on file path and last modified time
    public async Task<ConstraintPack?> LoadAsync(string? configurationSource)
    {
        if (string.IsNullOrEmpty(configurationSource))
            return null;

        var cacheKey = GenerateCacheKey(configurationSource);

        if (_cache.TryGetValue(cacheKey, out ConstraintPack? cached))
        {
            return cached;
        }

        var loaded = await _innerLoader.LoadAsync(configurationSource);

        if (loaded != null)
        {
            var options = new MemoryCacheEntryOptions
            {
                SlidingExpiration = TimeSpan.FromMinutes(10),
                Priority = CacheItemPriority.High
            };

            _cache.Set(cacheKey, loaded, options);
        }

        return loaded;
    }

    private string GenerateCacheKey(string filePath)
    {
        var fileInfo = new FileInfo(filePath);
        return $"{filePath}:{fileInfo.LastWriteTimeUtc:O}";
    }
}
```

#### Performance Targets
- **Configuration Loading**: <10ms for typical YAML files (<100KB)
- **Validation**: <5ms for constraint pack validation
- **Service Registration**: <2ms for ConfigurationModule
- **Memory Usage**: <1MB for loaded configuration state
- **Startup Overhead**: <20ms total additional time

### Memory Management
```csharp
/// <summary>
/// Configuration service with memory-conscious design.
/// </summary>
internal sealed class ConfigurationService : IConfigurationService, IDisposable
{
    // Use readonly collections to prevent modifications
    private IReadOnlyDictionary<string, object>? _configurationCache;

    // Dispose pattern for cleanup
    public void Dispose()
    {
        _configurationCache?.Clear();
        _configurationCache = null;

        // Dispose of any file watchers or cache instances
        _fileWatcher?.Dispose();
        _memoryCache?.Dispose();
    }
}
```

## Testing Strategy

### Unit Test Coverage Requirements

#### ConfigurationService Tests
```csharp
[TestFixture]
public class ConfigurationServiceShould
{
    [Test]
    public async Task LoadConfigurationFromCommandLineArguments()
    {
        // Given: Command-line arguments with config path
        var args = new[] { "--config", "/path/to/config.yaml" };
        var mockLoader = new Mock<IConfigurationLoader>();
        var expectedPack = CreateTestConstraintPack();
        mockLoader.Setup(x => x.LoadAsync("/path/to/config.yaml"))
               .ReturnsAsync(expectedPack);

        var service = CreateConfigurationService(mockLoader.Object);

        // When: Initializing configuration
        await service.InitializeAsync(args);

        // Then: Configuration should be loaded from specified path
        Assert.That(service.ConstraintPack, Is.EqualTo(expectedPack));
        Assert.That(service.ConfigurationPath, Is.EqualTo("/path/to/config.yaml"));
        Assert.That(service.IsInitialized, Is.True);
    }

    [Test]
    public async Task OverrideWithEnvironmentVariable()
    {
        // Given: Environment variable set and command-line args empty
        Environment.SetEnvironmentVariable("CONSTRAINT_CONFIG_PATH", "/env/config.yaml");
        var args = Array.Empty<string>();

        // When: Initializing configuration
        // Then: Should load from environment variable path
        // ... test implementation
    }

    [Test]
    public void ValidateConfiguration_ShouldReturnErrors_WhenConstraintPackInvalid()
    {
        // Given: Invalid constraint pack loaded
        // When: Validating configuration
        // Then: Should return validation errors with specific messages
        // ... test implementation
    }
}
```

#### Integration Test Requirements
```csharp
[TestFixture]
public class ConfigurationIntegrationShould
{
    [Test]
    public async Task LoadRealYamlConfiguration()
    {
        // Given: Real YAML configuration file
        var configPath = Path.Combine(TestContext.CurrentContext.TestDirectory,
                                     "TestData", "valid-constraints.yaml");

        // When: Loading through full configuration service
        var services = new ServiceCollection();
        services.RegisterModulesFromAssembly(Assembly.GetExecutingAssembly());
        var provider = services.BuildServiceProvider();

        var configService = provider.GetRequiredService<IConfigurationService>();
        await configService.InitializeAsync(new[] { "--config", configPath });

        // Then: Configuration should be loaded and valid
        Assert.That(configService.IsInitialized, Is.True);
        Assert.That(configService.ConstraintPack, Is.Not.Null);

        var validation = configService.ValidateConfiguration();
        Assert.That(validation.IsValid, Is.True);
    }
}
```

## Migration and Compatibility

### Parallel Change Implementation

#### Phase 1: Create Services Alongside Static Code
```csharp
// Program.cs (Parallel Change - Phase 1)
public static async Task<int> Main(string[] args)
{
    // NEW: Service-based approach (parallel implementation)
    if (ShouldUseServiceBasedApproach())
    {
        return await RunWithServices(args);
    }

    // EXISTING: Static approach (preserved for compatibility)
    return await RunWithStaticMethods(args);
}

private static async Task<int> RunWithServices(string[] args)
{
    var services = new ServiceCollection();
    ModuleLoader.ConfigureServices(services);
    var provider = services.BuildServiceProvider();

    var hostingService = provider.GetRequiredService<IHostingService>();
    return await hostingService.RunAsync(args);
}

private static bool ShouldUseServiceBasedApproach()
{
    // Feature flag for gradual rollout
    return Environment.GetEnvironmentVariable("USE_SERVICE_BASED_HOSTING") == "true";
}
```

#### Phase 2: Migrate Gradually
```csharp
// Program.cs (Parallel Change - Phase 2)
public static async Task<int> Main(string[] args)
{
    // Default to service-based approach
    // Static approach only for explicit fallback
    var useStatic = Environment.GetEnvironmentVariable("USE_STATIC_HOSTING") == "true";

    if (useStatic)
    {
        return await RunWithStaticMethods(args); // Fallback
    }

    return await RunWithServices(args); // Default
}
```

#### Phase 3: Remove Static Implementation
```csharp
// Program.cs (Final - Phase 3)
public static async Task<int> Main(string[] args)
{
    var services = new ServiceCollection();
    ModuleLoader.ConfigureServices(services);
    var provider = services.BuildServiceProvider();

    var hostingService = provider.GetRequiredService<IHostingService>();
    return await hostingService.RunAsync(args);
}
```

### Backward Compatibility Requirements

#### Command-Line Interface Compatibility
- ✅ `--help` and `-h` must continue to work exactly as before
- ✅ `--version` and `-v` must continue to work exactly as before
- ✅ Exit codes must remain the same (0 for success, 1 for errors)
- ✅ Help message format must remain consistent
- ✅ Version message format must remain consistent

#### Configuration File Compatibility
- ✅ Existing YAML files must continue to work without changes
- ✅ File path resolution behavior must remain the same
- ✅ Error messages should be improved but not break automation
- ✅ Validation rules must remain backward compatible

This architecture provides a solid foundation for modern configuration management while maintaining the quality and performance standards established in the previous modernization phases.