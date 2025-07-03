// This file is part of the Genova project licensed under the GNU General Public License v3.0.
// See the LICENSE file in the project root for more information.

using System.Net;
using FluentAssertions;
using Genova.Crawler.Resources;

namespace Genova.Crawler.UnitTests.Resources;

public class TextResourceDetails_Tests
{
    [Fact]
    public void Constructor_should_initialize_properties_correctly()
    {
        // Arrange
        ResourceDetailsParameters parameters = new()
        {
            Url = new Uri("https://example.com"),
            Headers = new Dictionary<string, string> { { "Content-Type", "text/plain" } },
            Cookies = ["SessionId=abc123"],
            StatusCode = HttpStatusCode.OK,
            ContentLength = 512,
            ContentType = "text/plain",
            RedirectLocation = new Uri("https://example.com/redirect"),
            ResponseTime = TimeSpan.FromMilliseconds(100)
        };
        string? body = "This is a sample text resource.";

        // Act
        TextResourceDetails textResourceDetails = new(parameters, body);

        // Assert
        textResourceDetails.Url.Should().Be(parameters.Url);
        textResourceDetails.Headers.Should().BeEquivalentTo(parameters.Headers);
        textResourceDetails.Cookies.Should().BeEquivalentTo(parameters.Cookies);
        textResourceDetails.StatusCode.Should().Be(parameters.StatusCode);
        textResourceDetails.ContentLength.Should().Be(parameters.ContentLength);
        textResourceDetails.ContentType.Should().Be(parameters.ContentType);
        textResourceDetails.RedirectLocation.Should().Be(parameters.RedirectLocation);
        textResourceDetails.ResponseTime.Should().Be(parameters.ResponseTime);
        textResourceDetails.Body.Should().Be(body);
    }

    [Fact]
    public void Constructor_should_initialize_properties_with_null_body()
    {
        // Arrange
        ResourceDetailsParameters parameters = new()
        {
            Url = new Uri("https://example.com"),
            Headers = new Dictionary<string, string> { { "Content-Type", "text/plain" } },
            Cookies = ["SessionId=abc123"],
            StatusCode = HttpStatusCode.OK,
            ContentLength = 512,
            ContentType = "text/plain",
            RedirectLocation = new Uri("https://example.com/redirect"),
            ResponseTime = TimeSpan.FromMilliseconds(100)
        };
        string? body = null;

        // Act
        TextResourceDetails textResourceDetails = new(parameters, body);

        // Assert
        textResourceDetails.Url.Should().Be(parameters.Url);
        textResourceDetails.Headers.Should().BeEquivalentTo(parameters.Headers);
        textResourceDetails.Cookies.Should().BeEquivalentTo(parameters.Cookies);
        textResourceDetails.StatusCode.Should().Be(parameters.StatusCode);
        textResourceDetails.ContentLength.Should().Be(parameters.ContentLength);
        textResourceDetails.ContentType.Should().Be(parameters.ContentType);
        textResourceDetails.RedirectLocation.Should().Be(parameters.RedirectLocation);
        textResourceDetails.ResponseTime.Should().Be(parameters.ResponseTime);
        textResourceDetails.Body.Should().BeNull();
    }

    [Fact]
    public void Constructor_should_throw_ArgumentNullException_when_parameters_is_null()
    {
        // Arrange
        string? body = "This is a sample text resource.";

        // Act
        Action act = () => _ = new TextResourceDetails(null!, body);

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("parameters");
    }

    [Fact]
    public void Constructor_should_throw_ArgumentOutOfRangeException_when_contentLength_is_negative()
    {
        // Arrange
        ResourceDetailsParameters parameters = new()
        {
            Url = new Uri("https://example.com"),
            Headers = new Dictionary<string, string> { { "Content-Type", "text/plain" } },
            Cookies = ["SessionId=abc123"],
            StatusCode = HttpStatusCode.OK,
            ContentLength = -1, // Invalid value
            ContentType = "text/plain",
            RedirectLocation = new Uri("https://example.com/redirect"),
            ResponseTime = TimeSpan.FromMilliseconds(100)
        };
        string? body = "This is a sample text resource.";

        // Act
        Action act = () => _ = new TextResourceDetails(parameters, body);

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
            Headers = new Dictionary<string, string> { { "Content-Type", "text/plain" } },
            Cookies = ["SessionId=abc123"],
            StatusCode = HttpStatusCode.OK,
            ContentLength = 512,
            ContentType = "text/plain",
            RedirectLocation = new Uri("https://example.com/redirect"),
            ResponseTime = TimeSpan.FromMilliseconds(-1) // Invalid value
        };
        string? body = "This is a sample text resource.";

        // Act
        Action act = () => _ = new TextResourceDetails(parameters, body);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>().WithParameterName("parameters.ResponseTime");
    }
}
