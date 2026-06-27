using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using UrlShortner.Controllers;
using UrlShortner.Models.Requests;
using UrlShortner.Models.Responses;
using UrlShortner.Services;

namespace UrlShortner.Tests.Controllers;

public class UrlsControllerTests
{
    private readonly Mock<IUrlShortenerService> _mockService;
    private readonly Mock<ILogger<UrlsController>> _mockLogger;
    private readonly UrlsController _controller;

    public UrlsControllerTests()
    {
        _mockService = new Mock<IUrlShortenerService>();
        _mockLogger = new Mock<ILogger<UrlsController>>();
        _controller = new UrlsController(_mockService.Object, _mockLogger.Object);
    }

    #region CreateShortUrl Tests

    [Fact]
    public async Task CreateShortUrl_WithValidRequest_ShouldReturnCreatedAtAction()
    {
        // Arrange
        var request = new CreateUrlRequest
        {
            OriginalUrl = "https://www.example.com/very/long/url"
        };

        var response = new UrlShortenerResponse
        {
            Id = Guid.NewGuid(),
            OriginalUrl = request.OriginalUrl,
            ShortCode = "abc123"
        };

        _mockService
            .Setup(s => s.CreateShortUrlAsync(request, default))
            .ReturnsAsync(response);

        // Act
        var result = await _controller.CreateShortUrl(request);

        // Assert
        Assert.NotNull(result);
        var createdResult = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal(nameof(UrlsController.GetUrlById), createdResult.ActionName);
        Assert.Equal(response.Id, ((UrlShortenerResponse)createdResult.Value!).Id);
        _mockService.Verify(s => s.CreateShortUrlAsync(request, default), Times.Once);
    }



    [Fact]
    public async Task CreateShortUrl_WithServiceException_ShouldReturnInternalServerError()
    {
        // Arrange
        var request = new CreateUrlRequest
        {
            OriginalUrl = "https://www.example.com"
        };

        _mockService
            .Setup(s => s.CreateShortUrlAsync(request, default))
            .ThrowsAsync(new Exception("Unexpected error"));

        // Act
        var result = await _controller.CreateShortUrl(request);

        // Assert
        Assert.NotNull(result);
        var statusResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(StatusCodes.Status500InternalServerError, statusResult.StatusCode);
    }

    #endregion

    #region GetUrlById Tests

    [Fact]
    public async Task GetUrlById_WithValidId_ShouldReturnOk()
    {
        // Arrange
        var id = Guid.NewGuid();
        var response = new UrlShortenerResponse
        {
            Id = id,
            OriginalUrl = "https://www.example.com",
            ShortCode = "abc123"
        };

        _mockService
            .Setup(s => s.GetUrlByIdAsync(id, default))
            .ReturnsAsync(response);

        // Act
        var result = await _controller.GetUrlById(id);

        // Assert
        Assert.NotNull(result);
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);
        Assert.Equal(response.Id, ((UrlShortenerResponse)okResult.Value!).Id);
    }

    [Fact]
    public async Task GetUrlById_WithInvalidId_ShouldReturnNotFound()
    {
        // Arrange
        var id = Guid.NewGuid();
        _mockService
            .Setup(s => s.GetUrlByIdAsync(id, default))
            .ReturnsAsync((UrlShortenerResponse?)null);

        // Act
        var result = await _controller.GetUrlById(id);

        // Assert
        Assert.NotNull(result);
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal(StatusCodes.Status404NotFound, notFoundResult.StatusCode);
    }

    #endregion

    #region GetUrlByShortCode Tests

    [Fact]
    public async Task GetUrlByShortCode_WithValidCode_ShouldReturnOk()
    {
        // Arrange
        var shortCode = "abc123";
        var response = new UrlShortenerResponse
        {
            Id = Guid.NewGuid(),
            OriginalUrl = "https://www.example.com",
            ShortCode = shortCode
        };

        _mockService
            .Setup(s => s.GetUrlByShortCodeAsync(shortCode, default))
            .ReturnsAsync(response);

        // Act
        var result = await _controller.GetUrlByShortCode(shortCode);

        // Assert
        Assert.NotNull(result);
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);
    }

    [Fact]
    public async Task GetUrlByShortCode_WithInvalidCode_ShouldReturnNotFound()
    {
        // Arrange
        var shortCode = "invalid";
        _mockService
            .Setup(s => s.GetUrlByShortCodeAsync(shortCode, default))
            .ReturnsAsync((UrlShortenerResponse?)null);

        // Act
        var result = await _controller.GetUrlByShortCode(shortCode);

        // Assert
        Assert.NotNull(result);
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal(StatusCodes.Status404NotFound, notFoundResult.StatusCode);
    }

    [Fact]
    public async Task GetUrlByShortCode_WithEmptyCode_ShouldReturnBadRequest()
    {
        // Act
        var result = await _controller.GetUrlByShortCode(string.Empty);

        // Assert
        Assert.NotNull(result);
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal(StatusCodes.Status400BadRequest, badRequestResult.StatusCode);
    }

    #endregion



    #region GetAllUrls Tests

    [Fact]
    public async Task GetAllUrls_WithValidParameters_ShouldReturnOk()
    {
        // Arrange
        var response = new PaginatedResponse<UrlShortenerResponse>
        {
            Items = new List<UrlShortenerResponse>
            {
                new UrlShortenerResponse
                {
                    Id = Guid.NewGuid(),
                    OriginalUrl = "https://example1.com",
                    ShortCode = "code1"
                }
            },
            TotalCount = 1,
            PageNumber = 1,
            PageSize = 10
        };

        _mockService
            .Setup(s => s.GetAllUrlsAsync(1, 10, default))
            .ReturnsAsync(response);

        // Act
        var result = await _controller.GetAllUrls(1, 10);

        // Assert
        Assert.NotNull(result);
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);
        Assert.Equal(response, (PaginatedResponse<UrlShortenerResponse>)okResult.Value!);
    }

    [Fact]
    public async Task GetAllUrls_WithInvalidPageParameters_ShouldReturnBadRequest()
    {
        // Arrange
        _mockService
            .Setup(s => s.GetAllUrlsAsync(0, 10, default))
            .ThrowsAsync(new ArgumentException("Invalid page number"));

        // Act
        var result = await _controller.GetAllUrls(0, 10);

        // Assert
        Assert.NotNull(result);
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal(StatusCodes.Status400BadRequest, badRequestResult.StatusCode);
    }

    #endregion

    #region UpdateUrl Tests

    [Fact]
    public async Task UpdateUrl_WithValidRequest_ShouldReturnOk()
    {
        // Arrange
        var id = Guid.NewGuid();
        var request = new UpdateUrlRequest
        {
            OriginalUrl = "https://www.updated.com"
        };

        var response = new UrlShortenerResponse
        {
            Id = id,
            OriginalUrl = request.OriginalUrl,
            ShortCode = "abc123"
        };

        _mockService
            .Setup(s => s.UpdateUrlAsync(id, request, default))
            .ReturnsAsync(response);

        // Act
        var result = await _controller.UpdateUrl(id, request);

        // Assert
        Assert.NotNull(result);
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);
        Assert.Equal(response.OriginalUrl, ((UrlShortenerResponse)okResult.Value!).OriginalUrl);
    }

    [Fact]
    public async Task UpdateUrl_WithNonexistentId_ShouldReturnNotFound()
    {
        // Arrange
        var id = Guid.NewGuid();
        var request = new UpdateUrlRequest
        {
            OriginalUrl = "https://www.updated.com"
        };

        _mockService
            .Setup(s => s.UpdateUrlAsync(id, request, default))
            .ReturnsAsync((UrlShortenerResponse?)null);

        // Act
        var result = await _controller.UpdateUrl(id, request);

        // Assert
        Assert.NotNull(result);
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal(StatusCodes.Status404NotFound, notFoundResult.StatusCode);
    }

    #endregion

    #region DeleteUrl Tests

    [Fact]
    public async Task DeleteUrl_WithValidId_ShouldReturnNoContent()
    {
        // Arrange
        var id = Guid.NewGuid();
        _mockService
            .Setup(s => s.DeleteUrlAsync(id, default))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.DeleteUrl(id);

        // Assert
        Assert.NotNull(result);
        var noContentResult = Assert.IsType<NoContentResult>(result);
        Assert.Equal(StatusCodes.Status204NoContent, noContentResult.StatusCode);
        _mockService.Verify(s => s.DeleteUrlAsync(id, default), Times.Once);
    }

    [Fact]
    public async Task DeleteUrl_WithInvalidId_ShouldReturnNotFound()
    {
        // Arrange
        var id = Guid.NewGuid();
        _mockService
            .Setup(s => s.DeleteUrlAsync(id, default))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.DeleteUrl(id);

        // Assert
        Assert.NotNull(result);
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal(StatusCodes.Status404NotFound, notFoundResult.StatusCode);
    }

    #endregion
}
