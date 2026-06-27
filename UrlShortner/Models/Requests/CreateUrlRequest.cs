using System.ComponentModel.DataAnnotations;

namespace UrlShortner.Models.Requests;

/// <summary>
/// Request model for creating a new shortened URL
/// </summary>
public class CreateUrlRequest
{
    /// <summary>
    /// The original URL to be shortened (required)
    /// </summary>
    [Required(ErrorMessage = "Original URL is required")]
    [Url(ErrorMessage = "Please provide a valid URL")]
    [StringLength(2048, MinimumLength = 10, ErrorMessage = "URL must be between 10 and 2048 characters")]
    public required string OriginalUrl { get; set; }

    /// <summary>
    /// Custom alias for the short URL (optional, must be unique)
    /// </summary>
    [StringLength(50, MinimumLength = 3, ErrorMessage = "Custom alias must be between 3 and 50 characters")]
    [RegularExpression("^[a-zA-Z0-9_-]+$", ErrorMessage = "Custom alias can only contain alphanumeric characters, hyphens, and underscores")]
    public string? CustomAlias { get; set; }

    /// <summary>
    /// Optional expiration date for the short URL
    /// </summary>
    [DataType(DataType.DateTime)]
    public DateTime? ExpiresAt { get; set; }
}

/// <summary>
/// Request model for updating an existing shortened URL
/// </summary>
public class UpdateUrlRequest
{
    /// <summary>
    /// The new original URL
    /// </summary>
    [Required(ErrorMessage = "Original URL is required")]
    [Url(ErrorMessage = "Please provide a valid URL")]
    [StringLength(2048, MinimumLength = 10, ErrorMessage = "URL must be between 10 and 2048 characters")]
    public required string OriginalUrl { get; set; }

    /// <summary>
    /// Updated expiration date
    /// </summary>
    [DataType(DataType.DateTime)]
    public DateTime? ExpiresAt { get; set; }
}
