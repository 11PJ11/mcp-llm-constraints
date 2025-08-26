# Constraint MCP Server Performance Benchmarks

Performance benchmarks for the Constraint MCP Server, focusing on sub-50ms p95 latency and sub-100MB memory requirements.

## Running Benchmarks

### Prerequisites
- .NET 8.0 SDK
- BenchmarkDotNet 0.14.0

### Quick Start
```bash
# Build the benchmarks
dotnet build tests/ConstraintMcpServer.Performance

# Run all benchmarks
dotnet run --project tests/ConstraintMcpServer.Performance --configuration Release

# Run specific benchmark categories
dotnet run --project tests/ConstraintMcpServer.Performance --configuration Release -- --filter "*TriggerBenchmark*"
dotnet run --project tests/ConstraintMcpServer.Performance --configuration Release -- --filter "*ConstraintLibraryBenchmark*"
```

### Available Benchmark Categories

#### SimplifiedTriggerBenchmark
- **Relevance_Scoring**: TriggerContext relevance calculation performance
- **Keyword_Matching**: Keyword containment checking
- **Pattern_Matching**: File and context pattern matching
- **Anti_Pattern_Detection**: Anti-pattern detection performance
- **Context_Creation**: TriggerContext instantiation overhead
- **Configuration_Performance**: TriggerConfiguration property access
- **Batch_Operations**: Batch relevance calculations

#### SimplifiedConstraintLibraryBenchmark
- **Library_Operations**: Basic library operations (count, access)
- **Constraint_Resolution**: Individual constraint resolution
- **Batch_Resolution**: Batch constraint resolution
- **Library_Creation**: Library construction with varying sizes
- **Collection_Operations**: Collection access patterns
- **Validation**: Library integrity validation

## Performance Requirements

### Latency Targets
- **P95 Latency**: ≤ 50ms per tool call
- **P99 Latency**: ≤ 100ms per tool call

### Memory Targets
- **Server Process**: ≤ 100MB resident memory
- **Allocation Rate**: ≤ 10KB per operation
- **GC Pressure**: ≤ 0.01 Gen2 collections per operation

### Test Scenarios
- **Small Library**: 10 atomic + 5 composite constraints
- **Medium Library**: 50 atomic + 25 composite constraints  
- **Large Library**: 200 atomic + 100 composite constraints

## Benchmark Configuration

The benchmarks use custom `ConstraintServerBenchmarkConfig` which includes:
- .NET 8.0 Core Runtime with RyuJit
- Server GC with concurrent collection
- Memory diagnostics enabled
- HTML and Markdown export formats
- Performance threshold validation

## Output Formats

Results are exported in multiple formats:
- **HTML**: Detailed interactive results with charts
- **Markdown**: GitHub-friendly summary tables
- **Console**: Real-time progress and summary

## Integration with CI/CD

The benchmarks are designed to integrate with automated performance validation:

1. **Baseline Establishment**: Initial runs establish performance baselines
2. **Regression Detection**: Subsequent runs detect performance regressions
3. **Quality Gates**: Automated pass/fail based on performance thresholds
4. **Trend Analysis**: Historical performance tracking over time

## Composition-Specific Testing

The benchmarks specifically test the new library-based constraint system:
- **ConstraintReference** resolution patterns
- **CompositionType** performance characteristics  
- **TriggerConfiguration** matching efficiency
- **LibraryConstraintResolver** lookup performance
- **Batch operations** for realistic usage patterns

## Memory Analysis

Memory benchmarks focus on:
- **Allocation patterns**: Object creation overhead
- **Collection efficiency**: GC behavior under load
- **Memory usage scaling**: Growth patterns with library size
- **Reference management**: ConstraintReference overhead
- **String allocations**: Keyword and pattern matching costs