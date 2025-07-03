// This file is part of the Genova project licensed under the GNU General Public License v3.0.
// See the LICENSE file in the project root for more information.

using System.Net;
using AngleSharp;
using AngleSharp.Dom;
using FluentAssertions;
using Genova.Crawler.Resources;

namespace Genova.Crawler.UnitTests.Resources;

public class HtmlResourceDetails_Tests
{
    [Fact]
    public void Constructor_should_initialize_properties_with_null_IDocument()
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
        string? body = "<!DOCTYPE html><html><head><title>Example Title</title></head><body></body></html>";
        IDocument? htmlDocument = null;

        // Act
        HtmlResourceDetails htmlResourceDetails = new(parameters, body, htmlDocument);

        // Assert
        htmlResourceDetails.Url.Should().Be(parameters.Url);
        htmlResourceDetails.Headers.Should().BeEquivalentTo(parameters.Headers);
        htmlResourceDetails.Cookies.Should().BeEquivalentTo(parameters.Cookies);
        htmlResourceDetails.StatusCode.Should().Be(parameters.StatusCode);
        htmlResourceDetails.ContentLength.Should().Be(parameters.ContentLength);
        htmlResourceDetails.ContentType.Should().Be(parameters.ContentType);
        htmlResourceDetails.RedirectLocation.Should().Be(parameters.RedirectLocation);
        htmlResourceDetails.ResponseTime.Should().Be(parameters.ResponseTime);
        htmlResourceDetails.Body.Should().Be(body);
        htmlResourceDetails.HtmlDocument.Should().BeNull();
    }

    [Fact]
    public async Task Constructor_should_initialize_properties_with_IDocument_instance()
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
        string? body = "<!DOCTYPE html><html><head><title>Example Title</title></head><body></body></html>";

        // Create a real IDocument by parsing the body content
        IBrowsingContext browsingContext = BrowsingContext.New(Configuration.Default);
        IDocument htmlDocument = await browsingContext.OpenAsync(req => req.Content(body));

        // Act
        HtmlResourceDetails htmlResourceDetails = new(parameters, body, htmlDocument);

        // Assert
        htmlResourceDetails.Url.Should().Be(parameters.Url);
        htmlResourceDetails.Headers.Should().BeEquivalentTo(parameters.Headers);
        htmlResourceDetails.Cookies.Should().BeEquivalentTo(parameters.Cookies);
        htmlResourceDetails.StatusCode.Should().Be(parameters.StatusCode);
        htmlResourceDetails.ContentLength.Should().Be(parameters.ContentLength);
        htmlResourceDetails.ContentType.Should().Be(parameters.ContentType);
        htmlResourceDetails.RedirectLocation.Should().Be(parameters.RedirectLocation);
        htmlResourceDetails.ResponseTime.Should().Be(parameters.ResponseTime);
        htmlResourceDetails.Body.Should().Be(body);
        htmlResourceDetails.HtmlDocument.Should().NotBeNull();
        htmlResourceDetails.HtmlDocument.Title.Should().Be("Example Title");
    }

    [Fact]
    public void Constructor_should_throw_ArgumentNullException_when_parameters_is_null()
    {
        // Arrange
        string? body = "<!DOCTYPE html><html><head><title>Example Title</title></head><body></body></html>";
        IDocument? htmlDocument = null;

        // Act
        Action act = () => _ = new HtmlResourceDetails(null!, body, htmlDocument);

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("parameters");
    }
}
