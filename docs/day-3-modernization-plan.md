# Mikado Method Modernization Plan - Days 3-10 Implementation

## Current State Analysis

**✅ COMPLETED (Days 1-2):**
- ✅ Hosting infrastructure skeleton created
- ✅ Console abstraction (IConsoleAbstraction) implemented
- ✅ Argument handling extracted to McpServerHost
- ✅ All 391 tests passing
- ✅ CLI behavior validated and preserved

**📊 Current Test Status:** 391 tests passing (maintained from previous 391)

**🏗️ Infrastructure Foundation Status:**
```
Infrastructure/
├── Hosting/                    ✅ COMPLETED (Day 1-2)
│   ├── IHostingService.cs     ✅ Interface defined
│   ├── McpServerHost.cs       ✅ Basic implementation
│   ├── IConsoleAbstraction.cs ✅ Console abstraction
│   └── ConsoleAbstraction.cs  ✅ Implementation
├── Configuration/              🔄 EXISTS (needs DI integration)
│   ├── IConfigurationLoader.cs ✅ Available
│   ├── ConfigurationLoader.cs  ✅ Available
│   └── [Other config classes]  ✅ Available
├── Communication/              ✅ EXISTS (needs DI integration)
├── Logging/                    ✅ EXISTS (needs DI integration)
└── [To Be Added]
    ├── DependencyInjection/    🆕 Day 3-4 TARGET
    └── ServiceRegistration/    🆕 Day 3-4 TARGET
```

## Mikado Dependency Graph Update

**Phase 1: Infrastructure Foundation (Days 3-10)**

```
🎯 GOAL: Complete Infrastructure Foundation with DI Container
├── Day 3-4: Dependency Injection Container ⏭️ NEXT
│   ├── ⚡ HIGH PRIORITY: Create IServiceInstaller pattern
│   ├── ⚡ HIGH PRIORITY: Implement reflection-based DI registration
│   ├── ⚡ HIGH PRIORITY: Create HostingModule with service registrations
│   └── ⚡ HIGH PRIORITY: Integrate IServiceProvider into McpServerHost
├── Day 5-6: Configuration Infrastructure ⏳ DEPENDS ON DI
│   ├── Extract configuration loading from Program.cs
│   ├── Create IConfigurationService abstraction
│   ├── Add configuration validation and error handling
│   └── Register configuration services in DI container
├── Day 7-8: Logging Infrastructure ⏳ DEPENDS ON CONFIG
│   ├── Extract Serilog configuration to hosting layer
│   ├── Create ILoggingService abstraction
│   ├── Implement structured logging patterns
│   └── Register logging services in DI container
└── Day 9-10: Integration & Validation ⏳ DEPENDS ON ALL
    ├── Complete McpServerHost integration with all services
    ├── Performance validation (sub-50ms requirements)
    ├── Full regression testing with existing functionality
    └── Prepare for Phase 2 (Application Services)
```

## Detailed Implementation Plan

### Day 3-4: Dependency Injection Container

**🎯 Objective:** Implement reflection-based DI container with IServiceInstaller pattern

#### Step 1: Create DI Infrastructure (Day 3 Morning)

**Files to Create:**
1. `src/ConstraintMcpServer/Infrastructure/DependencyInjection/IServiceInstaller.cs`
2. `src/ConstraintMcpServer/Infrastructure/DependencyInjection/ServiceRegistrationExtensions.cs`
3. `src/ConstraintMcpServer/Infrastructure/DependencyInjection/ModuleLoader.cs`

**Step 1.1: IServiceInstaller Pattern**
```csharp
namespace ConstraintMcpServer.Infrastructure.DependencyInjection;

/// <summary>
/// Defines a contract for service installers using reflection-based registration.
/// </summary>
public interface IServiceInstaller
{
    /// <summary>
    /// Registers services in the dependency injection container.
    /// </summary>
    /// <param name="services">The service collection to register services in.</param>
    void RegisterServices(IServiceCollection services);

    /// <summary>
    /// Gets the installation priority for ordering service registration.
    /// </summary>
    int Priority { get; }
}
```

**Step 1.2: Service Registration Extensions**
```csharp
namespace ConstraintMcpServer.Infrastructure.DependencyInjection;

/// <summary>
/// Extensions for automatic service registration using reflection.
/// </summary>
public static class ServiceRegistrationExtensions
{
    /// <summary>
    /// Registers all service installers found in the assembly.
    /// </summary>
    public static IServiceCollection RegisterModulesFromAssembly(
        this IServiceCollection services,
        Assembly assembly)
    {
        var installers = assembly.GetTypes()
            .Where(type => typeof(IServiceInstaller).IsAssignableFrom(type) &&
                          !type.IsInterface &&
                          !type.IsAbstract)
            .Select(Activator.CreateInstance)
            .Cast<IServiceInstaller>()
            .OrderBy(installer => installer.Priority)
            .ToList();

        foreach (var installer in installers)
        {
            installer.RegisterServices(services);
        }

        return services;
    }
}
```

**Step 1.3: Module Loader**
```csharp
namespace ConstraintMcpServer.Infrastructure.DependencyInjection;

/// <summary>
/// Loads and configures service modules for dependency injection.
/// </summary>
public sealed class ModuleLoader
{
    /// <summary>
    /// Configures all service modules in the application.
    /// </summary>
    public static IServiceCollection ConfigureServices(IServiceCollection? services = null)
    {
        services ??= new ServiceCollection();

        // Register modules from current assembly using reflection
        services.RegisterModulesFromAssembly(Assembly.GetExecutingAssembly());

        return services;
    }
}
```

#### Step 2: Create Service Modules (Day 3 Afternoon)

**Files to Create:**
1. `src/ConstraintMcpServer/Infrastructure/Hosting/HostingModule.cs`
2. `src/ConstraintMcpServer/Infrastructure/Configuration/ConfigurationModule.cs`
3. `src/ConstraintMcpServer/Infrastructure/Communication/CommunicationModule.cs`
4. `src/ConstraintMcpServer/Infrastructure/Logging/LoggingModule.cs`

**Step 2.1: HostingModule**
```csharp
namespace ConstraintMcpServer.Infrastructure.Hosting;

/// <summary>
/// Service installer for hosting infrastructure components.
/// </summary>
public sealed class HostingModule : IServiceInstaller
{
    /// <inheritdoc />
    public int Priority => 10; // High priority - foundational services

    /// <inheritdoc />
    public void RegisterServices(IServiceCollection services)
    {
        // Console abstraction
        services.AddSingleton<IConsoleAbstraction, ConsoleAbstraction>();

        // Hosting service
        services.AddSingleton<IHostingService, McpServerHost>();
    }
}
```

#### Step 3: Update McpServerHost Integration (Day 4 Morning)

**Files to Modify:**
1. `src/ConstraintMcpServer/Infrastructure/Hosting/McpServerHost.cs`

**Step 3.1: Add DI Constructor**
```csharp
public sealed class McpServerHost : IHostingService
{
    private readonly IConsoleAbstraction _console;
    private readonly IServiceProvider _serviceProvider;

    /// <summary>
    /// Initializes with dependency injection.
    /// </summary>
    public McpServerHost(
        IConsoleAbstraction console,
        IServiceProvider serviceProvider)
    {
        _console = console;
        _serviceProvider = serviceProvider;
    }

    /// <summary>
    /// Legacy constructor for backward compatibility.
    /// </summary>
    public McpServerHost(IConsoleAbstraction? console = null)
        : this(console ?? new ConsoleAbstraction(), BuildServiceProvider())
    {
    }

    private static IServiceProvider BuildServiceProvider()
    {
        var services = ModuleLoader.ConfigureServices();
        return services.BuildServiceProvider();
    }

    // ... rest of implementation
}
```

#### Step 4: Integration Testing (Day 4 Afternoon)

**Files to Create:**
1. `tests/ConstraintMcpServer.Tests/Infrastructure/DependencyInjection/ModuleLoaderTests.cs`
2. `tests/ConstraintMcpServer.Tests/Infrastructure/DependencyInjection/ServiceRegistrationTests.cs`

**Step 4.1: DI Container Testing**
```csharp
[TestFixture]
public sealed class ModuleLoaderTests
{
    [Test]
    public void ConfigureServices_Should_RegisterAllModules()
    {
        // Given
        var services = new ServiceCollection();

        // When
        var result = ModuleLoader.ConfigureServices(services);

        // Then
        var serviceProvider = result.BuildServiceProvider();

        serviceProvider.GetRequiredService<IConsoleAbstraction>()
            .Should().NotBeNull();
        serviceProvider.GetRequiredService<IHostingService>()
            .Should().NotBeNull();
    }
}
```

### Day 5-6: Configuration Infrastructure

**🎯 Objective:** Extract configuration management to hosting layer with DI integration

#### Step 1: Create Configuration Service (Day 5 Morning)

**Files to Create:**
1. `src/ConstraintMcpServer/Infrastructure/Configuration/IConfigurationService.cs`
2. `src/ConstraintMcpServer/Infrastructure/Configuration/ConfigurationService.cs`

#### Step 2: Configuration Module Implementation (Day 5 Afternoon)

**Files to Modify:**
1. `src/ConstraintMcpServer/Infrastructure/Configuration/ConfigurationModule.cs`

#### Step 3: Extract from Program.cs (Day 6)

**Files to Modify:**
1. `src/ConstraintMcpServer/Program.cs` - Remove inline configuration logic
2. `src/ConstraintMcpServer/Infrastructure/Hosting/McpServerHost.cs` - Add configuration service usage

### Day 7-8: Logging Infrastructure

**🎯 Objective:** Implement structured logging with DI integration

#### Step 1: Logging Service Abstraction (Day 7)

**Files to Create:**
1. `src/ConstraintMcpServer/Infrastructure/Logging/ILoggingService.cs`
2. `src/ConstraintMcpServer/Infrastructure/Logging/LoggingService.cs`

#### Step 2: Serilog Integration (Day 8)

**Files to Modify:**
1. `src/ConstraintMcpServer/Infrastructure/Logging/LoggingModule.cs`

### Day 9-10: Integration & Validation

**🎯 Objective:** Complete integration and validate performance requirements

#### Step 1: Full Integration (Day 9)

**Files to Modify:**
1. `src/ConstraintMcpServer/Program.cs` - Minimal bootstrap code
2. `src/ConstraintMcpServer/Infrastructure/Hosting/McpServerHost.cs` - Complete integration

#### Step 2: Performance & Regression Testing (Day 10)

**Performance Targets:**
- ✅ Sub-50ms p95 latency per tool call
- ✅ Sub-100MB memory ceiling
- ✅ All 391+ tests passing
- ✅ Deterministic behavior maintained

## Risk Assessment & Mitigation

### High Risk Items

1. **🔴 Breaking Existing Tests**
   - **Mitigation:** Apply parallel change pattern (EXPAND → VALIDATE → MIGRATE → CONTRACT)
   - **Validation:** Run tests after each step

2. **🔴 Performance Regression**
   - **Mitigation:** Profile before/after each major change
   - **Validation:** Performance benchmarks on Day 10

3. **🔴 DI Container Complexity**
   - **Mitigation:** Keep reflection-based approach simple
   - **Validation:** Unit tests for service registration

### Medium Risk Items

1. **🟡 Configuration Loading Changes**
   - **Mitigation:** Preserve existing configuration behavior
   - **Validation:** Configuration integration tests

2. **🟡 Logging Configuration**
   - **Mitigation:** Maintain structured logging compatibility
   - **Validation:** Logging output format tests

## Success Criteria

### Day 3-4 Completion Criteria
- ✅ IServiceInstaller pattern implemented
- ✅ Reflection-based DI registration working
- ✅ HostingModule registering services correctly
- ✅ McpServerHost using DI container
- ✅ All existing tests passing (391+)

### Day 5-6 Completion Criteria
- ✅ Configuration extracted from Program.cs
- ✅ IConfigurationService abstraction created
- ✅ Configuration validation integrated
- ✅ All existing tests passing

### Day 7-8 Completion Criteria
- ✅ Logging infrastructure abstracted
- ✅ Serilog configuration moved to hosting layer
- ✅ Structured logging patterns implemented
- ✅ All existing tests passing

### Day 9-10 Completion Criteria
- ✅ Complete integration achieved
- ✅ Performance requirements met (sub-50ms p95)
- ✅ Memory requirements met (sub-100MB)
- ✅ All tests passing (391+)
- ✅ Ready for Phase 2 implementation

## Next Phase Preview

**Phase 2: Application Services (Days 11-20)**
- Application service layer implementation
- Domain service integration
- Business logic extraction
- Advanced constraint selection logic

This plan maintains the systematic Mikado Method approach while ensuring continuous validation and risk mitigation throughout the implementation process.