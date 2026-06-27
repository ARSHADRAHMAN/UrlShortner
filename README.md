# URL Shortener API - Production Grade

A production-ready URL Shortener API built with ASP.NET Core 10.0, following **SOLID principles** and **Clean Architecture** patterns with **Entity Framework Core** for persistent data storage.

## Features

✅ **CRUD Operations** - Create, Read, Update, Delete shortened URLs  
✅ **REST API** - RESTful endpoints with proper HTTP status codes  
✅ **Validation** - Input validation with DataAnnotations and ModelState  
✅ **Error Handling** - ProblemDetails format for error responses  
✅ **Async/Await** - Full async programming throughout  
✅ **Cancellation Tokens** - Support for operation cancellation  
✅ **Rate Limiting** - Global rate limiting policy configured to prevent abuse  
✅ **Swagger UI** - Configured with Swashbuckle under `/swagger`  
✅ **SOLID Principles**:
  - **S**ingle Responsibility - Separate concerns (Controllers, Services, Repositories)
  - **O**pen/Closed - Open for extension, closed for modification
  - **L**iskov Substitution - Proper interface implementation
  - **I**nterface Segregation - Focused, minimal interfaces
  - **D**ependency Inversion - Depend on abstractions, not concrete implementations

✅ **Database Persistence**:
  - SQL Server support (primary for production, configured for development LocalDB instance)
  - SQLite support (for local development)
  - Entity Framework Core with migrations
  - Automatic database initialization

✅ **Unit & Integration Tests** - 62 passing tests using xUnit, Moq, and SQLite in-memory databases  
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
├── Program.cs                          # DI configuration, rate limiting & startup
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
- **SQL Server 2019** or later (configured to use LocalDB/Named Pipe locally)
- Optional: **SQL Server Management Studio (SSMS)**

### Database Setup

#### Option 1: Using SQL Server (LocalDB / Local Instance)

LocalDB / Local instance is configured by default using direct named pipes in `appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=np:\\\\.\\pipe\\LOCALDB#3F59C69F\\tsql\\query;Database=UrlShortnerDb;Trusted_Connection=true;TrustServerCertificate=true;Encrypt=false;"
  },
  "UseDatabase": {
    "Sqlite": false
  }
}
```

The database and schema are created and seeded automatically on first run.

#### Option 2: Using SQLite (Lightweight Local Development)

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

2. The database will be created automatically on first run.

### Running the Application

1. **Navigate to the web project**:
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

The API will start and serve the Swagger UI at: `http://localhost:5272/swagger` (or matching port in `launchSettings.json`).

### Database Migrations

#### Create New Migration
```bash
cd UrlShortner
dotnet ef migrations add YourMigrationName -o Data/Migrations
```

#### Apply Migrations
```bash
dotnet ef database update
```

## API Endpoints

### Create Short URL (POST)
```http
POST /api/urls
Content-Type: application/json

{
  "originalUrl": "https://www.example.com/very/long/url"
}
```

**Response (201 Created)**:
```json
{
  "id": "550e8400-e29b-41d4-a716-446655440000",
  "originalUrl": "https://www.example.com/very/long/url",
  "shortCode": "ghmdot",
  "shortUrl": "https://short.url/ghmdot",
  "accessCount": 0,
  "createdAt": "2026-06-27T10:30:00Z",
  "updatedAt": "2026-06-27T10:30:00Z",
  "isActive": true
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

### Get All URLs with Pagination (GET)
```http
GET /api/urls?pageNumber=1&pageSize=10
```

### Update URL (PUT)
```http
PUT /api/urls/{id}
Content-Type: application/json

{
  "originalUrl": "https://www.updated-example.com"
}
```

### Delete URL (DELETE)
```http
DELETE /api/urls/{id}
```

**Response (204 No Content)**

---

## Running Tests

### Run All Tests
```bash
dotnet test
```

### Run Specific Test Project
```bash
dotnet test UrlShortner.Tests/UrlShortner.Tests.csproj
```

---

## Architecture Patterns

### 1. **Repository Pattern**
- `IUrlRepository` interface defines data access contract.
- Multiple implementations: `UrlRepository` (in-memory), `EfCoreUrlRepository` (EF Core).

### 2. **Service Layer**
- `IUrlShortenerService` contains business logic (e.g., short code generation).

### 3. **Rate Limiting**
- ASP.NET Core built-in concurrency & fixed-window rate limiters.

---

**Built with ❤️ using ASP.NET Core 10.0, EF Core, and Clean Architecture Principles**
