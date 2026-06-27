namespace UrlShortner.Models.Responses;

/// <summary>
/// Response model for URL shortener operations
/// </summary>
public class UrlShortenerResponse
{
    /// <summary>
    /// Unique identifier of the URL entry
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// The original long URL
    /// </summary>
    public required string OriginalUrl { get; set; }

    /// <summary>
    /// The shortened URL code
    /// </summary>
    public required string ShortCode { get; set; }

    /// <summary>
    /// Full short URL (e.g., https://short.url/abc123)
    /// </summary>
    public string? ShortUrl { get; set; }

    /// <summary>
    /// Number of times this short URL has been accessed (clicks)
    /// </summary>
    public int ClickCount { get; set; }

    /// <summary>
    /// Date and time of the most recent access
    /// </summary>
    public DateTime? LastVisited { get; set; }

    /// <summary>
    /// Date and time when the entry was created
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Date and time when the entry was last updated
    /// </summary>
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// Optional expiration date
    /// </summary>
    public DateTime? ExpiresAt { get; set; }

    /// <summary>
    /// Custom alias if provided
    /// </summary>
    public string? CustomAlias { get; set; }

    /// <summary>
    /// Whether this entry is active
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// Whether this entry has expired
    /// </summary>
    public bool IsExpired => ExpiresAt.HasValue && ExpiresAt < DateTime.UtcNow;
}

/// <summary>
/// Paginated list response
/// </summary>
public class PaginatedResponse<T>
{
    /// <summary>
    /// Items in the current page
    /// </summary>
    public required List<T> Items { get; set; }

    /// <summary>
    /// Total number of items
    /// </summary>
    public int TotalCount { get; set; }

    /// <summary>
    /// Current page number
    /// </summary>
    public int PageNumber { get; set; }

    /// <summary>
    /// Page size
    /// </summary>
    public int PageSize { get; set; }

    /// <summary>
    /// Total number of pages
    /// </summary>
    public int TotalPages => (TotalCount + PageSize - 1) / PageSize;
}
