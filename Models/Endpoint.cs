namespace RedisRateLimitAPI.Models;

public class Endpoint
{
    public string? Path { get; set; }
    public RateLimit? RateLimit { get; set; }
}