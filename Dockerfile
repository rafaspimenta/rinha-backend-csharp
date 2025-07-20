# Stage 1: Build AOT-compiled application
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy project files
COPY . .

# Publish the app with AOT, trimming and no debug symbols
RUN dotnet publish -c Release -o /app

# Stage 2: Create minimal runtime image (distroless style)
FROM mcr.microsoft.com/dotnet/runtime-deps:9.0 AS runtime

# Set environment variables
ENV DOTNET_EnableDiagnostics=0 \
    ASPNETCORE_URLS=http://+:8080 \
    DOTNET_ENVIRONMENT=Production

WORKDIR /app

# Copy AOT binary from build stage
COPY --from=build /app .

# Expose default HTTP port
EXPOSE 8080

# Set the entrypoint to the compiled binary
ENTRYPOINT ["./rinha-backend-csharp"]
