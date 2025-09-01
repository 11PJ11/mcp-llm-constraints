using System.Collections.Generic;
using NUnit.Framework;
using ConstraintMcpServer.Domain.Composition;
using ConstraintMcpServer.Domain.Common;

namespace ConstraintMcpServer.Tests.Domain.Composition.MethodologyValidation;

/// <summary>
/// Validation tests proving that our generic system can correctly express Clean Architecture methodology.
/// These tests demonstrate that the methodology-agnostic LayeredComposition can support
/// the original Clean Architecture principles (Domain → Application → Infrastructure → Presentation)
/// through user-defined configuration.
/// </summary>
[TestFixture]
[Category("MethodologyValidation")]
[Category("CleanArchitecture")]
public class CleanArchitectureValidationTests
{
    private LayeredComposition _layeredComposition;
    private UserDefinedLayerHierarchy _cleanArchitectureHierarchy;
    private LayeredCompositionState _initialState;

    [SetUp]
    public void SetUp()
    {
        _layeredComposition = new LayeredComposition();
        
        // Configure Clean Architecture through generic system
        // Domain (0) → Application (1) → Infrastructure (2) → Presentation (3)
        var cleanArchLayers = new List<UserDefinedLayerInfo>
        {
            new UserDefinedLayerInfo("arch.domain-layer", 0, "Domain", 
                "Pure business logic with no external dependencies"),
            new UserDefinedLayerInfo("arch.application-layer", 1, "Application", 
                "Use cases and business workflows, depend only on Domain"),
            new UserDefinedLayerInfo("arch.infrastructure-layer", 2, "Infrastructure", 
                "External concerns (database, web APIs), implement Application interfaces"),
            new UserDefinedLayerInfo("arch.presentation-layer", 3, "Presentation", 
                "UI concerns, depend on Application for business logic")
        };
        
        // Define namespace patterns for layer detection
        var namespacePatterns = new Dictionary<int, IReadOnlyList<string>>
        {
            { 0, new[] { "domain", "core", "business" } },
            { 1, new[] { "application", "services", "usecases" } },
            { 2, new[] { "infrastructure", "data", "external" } },
            { 3, new[] { "presentation", "web", "api", "ui" } }
        };
        
        // Define Clean Architecture dependency rules (inner layers don't depend on outer layers)
        var dependencyRules = new Dictionary<int, IReadOnlyList<int>>
        {
            { 0, new List<int>() },              // Domain depends on nothing
            { 1, new[] { 0 } },                  // Application depends only on Domain
            { 2, new[] { 0, 1 } },               // Infrastructure depends on Domain & Application
            { 3, new[] { 0, 1 } }                // Presentation depends on Domain & Application (not Infrastructure)
        };
        
        _cleanArchitectureHierarchy = new UserDefinedLayerHierarchy(
            "clean-architecture",
            cleanArchLayers,
            namespacePatterns,
            dependencyRules,
            "Clean Architecture with proper dependency inversion");
        
        _initialState = new LayeredCompositionState(
            completedLayers: new HashSet<int>(),
            currentLayer: 0,
            lastActivation: DateTime.UtcNow,
            violationsDetected: new List<UserDefinedLayerViolation>());
    }

    /// <summary>
    /// VALIDATION TEST: Proves generic system can express Clean Architecture layer detection
    /// Original requirement: "Detect which layer developer is working in based on namespace"
    /// </summary>
    [Test]
    public void GenericSystem_Should_Detect_Clean_Architecture_Layers_Through_Configuration()
    {
        // Act - Test namespace-based layer detection
        var domainLayer = _cleanArchitectureHierarchy.DetermineLayerFromNamespace("MyApp.Domain.Entities");
        var applicationLayer = _cleanArchitectureHierarchy.DetermineLayerFromNamespace("MyApp.Application.Services");
        var infrastructureLayer = _cleanArchitectureHierarchy.DetermineLayerFromNamespace("MyApp.Infrastructure.Data");
        var presentationLayer = _cleanArchitectureHierarchy.DetermineLayerFromNamespace("MyApp.Web.Controllers");
        
        // Assert - Validates Clean Architecture layer detection
        Assert.That(domainLayer, Is.EqualTo(0), "Should detect Domain layer (level 0)");
        Assert.That(applicationLayer, Is.EqualTo(1), "Should detect Application layer (level 1)");
        Assert.That(infrastructureLayer, Is.EqualTo(2), "Should detect Infrastructure layer (level 2)");
        Assert.That(presentationLayer, Is.EqualTo(3), "Should detect Presentation layer (level 3)");
    }

    /// <summary>
    /// VALIDATION TEST: Proves generic system can detect Clean Architecture violations
    /// Original requirement: "Domain must not depend on Infrastructure - detect violations"
    /// </summary>
    [Test]
    public void GenericSystem_Should_Detect_Clean_Architecture_Dependency_Violations()
    {
        // Arrange - Create context with Domain → Infrastructure violation
        var mockDependencies = new List<CodeDependency>
        {
            new CodeDependency("MyApp.Domain.Entities", "MyApp.Infrastructure.Data") // VIOLATION!
        };
        
        var context = new CompositionContext
        {
            CodeAnalysis = new CodeAnalysisResult { Dependencies = mockDependencies }
        };
        
        // Act - Generic system detects violations using user-defined rules
        var result = _layeredComposition.GetNextConstraint(_initialState, _cleanArchitectureHierarchy, context);
        
        // Assert - Validates Clean Architecture violation detection
        Assert.That(result.IsSuccess, Is.True);
        var activation = result.Value;
        Assert.That(activation.ConstraintId, Does.StartWith("arch.violation"), 
            "Should create violation constraint");
        Assert.That(activation.Guidance, Does.Contain("Domain"), 
            "Should identify Domain layer in violation");
        Assert.That(activation.Guidance, Does.Contain("Infrastructure"), 
            "Should identify Infrastructure layer in violation");
    }

    /// <summary>
    /// VALIDATION TEST: Proves generic system enforces Clean Architecture dependency rules
    /// Original requirement: "Inner layers cannot depend on outer layers"
    /// </summary>
    [Test]
    public void GenericSystem_Should_Enforce_Clean_Architecture_Dependency_Rules()
    {
        // Act - Test dependency rule validation
        var domainToApplication = _cleanArchitectureHierarchy.IsViolation(0, 1);    // Domain → Application: VIOLATION
        var domainToInfra = _cleanArchitectureHierarchy.IsViolation(0, 2);          // Domain → Infrastructure: VIOLATION  
        var applicationToDomain = _cleanArchitectureHierarchy.IsViolation(1, 0);    // Application → Domain: OK
        var infraToDomain = _cleanArchitectureHierarchy.IsViolation(2, 0);          // Infrastructure → Domain: OK
        var infraToApplication = _cleanArchitectureHierarchy.IsViolation(2, 1);     // Infrastructure → Application: OK
        
        // Assert - Validates Clean Architecture dependency rules
        Assert.That(domainToApplication, Is.True, "Domain depending on Application should be violation");
        Assert.That(domainToInfra, Is.True, "Domain depending on Infrastructure should be violation");
        Assert.That(applicationToDomain, Is.False, "Application depending on Domain should be allowed");
        Assert.That(infraToDomain, Is.False, "Infrastructure depending on Domain should be allowed");
        Assert.That(infraToApplication, Is.False, "Infrastructure depending on Application should be allowed");
    }

    /// <summary>
    /// VALIDATION TEST: Proves generic system supports Clean Architecture layer guidance
    /// Original requirement: "Provide layer-specific guidance for Clean Architecture"
    /// </summary>
    [Test]
    public void GenericSystem_Should_Provide_Clean_Architecture_Layer_Guidance()
    {
        // Arrange - Different layer contexts
        var layers = _cleanArchitectureHierarchy.Layers;
        
        // Act & Assert - Test each layer's guidance
        foreach (var layer in layers)
        {
            var layerName = _cleanArchitectureHierarchy.GetLayerName(layer.LayerLevel);
            var allowedDeps = _cleanArchitectureHierarchy.GetAllowedDependencies(layer.LayerLevel);
            
            switch (layer.LayerLevel)
            {
                case 0: // Domain
                    Assert.That(layerName, Is.EqualTo("Domain"));
                    Assert.That(allowedDeps, Is.Empty, "Domain should have no allowed dependencies");
                    break;
                case 1: // Application  
                    Assert.That(layerName, Is.EqualTo("Application"));
                    Assert.That(allowedDeps, Contains.Item(0), "Application should depend on Domain");
                    break;
                case 2: // Infrastructure
                    Assert.That(layerName, Is.EqualTo("Infrastructure"));
                    Assert.That(allowedDeps, Contains.Item(0), "Infrastructure should depend on Domain");
                    Assert.That(allowedDeps, Contains.Item(1), "Infrastructure should depend on Application");
                    break;
                case 3: // Presentation
                    Assert.That(layerName, Is.EqualTo("Presentation"));
                    Assert.That(allowedDeps, Contains.Item(0), "Presentation should depend on Domain");
                    Assert.That(allowedDeps, Contains.Item(1), "Presentation should depend on Application");
                    break;
            }
        }
    }

    /// <summary>
    /// VALIDATION TEST: Proves generic system can express different architectural approaches
    /// Shows how the same layered system can support Hexagonal Architecture, Onion Architecture, etc.
    /// </summary>
    [Test]
    public void GenericSystem_Should_Support_Alternative_Layered_Architectures()
    {
        // Arrange - Configure Hexagonal Architecture (different from Clean Architecture)
        var hexagonalLayers = UserDefinedLayerHierarchy.FromSimpleConfiguration(
            "hexagonal-architecture",
            new[]
            {
                (0, "Core", "Business core with ports", new[] { "core", "domain" }),
                (1, "Adapters", "External adapters implementing ports", new[] { "adapters", "infrastructure" })
            });
        
        // Act - Test Hexagonal Architecture configuration  
        var coreLayer = hexagonalLayers.DetermineLayerFromNamespace("MyApp.Core.Business");
        var adapterLayer = hexagonalLayers.DetermineLayerFromNamespace("MyApp.Adapters.Data");
        var coreToAdapter = hexagonalLayers.IsViolation(0, 1); // Core → Adapter: should be violation
        var adapterToCore = hexagonalLayers.IsViolation(1, 0); // Adapter → Core: should be allowed
        
        // Assert - Validates alternative architecture support
        Assert.That(coreLayer, Is.EqualTo(0), "Should detect Hexagonal Core layer");
        Assert.That(adapterLayer, Is.EqualTo(1), "Should detect Hexagonal Adapter layer");
        Assert.That(coreToAdapter, Is.True, "Core should not depend on Adapters in Hexagonal Architecture");
        Assert.That(adapterToCore, Is.False, "Adapters should be able to depend on Core");
    }

    /// <summary>
    /// VALIDATION TEST: Proves generic system supports Clean Architecture progression
    /// Original requirement: "Progress through layers systematically during development"
    /// </summary>
    [Test]
    public void GenericSystem_Should_Support_Clean_Architecture_Development_Progression()
    {
        // Arrange - Start with Domain layer
        var context = new CompositionContext 
        { 
            CurrentFile = new FileContext { Namespace = "MyApp.Domain.Entities" } 
        };
        
        // Act - Get next layer in progression
        var result = _layeredComposition.GetNextConstraint(_initialState, _cleanArchitectureHierarchy, context);
        
        // Assert - Validates Clean Architecture development flow
        Assert.That(result.IsSuccess, Is.True);
        var constraint = result.Value;
        Assert.That(constraint.ConstraintId, Is.EqualTo("arch.domain-layer"));
        Assert.That(constraint.LayerLevel, Is.EqualTo(0), "Should start with Domain layer (innermost)");
    }
}

/// <summary>
/// Mock classes to support testing
/// </summary>
public class CodeDependency
{
    public string Source { get; }
    public string Target { get; }
    
    public CodeDependency(string source, string target)
    {
        Source = source;
        Target = target;
    }
}

public class CodeAnalysisResult
{
    public List<CodeDependency> Dependencies { get; set; } = new();
}

public class CompositionContext
{
    public CodeAnalysisResult? CodeAnalysis { get; set; }
    public FileContext? CurrentFile { get; set; }
}

public class FileContext
{
    public string? Namespace { get; set; }
}