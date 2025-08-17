# Rinha Backend - C# Payment Processing API

[![Latest tag](https://img.shields.io/github/v/tag/rafaspimenta/rinha-backend-csharp?sort=semver&label=latest%20tag)](https://github.com/rafaspimenta/rinha-backend-csharp/tags)
[![CI](https://github.com/rafaspimenta/rinha-backend-csharp/actions/workflows/on-push-to-main.yml/badge.svg?branch=main)](https://github.com/rafaspimenta/rinha-backend-csharp/actions/workflows/on-push-to-main.yml)

A high-performance payment processing backend built with .NET 9 and optimized for the [Rinha de Backend](https://github.com/zanfranceschi/rinha-de-backend-2025) challenge. This system provides blazing-fast payment processing with queue management, health checks, and fault tolerance.

## 🚀 Features

- **High-Performance**: Built with .NET 9 AOT compilation and minimal APIs using CreateSlimBuilder
- **Scalable Architecture**: Channel-based unbounded queue with concurrent background processing
- **Fault Tolerance**: Multiple payment processor strategies (Default → Fallback) with automatic retry
- **Resilient HTTP**: Polly retry policies with exponential backoff and jitter for external calls
- **Database Optimized**: PostgreSQL with UNLOGGED tables, connection pooling, and composite indexes
- **Load Balanced**: Nginx upstream load balancer with 2 API instances and health checks
- **Multi-Platform**: Docker deployment supporting both ARM64 (development) and AMD64 (production)

## 🏗️ Architecture

```
┌─────────────┐    ┌─────────────┐    ┌─────────────┐
│   Nginx     │────│   API       │────│ PostgreSQL  │
│ Load        │    │ Instances   │    │ Database    │
│ Balancer    │    │ (2x)        │    │             │
└─────────────┘    └─────────────┘    └─────────────┘
                          │
                  ┌─────────────┐    ┌─────────────┐
                  │  In-Memory  │    │  External   │
                  │   Queue     │────│  Payment    │
                  │  +Worker    │    │ Processors  │
                  └─────────────┘    └─────────────┘
```

### Key Components

- **Nginx Load Balancer**: Routes traffic between API instances with upstream failover
- **API Instances (2x)**: .NET 9 AOT-compiled minimal APIs with queue-based processing
- **In-Memory Queue**: Channel-based unbounded queue for payment processing
- **Background Worker**: Concurrent payment processing with configurable concurrency limits (SemaphoreSlim)
- **PostgreSQL Database**: UNLOGGED tables with optimized indexes for high-performance writes
- **External Payment Processors**: Default and fallback processors with retry policies

## 🔧 Technical Specifications

- **.NET 9**: Latest framework with AOT compilation and trimming
- **ASP.NET Core**: Minimal APIs for optimal performance
- **PostgreSQL 16.1**: High-performance database with UNLOGGED tables and optimized settings
- **Nginx**: Load balancing with upstream failover configuration
- **Docker**: Containerized deployment with multi-platform support (ARM64/AMD64)

### Performance Optimizations

- **AOT Compilation**: Native code generation for faster startup and smaller footprint
- **Trimmed Publishing**: Reduced application size with symbol stripping
- **Invariant Globalization**: Faster string operations
- **Connection Pooling**: PostgreSQL connection pooling with multiplexing (200 max connections)
- **Channel-based Queue**: High-performance unbounded queue for payment processing
- **Concurrent Processing**: SemaphoreSlim-controlled concurrent payment processing (configurable limit)
- **Minimal APIs**: WebApplicationBuilder.CreateSlimBuilder for reduced overhead
- **Source-generated JSON**: AppJsonSerializerContext for zero-allocation serialization
- **UNLOGGED Tables**: PostgreSQL UNLOGGED tables for maximum write performance
- **Optimized Indexes**: Composite index (processor_type, requested_at) for summary queries
- **HTTP Client Pooling**: Reusable HTTP clients with Polly retry policies
- **Nginx Optimizations**: Epoll, multi-accept, and connection keep-alive

## 📋 Prerequisites

- [Docker](https://docs.docker.com/get-docker/) and Docker Compose
- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0) (for development)
- [Make](https://www.gnu.org/software/make/) (optional, for convenience commands)

### Resource Limits

The deployment is optimized for the [Rinha de Backend constraints](https://github.com/zanfranceschi/rinha-de-backend-2025):
- **Total CPU**: 1.5 cores (0.55 + 0.55 + 0.2 + 0.2)
- **Total Memory**: 350MB (75MB + 75MB + 40MB + 160MB)
- **API Instances**: 2x (75MB each, 0.55 CPU each)
- **Database**: 160MB (0.2 CPU)
- **Nginx**: 40MB (0.2 CPU)

## 🚀 Quick Start

### Development (ARM64 - Mac/Apple Silicon)
```bash
make dev
```

### Production Build (AMD64)
```bash
make prod
```

### Production Image Push
```bash
make prod-push
```

### Clean Up
```bash
make clean
```

## 📊 API Endpoints

- `POST /payments` - Submit payment for processing
- `GET /payments-summary` - Get payment summary with date filtering
- `POST /purge-payments` - Purge all payments from database

## 🔧 Configuration

The application uses environment-specific configuration:

### Payment Processor Settings
- **DefaultUrl**: Primary payment processor endpoint
- **FallbackUrl**: Backup payment processor endpoint  
- **HttpClientTimeoutMilliseconds**: HTTP client timeout (60s)
- **RetryCount**: Number of retry attempts (5)
- **RetryJitterMilliseconds**: Random jitter for retry delays (500ms)
- **MaxConcurrentPayments**: Maximum concurrent payment processing (2)

### Database Configuration
- **Development**: localhost:5432 with basic connection pooling
- **Production**: postgres:5432 with advanced pooling (200 max connections, multiplexing)

### Logging
- **Development**: Information level logging
- **Production**: Error-only logging for performance

## 🐳 Docker Configuration

- **Multi-stage builds**: Separate build and runtime stages for optimal image size
- **Multi-platform support**: ARM64 for development, AMD64 for production  
- **AOT-compiled binaries**: Native executables for faster startup and smaller memory footprint
- **Health checks**: PostgreSQL with `pg_isready` connection verification
- **Network isolation**: Separate networks for app and payment processor communication
- **Optimized PostgreSQL**: Alpine-based with performance tuning (fsync=off, wal_level=minimal)
- **Resource constraints**: Precise CPU and memory limits per service