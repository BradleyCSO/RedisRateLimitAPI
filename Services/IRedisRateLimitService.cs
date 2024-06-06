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