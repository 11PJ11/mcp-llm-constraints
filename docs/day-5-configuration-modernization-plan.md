# Day 5-6 Configuration Infrastructure Modernization Plan

## Current State Analysis

### Existing Configuration System
- **Program.cs**: Contains hardcoded static JSON-RPC handling with embedded configuration constants
- **Configuration Classes**: Well-designed but disconnected from DI container
  - `ServerConfiguration`: Simple server metadata (version, protocol)
  - `ConfigurationLoader`: YAML constraint pack loading with validation
  - `CommandLineOptions`: Command-line argument parsing
  - `CommandLineParser`: Argument validation logic
- **YAML Support**: YamlDotNet integration for constraint pack loading
- **Validation**: FluentValidation patterns for constraint pack validation

### Integration Points
- **McpServerHost**: Currently accepts IConsoleAbstraction but no configuration services
- **ConstraintServerFactory**: Hardcoded ServerConfiguration instantiation
- **DI Container**: ModuleLoader pattern with IServiceInstaller (Priority = 10, 20, etc.)
- **YAML Files**: Production constraint configurations in `/config` directory

### Current Gaps
- ❌ Configuration not integrated with DI container
- ❌ Program.cs contains static MCP protocol handling (should use services)
- ❌ No environment variable support
- ❌ No configuration validation during startup
- ❌ No hot-reload capability planning

## Day 5-6 Objectives

### Primary Goal: Configuration Service Extraction
Extract configuration loading from Program.cs and create modern configuration infrastructure that integrates with the new DI container while maintaining backward compatibility.

### Architecture Design

#### 1. IConfigurationService Interface
**Location**: `src/ConstraintMcpServer/Infrastructure/Configuration/IConfigurationService.cs`

```csharp
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

#### 2. ConfigurationService Implementation
**Location**: `src/ConstraintMcpServer/Infrastructure/Configuration/ConfigurationService.cs`

```csharp
namespace ConstraintMcpServer.Infrastructure.Configuration;

/// <summary>
/// Production implementation of configuration service with YAML and environment support.
/// Integrates multiple configuration sources with validation and error handling.
/// </summary>
internal sealed class ConfigurationService : IConfigurationService
{
    private readonly IConfigurationLoader _configurationLoader;
    private readonly IConstraintValidator _constraintValidator;
    private readonly ICommandLineParser _commandLineParser;
    private readonly IServerConfiguration _serverConfiguration;

    // Loaded configuration state
    private CommandLineOptions _commandLineOptions;
    private ConstraintPack? _constraintPack;

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
    }

    /// <summary>
    /// Initializes configuration from command-line arguments.
    /// Called during startup to load all configuration sources.
    /// </summary>
    public async Task InitializeAsync(string[] args)
    {
        // Parse command-line arguments
        _commandLineOptions = _commandLineParser.Parse(args);

        // Load constraint pack if configuration path provided
        if (!string.IsNullOrEmpty(_commandLineOptions.ConfigurationPath))
        {
            _constraintPack = await _configurationLoader.LoadAsync(_commandLineOptions.ConfigurationPath);
        }

        // Override with environment variables if present
        await LoadEnvironmentVariables();
    }

    private async Task LoadEnvironmentVariables()
    {
        // CONSTRAINT_CONFIG_PATH environment variable override
        var envConfigPath = Environment.GetEnvironmentVariable("CONSTRAINT_CONFIG_PATH");
        if (!string.IsNullOrEmpty(envConfigPath) && _constraintPack == null)
        {
            _constraintPack = await _configurationLoader.LoadAsync(envConfigPath);
        }
    }
}
```

#### 3. ConfigurationModule for DI Registration
**Location**: `src/ConstraintMcpServer/Infrastructure/Configuration/ConfigurationModule.cs`

```csharp
namespace ConstraintMcpServer.Infrastructure.Configuration;

/// <summary>
/// Service installer for configuration-related dependencies.
/// Registers configuration services with appropriate lifetimes.
/// </summary>
public sealed class ConfigurationModule : IServiceInstaller
{
    /// <summary>
    /// Configuration services have high priority as they're foundational.
    /// Priority = 20 (after hosting services at 10).
    /// </summary>
    public int Priority => 20;

    /// <summary>
    /// Registers all configuration services with the DI container.
    /// </summary>
    /// <param name="services">The service collection to register services with.</param>
    public void RegisterServices(IServiceCollection services)
    {
        // Core configuration services
        services.AddSingleton<IServerConfiguration, ServerConfiguration>();
        services.AddSingleton<ICommandLineParser, CommandLineParser>();

        // YAML loading and validation services
        services.AddSingleton<IConstraintPackReader, YamlConstraintPackReader>();
        services.AddSingleton<IConstraintValidator, ConstraintPackValidator>();
        services.AddSingleton<IConfigurationLoader, ConfigurationLoader>();

        // Central configuration service
        services.AddSingleton<IConfigurationService, ConfigurationService>();

        // Configuration validation
        services.AddSingleton<IConfigurationValidator, ConfigurationValidator>();
    }
}
```

#### 4. Enhanced McpServerHost Integration
**Location**: `src/ConstraintMcpServer/Infrastructure/Hosting/McpServerHost.cs` (Updated)

```csharp
namespace ConstraintMcpServer.Infrastructure.Hosting;

/// <summary>
/// Hosts the MCP server with configuration and dependency injection integration.
/// </summary>
public sealed class McpServerHost : IHostingService
{
    private readonly IConsoleAbstraction _console;
    private readonly IConfigurationService _configurationService;
    private readonly IServiceProvider _serviceProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="McpServerHost"/> class.
    /// </summary>
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
            // Initialize configuration from command-line arguments
            await _configurationService.InitializeAsync(args);

            // Handle help and version requests
            if (_configurationService.CommandLineOptions.ShowHelp)
            {
                await DisplayHelpMessage();
                return 0;
            }

            if (_configurationService.CommandLineOptions.ShowVersion)
            {
                await DisplayVersionMessage();
                return 0;
            }

            // Validate configuration
            var validationResult = _configurationService.ValidateConfiguration();
            if (!validationResult.IsValid)
            {
                await DisplayConfigurationErrors(validationResult);
                return 1;
            }

            // Start MCP server with proper services
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
}
```

## Implementation Strategy

### Phase 1: Foundation (Day 5 Morning)
**Goal**: Create configuration service interfaces and basic implementation

#### Files to Create:
1. `IConfigurationService.cs` - Central configuration interface
2. `ConfigurationService.cs` - Core implementation with DI integration
3. `ConfigurationValidationResult.cs` - Validation result container
4. `ConfigurationException.cs` - Configuration-specific exceptions
5. `ConfigurationModule.cs` - DI registration module

#### Files to Update:
1. `McpServerHost.cs` - Add IConfigurationService dependency
2. `HostingModule.cs` - Update service registrations

### Phase 2: Integration (Day 5 Afternoon)
**Goal**: Integrate configuration service with existing infrastructure

#### Tasks:
1. Update `ConstraintServerFactory` to use DI for configuration
2. Create configuration validation logic
3. Add environment variable support
4. Update Program.cs to use services instead of static methods

### Phase 3: Enhanced Features (Day 6 Morning)
**Goal**: Add advanced configuration features

#### Tasks:
1. Environment variable override support
2. Configuration file path resolution (relative/absolute)
3. Default configuration fallbacks
4. Enhanced error messages and validation

### Phase 4: Program.cs Modernization (Day 6 Afternoon)
**Goal**: Replace static Program.cs with service-based approach

#### Tasks:
1. Extract MCP protocol handling to services
2. Create proper application entry point using DI
3. Remove hardcoded JSON-RPC handling
4. Maintain backward compatibility

## Integration with Existing DI Container

### Service Registration Priority
```
Priority 10: HostingModule (Foundational services)
Priority 20: ConfigurationModule (Configuration services) ← NEW
Priority 30: ApplicationModule (Business logic)
Priority 40: InfrastructureModule (External adapters)
```

### Dependency Flow
```
Program.cs
  └── ModuleLoader.ConfigureServices()
      ├── HostingModule (IConsoleAbstraction, IHostingService)
      ├── ConfigurationModule (IConfigurationService, IConfigurationLoader) ← NEW
      └── McpServerHost(IConsoleAbstraction, IConfigurationService) ← UPDATED
```

## Risk Assessment & Mitigation

### High Risk Areas
1. **Program.cs Complexity**: Current static JSON-RPC handling is complex
   - **Mitigation**: Parallel change pattern - create services alongside static code

2. **Configuration Loading Timing**: DI container needs configuration before services
   - **Mitigation**: Two-phase initialization (configuration first, then services)

3. **Backward Compatibility**: Existing command-line interface must work
   - **Mitigation**: Maintain exact same command-line argument behavior

### Medium Risk Areas
1. **YAML Loading Integration**: Existing ConfigurationLoader needs DI integration
   - **Mitigation**: Wrapper pattern around existing implementation

2. **Test Compatibility**: 399 tests must continue passing
   - **Mitigation**: Incremental changes with continuous test validation

## Configuration Sources Priority

### Loading Order (Highest to Lowest Priority)
1. **Command-line arguments** (`--config /path/to/config.yaml`)
2. **Environment variables** (`CONSTRAINT_CONFIG_PATH`)
3. **Default configuration files** (`./config/constraints.yaml`)
4. **Built-in defaults** (Empty constraint pack)

### Environment Variables
- `CONSTRAINT_CONFIG_PATH`: Override configuration file path
- `DOTNET_ENVIRONMENT`: Development/Production mode
- `CONSTRAINT_LOG_LEVEL`: Override logging level (future)

## Validation Strategy

### Configuration Validation Levels
1. **Schema Validation**: YAML structure and required fields
2. **Business Validation**: Constraint pack logic and references
3. **Integration Validation**: Service dependencies and compatibility
4. **Runtime Validation**: Performance and resource constraints

### Error Handling
- **Graceful Degradation**: Continue with default configuration if optional configs fail
- **Clear Error Messages**: Specific guidance for fixing configuration issues
- **Validation Details**: Line numbers and field names for YAML errors

## Performance Requirements

### Startup Performance Targets
- **Configuration Loading**: <10ms for typical constraint packs
- **YAML Parsing**: <5ms for production configurations
- **Validation**: <5ms for schema and business rule validation
- **Total Configuration Overhead**: <20ms additional to current startup time

### Memory Constraints
- **Configuration Objects**: <1MB for typical constraint packs
- **Service Registration**: <100KB additional DI container overhead
- **YAML Parser**: Release after loading to minimize memory footprint

## Testing Strategy

### Unit Tests Required
1. `ConfigurationServiceShould.cs` - Core configuration service behavior
2. `ConfigurationModuleShould.cs` - DI registration validation
3. `ConfigurationValidationShould.cs` - Validation logic testing
4. `EnvironmentVariableOverrideShould.cs` - Environment variable integration

### Integration Tests Required
1. `McpServerHostWithConfigurationShould.cs` - Full integration testing
2. `ConfigurationLoadingIntegrationShould.cs` - File loading scenarios
3. `CommandLineConfigurationIntegrationShould.cs` - Argument processing

### E2E Tests Updates
1. Update existing E2E tests to work with new configuration approach
2. Add configuration error scenarios (invalid YAML, missing files)
3. Validate backward compatibility with existing command-line behavior

## Success Criteria

### Day 5 Completion
- ✅ IConfigurationService interface defined
- ✅ ConfigurationService implementation working
- ✅ ConfigurationModule registering services correctly
- ✅ McpServerHost using IConfigurationService
- ✅ All existing tests passing (399+)
- ✅ Basic YAML configuration loading through DI

### Day 6 Completion
- ✅ Environment variable override support
- ✅ Enhanced validation and error handling
- ✅ Program.cs modernized with service-based approach
- ✅ Backward compatibility maintained
- ✅ Performance within 10% of baseline
- ✅ Configuration hot-reload foundation (interface only)

### Quality Gates
- **Test Coverage**: ≥95% for new configuration services
- **Performance**: Configuration loading <20ms overhead
- **Memory**: <1MB additional memory usage
- **Compatibility**: 100% command-line argument compatibility
- **Error Handling**: Clear error messages for all failure scenarios

## Future Considerations

### Day 7+ Enhancement Opportunities
1. **Configuration Hot-Reload**: Implement IConfigurationService.ReloadAsync()
2. **Configuration Caching**: Cache parsed YAML for improved performance
3. **Configuration Profiles**: Development/Production configuration profiles
4. **Configuration Validation UI**: Interactive configuration validation tools
5. **Configuration Templates**: Generate configuration from templates

### Extensibility Points
- **IConfigurationSource**: Support for additional configuration sources (JSON, INI, remote)
- **IConfigurationTransform**: Configuration transformation pipeline
- **IConfigurationMonitor**: File system watching for hot-reload
- **IConfigurationEncryption**: Encrypted configuration support

This plan provides a comprehensive approach to modernizing the configuration infrastructure while maintaining the high quality and performance standards established in Days 1-4.