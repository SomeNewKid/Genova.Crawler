// This file is part of the Genova project licensed under the GNU General Public License v3.0.
// See the LICENSE file in the project root for more information.

using System.Net;
using FluentAssertions;
using Genova.Crawler.Resources;

namespace Genova.Crawler.UnitTests.Resources;

public class HtmlResourceDetailsBuilder_Tests
{
    [Fact]
    public void Build_should_initialize_HtmlResourceDetails_correctly()
    {
        // Arrange
        Uri url = new("https://example.com");
        HttpResponseMessage response = CreateHttpResponseMessage(
            HttpStatusCode.OK,
            new Dictionary<string, string>
            {
                { "Content-Type", "text/html" },
                { "Set-Cookie", "SessionId=abc123" }
            },
            "<!DOCTYPE html><html><head><title>Example</title></head><body></body></html>"
        );
        TimeSpan responseTime = TimeSpan.FromMilliseconds(150);

        HtmlResourceDetailsBuilder builder = new(url, response, responseTime);

        // Act
        HtmlResourceDetails htmlResourceDetails = (HtmlResourceDetails)builder.Build();

        // Assert
        htmlResourceDetails.Url.Should().Be(url);
        htmlResourceDetails.Cookies.Should().ContainSingle().Which.Should().Be("SessionId=abc123");
        htmlResourceDetails.StatusCode.Should().Be(HttpStatusCode.OK);
        htmlResourceDetails.ContentType.Should().Be("text/html");
        htmlResourceDetails.RedirectLocation.Should().BeNull();
        htmlResourceDetails.ResponseTime.Should().Be(responseTime);
        htmlResourceDetails.Body.Should().Be("<!DOCTYPE html><html><head><title>Example</title></head><body></body></html>");
        htmlResourceDetails.HtmlDocument.Should().NotBeNull();
        htmlResourceDetails.HtmlDocument!.Title.Should().Be("Example");
    }

    [Fact]
    public void Build_should_handle_null_body_content()
    {
        // Arrange
        Uri url = new("https://example.com");
        HttpResponseMessage response = CreateHttpResponseMessage(
            HttpStatusCode.OK,
            new Dictionary<string, string> { { "Content-Type", "text/html" } },
            null // No content
        );
        TimeSpan responseTime = TimeSpan.FromMilliseconds(150);

        HtmlResourceDetailsBuilder builder = new(url, response, responseTime);

        // Act
        HtmlResourceDetails htmlResourceDetails = (HtmlResourceDetails)builder.Build();

        // Assert
        Assert.NotNull(htmlResourceDetails);
        Assert.NotNull(htmlResourceDetails.Body);
        Assert.Empty(htmlResourceDetails.Body);
        Assert.Null(htmlResourceDetails.HtmlDocument);
    }

    [Fact]
    public void Build_should_handle_invalid_html_body()
    {
        // Arrange
        Uri url = new("https://example.com");
        HttpResponseMessage response = CreateHttpResponseMessage(
            HttpStatusCode.OK,
            new Dictionary<string, string> { { "Content-Type", "text/html" } },
            "<html><head><title>Invalid HTML"
        );
        TimeSpan responseTime = TimeSpan.FromMilliseconds(150);

        HtmlResourceDetailsBuilder builder = new(url, response, responseTime);

        // Act
        HtmlResourceDetails htmlResourceDetails = (HtmlResourceDetails)builder.Build();

        // Assert
        htmlResourceDetails.Body.Should().Be("<html><head><title>Invalid HTML");
        htmlResourceDetails.HtmlDocument.Should().NotBeNull();
        htmlResourceDetails.HtmlDocument!.Title.Should().Be("Invalid HTML");
    }

    [Fact]
    public void Build_should_handle_redirect_location()
    {
        // Arrange
        Uri url = new("https://example.com");
        HttpResponseMessage response = CreateHttpResponseMessage(
            HttpStatusCode.Redirect,
            new Dictionary<string, string> { { "Location", "/redirect" } },
            null
        );
        TimeSpan responseTime = TimeSpan.FromMilliseconds(150);

        HtmlResourceDetailsBuilder builder = new(url, response, responseTime);

        // Act
        HtmlResourceDetails htmlResourceDetails = (HtmlResourceDetails)builder.Build();

        // Assert
        htmlResourceDetails.RedirectLocation.Should().Be(new Uri("https://example.com/redirect"));
    }

    [Fact]
    public void Build_should_handle_missing_cookies()
    {
        // Arrange
        Uri url = new("https://example.com");
        HttpResponseMessage response = CreateHttpResponseMessage(
            HttpStatusCode.OK,
            [], // No Set-Cookie header
            "<!DOCTYPE html><html><head><title>Example</title></head><body></body></html>"
        );
        TimeSpan responseTime = TimeSpan.FromMilliseconds(150);

        HtmlResourceDetailsBuilder builder = new(url, response, responseTime);

        // Act
        HtmlResourceDetails htmlResourceDetails = (HtmlResourceDetails)builder.Build();

        // Assert
        htmlResourceDetails.Cookies.Should().BeEmpty();
    }

    private static HttpResponseMessage CreateHttpResponseMessage(
        HttpStatusCode statusCode,
        Dictionary<string, string> headers,
        string? content)
    {
        HttpResponseMessage response = new(statusCode)
        {
            Content = content != null ? new StringContent(content) : null
        };

        foreach (KeyValuePair<string, string> header in headers)
        {
            if (header.Key.Equals("Content-Type", StringComparison.OrdinalIgnoreCase))
            {
                response.Content!.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(header.Value);
            }
            else if (header.Key.Equals("Set-Cookie", StringComparison.OrdinalIgnoreCase))
            {
                response.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }
            else if (header.Key.Equals("Location", StringComparison.OrdinalIgnoreCase))
            {
                response.Headers.Location = new Uri(header.Value, UriKind.RelativeOrAbsolute);
            }
            else
            {
                response.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }
        }

        return response;
    }
}
