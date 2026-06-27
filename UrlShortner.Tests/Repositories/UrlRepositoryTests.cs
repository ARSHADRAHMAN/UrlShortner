using Xunit;
using UrlShortner.Models;
using UrlShortner.Repositories;

namespace UrlShortner.Tests.Repositories;

public class UrlRepositoryTests
{
    private readonly IUrlRepository _repository;

    public UrlRepositoryTests()
    {
        _repository = new UrlRepository();
    }

    #region CreateAsync Tests

    [Fact]
    public async Task CreateAsync_WithValidEntry_ShouldReturnCreatedEntry()
    {
        // Arrange
        var entry = new UrlShortenerEntry
        {
            OriginalUrl = "https://www.example.com/very/long/url",
            ShortCode = "abc123"
        };

        // Act
        var result = await _repository.CreateAsync(entry);

        // Assert
        Assert.NotNull(result);
        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Equal(entry.OriginalUrl, result.OriginalUrl);
        Assert.Equal(entry.ShortCode, result.ShortCode);
        Assert.NotEqual(DateTime.MinValue, result.CreatedAt);
        Assert.NotEqual(DateTime.MinValue, result.UpdatedAt);
        Assert.Equal(0, result.ClickCount);
        Assert.True(result.IsActive);
    }

    [Fact]
    public async Task CreateAsync_WithNullEntry_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => _repository.CreateAsync(null!));
    }

    [Fact]
    public async Task CreateAsync_MultipleEntries_ShouldCreateAll()
    {
        // Arrange
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
            results.Add(await _repository.CreateAsync(entry));
        }

        // Assert
        Assert.Equal(3, results.Count);
        Assert.All(results, r => Assert.NotEqual(Guid.Empty, r.Id));
    }

    #endregion

    #region GetByIdAsync Tests

    [Fact]
    public async Task GetByIdAsync_WithValidId_ShouldReturnEntry()
    {
        // Arrange
        var entry = new UrlShortenerEntry
        {
            OriginalUrl = "https://www.example.com",
            ShortCode = "abc123"
        };
        var createdEntry = await _repository.CreateAsync(entry);

        // Act
        var result = await _repository.GetByIdAsync(createdEntry.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(createdEntry.Id, result.Id);
        Assert.Equal(entry.OriginalUrl, result.OriginalUrl);
    }

    [Fact]
    public async Task GetByIdAsync_WithInvalidId_ShouldReturnNull()
    {
        // Act
        var result = await _repository.GetByIdAsync(Guid.NewGuid());

        // Assert
        Assert.Null(result);
    }

    #endregion

    #region GetByShortCodeAsync Tests

    [Fact]
    public async Task GetByShortCodeAsync_WithValidShortCode_ShouldReturnEntry()
    {
        // Arrange
        var shortCode = "uniquecode";
        var entry = new UrlShortenerEntry
        {
            OriginalUrl = "https://www.example.com",
            ShortCode = shortCode
        };
        await _repository.CreateAsync(entry);

        // Act
        var result = await _repository.GetByShortCodeAsync(shortCode);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(shortCode, result.ShortCode);
    }

    [Fact]
    public async Task GetByShortCodeAsync_WithInvalidShortCode_ShouldReturnNull()
    {
        // Act
        var result = await _repository.GetByShortCodeAsync("nonexistent");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetByShortCodeAsync_WithNullOrEmptyShortCode_ShouldThrowArgumentException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _repository.GetByShortCodeAsync(null!));
        await Assert.ThrowsAsync<ArgumentException>(() => _repository.GetByShortCodeAsync(string.Empty));
    }

    #endregion

    #region GetByCustomAliasAsync Tests

    [Fact]
    public async Task GetByCustomAliasAsync_WithValidAlias_ShouldReturnEntry()
    {
        // Arrange
        var alias = "myalias";
        var entry = new UrlShortenerEntry
        {
            OriginalUrl = "https://www.example.com",
            ShortCode = "code1",
            CustomAlias = alias
        };
        await _repository.CreateAsync(entry);

        // Act
        var result = await _repository.GetByCustomAliasAsync(alias);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(alias, result.CustomAlias);
    }

    [Fact]
    public async Task GetByCustomAliasAsync_WithInvalidAlias_ShouldReturnNull()
    {
        // Act
        var result = await _repository.GetByCustomAliasAsync("nonexistent");

        // Assert
        Assert.Null(result);
    }

    #endregion

    #region GetAllAsync Tests

    [Fact]
    public async Task GetAllAsync_WithMultipleEntries_ShouldReturnPagedResults()
    {
        // Arrange
        var entries = new[]
        {
            new UrlShortenerEntry { OriginalUrl = "https://example1.com", ShortCode = "code1" },
            new UrlShortenerEntry { OriginalUrl = "https://example2.com", ShortCode = "code2" },
            new UrlShortenerEntry { OriginalUrl = "https://example3.com", ShortCode = "code3" }
        };

        foreach (var entry in entries)
        {
            await _repository.CreateAsync(entry);
        }

        // Act
        var (items, totalCount) = await _repository.GetAllAsync(pageNumber: 1, pageSize: 2);

        // Assert
        Assert.Equal(2, items.Count);
        Assert.Equal(3, totalCount);
    }

    [Fact]
    public async Task GetAllAsync_EmptyRepository_ShouldReturnEmptyList()
    {
        // Act
        var (items, totalCount) = await _repository.GetAllAsync();

        // Assert
        Assert.Empty(items);
        Assert.Equal(0, totalCount);
    }

    #endregion

    #region UpdateAsync Tests

    [Fact]
    public async Task UpdateAsync_WithValidEntry_ShouldUpdateEntry()
    {
        // Arrange
        var entry = new UrlShortenerEntry
        {
            OriginalUrl = "https://www.example.com",
            ShortCode = "code1"
        };
        var createdEntry = await _repository.CreateAsync(entry);

        var updatedEntry = new UrlShortenerEntry
        {
            Id = createdEntry.Id,
            OriginalUrl = "https://www.updated-example.com",
            ShortCode = createdEntry.ShortCode
        };

        // Act
        var result = await _repository.UpdateAsync(updatedEntry);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(updatedEntry.OriginalUrl, result.OriginalUrl);
        Assert.Equal(createdEntry.Id, result.Id);
    }

    [Fact]
    public async Task UpdateAsync_WithNonexistentEntry_ShouldThrowKeyNotFoundException()
    {
        // Arrange
        var entry = new UrlShortenerEntry
        {
            Id = Guid.NewGuid(),
            OriginalUrl = "https://www.example.com",
            ShortCode = "code1"
        };

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() => _repository.UpdateAsync(entry));
    }

    #endregion

    #region DeleteAsync Tests

    [Fact]
    public async Task DeleteAsync_WithValidId_ShouldDeleteEntry()
    {
        // Arrange
        var entry = new UrlShortenerEntry
        {
            OriginalUrl = "https://www.example.com",
            ShortCode = "code1"
        };
        var createdEntry = await _repository.CreateAsync(entry);

        // Act
        var result = await _repository.DeleteAsync(createdEntry.Id);

        // Assert
        Assert.True(result);

        // Verify deletion
        var deletedEntry = await _repository.GetByIdAsync(createdEntry.Id);
        Assert.Null(deletedEntry);
    }

    [Fact]
    public async Task DeleteAsync_WithInvalidId_ShouldReturnFalse()
    {
        // Act
        var result = await _repository.DeleteAsync(Guid.NewGuid());

        // Assert
        Assert.False(result);
    }

    #endregion

    #region ShortCodeExistsAsync Tests

    [Fact]
    public async Task ShortCodeExistsAsync_WithExistingCode_ShouldReturnTrue()
    {
        // Arrange
        var shortCode = "exists";
        var entry = new UrlShortenerEntry
        {
            OriginalUrl = "https://www.example.com",
            ShortCode = shortCode
        };
        await _repository.CreateAsync(entry);

        // Act
        var result = await _repository.ShortCodeExistsAsync(shortCode);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task ShortCodeExistsAsync_WithNonexistentCode_ShouldReturnFalse()
    {
        // Act
        var result = await _repository.ShortCodeExistsAsync("nonexistent");

        // Assert
        Assert.False(result);
    }

    #endregion

    #region IncrementAccessCountAsync Tests

    [Fact]
    public async Task IncrementAccessCountAsync_WithValidId_ShouldIncrementCount()
    {
        // Arrange
        var entry = new UrlShortenerEntry
        {
            OriginalUrl = "https://www.example.com",
            ShortCode = "code1"
        };
        var createdEntry = await _repository.CreateAsync(entry);
        var initialCount = createdEntry.ClickCount;

        // Act
        var result = await _repository.IncrementAccessCountAsync(createdEntry.Id);

        // Assert
        Assert.True(result);

        // Verify increment
        var updatedEntry = await _repository.GetByIdAsync(createdEntry.Id);
        Assert.NotNull(updatedEntry);
        Assert.Equal(initialCount + 1, updatedEntry.ClickCount);
    }

    [Fact]
    public async Task IncrementAccessCountAsync_WithInvalidId_ShouldReturnFalse()
    {
        // Act
        var result = await _repository.IncrementAccessCountAsync(Guid.NewGuid());

        // Assert
        Assert.False(result);
    }

    #endregion
}
