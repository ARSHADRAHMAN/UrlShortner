using Microsoft.AspNetCore.Mvc;
using UrlShortner.Models.Requests;
using UrlShortner.Models.Responses;
using UrlShortner.Services;

namespace UrlShortner.Controllers;

/// <summary>
/// API controller for URL shortener operations
/// Provides endpoints for creating, retrieving, updating, and deleting shortened URLs
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class UrlsController : ControllerBase
{
    private readonly IUrlShortenerService _service;
    private readonly ILogger<UrlsController> _logger;

    public UrlsController(IUrlShortenerService service, ILogger<UrlsController> logger)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Create a new shortened URL
    /// </summary>
    /// <param name="request">The request containing the URL to shorten</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The created shortened URL response</returns>
    /// <response code="201">URL successfully created</response>
    /// <response code="400">Invalid request or validation error</response>
    /// <response code="409">Custom alias already exists</response>
    /// <response code="500">Internal server error</response>
    [HttpPost]
    [ProducesResponseType(typeof(UrlShortenerResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateShortUrl(
        [FromBody] CreateUrlRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // ModelState validation is handled by the framework
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _logger.LogInformation("Creating short URL for: {OriginalUrl}", request.OriginalUrl);

            var result = await _service.CreateShortUrlAsync(request, cancellationToken).ConfigureAwait(false);

            PopulateShortUrl(result);

            _logger.LogInformation("Short URL created successfully with ID: {Id}", result.Id);

            return CreatedAtAction(nameof(GetUrlById), new { id = result.Id }, result);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Conflict creating short URL: {Message}", ex.Message);
            return Conflict(new ProblemDetails
            {
                Status = StatusCodes.Status409Conflict,
                Title = "Conflict",
                Detail = ex.Message,
                Type = "https://tools.ietf.org/html/rfc7231#section-6.5.8"
            });
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Argument error creating short URL: {Message}", ex.Message);
            return BadRequest(new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Bad Request",
                Detail = ex.Message,
                Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating short URL");
            return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = "Internal Server Error",
                Detail = "An unexpected error occurred while creating the short URL",
                Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1"
            });
        }
    }

    /// <summary>
    /// Get a shortened URL by its ID
    /// </summary>
    /// <param name="id">The URL entry ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The URL response</returns>
    /// <response code="200">URL found</response>
    /// <response code="404">URL not found</response>
    /// <response code="500">Internal server error</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(UrlShortenerResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetUrlById(
        [FromRoute] Guid id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Retrieving URL with ID: {Id}", id);

            var result = await _service.GetUrlByIdAsync(id, cancellationToken).ConfigureAwait(false);

            if (result == null)
            {
                _logger.LogWarning("URL not found with ID: {Id}", id);
                return NotFound(new ProblemDetails
                {
                    Status = StatusCodes.Status404NotFound,
                    Title = "Not Found",
                    Detail = $"URL entry with ID '{id}' not found or has expired",
                    Type = "https://tools.ietf.org/html/rfc7231#section-6.5.4"
                });
            }

            PopulateShortUrl(result);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving URL with ID: {Id}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = "Internal Server Error",
                Detail = "An unexpected error occurred while retrieving the URL",
                Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1"
            });
        }
    }

    /// <summary>
    /// Get the original URL by short code
    /// </summary>
    /// <param name="shortCode">The short code</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The URL response with updated access count</returns>
    /// <response code="200">URL found</response>
    /// <response code="404">URL not found</response>
    /// <response code="500">Internal server error</response>
    [HttpGet("shortcode/{shortCode}")]
    [ProducesResponseType(typeof(UrlShortenerResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetUrlByShortCode(
        [FromRoute] string shortCode,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(shortCode))
            {
                return BadRequest(new ProblemDetails
                {
                    Status = StatusCodes.Status400BadRequest,
                    Title = "Bad Request",
                    Detail = "Short code cannot be empty",
                    Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1"
                });
            }

            _logger.LogInformation("Retrieving URL by short code: {ShortCode}", shortCode);

            var result = await _service.GetUrlByShortCodeAsync(shortCode, cancellationToken).ConfigureAwait(false);

            if (result == null)
            {
                _logger.LogWarning("URL not found with short code: {ShortCode}", shortCode);
                return NotFound(new ProblemDetails
                {
                    Status = StatusCodes.Status404NotFound,
                    Title = "Not Found",
                    Detail = $"URL entry with short code '{shortCode}' not found or has expired",
                    Type = "https://tools.ietf.org/html/rfc7231#section-6.5.4"
                });
            }

            PopulateShortUrl(result);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving URL by short code: {ShortCode}", shortCode);
            return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = "Internal Server Error",
                Detail = "An unexpected error occurred while retrieving the URL",
                Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1"
            });
        }
    }



    /// <summary>
    /// Get all shortened URLs with pagination
    /// </summary>
    /// <param name="pageNumber">The page number (default: 1)</param>
    /// <param name="pageSize">The page size (default: 10)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Paginated list of URLs</returns>
    /// <response code="200">URLs retrieved successfully</response>
    /// <response code="400">Invalid pagination parameters</response>
    /// <response code="500">Internal server error</response>
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedResponse<UrlShortenerResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAllUrls(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Retrieving all URLs - Page: {PageNumber}, Size: {PageSize}", pageNumber, pageSize);

            var result = await _service.GetAllUrlsAsync(pageNumber, pageSize, cancellationToken).ConfigureAwait(false);

            PopulateShortUrls(result);

            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid pagination parameters");
            return BadRequest(new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Bad Request",
                Detail = ex.Message,
                Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all URLs");
            return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = "Internal Server Error",
                Detail = "An unexpected error occurred while retrieving URLs",
                Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1"
            });
        }
    }

    /// <summary>
    /// Update an existing shortened URL
    /// </summary>
    /// <param name="id">The URL entry ID</param>
    /// <param name="request">The update request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The updated URL response</returns>
    /// <response code="200">URL successfully updated</response>
    /// <response code="400">Invalid request or validation error</response>
    /// <response code="404">URL not found</response>
    /// <response code="500">Internal server error</response>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(UrlShortenerResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateUrl(
        [FromRoute] Guid id,
        [FromBody] UpdateUrlRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // ModelState validation is handled by the framework
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _logger.LogInformation("Updating URL with ID: {Id}", id);

            var result = await _service.UpdateUrlAsync(id, request, cancellationToken).ConfigureAwait(false);

            if (result == null)
            {
                _logger.LogWarning("URL not found for update with ID: {Id}", id);
                return NotFound(new ProblemDetails
                {
                    Status = StatusCodes.Status404NotFound,
                    Title = "Not Found",
                    Detail = $"URL entry with ID '{id}' not found",
                    Type = "https://tools.ietf.org/html/rfc7231#section-6.5.4"
                });
            }

            PopulateShortUrl(result);

            _logger.LogInformation("URL updated successfully with ID: {Id}", id);

            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Argument error updating URL: {Message}", ex.Message);
            return BadRequest(new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Bad Request",
                Detail = ex.Message,
                Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating URL with ID: {Id}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = "Internal Server Error",
                Detail = "An unexpected error occurred while updating the URL",
                Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1"
            });
        }
    }

    /// <summary>
    /// Delete a shortened URL
    /// </summary>
    /// <param name="id">The URL entry ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>No content response</returns>
    /// <response code="204">URL successfully deleted</response>
    /// <response code="404">URL not found</response>
    /// <response code="500">Internal server error</response>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteUrl(
        [FromRoute] Guid id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Deleting URL with ID: {Id}", id);

            var result = await _service.DeleteUrlAsync(id, cancellationToken).ConfigureAwait(false);

            if (!result)
            {
                _logger.LogWarning("URL not found for deletion with ID: {Id}", id);
                return NotFound(new ProblemDetails
                {
                    Status = StatusCodes.Status404NotFound,
                    Title = "Not Found",
                    Detail = $"URL entry with ID '{id}' not found",
                    Type = "https://tools.ietf.org/html/rfc7231#section-6.5.4"
                });
            }

            _logger.LogInformation("URL deleted successfully with ID: {Id}", id);

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting URL with ID: {Id}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = "Internal Server Error",
                Detail = "An unexpected error occurred while deleting the URL",
                Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1"
            });
        }
    }

    private void PopulateShortUrl(UrlShortenerResponse? response)
    {
        if (response == null) return;
        var request = HttpContext?.Request;
        if (request != null)
        {
            response.ShortUrl = $"{request.Scheme}://{request.Host}/api/urls/shortcode/{response.ShortCode}";
        }
        else
        {
            response.ShortUrl = $"http://localhost/api/urls/shortcode/{response.ShortCode}";
        }
    }

    private void PopulateShortUrls(PaginatedResponse<UrlShortenerResponse>? response)
    {
        if (response?.Items == null) return;
        foreach (var item in response.Items)
        {
            PopulateShortUrl(item);
        }
    }
}
