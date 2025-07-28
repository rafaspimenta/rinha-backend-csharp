# Rinha Backend - C# Payment Processing API

A high-performance payment processing backend built with .NET 9 and optimized for the [Rinha de Backend](https://github.com/zanfranceschi/rinha-de-backend-2025) challenge. This system provides blazing-fast payment processing with queue management, health checks, and fault tolerance.

## 🚀 Features

- **High-Performance**: Built with .NET 9 AOT compilation and minimal APIs
- **Scalable Architecture**: Queue-based payment processing with worker pattern
- **Fault Tolerance**: Multiple payment processor strategies with fallback mechanisms
- **Fault Tolerance**: HTTP client retry policies with jitter for payment processor communication
- **Database Optimized**: PostgreSQL with performance-tuned indexes and UNLOGGED tables
- **Load Balanced**: Nginx load balancer with 2 API instances
- **Dockerized**: Complete container-based deployment with multi-platform support

## 🏗️ Architecture

```
┌─────────────┐    ┌─────────────┐    ┌─────────────┐
│   Nginx     │────│   API       │────│ PostgreSQL  │
│ Load        │    │ Instances   │    │ Database    │
│ Balancer    │    │ (2x)        │    │             │
└─────────────┘    └─────────────┘    └─────────────┘
                           │
                   ┌─────────────┐
                   │   Payment   │
                   │  Processor  │
                   │             │
                   └─────────────┘
```

## 🔧 Technical Specifications

- **.NET 9**: Latest framework with AOT compilation and trimming
- **ASP.NET Core**: Minimal APIs for optimal performance
- **PostgreSQL 16.1**: High-performance database with UNLOGGED tables and optimized settings
- **Nginx**: Load balancing with upstream failover configuration
- **Docker**: Containerized deployment with multi-platform support (ARM64/AMD64)

### Performance Optimizations

- **AOT Compilation**: Native code generation for faster startup
- **Trimmed Publishing**: Reduced application size with symbol stripping
- **Invariant Globalization**: Faster string operations
- **Connection Pooling**: Optimized database connections
- **Async Processing**: Queue-based payment handling
- **Minimal Allocations**: Reduced GC pressure
- **Optimized JSON**: Source-generated serialization
- **UNLOGGED Tables**: Faster write performance for payments
- **Optimized Indexes**: Composite index for efficient summary queries

## 📋 Prerequisites

- [Docker](https://docs.docker.com/get-docker/) and Docker Compose
- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0) (for development)
- [Make](https://www.gnu.org/software/make/) (optional, for convenience commands)

### Resource Limits

The deployment is optimized for the [Rinha de Backend constraints](https://github.com/zanfranceschi/rinha-de-backend-2025):
- **Total CPU**: 1.5 cores (0.6 + 0.6 + 0.1 + 0.2)
- **Total Memory**: 350MB (90MB + 90MB + 50MB + 120MB)
- **API Instances**: 2x (90MB each, 0.6 CPU each)
- **Database**: 120MB (0.2 CPU)
- **Nginx**: 50MB (0.1 CPU)

## 🚀 Quick Start

### Development (ARM64 only)
```bash
make dev
```

### Production Build
```bash
make build-prod
```

### Production Deployment
```bash
make push-prod
```

## 📊 API Endpoints

- `POST /payments` - Submit payment for processing
- `GET /payments-summary` - Get payment summary with date filtering
- `POST /purge-payments` - Purge all payments from database

## 🔧 Configuration

The application uses configuration-based settings for:
- Payment processor URLs (default and fallback)
- HTTP client timeouts and retry policies
- Database connection settings
- Kestrel server optimizations

## 🐳 Docker Configuration

- **Multi-platform support**: ARM64 for development, AMD64 for production
- **Health checks**: PostgreSQL with connection verification
- **Network isolation**: Separate networks for app and payment processor communication
- **Optimized images**: Alpine-based PostgreSQL for smaller footprint
- **Development mode**: Uses docker-compose.override.yml for local ARM64 builds