using NUnit.Framework;

// Reduce parallel execution to minimize GitHub API rate limiting during test execution
[assembly: LevelOfParallelism(1)]

// Set reasonable default timeout for all tests
[assembly: Timeout(60000)]

// Configure test categories for selective execution
[assembly: Category("E2E")]