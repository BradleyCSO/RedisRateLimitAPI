# Use the Microsoft's official build image.
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

# Install Clang, required for AOT compilation.
RUN apt-get update && \
    apt-get install -y clang zlib1g-dev

# Set the working directory in the container.
WORKDIR /src

# Copy csproj and restore any dependencies (via NuGet).
COPY *.csproj .
RUN dotnet restore

# Copy the project files and build our release.
COPY . .
RUN dotnet publish -c Release -o /app -r linux-x64 --self-contained true /p:PublishAot=true

# Generate the runtime image.
FROM mcr.microsoft.com/dotnet/runtime-deps:8.0 AS runtime
WORKDIR /app
COPY --from=build /app .