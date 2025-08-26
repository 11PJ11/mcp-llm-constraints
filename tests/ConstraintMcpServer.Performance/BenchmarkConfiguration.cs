using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Exporters;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Loggers;
using BenchmarkDotNet.Order;

namespace ConstraintMcpServer.Performance;

/// <summary>
/// Benchmark configuration for constraint MCP server performance testing.
/// Enforces sub-50ms p95 latency and sub-100MB memory requirements.
/// </summary>
public class ConstraintServerBenchmarkConfig : ManualConfig
{
    public ConstraintServerBenchmarkConfig()
    {
        AddJob(Job.Default
            .WithRuntime(BenchmarkDotNet.Environments.CoreRuntime.Core80)
            .WithPlatform(BenchmarkDotNet.Environments.Platform.X64)
            .WithJit(BenchmarkDotNet.Environments.Jit.RyuJit)
            .WithGcServer(true)
            .WithGcConcurrent(true)
            .WithId("Production-Config"));

        AddDiagnoser(MemoryDiagnoser.Default);
        
        // Add custom performance thresholds
        AddValidator(BenchmarkDotNet.Validators.JitOptimizationsValidator.FailOnError);
        AddValidator(BenchmarkDotNet.Validators.BaselineValidator.FailOnError);
        
        // Export results in multiple formats
        AddExporter(HtmlExporter.Default);
        AddExporter(MarkdownExporter.GitHub);
        
        // Configure logging
        AddLogger(ConsoleLogger.Default);
        
        // Order results by performance
        WithOrderer(new DefaultOrderer(SummaryOrderPolicy.FastestToSlowest));
        
        // Global setup
        WithOptions(ConfigOptions.DisableOptimizationsValidator);
    }
}

/// <summary>
/// Performance thresholds for constraint server operations.
/// </summary>
public static class PerformanceThresholds
{
    /// <summary>
    /// Maximum acceptable P95 latency for tool calls (50ms requirement).
    /// </summary>
    public const double MaxP95LatencyMs = 50.0;
    
    /// <summary>
    /// Maximum acceptable P99 latency for tool calls (100ms soft limit).
    /// </summary>
    public const double MaxP99LatencyMs = 100.0;
    
    /// <summary>
    /// Maximum acceptable memory usage for server process (100MB requirement).
    /// </summary>
    public const long MaxMemoryUsageBytes = 100 * 1024 * 1024; // 100MB
    
    /// <summary>
    /// Maximum acceptable GC pressure (Gen2 collections per operation).
    /// </summary>
    public const double MaxGen2CollectionsPerOp = 0.01;
    
    /// <summary>
    /// Maximum acceptable allocation rate (bytes per operation).
    /// </summary>
    public const long MaxAllocationPerOpBytes = 10 * 1024; // 10KB per operation
}

/// <summary>
/// Benchmark categories for organized performance testing.
/// </summary>
public static class BenchmarkCategories
{
    public const string LibraryLookup = "Library_Lookup";
    public const string ConstraintResolution = "Constraint_Resolution";
    public const string CompositionResolution = "Composition_Resolution";
    public const string TriggerMatching = "Basic_Matching";
    public const string AdvancedMatching = "Advanced_Matching";
    public const string BatchOperations = "Batch_Operations";
    public const string MemoryIntensive = "Memory_Intensive";
    public const string PerformanceCritical = "Performance_Critical";
    public const string ComplexComposition = "Complex_Composition";
    public const string ValidationPerformance = "Validation_Performance";
    public const string HierarchyMemory = "Hierarchy_Memory";
    public const string TemporaryAllocations = "Temporary_Allocations";
}