using RedisRateLimitAPI.Models;
using RedisRateLimitAPI.Services;
using StackExchange.Redis;

namespace RedisRateLimitAPI;

public class Program
{
    static void Main(string[] args)
    {
        WebApplicationBuilder? builder = WebApplication.CreateSlimBuilder(args);
        builder.Services.ConfigureHttpJsonOptions(options =>
        {
            options.SerializerOptions.TypeInfoResolverChain.Insert(0, SourceGenerationContext.Default);
        });
        builder.Services.AddSingleton<IRedisRateLimitService, RedisRateLimitService>(provider =>
        {
            ConnectionMultiplexer connection = ConnectionMultiplexer.Connect("localhost,abortConnect=false");
            return new RedisRateLimitService(connection);
        });

        WebApplication? app = builder.Build();

        app.MapGet("{**endpoint}", async (HttpContext context, IRedisRateLimitService redisRateLimitService) =>
        {
            bool? isRateLimited = await redisRateLimitService.IsRequestRateLimitedAsync(context.Request.Path);

            if (isRateLimited == null)
                return Results.Forbid();
            else if (isRateLimited ?? false)
                return Results.StatusCode(429);
            else
                return Results.Accepted();
        });

        app.Run();
    }
}