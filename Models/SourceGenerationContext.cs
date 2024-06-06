using System.Text.Json.Serialization;

namespace RedisRateLimitAPI.Models;

[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(RateLimitConfig))]
internal partial class SourceGenerationContext : JsonSerializerContext { }