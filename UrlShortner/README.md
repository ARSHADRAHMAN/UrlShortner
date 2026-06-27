# URL Shortener API - Production Grade

A production-ready URL Shortener API built with ASP.NET Core 10.0, following **SOLID principles** and **Clean Architecture** patterns with **Entity Framework Core** for persistent data storage.

## Features

✅ **CRUD Operations** - Create, Read, Update, Delete shortened URLs  
✅ **REST API** - RESTful endpoints with proper HTTP status codes  
✅ **Validation** - Input validation with DataAnnotations and ModelState  
✅ **Error Handling** - ProblemDetails format for error responses  
✅ **Async/Await** - Full async programming throughout  
✅ **Cancellation Tokens** - Support for operation cancellation  
✅ **SOLID Principles**:
  - **S**ingle Responsibility - Separate concerns (Controllers, Services, Repositories)
  - **O**pen/Closed - Open for extension, closed for modification
  - **L**iskov Substitution - Proper interface implementation
  - **I**nterface Segregation - Focused, minimal interfaces
  - **D**ependency Inversion - Depend on abstractions, not concrete implementations

✅ **Database Persistence**:
  - SQL Server support (primary for production)
  - SQLite support (for local development)
  - Entity Framework Core with migrations
  - Automatic database initialization

✅ **Unit Tests** - Comprehensive test coverage with xUnit and Moq  
✅ **Integration Tests** - EF Core repository integration tests  
✅ **Custom Aliases** - Support for user-defined short URL aliases  
✅ **Expiration** - Time-based URL expiration support  
✅ **Access Tracking** - Track URL access count

## Project Structure

```
UrlShortner/
├── Controllers/
│   └── UrlsController.cs              # API endpoints
├── Models/
│   ├── UrlShortenerEntry.cs           # Domain model
│   ├── Requests/
│   │   └── CreateUrlRequest.cs        # Request DTOs with validation
│   └── Responses/
│       └── UrlShortenerResponse.cs    # Response DTOs
├── Services/
│   └── UrlShortenerService.cs         # Business logic layer
├── Repositories/
│   ├── IUrlRepository.cs              # Repository interface (abstraction)
│   ├── UrlRepository.cs               # In-memory implementation
│   └── EfCoreUrlRepository.cs         # EF Core persistent implementation
├── Data/
│   ├── UrlShortenerDbContext.cs       # EF Core DbContext
│   ├── DbInitializer.cs               # Database seeding utility
│   └── Migrations/                    # Database migration files
├── Program.cs                          # DI configuration & startup
├── appsettings.json                   # Configuration
└── appsettings.Development.json       # Development overrides

UrlShortner.Tests/
├── Controllers/
│   └── UrlsControllerTests.cs         # Controller unit tests
├── Services/
│   └── UrlShortenerServiceTests.cs    # Service unit tests
└── Repositories/
	├── UrlRepositoryTests.cs          # In-memory repo tests
	└── EfCoreUrlRepositoryTests.cs    # Integration tests
```

## Getting Started

### Prerequisites

- **.NET 10.0 SDK** or later
- **Visual Studio 2022** or **VS Code**
- **SQL Server 2019** or later (or use LocalDB for development)
- Optional: **SQL Server Management Studio (SSMS)**

### Database Setup

#### Option 1: Using SQL Server (Recommended for Production)

1. **Update Connection String** in `appsettings.json`:
```json
{
  "ConnectionStrings": {
	"DefaultConnection": "Server=YOUR_SERVER;Database=UrlShortnerDb;User Id=YOUR_USER;Password=YOUR_PASSWORD;TrustServerCertificate=true;"
  },
  "UseDatabase": {
	"Sqlite": false
  }
}
```

2. **Ensure Database Exists**:
```bash
# The database will be created automatically on first run
# Or manually create it in SQL Server
```

#### Option 2: Using LocalDB (Development)

LocalDB is configured by default. No additional setup required!

```json
{
  "ConnectionStrings": {
	"DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=UrlShortnerDb;Trusted_Connection=true;TrustServerCertificate=true;Encrypt=false;"
  },
  "UseDatabase": {
	"Sqlite": false
  }
}
```

#### Option 3: Using SQLite (Local Development - Lightweight)

1. **Update `appsettings.json`**:
```json
{
  "ConnectionStrings": {
	"DefaultConnection": "Data Source=UrlShortner.db"
  },
  "UseDatabase": {
	"Sqlite": true
  }
}
```

2. **Database will be created automatically** on first run.

### Running the Application

1. **Clone/Navigate to project**:
```bash
cd UrlShortner
```

2. **Restore packages**:
```bash
dotnet restore
```

3. **Build solution**:
```bash
dotnet build
```

4. **Run application**:
```bash
dotnet run
```

The API will start at: `
dot` (or as configured)

### Database Migrations

#### Create New Migration

```bash
cd UrlShortner

dotnet ef migrations add YourMigrationName -o Data/Migrations
```

#### Apply Migrations

```bash
# Automatic on startup, or manually:
dotnet ef database update
```

#### View Pending Migrations

```bash
dotnet ef migrations list
```

#### Rollback Migration

```bash
dotnet ef migrations remove

# Or downgrade to specific migration:
dotnet ef database update PreviousMigrationName
```

## API Endpoints

### Create Short URL (POST)
```http
POST /api/urls
Content-Type: application/json

{
  "originalUrl": "https://www.example.com/very/long/url",
  "customAlias": "myalias",
  "expiresAt": "2025-12-31T23:59:59Z"
}
```

**Response (201 Created)**:
```json
{
  "id": "550e8400-e29b-41d4-a716-446655440000",
  "originalUrl": "https://www.example.com/very/long/url",
  "shortCode": "myalias",
  "shortUrl": "https://short.url/myalias",
  "accessCount": 0,
  "createdAt": "2025-01-15T10:30:00Z",
  "updatedAt": "2025-01-15T10:30:00Z",
  "expiresAt": "2025-12-31T23:59:59Z",
  "customAlias": "myalias",
  "isActive": true,
  "isExpired": false
}
```

### Get URL by ID (GET)
```http
GET /api/urls/{id}
```

### Get URL by Short Code (GET)
```http
GET /api/urls/shortcode/{shortCode}
```

### Get URL by Custom Alias (GET)
```http
GET /api/urls/alias/{customAlias}
```

### Get All URLs with Pagination (GET)
```http
GET /api/urls?pageNumber=1&pageSize=10
```

### Update URL (PUT)
```http
PUT /api/urls/{id}
Content-Type: application/json

{
  "originalUrl": "https://www.updated-example.com",
  "expiresAt": "2025-12-31T23:59:59Z"
}
```

### Delete URL (DELETE)
```http
DELETE /api/urls/{id}
```

**Response (204 No Content)**

## Running Tests

### Run All Tests
```bash
dotnet test
```

### Run Specific Test Project
```bash
dotnet test UrlShortner.Tests/UrlShortner.Tests.csproj
```

### Run Tests by Category
```bash
# Unit tests
dotnet test --filter "Category=Unit"

# Integration tests
dotnet test --filter "Category=Integration"
```

### Test Results
- **Total Tests**: 50+
- **Coverage Areas**:
  - Repository pattern implementations
  - Service business logic
  - Controller endpoints
  - Input validation
  - Error handling
  - Database constraints

## Configuration

### appsettings.json - Key Settings

```json
{
  "ConnectionStrings": {
	"DefaultConnection": "..."  // Database connection string
  },
  "UseDatabase": {
	"Sqlite": false  // false = SQL Server, true = SQLite
  },
  "Logging": {
	"LogLevel": {
	  "Default": "Information",
	  "Microsoft.AspNetCore": "Warning",
	  "Microsoft.EntityFrameworkCore.Database.Command": "Debug"  // SQL queries logging
	}
  }
}
```

## Architecture Patterns

### 1. **Repository Pattern**
- `IUrlRepository` interface defines data access contract
- Multiple implementations: `UrlRepository` (in-memory), `EfCoreUrlRepository` (EF Core)
- Enables easy swapping of implementations

### 2. **Service Layer**
- `IUrlShortenerService` contains business logic
- Validates input, handles URL generation, expiration logic
- Maintains separation from data access and presentation

### 3. **Dependency Injection**
- Configured in `Program.cs`
- Constructor injection for loose coupling
- Testable through mock implementations

### 4. **Error Handling**
- `ProblemDetails` format per RFC 7807
- Custom validation error handling
- Comprehensive exception logging

### 5. **Async/Await Pattern**
- All I/O operations are asynchronous
- Cancellation token support throughout
- Scalable under high load

## Performance Considerations

### Database Indexes
- `ShortCode` - Unique index for fast lookups
- `CustomAlias` - Unique index for alias resolution
- `CreatedAt` - Index for sorting and filtering
- `ExpiresAt` - Index for expiration queries
- `IsActive` - Index for active records filtering

### Query Optimization
- `AsNoTracking()` for read-only queries
- Pagination support to limit result sets
- Proper use of `await` for non-blocking I/O

### Database Best Practices
- Connection pooling (automatic in EF Core)
- Parameterized queries (built-in with EF Core)
- Transaction support (available via DbContext)

## Troubleshooting

### Database Connection Issues

**LocalDB not found**:
```bash
# Reinstall LocalDB
# Download from: https://learn.microsoft.com/en-us/sql/database-engine/configure-windows/sql-server-express-localdb
```

**Connection timeout**:
- Verify SQL Server is running
- Check connection string
- Ensure network connectivity

### Migration Issues

**Pending migrations**:
```bash
dotnet ef database update
```

**Migration conflicts**:
```bash
dotnet ef migrations remove
dotnet ef migrations add FixedMigration
```

### Test Failures

**SQLite in-memory database issues**:
```bash
# Clear and rebuild
dotnet clean
dotnet build
dotnet test
```

## Production Deployment

1. **Update `appsettings.json`** with production connection string
2. **Ensure database is created** and migrations applied
3. **Set appropriate logging levels** (avoid Debug in production)
4. **Configure SQL Server** for backups and maintenance
5. **Use connection pooling** for optimal performance
6. **Enable HTTPS** with valid certificate
7. **Implement rate limiting** (recommended middleware addition)
8. **Add authentication/authorization** (recommended enhancement)

## Security Considerations

- ✅ Input validation on all endpoints
- ✅ Parameterized queries (EF Core default)
- ✅ HTTPS enforced by default
- 🔶 Consider adding: Authentication, Authorization, Rate limiting, CORS

## Future Enhancements

- [ ] User authentication and authorization
- [ ] Rate limiting and throttling
- [ ] Caching layer (Redis)
- [ ] Advanced analytics (clicks, geographic data)
- [ ] Bulk operations support
- [ ] API versioning
- [ ] Swagger/OpenAPI documentation
- [ ] GraphQL alternative endpoint

## Contributing

Contributions welcome! Please:
1. Fork the repository
2. Create a feature branch
3. Add tests for new functionality
4. Submit a pull request

## License

MIT License - See LICENSE file for details

## Contact

For issues, questions, or suggestions:
- Open a GitHub issue
- Create a discussion
- Submit a pull request

---

**Built with ❤️ using ASP.NET Core 10.0, EF Core, and Clean Architecture Principles**
