using System.Text.Json;

namespace RedisRateLimitAPI.Models;

public class RateLimitConfig
{
    public List<Endpoint>? Endpoints { get; set; }
}