// This file is part of the Genova project licensed under the GNU General Public License v3.0.
// See the LICENSE file in the project root for more information.

using System.Net;
using FluentAssertions;
using Genova.Crawler.Resources;

namespace Genova.Crawler.UnitTests.Resources;

public class ResourceDetails_Tests
{
    [Fact]
    public void Constructor_should_initialize_properties_correctly()
    {
        // Arrange
        ResourceDetailsParameters parameters = new()
        {
            Url = new Uri("https://example.com"),
            Headers = new Dictionary<string, string> { { "Content-Type", "text/html" } },
            Cookies = ["SessionId=abc123"],
            StatusCode = HttpStatusCode.OK,
            ContentLength = 1024,
            ContentType = "text/html",
            RedirectLocation = new Uri("https://example.com/redirect"),
            ResponseTime = TimeSpan.FromMilliseconds(150)
        };

        // Act
        ResourceDetails resourceDetails = new(parameters);

        // Assert
        resourceDetails.Url.Should().Be(parameters.Url);
        resourceDetails.Headers.Should().BeEquivalentTo(parameters.Headers);
        resourceDetails.Cookies.Should().BeEquivalentTo(parameters.Cookies);
        resourceDetails.StatusCode.Should().Be(parameters.StatusCode);
        resourceDetails.ContentLength.Should().Be(parameters.ContentLength);
        resourceDetails.ContentType.Should().Be(parameters.ContentType);
        resourceDetails.RedirectLocation.Should().Be(parameters.RedirectLocation);
        resourceDetails.ResponseTime.Should().Be(parameters.ResponseTime);
    }

    [Fact]
    public void Constructor_should_throw_ArgumentNullException_when_parameters_is_null()
    {
        // Act
        Action act = () => _ = new ResourceDetails(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("parameters");
    }

    [Fact]
    public void Constructor_should_throw_ArgumentNullException_when_url_is_null()
    {
        // Arrange
        ResourceDetailsParameters parameters = new()
        {
            Url = null!,
            Headers = new Dictionary<string, string> { { "Content-Type", "text/html" } },
            Cookies = ["SessionId=abc123"],
            StatusCode = HttpStatusCode.OK,
            ContentLength = 1024,
            ContentType = "text/html",
            RedirectLocation = new Uri("https://example.com/redirect"),
            ResponseTime = TimeSpan.FromMilliseconds(150)
        };

        // Act
        Action act = () => _ = new ResourceDetails(parameters);

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("parameters.Url");
    }

    [Fact]
    public void Constructor_should_throw_ArgumentNullException_when_headers_is_null()
    {
        // Arrange
        ResourceDetailsParameters parameters = new()
        {
            Url = new Uri("https://example.com"),
            Headers = null!,
            Cookies = ["SessionId=abc123"],
            StatusCode = HttpStatusCode.OK,
            ContentLength = 1024,
            ContentType = "text/html",
            RedirectLocation = new Uri("https://example.com/redirect"),
            ResponseTime = TimeSpan.FromMilliseconds(150)
        };

        // Act
        Action act = () => _ = new ResourceDetails(parameters);

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("parameters.Headers");
    }

    [Fact]
    public void Constructor_should_throw_ArgumentNullException_when_cookies_is_null()
    {
        // Arrange
        ResourceDetailsParameters parameters = new()
        {
            Url = new Uri("https://example.com"),
            Headers = new Dictionary<string, string> { { "Content-Type", "text/html" } },
            Cookies = null!,
            StatusCode = HttpStatusCode.OK,
            ContentLength = 1024,
            ContentType = "text/html",
            RedirectLocation = new Uri("https://example.com/redirect"),
            ResponseTime = TimeSpan.FromMilliseconds(150)
        };

        // Act
        Action act = () => _ = new ResourceDetails(parameters);

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("parameters.Cookies");
    }

    [Fact]
    public void Constructor_should_throw_ArgumentOutOfRangeException_when_contentLength_is_negative()
    {
        // Arrange
        ResourceDetailsParameters parameters = new()
        {
            Url = new Uri("https://example.com"),
            Headers = new Dictionary<string, string> { { "Content-Type", "text/html" } },
            Cookies = ["SessionId=abc123"],
            StatusCode = HttpStatusCode.OK,
            ContentLength = -1, // Invalid value
            ContentType = "text/html",
            RedirectLocation = new Uri("https://example.com/redirect"),
            ResponseTime = TimeSpan.FromMilliseconds(150)
        };

        // Act
        Action act = () => _ = new ResourceDetails(parameters);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>().WithParameterName("parameters.ContentLength");
    }

    [Fact]
    public void Constructor_should_throw_ArgumentOutOfRangeException_when_responseTime_is_negative()
    {
        // Arrange
        ResourceDetailsParameters parameters = new()
        {
            Url = new Uri("https://example.com"),
            Headers = new Dictionary<string, string> { { "Content-Type", "text/html" } },
            Cookies = ["SessionId=abc123"],
            StatusCode = HttpStatusCode.OK,
            ContentLength = 1024,
            ContentType = "text/html",
            RedirectLocation = new Uri("https://example.com/redirect"),
            ResponseTime = TimeSpan.FromMilliseconds(-1) // Invalid value
        };

        // Act
        Action act = () => _ = new ResourceDetails(parameters);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>().WithParameterName("parameters.ResponseTime");
    }
}
