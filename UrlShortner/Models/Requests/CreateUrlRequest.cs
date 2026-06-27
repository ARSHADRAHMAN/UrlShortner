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
}
