using Microsoft.EntityFrameworkCore;
using Xunit;
using UrlShortner.Data;
using UrlShortner.Models;
using UrlShortner.Repositories;

namespace UrlShortner.Tests.Repositories;

/// <summary>
/// Integration tests for EfCoreUrlRepository
/// Uses in-memory SQLite database for testing
/// </summary>
public class EfCoreUrlRepositoryTests
{
    private UrlShortenerDbContext GetDbContext()
    {
        var connection = new Microsoft.Data.Sqlite.SqliteConnection("Data Source=:memory:");
        connection.Open();

        var options = new DbContextOptionsBuilder<UrlShortenerDbContext>()
            .UseSqlite(connection)
            .Options;

        var context = new UrlShortenerDbContext(options);
        context.Database.EnsureCreated();
        return context;
    }

    #region CreateAsync Tests

    [Fact]
    public async Task CreateAsync_WithValidEntry_ShouldPersistToDatabase()
    {
        // Arrange
        using var context = GetDbContext();
        var repository = new EfCoreUrlRepository(context);

        var entry = new UrlShortenerEntry
        {
            OriginalUrl = "https://www.example.com/very/long/url",
            ShortCode = "abc123"
        };

        // Act
        var result = await repository.CreateAsync(entry);

        // Assert
        Assert.NotNull(result);
        Assert.NotEqual(Guid.Empty, result.Id);

        // Verify in database
        var dbEntry = await context.UrlEntries.FirstOrDefaultAsync(x => x.Id == result.Id);
        Assert.NotNull(dbEntry);
        Assert.Equal(entry.OriginalUrl, dbEntry.OriginalUrl);
        Assert.Equal(entry.ShortCode, dbEntry.ShortCode);
    }

    [Fact]
    public async Task CreateAsync_WithMultipleEntries_ShouldPersistAll()
    {
        // Arrange
        using var context = GetDbContext();
        var repository = new EfCoreUrlRepository(context);

        var entries = new[]
        {
            new UrlShortenerEntry { OriginalUrl = "https://example1.com", ShortCode = "code1" },
            new UrlShortenerEntry { OriginalUrl = "https://example2.com", ShortCode = "code2" },
            new UrlShortenerEntry { OriginalUrl = "https://example3.com", ShortCode = "code3" }
        };

        // Act
        var results = new List<UrlShortenerEntry>();
        foreach (var entry in entries)
        {
            results.Add(await repository.CreateAsync(entry));
        }

        // Assert
        Assert.Equal(3, results.Count);

        var dbCount = await context.UrlEntries.CountAsync();
        Assert.Equal(3, dbCount);
    }

    #endregion

    #region GetByIdAsync Tests

    [Fact]
    public async Task GetByIdAsync_WithValidId_ShouldReturnEntry()
    {
        // Arrange
        using var context = GetDbContext();
        var repository = new EfCoreUrlRepository(context);

        var entry = new UrlShortenerEntry
        {
            OriginalUrl = "https://www.example.com",
            ShortCode = "abc123"
        };
        var createdEntry = await repository.CreateAsync(entry);

        // Act
        var result = await repository.GetByIdAsync(createdEntry.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(createdEntry.Id, result.Id);
        Assert.Equal(entry.OriginalUrl, result.OriginalUrl);
    }

    [Fact]
    public async Task GetByIdAsync_WithInvalidId_ShouldReturnNull()
    {
        // Arrange
        using var context = GetDbContext();
        var repository = new EfCoreUrlRepository(context);

        // Act
        var result = await repository.GetByIdAsync(Guid.NewGuid());

        // Assert
        Assert.Null(result);
    }

    #endregion

    #region GetByShortCodeAsync Tests

    [Fact]
    public async Task GetByShortCodeAsync_WithValidCode_ShouldReturnEntry()
    {
        // Arrange
        using var context = GetDbContext();
        var repository = new EfCoreUrlRepository(context);

        var shortCode = "uniquecode";
        var entry = new UrlShortenerEntry
        {
            OriginalUrl = "https://www.example.com",
            ShortCode = shortCode
        };
        await repository.CreateAsync(entry);

        // Act
        var result = await repository.GetByShortCodeAsync(shortCode);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(shortCode, result.ShortCode);
    }

    [Fact]
    public async Task GetByShortCodeAsync_WithInvalidCode_ShouldReturnNull()
    {
        // Arrange
        using var context = GetDbContext();
        var repository = new EfCoreUrlRepository(context);

        // Act
        var result = await repository.GetByShortCodeAsync("nonexistent");

        // Assert
        Assert.Null(result);
    }

    #endregion



    #region GetAllAsync Tests

    [Fact]
    public async Task GetAllAsync_WithMultipleEntries_ShouldReturnPaginatedResults()
    {
        // Arrange
        using var context = GetDbContext();
        var repository = new EfCoreUrlRepository(context);

        var entries = new[]
        {
            new UrlShortenerEntry { OriginalUrl = "https://example1.com", ShortCode = "code1" },
            new UrlShortenerEntry { OriginalUrl = "https://example2.com", ShortCode = "code2" },
            new UrlShortenerEntry { OriginalUrl = "https://example3.com", ShortCode = "code3" }
        };

        foreach (var e in entries)
            await repository.CreateAsync(e);

        // Act
        var (items, totalCount) = await repository.GetAllAsync(pageNumber: 1, pageSize: 2);

        // Assert
        Assert.Equal(2, items.Count);
        Assert.Equal(3, totalCount);
    }

    [Fact]
    public async Task GetAllAsync_EmptyRepository_ShouldReturnEmpty()
    {
        // Arrange
        using var context = GetDbContext();
        var repository = new EfCoreUrlRepository(context);

        // Act
        var (items, totalCount) = await repository.GetAllAsync();

        // Assert
        Assert.Empty(items);
        Assert.Equal(0, totalCount);
    }

    #endregion

    #region UpdateAsync Tests

    [Fact]
    public async Task UpdateAsync_WithValidEntry_ShouldUpdateInDatabase()
    {
        // Arrange
        using var context = GetDbContext();
        var repository = new EfCoreUrlRepository(context);

        var entry = new UrlShortenerEntry
        {
            OriginalUrl = "https://www.example.com",
            ShortCode = "code1"
        };
        var createdEntry = await repository.CreateAsync(entry);

        var updatedEntry = new UrlShortenerEntry
        {
            Id = createdEntry.Id,
            OriginalUrl = "https://www.updated-example.com",
            ShortCode = createdEntry.ShortCode
        };

        // Act
        var result = await repository.UpdateAsync(updatedEntry);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(updatedEntry.OriginalUrl, result.OriginalUrl);

        // Verify in database
        var dbEntry = await context.UrlEntries.FirstOrDefaultAsync(x => x.Id == createdEntry.Id);
        Assert.NotNull(dbEntry);
        Assert.Equal("https://www.updated-example.com", dbEntry.OriginalUrl);
    }

    [Fact]
    public async Task UpdateAsync_WithNonexistentEntry_ShouldThrowKeyNotFoundException()
    {
        // Arrange
        using var context = GetDbContext();
        var repository = new EfCoreUrlRepository(context);

        var entry = new UrlShortenerEntry
        {
            Id = Guid.NewGuid(),
            OriginalUrl = "https://www.example.com",
            ShortCode = "code1"
        };

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => repository.UpdateAsync(entry));
    }

    #endregion

    #region DeleteAsync Tests

    [Fact]
    public async Task DeleteAsync_WithValidId_ShouldDeleteFromDatabase()
    {
        // Arrange
        using var context = GetDbContext();
        var repository = new EfCoreUrlRepository(context);

        var entry = new UrlShortenerEntry
        {
            OriginalUrl = "https://www.example.com",
            ShortCode = "code1"
        };
        var createdEntry = await repository.CreateAsync(entry);

        // Act
        var result = await repository.DeleteAsync(createdEntry.Id);

        // Assert
        Assert.True(result);

        // Verify deleted from database
        var deletedEntry = await context.UrlEntries.FirstOrDefaultAsync(x => x.Id == createdEntry.Id);
        Assert.Null(deletedEntry);
    }

    [Fact]
    public async Task DeleteAsync_WithInvalidId_ShouldReturnFalse()
    {
        // Arrange
        using var context = GetDbContext();
        var repository = new EfCoreUrlRepository(context);

        // Act
        var result = await repository.DeleteAsync(Guid.NewGuid());

        // Assert
        Assert.False(result);
    }

    #endregion

    #region ShortCodeExistsAsync Tests

    [Fact]
    public async Task ShortCodeExistsAsync_WithExistingCode_ShouldReturnTrue()
    {
        // Arrange
        using var context = GetDbContext();
        var repository = new EfCoreUrlRepository(context);

        var shortCode = "exists";
        var entry = new UrlShortenerEntry
        {
            OriginalUrl = "https://www.example.com",
            ShortCode = shortCode
        };
        await repository.CreateAsync(entry);

        // Act
        var result = await repository.ShortCodeExistsAsync(shortCode);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task ShortCodeExistsAsync_WithNonexistentCode_ShouldReturnFalse()
    {
        // Arrange
        using var context = GetDbContext();
        var repository = new EfCoreUrlRepository(context);

        // Act
        var result = await repository.ShortCodeExistsAsync("nonexistent");

        // Assert
        Assert.False(result);
    }

    #endregion

    #region IncrementAccessCountAsync Tests

    [Fact]
    public async Task IncrementAccessCountAsync_WithValidId_ShouldIncrementInDatabase()
    {
        // Arrange
        using var context = GetDbContext();
        var repository = new EfCoreUrlRepository(context);

        var entry = new UrlShortenerEntry
        {
            OriginalUrl = "https://www.example.com",
            ShortCode = "code1"
        };
        var createdEntry = await repository.CreateAsync(entry);
        var initialCount = createdEntry.ClickCount;

        // Act
        var result = await repository.IncrementAccessCountAsync(createdEntry.Id);

        // Assert
        Assert.True(result);

        // Verify in database
        var updatedEntry = await context.UrlEntries.FirstOrDefaultAsync(x => x.Id == createdEntry.Id);
        Assert.NotNull(updatedEntry);
        Assert.Equal(initialCount + 1, updatedEntry.ClickCount);
    }

    [Fact]
    public async Task IncrementAccessCountAsync_WithInvalidId_ShouldReturnFalse()
    {
        // Arrange
        using var context = GetDbContext();
        var repository = new EfCoreUrlRepository(context);

        // Act
        var result = await repository.IncrementAccessCountAsync(Guid.NewGuid());

        // Assert
        Assert.False(result);
    }

    #endregion

    #region Unique Constraint Tests

    [Fact]
    public async Task CreateAsync_WithDuplicateShortCode_ShouldThrow()
    {
        // Arrange
        using var context = GetDbContext();
        var repository = new EfCoreUrlRepository(context);

        var shortCode = "duplicate";
        var entry1 = new UrlShortenerEntry
        {
            OriginalUrl = "https://www.example1.com",
            ShortCode = shortCode
        };
        var entry2 = new UrlShortenerEntry
        {
            OriginalUrl = "https://www.example2.com",
            ShortCode = shortCode
        };

        await repository.CreateAsync(entry1);

        // Act & Assert - Should throw DbUpdateException due to unique constraint
        await Assert.ThrowsAsync<DbUpdateException>(
            () => repository.CreateAsync(entry2));
    }



    #endregion
}
