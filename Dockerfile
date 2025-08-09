# Stage 1: Build AOT-compiled application
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

ARG TARGETPLATFORM
ARG TARGETARCH

# Install native AOT prerequisites
RUN apt-get update && \
    apt-get install -y clang zlib1g-dev && \
    apt-get clean

# Copy project files
COPY . .

RUN if [ "$TARGETARCH" = "arm64" ]; then \
        RUNTIME_ID="linux-arm64"; \
    else \
        RUNTIME_ID="linux-x64"; \
    fi && \
    echo "Building for runtime: $RUNTIME_ID" && \
    dotnet publish src/rinha-backend-csharp.csproj -c Release -r $RUNTIME_ID -o /app

# Stage 2: Create minimal runtime image (distroless style)
FROM mcr.microsoft.com/dotnet/runtime-deps:9.0 AS runtime

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
