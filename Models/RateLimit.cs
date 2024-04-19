namespace RedisRateLimitAPI.Models;

public class RateLimit
{
    public int RequestLimit { get; set; }
    public int Window { get; set; }
}