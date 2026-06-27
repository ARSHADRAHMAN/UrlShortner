using Moq;
using Xunit;
using UrlShortner.Models;
using UrlShortner.Models.Requests;
using UrlShortner.Repositories;
using UrlShortner.Services;

namespace UrlShortner.Tests.Services;

public class UrlShortenerServiceTests
{
    private readonly Mock<IUrlRepository> _mockRepository;
    private readonly IUrlShortenerService _service;

    public UrlShortenerServiceTests()
    {
        _mockRepository = new Mock<IUrlRepository>();
        _service = new UrlShortenerService(_mockRepository.Object);
    }

    #region CreateShortUrlAsync Tests

    [Fact]
    public async Task CreateShortUrlAsync_WithValidRequest_ShouldCreateUrl()
    {
        // Arrange
        var request = new CreateUrlRequest
        {
            OriginalUrl = "https://www.example.com/very/long/url"
        };



        _mockRepository
            .Setup(r => r.GetByOriginalUrlAsync(It.IsAny<string>(), default))
            .ReturnsAsync((UrlShortenerEntry?)null);

        _mockRepository
            .Setup(r => r.ShortCodeExistsAsync(It.IsAny<string>(), default))
            .ReturnsAsync(false);

        _mockRepository
            .Setup(r => r.CreateAsync(It.IsAny<UrlShortenerEntry>(), default))
            .ReturnsAsync((UrlShortenerEntry entry, CancellationToken _) =>
            {
                entry.Id = Guid.NewGuid();
                entry.CreatedAt = DateTime.UtcNow;
                entry.UpdatedAt = DateTime.UtcNow;
                return entry;
            });

        // Act
        var result = await _service.CreateShortUrlAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Equal(request.OriginalUrl, result.OriginalUrl);
        Assert.NotNull(result.ShortCode);
        Assert.NotEmpty(result.ShortCode);
        _mockRepository.Verify(r => r.CreateAsync(It.IsAny<UrlShortenerEntry>(), default), Times.Once);
    }

    [Fact]
    public async Task CreateShortUrlAsync_WithDuplicateOriginalUrl_ShouldReturnExistingUrl()
    {
        // Arrange
        var originalUrl = "https://www.example.com/duplicate";
        var request = new CreateUrlRequest
        {
            OriginalUrl = originalUrl
        };

        var existingEntry = new UrlShortenerEntry
        {
            Id = Guid.NewGuid(),
            OriginalUrl = originalUrl,
            ShortCode = "exist1",
            IsActive = true
        };

        _mockRepository
            .Setup(r => r.GetByOriginalUrlAsync(originalUrl, default))
            .ReturnsAsync(existingEntry);

        // Act
        var result = await _service.CreateShortUrlAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(existingEntry.Id, result.Id);
        Assert.Equal(existingEntry.ShortCode, result.ShortCode);
        _mockRepository.Verify(r => r.CreateAsync(It.IsAny<UrlShortenerEntry>(), default), Times.Never);
    }



    [Fact]
    public async Task CreateShortUrlAsync_WithNullRequest_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(
            () => _service.CreateShortUrlAsync(null!));
    }

    #endregion

    #region GetUrlByIdAsync Tests

    [Fact]
    public async Task GetUrlByIdAsync_WithValidId_ShouldReturnUrl()
    {
        // Arrange
        var id = Guid.NewGuid();
        var entry = new UrlShortenerEntry
        {
            Id = id,
            OriginalUrl = "https://www.example.com",
            ShortCode = "abc123",
            IsActive = true
        };

        _mockRepository
            .Setup(r => r.GetByIdAsync(id, default))
            .ReturnsAsync(entry);

        // Act
        var result = await _service.GetUrlByIdAsync(id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(entry.OriginalUrl, result.OriginalUrl);
        Assert.Equal(entry.ShortCode, result.ShortCode);
    }

    [Fact]
    public async Task GetUrlByIdAsync_WithInvalidId_ShouldReturnNull()
    {
        // Arrange
        _mockRepository
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), default))
            .ReturnsAsync((UrlShortenerEntry?)null);

        // Act
        var result = await _service.GetUrlByIdAsync(Guid.NewGuid());

        // Assert
        Assert.Null(result);
    }



    #endregion

    #region GetUrlByShortCodeAsync Tests

    [Fact]
    public async Task GetUrlByShortCodeAsync_WithValidCode_ShouldIncrementAccessCount()
    {
        // Arrange
        var shortCode = "abc123";
        var entry = new UrlShortenerEntry
        {
            Id = Guid.NewGuid(),
            OriginalUrl = "https://www.example.com",
            ShortCode = shortCode,
            IsActive = true,
            ClickCount = 5
        };

        _mockRepository
            .Setup(r => r.GetByShortCodeAsync(shortCode, default))
            .ReturnsAsync(entry);

        _mockRepository
            .Setup(r => r.UpdateAsync(It.IsAny<UrlShortenerEntry>(), default))
            .ReturnsAsync(entry);

        // Act
        var result = await _service.GetUrlByShortCodeAsync(shortCode);

        // Assert
        Assert.NotNull(result);
        _mockRepository.Verify(r => r.UpdateAsync(It.IsAny<UrlShortenerEntry>(), default), Times.Once);
    }

    [Fact]
    public async Task GetUrlByShortCodeAsync_WithInactiveUrl_ShouldReturnNull()
    {
        // Arrange
        var shortCode = "inactive";
        var entry = new UrlShortenerEntry
        {
            Id = Guid.NewGuid(),
            OriginalUrl = "https://www.example.com",
            ShortCode = shortCode,
            IsActive = false
        };

        _mockRepository
            .Setup(r => r.GetByShortCodeAsync(shortCode, default))
            .ReturnsAsync(entry);

        // Act
        var result = await _service.GetUrlByShortCodeAsync(shortCode);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetUrlByShortCodeAsync_WithNullCode_ShouldThrowArgumentException()
    {
        // Act & Assert
        await Assert.ThrowsAnyAsync<ArgumentException>(
            () => _service.GetUrlByShortCodeAsync(null!));
    }

    #endregion



    #region GetAllUrlsAsync Tests

    [Fact]
    public async Task GetAllUrlsAsync_WithValidParameters_ShouldReturnPaginatedResults()
    {
        // Arrange
        var entries = new List<UrlShortenerEntry>
        {
            new UrlShortenerEntry
            {
                Id = Guid.NewGuid(),
                OriginalUrl = "https://example1.com",
                ShortCode = "code1",
                IsActive = true
            },
            new UrlShortenerEntry
            {
                Id = Guid.NewGuid(),
                OriginalUrl = "https://example2.com",
                ShortCode = "code2",
                IsActive = true
            }
        };

        _mockRepository
            .Setup(r => r.GetAllAsync(1, 10, default))
            .ReturnsAsync((entries, 2));

        // Act
        var result = await _service.GetAllUrlsAsync(1, 10);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Items.Count);
        Assert.Equal(2, result.TotalCount);
        Assert.Equal(1, result.PageNumber);
        Assert.Equal(10, result.PageSize);
    }

    [Fact]
    public async Task GetAllUrlsAsync_WithInvalidPageNumber_ShouldThrowArgumentException()
    {
        // Act & Assert
        var ex = await Assert.ThrowsAsync<ArgumentException>(
            () => _service.GetAllUrlsAsync(pageNumber: 0));

        Assert.Contains("Page number", ex.Message);
    }

    [Fact]
    public async Task GetAllUrlsAsync_WithInvalidPageSize_ShouldThrowArgumentException()
    {
        // Act & Assert
        var ex = await Assert.ThrowsAsync<ArgumentException>(
            () => _service.GetAllUrlsAsync(pageSize: 101));

        Assert.Contains("Page size", ex.Message);
    }

    #endregion

    #region UpdateUrlAsync Tests

    [Fact]
    public async Task UpdateUrlAsync_WithValidRequest_ShouldUpdateUrl()
    {
        // Arrange
        var id = Guid.NewGuid();
        var existingEntry = new UrlShortenerEntry
        {
            Id = id,
            OriginalUrl = "https://www.old.com",
            ShortCode = "code1"
        };

        var request = new UpdateUrlRequest
        {
            OriginalUrl = "https://www.new.com"
        };

        _mockRepository
            .Setup(r => r.GetByIdAsync(id, default))
            .ReturnsAsync(existingEntry);

        _mockRepository
            .Setup(r => r.UpdateAsync(It.IsAny<UrlShortenerEntry>(), default))
            .ReturnsAsync((UrlShortenerEntry entry, CancellationToken _) =>
            {
                entry.UpdatedAt = DateTime.UtcNow;
                return entry;
            });

        // Act
        var result = await _service.UpdateUrlAsync(id, request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(request.OriginalUrl, result.OriginalUrl);
        _mockRepository.Verify(r => r.UpdateAsync(It.IsAny<UrlShortenerEntry>(), default), Times.Once);
    }

    [Fact]
    public async Task UpdateUrlAsync_WithNonexistentId_ShouldReturnNull()
    {
        // Arrange
        var request = new UpdateUrlRequest
        {
            OriginalUrl = "https://www.new.com"
        };

        _mockRepository
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), default))
            .ReturnsAsync((UrlShortenerEntry?)null);

        // Act
        var result = await _service.UpdateUrlAsync(Guid.NewGuid(), request);

        // Assert
        Assert.Null(result);
    }

    #endregion

    #region DeleteUrlAsync Tests

    [Fact]
    public async Task DeleteUrlAsync_WithValidId_ShouldDeleteUrl()
    {
        // Arrange
        var id = Guid.NewGuid();
        _mockRepository
            .Setup(r => r.DeleteAsync(id, default))
            .ReturnsAsync(true);

        // Act
        var result = await _service.DeleteUrlAsync(id);

        // Assert
        Assert.True(result);
        _mockRepository.Verify(r => r.DeleteAsync(id, default), Times.Once);
    }

    [Fact]
    public async Task DeleteUrlAsync_WithInvalidId_ShouldReturnFalse()
    {
        // Arrange
        _mockRepository
            .Setup(r => r.DeleteAsync(It.IsAny<Guid>(), default))
            .ReturnsAsync(false);

        // Act
        var result = await _service.DeleteUrlAsync(Guid.NewGuid());

        // Assert
        Assert.False(result);
    }

    #endregion
}
