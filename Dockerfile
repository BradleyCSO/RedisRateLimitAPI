#https://github.com/dotnet/samples/blob/main/core/nativeaot/HelloWorld/Dockerfile

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

# Install NativeAOT build prerequisites
RUN apt-get update \
    && apt-get install -y --no-install-recommends \
       clang zlib1g-dev

WORKDIR /source

COPY . .
RUN dotnet publish -o /app RedisRateLimitAPI.csproj

FROM mcr.microsoft.com/dotnet/runtime-deps:8.0
WORKDIR /app
COPY --from=build /app .
ENTRYPOINT ["/app/RedisRateLimitAPI"]