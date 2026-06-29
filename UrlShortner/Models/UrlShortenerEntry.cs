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
    /// User/owner ID who created this URL (optional, for multi-tenant scenarios)
    /// </summary>
    public string? OwnerId { get; set; }

    /// <summary>
    /// Description or tags for this URL (optional)
    /// </summary>
    public string? Description { get; set; }

    // Computed properties



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
        ClickCount++;
        LastVisited = DateTime.UtcNow;
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
    /// Get analytics snapshot for this URL
    /// </summary>
    /// <returns>Anonymized analytics data</returns>
    public (int TotalClicks, DateTime? LastAccess, int AgeInDays) GetAnalytics()
        => (ClickCount, LastVisited, AgeInDays);
}
