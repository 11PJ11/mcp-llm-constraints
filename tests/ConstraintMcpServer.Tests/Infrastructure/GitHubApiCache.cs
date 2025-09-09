using System.Collections.Concurrent;
using System.Net;

namespace ConstraintMcpServer.Tests.Infrastructure;

/// <summary>
/// Thread-safe GitHub API cache with rate limiting protection for E2E tests.
/// Prevents API rate limit exceeded errors during full test suite execution.
/// </summary>
public sealed class GitHubApiCache : IDisposable
{
    private const int DefaultCacheTtlMinutes = 10;
    private const int RateLimitDelaySeconds = 2;
    private const int MaxConcurrentRequests = 1;

    private readonly HttpClient _httpClient;
    private readonly ConcurrentDictionary<string, CacheEntry> _cache;
    private readonly SemaphoreSlim _throttleSemaphore;
    private readonly TimeSpan _defaultCacheTtl;
    private readonly TimeSpan _rateLimitDelay;
    private bool _disposed;

    public GitHubApiCache(HttpClient? httpClient = null)
    {
        _httpClient = httpClient ?? new HttpClient();
        _cache = new ConcurrentDictionary<string, CacheEntry>();
        _throttleSemaphore = new SemaphoreSlim(MaxConcurrentRequests, MaxConcurrentRequests);
        _defaultCacheTtl = TimeSpan.FromMinutes(DefaultCacheTtlMinutes);
        _rateLimitDelay = TimeSpan.FromSeconds(RateLimitDelaySeconds);
    }

    public async Task<HttpResponseMessage> GetAsync(string url, TimeSpan? cacheTtl = null)
    {
        ThrowIfDisposed();

        var effectiveTtl = cacheTtl ?? _defaultCacheTtl;

        var cachedResponse = TryGetFromCache(url, effectiveTtl);
        if (cachedResponse != null)
        {
            return cachedResponse;
        }

        return await MakeThrottledApiCall(url, effectiveTtl);
    }

    public void ClearCache()
    {
        _cache.Clear();
        LogCacheCleared();
    }

    public CacheStats GetCacheStats()
    {
        return CalculateCacheStatistics();
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _throttleSemaphore?.Dispose();
        _cache.Clear();
        _disposed = true;
    }

    private void ThrowIfDisposed()
    {
        if (_disposed)
        {
            throw new ObjectDisposedException(nameof(GitHubApiCache));
        }
    }

    private HttpResponseMessage? TryGetFromCache(string url, TimeSpan ttl)
    {
        if (!_cache.TryGetValue(url, out var cachedEntry))
        {
            return null;
        }

        if (!IsCacheEntryValid(cachedEntry, ttl))
        {
            return null;
        }

        LogCacheHit(url);
        return CreateResponseFromCacheEntry(cachedEntry);
    }

    private async Task<HttpResponseMessage> MakeThrottledApiCall(string url, TimeSpan ttl)
    {
        await _throttleSemaphore.WaitAsync();
        try
        {
            return await ExecuteApiCallWithCaching(url, ttl);
        }
        finally
        {
            _throttleSemaphore.Release();
        }
    }

    private async Task<HttpResponseMessage> ExecuteApiCallWithCaching(string url, TimeSpan ttl)
    {
        var cachedResponse = TryGetFromCacheAfterLock(url, ttl);
        if (cachedResponse != null)
        {
            return cachedResponse;
        }

        LogApiCall(url);
        await ApplyRateLimitDelay();

        var response = await _httpClient.GetAsync(url);

        return await HandleApiResponse(url, response);
    }

    private HttpResponseMessage? TryGetFromCacheAfterLock(string url, TimeSpan ttl)
    {
        if (!_cache.TryGetValue(url, out var cachedEntry))
        {
            return null;
        }

        if (!IsCacheEntryValid(cachedEntry, ttl))
        {
            return null;
        }

        LogCacheHitAfterLock(url);
        return CreateResponseFromCacheEntry(cachedEntry);
    }

    private async Task<HttpResponseMessage> HandleApiResponse(string url, HttpResponseMessage response)
    {
        if (IsRateLimitResponse(response))
        {
            LogRateLimitDetailed(response, url);
            return HandleRateLimitResponse(url, response);
        }

        if (response.IsSuccessStatusCode)
        {
            return await CacheSuccessfulResponse(url, response);
        }

        return response;
    }

    private HttpResponseMessage HandleRateLimitResponse(string url, HttpResponseMessage response)
    {
        var staleResponse = TryGetStaleFromCache(url);
        if (staleResponse != null)
        {
            LogRateLimitWithStaleCache(url);
            return staleResponse;
        }

        LogRateLimitNoCache(url);
        return response;
    }

    private async Task<HttpResponseMessage> CacheSuccessfulResponse(string url, HttpResponseMessage response)
    {
        var content = await response.Content.ReadAsStringAsync();
        var cacheEntry = new CacheEntry(content, response.StatusCode, DateTime.UtcNow);

        _cache.AddOrUpdate(url, cacheEntry, (key, existing) => cacheEntry);

        LogResponseCached(url);
        return CreateNewResponseFromContent(content, response.StatusCode, response.ReasonPhrase);
    }

    private HttpResponseMessage? TryGetStaleFromCache(string url)
    {
        if (!_cache.TryGetValue(url, out var staleEntry))
        {
            return null;
        }

        return CreateResponseFromCacheEntry(staleEntry);
    }

    private CacheStats CalculateCacheStatistics()
    {
        var now = DateTime.UtcNow;
        var validEntries = 0;
        var expiredEntries = 0;

        foreach (var entry in _cache.Values)
        {
            if (IsCacheEntryValid(entry, _defaultCacheTtl, now))
            {
                validEntries++;
            }
            else
            {
                expiredEntries++;
            }
        }

        return new CacheStats(validEntries, expiredEntries, _cache.Count);
    }

    private static bool IsCacheEntryValid(CacheEntry entry, TimeSpan ttl, DateTime? currentTime = null)
    {
        var now = currentTime ?? DateTime.UtcNow;
        return now - entry.Timestamp < ttl;
    }

    private static bool IsRateLimitResponse(HttpResponseMessage response)
    {
        // GitHub rate limiting can return multiple status codes
        return response.StatusCode == HttpStatusCode.Forbidden ||              // 403 - Standard rate limit
               response.StatusCode == HttpStatusCode.TooManyRequests ||           // 429 - OAuth rate limit
               (response.StatusCode == HttpStatusCode.NotFound &&                 // 404 - Unauthenticated limit exceeded
                IsGitHubApiUrl(response.RequestMessage?.RequestUri));
    }

    private static bool IsGitHubApiUrl(Uri? uri)
    {
        return uri?.Host?.Contains("api.github.com") == true;
    }

    private static HttpResponseMessage CreateResponseFromCacheEntry(CacheEntry entry)
    {
        return CreateNewResponseFromContent(entry.Content, entry.StatusCode, "Cached Response");
    }

    private static HttpResponseMessage CreateNewResponseFromContent(string content, HttpStatusCode statusCode, string? reasonPhrase)
    {
        return new HttpResponseMessage(statusCode)
        {
            Content = new StringContent(content),
            ReasonPhrase = reasonPhrase ?? (statusCode == HttpStatusCode.OK ? "OK" : "Response")
        };
    }

    private static async Task ApplyRateLimitDelay()
    {
        await Task.Delay(TimeSpan.FromSeconds(RateLimitDelaySeconds));
    }

    private static void LogCacheHit(string url)
    {
        Console.WriteLine($"✅ GitHub API cache hit for: {url}");
    }

    private static void LogCacheHitAfterLock(string url)
    {
        Console.WriteLine($"✅ GitHub API cache hit after lock for: {url}");
    }

    private static void LogApiCall(string url)
    {
        Console.WriteLine($"🌐 Making GitHub API call to: {url}");
    }

    private static void LogRateLimitWithStaleCache(string url)
    {
        Console.WriteLine($"⚠️ GitHub API rate limited, using stale cache for: {url}");
    }

    private static void LogRateLimitNoCache(string url)
    {
        Console.WriteLine($"❌ GitHub API rate limited, no cache available for: {url}");
    }

    private static void LogRateLimitDetailed(HttpResponseMessage response, string url)
    {
        var statusCode = (int)response.StatusCode;
        var reasonPhrase = response.ReasonPhrase ?? "Unknown";
        Console.WriteLine($"🚫 GitHub API rate limit detected: {statusCode} {reasonPhrase} for: {url}");

        if (response.Headers.TryGetValues("X-RateLimit-Remaining", out var remaining))
        {
            Console.WriteLine($"   Rate limit remaining: {string.Join(",", remaining)}");
        }
        if (response.Headers.TryGetValues("X-RateLimit-Reset", out var reset))
        {
            Console.WriteLine($"   Rate limit reset: {string.Join(",", reset)}");
        }
    }

    private static void LogResponseCached(string url)
    {
        Console.WriteLine($"✅ GitHub API response cached for: {url}");
    }

    private static void LogCacheCleared()
    {
        Console.WriteLine("🗑️ GitHub API cache cleared");
    }

    private sealed record CacheEntry(string Content, HttpStatusCode StatusCode, DateTime Timestamp);
}

public sealed record CacheStats(int ValidEntries, int ExpiredEntries, int TotalEntries);
