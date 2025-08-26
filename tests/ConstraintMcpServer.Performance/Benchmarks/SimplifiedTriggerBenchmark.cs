using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using ConstraintMcpServer.Domain.Constraints;

namespace ConstraintMcpServer.Performance.Benchmarks;

/// <summary>
/// Simplified trigger matching performance benchmarks using the actual TriggerContext API.
/// Focuses on critical performance paths for sub-50ms p95 latency requirements.
/// </summary>
[SimpleJob(RuntimeMoniker.Net80)]
[MemoryDiagnoser]
[MeanColumn]
public class SimplifiedTriggerBenchmark
{
    private TriggerContext _basicContext = null!;
    private TriggerContext _complexContext = null!;
    private TriggerContext _emptyContext = null!;
    
    private TriggerConfiguration _keywordConfig = null!;
    private TriggerConfiguration _filePatternConfig = null!;
    private TriggerConfiguration _contextPatternConfig = null!;
    private TriggerConfiguration _combinedConfig = null!;

    [GlobalSetup]
    public void GlobalSetup()
    {
        // Create test contexts
        _basicContext = new TriggerContext(
            keywords: new[] { "test", "tdd", "failing" },
            filePath: "/src/test/TestFile.cs",
            contextType: "testing"
        );
        
        _complexContext = new TriggerContext(
            keywords: new[] { "implement", "feature", "refactor", "clean", "architecture", "design" },
            filePath: "/src/complex/ComplexFeature.cs",
            contextType: "implementation"
        );
        
        _emptyContext = new TriggerContext();
        
        // Create test configurations
        _keywordConfig = new TriggerConfiguration(
            keywords: new[] { "test", "tdd" }
        );
        
        _filePatternConfig = new TriggerConfiguration(
            filePatterns: new[] { "*.cs", "*.test.*" }
        );
        
        _contextPatternConfig = new TriggerConfiguration(
            contextPatterns: new[] { "testing", "implementation" }
        );
        
        _combinedConfig = new TriggerConfiguration(
            keywords: new[] { "implement", "feature" },
            filePatterns: new[] { "*.cs" },
            contextPatterns: new[] { "implementation" }
        );
    }

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("Relevance_Scoring")]
    public double CalculateRelevanceScore_Keywords()
    {
        return _basicContext.CalculateRelevanceScore(_keywordConfig);
    }

    [Benchmark]
    [BenchmarkCategory("Relevance_Scoring")]
    public double CalculateRelevanceScore_FilePatterns()
    {
        return _basicContext.CalculateRelevanceScore(_filePatternConfig);
    }

    [Benchmark]
    [BenchmarkCategory("Relevance_Scoring")]
    public double CalculateRelevanceScore_ContextPatterns()
    {
        return _basicContext.CalculateRelevanceScore(_contextPatternConfig);
    }

    [Benchmark]
    [BenchmarkCategory("Relevance_Scoring")]
    public double CalculateRelevanceScore_Combined()
    {
        return _complexContext.CalculateRelevanceScore(_combinedConfig);
    }

    [Benchmark]
    [BenchmarkCategory("Keyword_Matching")]
    public bool ContainsKeyword_Hit()
    {
        return _basicContext.ContainsAnyKeyword(new[] { "test", "implementation" });
    }

    [Benchmark]
    [BenchmarkCategory("Keyword_Matching")]
    public bool ContainsKeyword_Miss()
    {
        return _basicContext.ContainsAnyKeyword(new[] { "deploy", "production" });
    }

    [Benchmark]
    [BenchmarkCategory("Pattern_Matching")]
    public bool MatchesFilePattern()
    {
        return _basicContext.MatchesAnyFilePattern(new[] { "*.cs", "*.test.*" });
    }

    [Benchmark]
    [BenchmarkCategory("Pattern_Matching")]
    public bool MatchesContextPattern()
    {
        return _basicContext.MatchesAnyContextPattern(new[] { "testing", "implementation" });
    }

    [Benchmark]
    [BenchmarkCategory("Anti_Pattern_Detection")]
    public bool HasAntiPattern_NoMatch()
    {
        return _basicContext.HasAnyAntiPattern(new[] { "deploy", "production" });
    }

    [Benchmark]
    [BenchmarkCategory("Anti_Pattern_Detection")]
    public bool HasAntiPattern_Match()
    {
        return _basicContext.HasAnyAntiPattern(new[] { "test", "failing" });
    }

    [Benchmark]
    [BenchmarkCategory("Context_Creation")]
    public TriggerContext CreateBasicContext()
    {
        return new TriggerContext(
            keywords: new[] { "benchmark", "test" },
            filePath: "/test/Benchmark.cs",
            contextType: "performance"
        );
    }

    [Benchmark]
    [BenchmarkCategory("Context_Creation")]
    public TriggerContext CreateComplexContext()
    {
        var metadata = new Dictionary<string, object>
        {
            ["tool"] = "test_tool",
            ["iteration"] = 42,
            ["timestamp"] = DateTimeOffset.UtcNow
        };
        
        return new TriggerContext(
            keywords: new[] { "complex", "benchmark", "performance", "test" },
            filePath: "/complex/performance/BenchmarkTest.cs",
            contextType: "performance_testing",
            metadata: metadata,
            sessionId: "session_123"
        );
    }

    [Benchmark]
    [BenchmarkCategory("Configuration_Performance")]
    public bool HasActivationCriteria_Keywords()
    {
        return _keywordConfig.HasActivationCriteria;
    }

    [Benchmark]
    [BenchmarkCategory("Configuration_Performance")]
    public bool HasActivationCriteria_Combined()
    {
        return _combinedConfig.HasActivationCriteria;
    }

    [Benchmark]
    [BenchmarkCategory("Batch_Operations")]
    public double[] BatchRelevanceCalculation()
    {
        var configs = new[] { _keywordConfig, _filePatternConfig, _contextPatternConfig, _combinedConfig };
        var results = new double[configs.Length];
        
        for (int i = 0; i < configs.Length; i++)
        {
            results[i] = _complexContext.CalculateRelevanceScore(configs[i]);
        }
        
        return results;
    }
}