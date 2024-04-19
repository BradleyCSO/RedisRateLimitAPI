using RedisRateLimitAPI.Models;
using StackExchange.Redis;
using System.Text.Json;

namespace RedisRateLimitAPI.Services;

public class RedisRateLimitService(ConnectionMultiplexer connection) : IRedisRateLimitService
{
    public async Task<bool?> IsRequestRateLimitedAsync(string endpoint)
    {
        RateLimit? rateLimit = GetRateLimitForEndpoint(endpoint);

        if (rateLimit == null)
            return null;

        try
        {
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
                await connection.GetDatabase().StringSetAsync(cacheKey, newCount, TimeSpan.FromSeconds(rateLimit.Window));
            }
            else // Set the initial count for this endpoint
                await connection.GetDatabase().StringSetAsync(cacheKey, 1, TimeSpan.FromSeconds(rateLimit.Window));
        }
        finally
        {
            //await connection.CloseAsync();
        }

        return false;
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