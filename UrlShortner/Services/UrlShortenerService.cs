using UrlShortner.Models;
using UrlShortner.Models.Requests;
using UrlShortner.Models.Responses;
using UrlShortner.Repositories;

namespace UrlShortner.Services;

/// <summary>
/// Service interface for URL shortener business logic
/// Follows Dependency Inversion and Single Responsibility principles
/// </summary>
public interface IUrlShortenerService
{
    /// <summary>
    /// Create a shortened URL from the provided request
    /// </summary>
    /// <param name="request">The create URL request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The created URL response</returns>
    Task<UrlShortenerResponse> CreateShortUrlAsync(CreateUrlRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieve a URL entry by ID
    /// </summary>
    /// <param name="id">The URL entry ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The URL response if found; otherwise null</returns>
    Task<UrlShortenerResponse?> GetUrlByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieve the original URL by short code
    /// </summary>
    /// <param name="shortCode">The short code</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The URL response if found; otherwise null</returns>
    Task<UrlShortenerResponse?> GetUrlByShortCodeAsync(string shortCode, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieve URL by custom alias
    /// </summary>
    /// <param name="customAlias">The custom alias</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The URL response if found; otherwise null</returns>
    Task<UrlShortenerResponse?> GetUrlByCustomAliasAsync(string customAlias, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all URLs with pagination
    /// </summary>
    /// <param name="pageNumber">The page number</param>
    /// <param name="pageSize">The page size</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Paginated response of URLs</returns>
    Task<PaginatedResponse<UrlShortenerResponse>> GetAllUrlsAsync(int pageNumber = 1, int pageSize = 10, CancellationToken cancellationToken = default);

    /// <summary>
    /// Update an existing shortened URL
    /// </summary>
    /// <param name="id">The URL entry ID</param>
    /// <param name="request">The update request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The updated URL response</returns>
    Task<UrlShortenerResponse?> UpdateUrlAsync(Guid id, UpdateUrlRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete a shortened URL
    /// </summary>
    /// <param name="id">The URL entry ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if deleted successfully; otherwise false</returns>
    Task<bool> DeleteUrlAsync(Guid id, CancellationToken cancellationToken = default);
}

/// <summary>
/// Implementation of URL shortener business logic
/// Encapsulates core business rules and operations
/// </summary>
public class UrlShortenerService : IUrlShortenerService
{
    private readonly IUrlRepository _repository;
    private const int MaxShortCodeLength = 6;
    private const string CharacterSet = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

    public UrlShortenerService(IUrlRepository repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    public async Task<UrlShortenerResponse> CreateShortUrlAsync(CreateUrlRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        // Check if custom alias is provided and already exists
        if (!string.IsNullOrWhiteSpace(request.CustomAlias))
        {
            var existingAlias = await _repository.GetByCustomAliasAsync(request.CustomAlias, cancellationToken).ConfigureAwait(false);
            if (existingAlias != null)
            {
                throw new InvalidOperationException($"Custom alias '{request.CustomAlias}' is already in use");
            }
        }

        // Generate or use custom alias as short code
        string shortCode = !string.IsNullOrWhiteSpace(request.CustomAlias)
            ? request.CustomAlias
            : GenerateShortCode();

        // Ensure short code is unique
        while (await _repository.ShortCodeExistsAsync(shortCode, cancellationToken).ConfigureAwait(false))
        {
            shortCode = GenerateShortCode();
        }

        var entry = new UrlShortenerEntry
        {
            OriginalUrl = request.OriginalUrl,
            ShortCode = shortCode,
            CustomAlias = request.CustomAlias,
            ExpiresAt = request.ExpiresAt,
            ClickCount = 0,
            IsActive = true
        };

        var createdEntry = await _repository.CreateAsync(entry, cancellationToken).ConfigureAwait(false);

        return MapToResponse(createdEntry);
    }

    public async Task<UrlShortenerResponse?> GetUrlByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entry = await _repository.GetByIdAsync(id, cancellationToken).ConfigureAwait(false);

        if (entry == null)
        {
            return null;
        }

        // Check if URL has expired
        if (entry.IsExpired)
        {
            return null;
        }

        return MapToResponse(entry);
    }

    public async Task<UrlShortenerResponse?> GetUrlByShortCodeAsync(string shortCode, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(shortCode);

        var entry = await _repository.GetByShortCodeAsync(shortCode, cancellationToken).ConfigureAwait(false);

        if (entry == null || !entry.IsActive)
        {
            return null;
        }

        // Check if URL has expired
        if (entry.IsExpired)
        {
            return null;
        }

        // Record the click - domain method handles incrementing and timestamp
        entry.RecordClick();
        await _repository.UpdateAsync(entry, cancellationToken).ConfigureAwait(false);

        return MapToResponse(entry);
    }

    public async Task<UrlShortenerResponse?> GetUrlByCustomAliasAsync(string customAlias, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(customAlias);

        var entry = await _repository.GetByCustomAliasAsync(customAlias, cancellationToken).ConfigureAwait(false);

        if (entry == null || !entry.IsActive)
        {
            return null;
        }

        // Check if URL has expired
        if (entry.IsExpired)
        {
            return null;
        }

        // Record the click - domain method handles incrementing and timestamp
        entry.RecordClick();
        await _repository.UpdateAsync(entry, cancellationToken).ConfigureAwait(false);

        return MapToResponse(entry);
    }

    public async Task<PaginatedResponse<UrlShortenerResponse>> GetAllUrlsAsync(int pageNumber = 1, int pageSize = 10, CancellationToken cancellationToken = default)
    {
        if (pageNumber < 1)
            throw new ArgumentException("Page number must be greater than 0", nameof(pageNumber));

        if (pageSize < 1 || pageSize > 100)
            throw new ArgumentException("Page size must be between 1 and 100", nameof(pageSize));

        var (entries, totalCount) = await _repository.GetAllAsync(pageNumber, pageSize, cancellationToken).ConfigureAwait(false);

        var responses = entries
            .Where(x => x.IsActive && !x.IsExpired)
            .Select(MapToResponse)
            .ToList();

        return new PaginatedResponse<UrlShortenerResponse>
        {
            Items = responses,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

    public async Task<UrlShortenerResponse?> UpdateUrlAsync(Guid id, UpdateUrlRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var entry = await _repository.GetByIdAsync(id, cancellationToken).ConfigureAwait(false);

        if (entry == null)
        {
            return null;
        }

        // Update fields
        entry.OriginalUrl = request.OriginalUrl;
        entry.ExpiresAt = request.ExpiresAt;

        var updatedEntry = await _repository.UpdateAsync(entry, cancellationToken).ConfigureAwait(false);

        return MapToResponse(updatedEntry);
    }

    public async Task<bool> DeleteUrlAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _repository.DeleteAsync(id, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Generate a random short code
    /// </summary>
    /// <returns>A random short code string</returns>
    private static string GenerateShortCode()
    {
        var random = new Random();
        var shortCode = new string(
            Enumerable.Range(0, MaxShortCodeLength)
                .Select(_ => CharacterSet[random.Next(CharacterSet.Length)])
                .ToArray()
        );

        return shortCode;
    }

    /// <summary>
    /// Map UrlShortenerEntry to UrlShortenerResponse
    /// </summary>
    /// <param name="entry">The domain entry</param>
    /// <returns>The response DTO</returns>
    private static UrlShortenerResponse MapToResponse(UrlShortenerEntry entry)
    {
        return new UrlShortenerResponse
        {
            Id = entry.Id,
            OriginalUrl = entry.OriginalUrl,
            ShortCode = entry.ShortCode,
            ClickCount = entry.ClickCount,
                LastVisited = entry.LastVisited,
            CreatedAt = entry.CreatedAt,
            UpdatedAt = entry.UpdatedAt,
            ExpiresAt = entry.ExpiresAt,
            CustomAlias = entry.CustomAlias,
            IsActive = entry.IsActive
        };
    }
}
