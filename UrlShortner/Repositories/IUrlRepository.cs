using UrlShortner.Models;

namespace UrlShortner.Repositories;

/// <summary>
/// Repository interface for URL shortener data access operations
/// Follows the Repository pattern and SOLID principles (Dependency Inversion)
/// </summary>
public interface IUrlRepository
{
    /// <summary>
    /// Create a new URL shortener entry asynchronously
    /// </summary>
    /// <param name="entry">The URL entry to create</param>
    /// <param name="cancellationToken">Cancellation token for the operation</param>
    /// <returns>The created URL entry</returns>
    Task<UrlShortenerEntry> CreateAsync(UrlShortenerEntry entry, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get a URL entry by ID asynchronously
    /// </summary>
    /// <param name="id">The ID of the URL entry</param>
    /// <param name="cancellationToken">Cancellation token for the operation</param>
    /// <returns>The URL entry if found; otherwise null</returns>
    Task<UrlShortenerEntry?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get a URL entry by short code asynchronously
    /// </summary>
    /// <param name="shortCode">The short code</param>
    /// <param name="cancellationToken">Cancellation token for the operation</param>
    /// <returns>The URL entry if found; otherwise null</returns>
    Task<UrlShortenerEntry?> GetByShortCodeAsync(string shortCode, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get a URL entry by its original URL asynchronously
    /// </summary>
    /// <param name="originalUrl">The original long URL</param>
    /// <param name="cancellationToken">Cancellation token for the operation</param>
    /// <returns>The URL entry if found; otherwise null</returns>
    Task<UrlShortenerEntry?> GetByOriginalUrlAsync(string originalUrl, CancellationToken cancellationToken = default);



    /// <summary>
    /// Get all URL entries asynchronously with pagination
    /// </summary>
    /// <param name="pageNumber">The page number (1-based)</param>
    /// <param name="pageSize">The number of items per page</param>
    /// <param name="cancellationToken">Cancellation token for the operation</param>
    /// <returns>List of URL entries and total count</returns>
    Task<(List<UrlShortenerEntry> Items, int TotalCount)> GetAllAsync(int pageNumber = 1, int pageSize = 10, CancellationToken cancellationToken = default);

    /// <summary>
    /// Update an existing URL entry asynchronously
    /// </summary>
    /// <param name="entry">The URL entry to update</param>
    /// <param name="cancellationToken">Cancellation token for the operation</param>
    /// <returns>The updated URL entry</returns>
    Task<UrlShortenerEntry> UpdateAsync(UrlShortenerEntry entry, CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete a URL entry by ID asynchronously
    /// </summary>
    /// <param name="id">The ID of the URL entry to delete</param>
    /// <param name="cancellationToken">Cancellation token for the operation</param>
    /// <returns>True if deletion was successful; otherwise false</returns>
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if a short code already exists asynchronously
    /// </summary>
    /// <param name="shortCode">The short code to check</param>
    /// <param name="cancellationToken">Cancellation token for the operation</param>
    /// <returns>True if exists; otherwise false</returns>
    Task<bool> ShortCodeExistsAsync(string shortCode, CancellationToken cancellationToken = default);

    /// <summary>
    /// Increment access count for a URL entry asynchronously
    /// </summary>
    /// <param name="id">The ID of the URL entry</param>
    /// <param name="cancellationToken">Cancellation token for the operation</param>
    /// <returns>True if successful; otherwise false</returns>
    Task<bool> IncrementAccessCountAsync(Guid id, CancellationToken cancellationToken = default);
}
