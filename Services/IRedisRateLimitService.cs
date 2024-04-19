namespace RedisRateLimitAPI.Services
{
    public interface IRedisRateLimitService
    {
        Task<bool?> IsRequestRateLimitedAsync(string endpoint);
    }
}