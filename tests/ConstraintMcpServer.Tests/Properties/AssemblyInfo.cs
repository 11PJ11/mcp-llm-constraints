using NUnit.Framework;

// Configure intelligent parallel execution:
// - Allow moderate parallelism for faster execution
// - API-sensitive tests use [NonParallelizable] attribute for safety
[assembly: LevelOfParallelism(3)]
[assembly: Parallelizable(ParallelScope.Fixtures)]

// Set reasonable default timeout for all tests
[assembly: Timeout(60000)]

// Configure test categories for selective execution
[assembly: Category("E2E")]
