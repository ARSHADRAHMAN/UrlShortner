using Microsoft.EntityFrameworkCore;
using UrlShortner.Data;
using UrlShortner.Models;

namespace UrlShortner.Data;

/// <summary>
/// Database initializer for seeding sample data
/// Use this for development and testing purposes
/// </summary>
public static class DbInitializer
{
    /// <summary>
    /// Initialize the database with seed data
    /// </summary>
    /// <param name="context">The DbContext instance</param>
    /// <param name="cancellationToken">Cancellation token</param>
    public static async Task InitializeAsync(UrlShortenerDbContext context, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);

        // Migrate the database
        await context.Database.MigrateAsync(cancellationToken).ConfigureAwait(false);

        // Seed data if no entries exist
        if (context.UrlEntries.Any())
        {
            return; // Database has been seeded
        }

        var sampleEntries = new[]
        {
            new UrlShortenerEntry
            {
                Id = Guid.NewGuid(),
                OriginalUrl = "https://www.github.com/Microsoft/dotnet",
                ShortCode = "ghmdot",
                CustomAlias = "github-dotnet",
                ClickCount = 42,
                CreatedAt = DateTime.UtcNow.AddDays(-30),
                UpdatedAt = DateTime.UtcNow.AddDays(-30),
                IsActive = true,
                ExpiresAt = null
            },
            new UrlShortenerEntry
            {
                Id = Guid.NewGuid(),
                OriginalUrl = "https://learn.microsoft.com/en-us/dotnet/core/whats-new/dotnet-10",
                ShortCode = "msdn10",
                CustomAlias = "learn-dotnet10",
                ClickCount = 28,
                CreatedAt = DateTime.UtcNow.AddDays(-15),
                UpdatedAt = DateTime.UtcNow.AddDays(-15),
                IsActive = true,
                ExpiresAt = null
            },
            new UrlShortenerEntry
            {
                Id = Guid.NewGuid(),
                OriginalUrl = "https://github.com/dotnet/aspnetcore",
                ShortCode = "aspnet",
                CustomAlias = "aspnetcore",
                ClickCount = 156,
                CreatedAt = DateTime.UtcNow.AddDays(-60),
                UpdatedAt = DateTime.UtcNow.AddDays(-5),
                IsActive = true,
                ExpiresAt = null
            },
            new UrlShortenerEntry
            {
                Id = Guid.NewGuid(),
                OriginalUrl = "https://www.example.com/very/long/and/complex/url/that/needs/shortening",
                ShortCode = "tmpurl",
                CustomAlias = null,
                ClickCount = 3,
                CreatedAt = DateTime.UtcNow.AddHours(-2),
                UpdatedAt = DateTime.UtcNow.AddHours(-2),
                IsActive = true,
                ExpiresAt = DateTime.UtcNow.AddDays(7) // Expires in 7 days
            }
        };

        await context.UrlEntries.AddRangeAsync(sampleEntries, cancellationToken).ConfigureAwait(false);
        await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Clear all data from the database (use with caution!)
    /// </summary>
    /// <param name="context">The DbContext instance</param>
    /// <param name="cancellationToken">Cancellation token</param>
    public static async Task ClearAllAsync(UrlShortenerDbContext context, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);

        await context.UrlEntries.ExecuteDeleteAsync(cancellationToken).ConfigureAwait(false);
    }
}
