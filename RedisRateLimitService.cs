using RedisRateLimitAPI.Models;
using StackExchange.Redis;
using System.Text.Json;

namespace RedisRateLimitAPI.Services;

/// <summary>
///     Service responsible for rate limiting a request 
///     provided that it has been configured in the rate_limits.json <see cref="Models.RateLimitConfig"/>
/// </summary>
public interface IRedisRateLimitService
{
    /// <summary>
    ///		Checks if a rate limit key exists for a given endpoint
    /// </summary>
    /// <param name="endpoint">The key we're querying to determine whether this endpoint has a rate limit configured</param>
    /// <returns>
    /// If key exists: add one, return false,
    /// If key exists and is below configured limit: increment the count, return false
    /// If key exists, and exceeds the configured limit: return true
    /// </returns>
    Task<bool?> IsRequestRateLimitedAsync(string endpoint);
}

public class RedisRateLimitService(ConnectionMultiplexer connection) : IRedisRateLimitService
{
    public async Task<bool?> IsRequestRateLimitedAsync(string endpoint)
    {
        RateLimit? rateLimit = GetRateLimitForEndpoint(endpoint);

        if (rateLimit == null)
            return null;

        // Use cache key for this endpoint
        string cacheKey = $"RateLimit:{endpoint}";

        // Count the number of requests in this window for this given endpoint
        RedisValue requestsThisWindowCount = await connection.GetDatabase().StringGetAsync(cacheKey);

        if (!requestsThisWindowCount.IsNull)
        {
            // Increment the existing count
            int newCount = (int)requestsThisWindowCount + 1;

            // Check if the rate limit has been exceeded
            if (newCount > rateLimit.RequestLimit)
                return true;

            // Update the count in Redis
            return await connection.GetDatabase().StringSetAsync(cacheKey, newCount, TimeSpan.FromSeconds(rateLimit.Window));
        }
        
        return await connection.GetDatabase().StringSetAsync(cacheKey, 1, TimeSpan.FromSeconds(rateLimit.Window)); // Set the initial count for this endpoint
    }

    private RateLimit? GetRateLimitForEndpoint(string endpoint)
    {
        // Load the configuration file
        RateLimitConfig? rateLimitConfig = JsonSerializer.Deserialize(File.ReadAllText("rate_limits.json"), SourceGenerationContext.Default.RateLimitConfig);

        if (rateLimitConfig == null)
            return null;

        // Find the rate limit for the endpoint
        return rateLimitConfig?.Endpoints?.Find(e => e.Path == endpoint)?.RateLimit;
    }
}