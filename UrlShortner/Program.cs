using Microsoft.EntityFrameworkCore;
using Serilog;
using UrlShortner.Configuration;
using UrlShortner.Data;
using UrlShortner.Middleware;
using UrlShortner.Repositories;
using UrlShortner.Services;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog logging early
Log.Logger = builder.Services.ConfigureSerilog(builder.Configuration, builder.Environment);

try
{
    Log.Information("Starting UrlShortener application...");

    // Add services to the container
    builder.Services.AddControllers()
        .ConfigureApiBehaviorOptions(options =>
        {
            options.InvalidModelStateResponseFactory = context =>
            {
                var problemDetails = new Microsoft.AspNetCore.Mvc.ProblemDetails
                {
                    Status = Microsoft.AspNetCore.Http.StatusCodes.Status400BadRequest,
                    Title = "One or more validation errors occurred.",
                    Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1"
                };

                foreach (var modelState in context.ModelState.Values)
                {
                    foreach (var error in modelState.Errors)
                    {
                        problemDetails.Detail = error.ErrorMessage;
                        break;
                    }
                }

                return new Microsoft.AspNetCore.Mvc.BadRequestObjectResult(problemDetails);
            };
        });

    // Configure Entity Framework Core with SQL Server
    // You can switch to SQLite by using UseSqlite instead
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
        ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

    var useSqlite = builder.Configuration.GetValue<bool>("UseDatabase:Sqlite", false);

    if (useSqlite)
    {
        builder.Services.AddDbContext<UrlShortenerDbContext>(options =>
            options.UseSqlite(connectionString));
    }
    else
    {
        builder.Services.AddDbContext<UrlShortenerDbContext>(options =>
            options.UseSqlServer(connectionString));
    }

    // Register CORS policy
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowFrontend", policy =>
        {
            policy.WithOrigins("http://localhost:4200")
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
    });

    // Register repositories and services
    // Using EF Core implementation for persistence
    builder.Services.AddScoped<IUrlRepository, EfCoreUrlRepository>();
    builder.Services.AddScoped<IUrlShortenerService, UrlShortenerService>();

    // Add Serilog logging
    builder.Services.AddSerilogLogging();

    // Add rate limiting services
    builder.Services.AddRateLimiter(options =>
    {
        options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
        options.AddFixedWindowLimiter(policyName: "fixed", opt =>
        {
            opt.PermitLimit = 100;
            opt.Window = TimeSpan.FromMinutes(1);
            opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
            opt.QueueLimit = 2;
        });
    });

    // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
    builder.Services.AddOpenApi();

    var app = builder.Build();

    // Add global exception middleware
    app.UseGlobalExceptionMiddleware();

    // Enable CORS middleware
    app.UseCors("AllowFrontend");

    // Apply pending migrations and create database if it doesn't exist
    using (var scope = app.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<UrlShortenerDbContext>();

        try
        {
            // Apply any pending migrations
            await dbContext.Database.MigrateAsync();

            // Ensure database is created
            await dbContext.Database.EnsureCreatedAsync();

            // Seed the database with sample data
            await DbInitializer.InitializeAsync(dbContext);

            Log.Information("Database migration, initialization, and seeding completed successfully");
        }
        catch (Exception ex)
        {
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
            logger.LogError(ex, "An error occurred while migrating or initializing the database.");
            Log.Fatal(ex, "Database migration failed");
            throw;
        }
    }

    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi();
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/openapi/v1.json", "UrlShortener API v1");
            options.RoutePrefix = "swagger";
        });
    }

    app.UseHttpsRedirection();

    app.UseAuthorization();

    app.UseRateLimiter();

    app.MapControllers().RequireRateLimiting("fixed");

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
