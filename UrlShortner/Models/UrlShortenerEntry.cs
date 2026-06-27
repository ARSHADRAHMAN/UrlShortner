namespace UrlShortner.Models;

/// <summary>
/// Domain model representing a shortened URL entry
/// Follows Clean Architecture principles with rich domain behavior
/// </summary>
public class UrlShortenerEntry
{
    /// <summary>
    /// Unique identifier for the URL entry
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// The original long URL that was shortened
    /// </summary>
    public required string OriginalUrl { get; set; }

    /// <summary>
    /// The shortened URL code (e.g., "abc123")
    /// Automatically generated and unique
    /// </summary>
    public required string ShortCode { get; set; }

    /// <summary>
    /// Total number of clicks/accesses to this shortened URL
    /// Tracked for analytics
    /// </summary>
    public int ClickCount { get; set; }

    /// <summary>
    /// Date and time of the most recent access
    /// Null if never accessed
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
    /// Optional expiration date for the short URL
    /// Null means no expiration
    /// </summary>
    public DateTime? ExpiresAt { get; set; }

    /// <summary>
    /// Custom alias provided by user (optional)
    /// Must be unique if provided
    /// </summary>
    public string? CustomAlias { get; set; }

    /// <summary>
    /// Whether this entry is active/enabled
    /// Can be soft-deleted by setting to false
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// User/owner ID who created this URL (optional, for multi-tenant scenarios)
    /// </summary>
    public string? OwnerId { get; set; }

    /// <summary>
    /// Description or tags for this URL (optional)
    /// </summary>
    public string? Description { get; set; }

    // Computed properties

    /// <summary>
    /// Whether this entry has expired based on ExpiresAt date
    /// </summary>
    public bool IsExpired => ExpiresAt.HasValue && ExpiresAt < DateTime.UtcNow;

    /// <summary>
    /// Whether this URL is accessible (not expired, active, and not soft-deleted)
    /// </summary>
    public bool IsAccessible => IsActive && !IsExpired;

    /// <summary>
    /// Age of this shortened URL in days
    /// </summary>
    public int AgeInDays => (int)(DateTime.UtcNow - CreatedAt).TotalDays;

    /// <summary>
    /// Time since last access in hours (-1 if never accessed)
    /// </summary>
    public int? HoursSinceLastAccess => LastVisited.HasValue 
        ? (int)(DateTime.UtcNow - LastVisited.Value).TotalHours 
        : null;

    // Domain methods

    /// <summary>
    /// Record a new click/access to this URL
    /// Updates LastVisited timestamp and increments ClickCount
    /// </summary>
    public void RecordClick()
    {
        if (!IsAccessible)
            throw new InvalidOperationException("Cannot record click on inactive or expired URL");

        ClickCount++;
        LastVisited = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Soft-delete this URL entry by marking it inactive
    /// </summary>
    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Reactivate a previously deactivated URL
    /// </summary>
    public void Reactivate()
    {
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Update the expiration date
    /// </summary>
    /// <param name="newExpirationDate">New expiration date, or null to remove expiration</param>
    public void SetExpiration(DateTime? newExpirationDate)
    {
        ExpiresAt = newExpirationDate;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Update the original URL
    /// </summary>
    /// <param name="newUrl">The new original URL</param>
    public void UpdateOriginalUrl(string newUrl)
    {
        if (string.IsNullOrWhiteSpace(newUrl))
            throw new ArgumentException("URL cannot be empty", nameof(newUrl));

        OriginalUrl = newUrl;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Determine if this URL is available for use
    /// (not expired, active, and not soft-deleted)
    /// </summary>
    /// <returns>True if URL can be accessed, false otherwise</returns>
    public bool CanBeAccessed() => IsAccessible;

    /// <summary>
    /// Get analytics snapshot for this URL
    /// </summary>
    /// <returns>Anonymized analytics data</returns>
    public (int TotalClicks, DateTime? LastAccess, int AgeInDays) GetAnalytics()
        => (ClickCount, LastVisited, AgeInDays);
}
