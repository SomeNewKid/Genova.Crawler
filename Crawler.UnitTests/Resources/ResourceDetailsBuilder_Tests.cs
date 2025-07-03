// This file is part of the Genova project licensed under the GNU General Public License v3.0.
// See the LICENSE file in the project root for more information.

using System.Net;
using System.Net.Http.Headers;
using FluentAssertions;
using Genova.Crawler.Resources;

namespace Genova.Crawler.UnitTests.Resources;

public class ResourceDetailsBuilder_Tests
{
    [Fact]
    public void Build_should_initialize_ResourceDetails_correctly()
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
            "Response body content"
        );
        TimeSpan responseTime = TimeSpan.FromMilliseconds(150);

        ResourceDetailsBuilder builder = new(url, response, responseTime);

        // Act
        ResourceDetails resourceDetails = builder.Build();

        // Assert
        resourceDetails.Url.Should().Be(url);
        resourceDetails.Cookies.Should().ContainSingle().Which.Should().Be("SessionId=abc123");
        resourceDetails.StatusCode.Should().Be(HttpStatusCode.OK);
        resourceDetails.ContentLength.Should().Be(21); // Length of "Response body content"
        resourceDetails.ContentType.Should().Be("text/html");
        resourceDetails.RedirectLocation.Should().BeNull();
        resourceDetails.ResponseTime.Should().Be(responseTime);
    }

    [Fact]
    public void Build_should_handle_missing_content_length()
    {
        // Arrange
        Uri url = new("https://example.com");
        HttpResponseMessage response = CreateHttpResponseMessage(
            HttpStatusCode.OK,
            new Dictionary<string, string> { { "Content-Type", "text/html" } },
            null // No content
        );
        TimeSpan responseTime = TimeSpan.FromMilliseconds(150);

        ResourceDetailsBuilder builder = new(url, response, responseTime);

        // Act
        ResourceDetails resourceDetails = builder.Build();

        // Assert
        resourceDetails.ContentLength.Should().Be(0);
    }

    [Fact]
    public void Build_should_handle_missing_content_type()
    {
        // Arrange
        Uri url = new("https://example.com");
        HttpResponseMessage response = CreateHttpResponseMessage(
            HttpStatusCode.OK,
            [], // No Content-Type header
            "Response body content"
        );
        TimeSpan responseTime = TimeSpan.FromMilliseconds(150);

        ResourceDetailsBuilder builder = new(url, response, responseTime);

        // Act
        ResourceDetails resourceDetails = builder.Build();

        // Assert
        Assert.Equal("text/plain", resourceDetails.ContentType); // default value
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

        ResourceDetailsBuilder builder = new(url, response, responseTime);

        // Act
        ResourceDetails resourceDetails = builder.Build();

        // Assert
        resourceDetails.RedirectLocation.Should().Be(new Uri("https://example.com/redirect"));
    }

    [Fact]
    public void Build_should_handle_missing_cookies()
    {
        // Arrange
        Uri url = new("https://example.com");
        HttpResponseMessage response = CreateHttpResponseMessage(
            HttpStatusCode.OK,
            [], // No Set-Cookie header
            "Response body content"
        );
        TimeSpan responseTime = TimeSpan.FromMilliseconds(150);

        ResourceDetailsBuilder builder = new(url, response, responseTime);

        // Act
        ResourceDetails resourceDetails = builder.Build();

        // Assert
        resourceDetails.Cookies.Should().BeEmpty();
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
                response.Content!.Headers.ContentType = new MediaTypeHeaderValue(header.Value);
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
