# Pulse Banking Platform

A multi-tenant banking platform built with .NET 8, following Clean Architecture principles.

## Project Overview

Pulse Banking Platform is a scalable banking system that can be deployed in two ways:
1. Regional Deployment: Multiple banks share a single platform instance while maintaining strict data isolation
2. Dedicated Deployment: A single bank gets their own dedicated platform instance

Each deployed instance of Pulse Banking Platform operates completely independently. They run in separate cloud accounts with no shared resources or cross-communication between instances. This deployment flexibility allows for:
- Data sovereignty through regional deployments
- Complete isolation for banks requiring dedicated infrastructure
- Cost-effective shared infrastructure for banks comfortable with tenant-based isolation

## Architecture

The solution follows Clean Architecture principles with the following structure:

```
PulseBanking/
├── src/
│   ├── PulseBanking.Domain/           # Enterprise business rules
│   ├── PulseBanking.Application/      # Application business rules
│   ├── PulseBanking.Infrastructure/   # External concerns
│   └── PulseBanking.Api/              # Entry point
└── tests/
    ├── PulseBanking.Domain.Tests/
    ├── PulseBanking.Application.Tests/
    ├── PulseBanking.Infrastructure.Tests/
    └── PulseBanking.Api.Tests/
```

## Key Features

- Flexible deployment options (Regional shared or Bank-specific dedicated)
- Tenant isolation within shared deployments
- Clean Architecture implementation
- Domain-Driven Design (DDD) practices
- CQRS pattern using MediatR
- Comprehensive test coverage
- Swagger API documentation
- Entity Framework Core with SQL Server

## Deployment Models
### Regional Deployment

- Multiple banks share a single platform instance
- Each bank is isolated through tenant-based separation
- All banks in the deployment share the same database
- Suitable for banks comfortable with tenant-based isolation
- Cost-effective through shared infrastructure

### Dedicated Deployment

- Single bank gets their own platform instance
- Complete infrastructure isolation
- Dedicated database and resources
- Suitable for banks requiring complete isolation
- Full control over the deployment

## Multi-tenant Implementation

The platform implements multi-tenancy through:

- Tenant identification via request headers
- Middleware-based tenant validation
- Global query filters for data isolation
- Tenant-specific configurations
- Strict data separation at the application level

## Getting Started

### Prerequisites

- .NET 8 SDK
- SQL Server
- Visual Studio 2022 or similar IDE

### Setup

1. Clone the repository:

```bash
git clone [repository-url]
```

2. Update the connection string in `src/PulseBanking.Api/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(local);Database=PulseBanking;Trusted_Connection=True;MultipleActiveResultSets=true;Trust Server Certificate=True;"
  }
}
```

3. Apply database migrations:

```bash
dotnet ef database update -p src/PulseBanking.Infrastructure -s src/PulseBanking.Api
```

### Running Tests

```bash
dotnet test
```

## Project Structure Details

### Domain Layer

- Contains enterprise business rules
- Implements domain entities (Account, etc.)
- Pure C# with no dependencies

### Application Layer

- Contains application business rules
- Implements use cases
- Uses CQRS with MediatR
- Defines interfaces for infrastructure

### Infrastructure Layer

- Implements interfaces defined in Application layer
- Handles database operations
- Manages multi-tenant context
- Implements cross-cutting concerns

### API Layer

- ASP.NET Core Web API
- Dependency injection configuration
- API endpoints and controllers
- Swagger documentation

## Development Workflow

1. Create feature branch from `dev`
2. Implement changes following Clean Architecture
3. Add tests for new functionality
4. Create pull request to `dev`
5. Merge to `main` after review

## Key Design Decisions

### Multi-tenancy

- Header-based tenant identification (`X-TenantId` header)
- Tenant middleware for validation
- Global query filters for data isolation
- Shared database with tenant-based separation

### Entity Framework

- Code-first approach
- Fluent API for entity configuration
- Tenant-aware DbContext

### Testing

- Unit tests for all layers
- Integration tests for Infrastructure
- In-memory database for testing
- Moq for mocking dependencies

## API Documentation

The API is documented using Swagger/OpenAPI. When running the application, navigate to:

```
https://localhost:7060/swagger
```

### Required Headers

- `X-TenantId`: Tenant identifier (required for all requests except tenant creation)

### Available Endpoints

- POST `/api/Accounts` - Create a new account
- GET `/api/Accounts/{id}` - Get account details
- POST `/api/Accounts/{id}/deposit` - Make a deposit
- POST `/api/Accounts/{id}/withdraw` - Make a withdrawal

## Contributing

1. Fork the repository
2. Create a feature branch
3. Commit changes
4. Create a pull request

## License

[Specify License Information]