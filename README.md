# Pulse Banking Platform

A multi-tenant banking platform built with .NET 8, following Clean Architecture principles.

## Project Overview

Pulse Banking Platform is a scalable, multi-tenant banking system that allows multiple banks to operate on a single platform while maintaining strict data isolation.

Pulse Banking Platform can be deployed separately for each region or country for data sovereignty and regulatory compliance.

Pulse Banking Platform can also be deployed for a single Bank or Financial Institution that wants a dedicated, single-tenant platform.

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

- Multi-tenant architecture with tenant isolation
- Clean Architecture implementation
- Domain-Driven Design (DDD) practices
- CQRS pattern using MediatR
- Comprehensive test coverage
- Swagger API documentation
- Entity Framework Core with SQL Server

## Multi-tenant Implementation

The platform implements multi-tenancy through:

- Tenant-specific database isolation
- Request header-based tenant identification
- Middleware-based tenant validation
- Global query filters for data isolation

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

3. Configure tenant settings in `appsettings.json`:

```json
{
  "Tenants": {
    "tenant1": {
      "Id": "tenant1",
      "Name": "First Bank",
      "ConnectionString": "Server=(local);Database=PulseBanking;Trusted_Connection=True;MultipleActiveResultSets=true;Trust Server Certificate=True;",
      "CurrencyCode": "USD",
      "DefaultTransactionLimit": "10000",
      "TimeZone": "UTC",
      "IsActive": true
    }
  }
}
```

4. Apply database migrations:

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
- Database-per-tenant strategy
- Tenant middleware for validation
- Global query filters for data isolation

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

- `X-TenantId`: Tenant identifier (required for all requests)

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