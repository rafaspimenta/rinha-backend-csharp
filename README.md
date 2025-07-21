# Rinha Backend - C# Payment Processing API

A high-performance payment processing backend built with .NET 9 and optimized for the [Rinha de Backend](https://github.com/zanfranceschi/rinha-de-backend-2025) challenge. This system provides blazing-fast payment processing with queue management, health checks, and fault tolerance.

## 🚀 Features

- **High-Performance**: Built with .NET 9 AOT compilation and minimal APIs
- **Scalable Architecture**: Queue-based payment processing with worker pattern
- **Fault Tolerance**: Multiple payment processor strategies with fallback mechanisms
- **Health Monitoring**: Real-time health checks for payment processors
- **Database Optimized**: PostgreSQL with performance-tuned indexes
- **Load Balanced**: Nginx load balancer with 4 API instances
- **Dockerized**: Complete container-based deployment

## 🏗️ Architecture

```
┌─────────────┐    ┌─────────────┐    ┌─────────────┐
│   Nginx     │────│   API       │────│ PostgreSQL  │
│ Load        │    │ Instances   │    │ Database    │
│ Balancer    │    │ (4x)        │    │             │
└─────────────┘    └─────────────┘    └─────────────┘
                           │
                   ┌─────────────┐
                   │   Payment   │
                   │  Processor  │
                   │             │
                   └─────────────┘
```

## 🔧 Technical Specifications

- **.NET 9**: Latest framework with AOT compilation
- **ASP.NET Core**: Minimal APIs for optimal performance
- **PostgreSQL 16**: High-performance database with UNLOGGED tables
- **Nginx**: Load balancing and request routing
- **Docker**: Containerized deployment

### Performance Optimizations

- **AOT Compilation**: Native code generation for faster startup
- **Trimmed Publishing**: Reduced application size
- **Connection Pooling**: Optimized database connections
- **Async Processing**: Queue-based payment handling
- **Minimal Allocations**: Reduced GC pressure
- **Optimized JSON**: Source-generated serialization

## 📋 Prerequisites

- [Docker](https://docs.docker.com/get-docker/) and Docker Compose
- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0) (for development)
- [Make](https://www.gnu.org/software/make/) (optional, for convenience commands)

### Resource Limits

The deployment is optimized for the [Rinha de Backend constraints](https://github.com/zanfranceschi/rinha-de-backend-2025):
- **CPU**: 1.5 cores total
- **Memory**: 550MB total
- **API Instances**: 4x (80MB each)
- **Database**: 150MB
- **Nginx**: 40MB