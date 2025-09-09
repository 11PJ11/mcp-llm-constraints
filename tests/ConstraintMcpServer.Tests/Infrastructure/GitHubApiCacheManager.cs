using System;
using System.Net.Http;

namespace ConstraintMcpServer.Tests.Infrastructure;

/// <summary>
/// Thread-safe singleton cache manager for GitHub API operations across all test instances.
/// Prevents race conditions during concurrent test execution and improves cache efficiency.
/// </summary>
public sealed class GitHubApiCacheManager
{
    private static readonly Lazy<GitHubApiCacheManager> _instance =
        new(() => new GitHubApiCacheManager());

    /// <summary>
    /// Gets the singleton instance of the cache manager.
    /// </summary>
    public static GitHubApiCacheManager Instance => _instance.Value;

    private readonly GitHubApiCache _cache;

    private GitHubApiCacheManager()
    {
        // Create shared HTTP client with appropriate configuration
        var httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.Add("User-Agent",
            "ConstraintMcpServer-E2E-Tests/1.0");

        // Create cache with shared HTTP client
        _cache = new GitHubApiCache(httpClient);

        Console.WriteLine("🔧 GitHubApiCacheManager initialized with shared cache instance");
    }

    /// <summary>
    /// Gets the shared cache instance for all tests.
    /// </summary>
    public GitHubApiCache Cache => _cache;

    /// <summary>
    /// Pre-warms the cache with critical GitHub API endpoints.
    /// Call this once before test execution to prevent cold start race conditions.
    /// </summary>
    public static async Task PreWarmCacheAsync()
    {
        var cache = Instance.Cache;

        var criticalEndpoints = new[]
        {
            "https://api.github.com",
            "https://api.github.com/repos/11PJ11/mcp-llm-constraints/releases/latest"
        };

        Console.WriteLine($"🔥 Pre-warming GitHub API cache with {criticalEndpoints.Length} endpoints...");

        foreach (var endpoint in criticalEndpoints)
        {
            try
            {
                var response = await cache.GetAsync(endpoint);
                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"✅ Pre-warmed: {endpoint}");
                }
                else
                {
                    Console.WriteLine($"⚠️ Pre-warm partial: {endpoint} ({response.StatusCode})");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Pre-warm failed: {endpoint} - {ex.Message}");
            }
        }

        Console.WriteLine("🔥 Cache pre-warming completed");
    }
}
