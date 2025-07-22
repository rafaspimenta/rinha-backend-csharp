# Stage 1: Build AOT-compiled application
FROM --platform=linux/amd64 mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Install native AOT prerequisites
RUN apt-get update && \
    apt-get install -y clang zlib1g-dev && \
    apt-get clean

# Copy project files
COPY . .

# Publish the app with AOT, trimming and no debug symbols for linux-x64
RUN dotnet publish -c Release -r linux-x64 -o /app

# Stage 2: Create minimal runtime image (distroless style)
FROM --platform=linux/amd64 mcr.microsoft.com/dotnet/runtime-deps:9.0 AS runtime

# Set environment variables
ENV DOTNET_EnableDiagnostics=0 \
    DOTNET_SYSTEM_CONSOLE_ALLOW_ANSI_COLOR_REDIRECTION=0 \
    DOTNET_NOLOGO=true \
    ASPNETCORE_URLS=http://+:8080 \
    DOTNET_ENVIRONMENT=Production

WORKDIR /app

# Copy AOT binary from build stage
COPY --from=build /app .

# Expose default HTTP port
EXPOSE 8080

# Set the entrypoint to the compiled binary
ENTRYPOINT ["./rinha-backend-csharp"]
