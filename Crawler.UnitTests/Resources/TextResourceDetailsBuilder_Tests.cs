// This file is part of the Genova project licensed under the GNU General Public License v3.0.
// See the LICENSE file in the project root for more information.

using System.Net;
using FluentAssertions;
using Genova.Crawler.Resources;

namespace Genova.Crawler.UnitTests.Resources;

public class TextResourceDetailsBuilder_Tests
{
    [Fact]
    public void Build_should_initialize_TextResourceDetails_correctly()
    {
        // Arrange
        Uri url = new("https://example.com");
        HttpResponseMessage response = CreateHttpResponseMessage(
            HttpStatusCode.OK,
            new Dictionary<string, string>
            {
                { "Content-Type", "text/plain" },
                { "Set-Cookie", "SessionId=abc123" }
            },
            "Sample text content"
        );
        TimeSpan responseTime = TimeSpan.FromMilliseconds(150);

        TextResourceDetailsBuilder builder = new(url, response, responseTime);

        // Act
        TextResourceDetails textResourceDetails = (TextResourceDetails)builder.Build();

        // Assert
        textResourceDetails.Url.Should().Be(url);
        textResourceDetails.Cookies.Should().ContainSingle().Which.Should().Be("SessionId=abc123");
        textResourceDetails.StatusCode.Should().Be(HttpStatusCode.OK);
        textResourceDetails.ContentLength.Should().Be(19); // Length of "Sample text content"
        textResourceDetails.ContentType.Should().Be("text/plain");
        textResourceDetails.RedirectLocation.Should().BeNull();
        textResourceDetails.ResponseTime.Should().Be(responseTime);
        textResourceDetails.Body.Should().Be("Sample text content");
    }

    [Fact]
    public void Build_should_handle_null_body_content()
    {
        // Arrange
        Uri url = new("https://example.com");
        HttpResponseMessage response = CreateHttpResponseMessage(
            HttpStatusCode.OK,
            new Dictionary<string, string> { { "Content-Type", "text/plain" } },
            null // No content
        );
        TimeSpan responseTime = TimeSpan.FromMilliseconds(150);

        TextResourceDetailsBuilder builder = new(url, response, responseTime);

        // Act
        TextResourceDetails textResourceDetails = (TextResourceDetails)builder.Build();

        // Assert
        Assert.NotNull(textResourceDetails.Body);
        Assert.Empty(textResourceDetails.Body);
    }

    [Fact]
    public void Build_should_handle_missing_content_type()
    {
        // Arrange
        Uri url = new("https://example.com");
        HttpResponseMessage response = CreateHttpResponseMessage(
            HttpStatusCode.OK,
            [], // No Content-Type header
            "Sample text content"
        );
        TimeSpan responseTime = TimeSpan.FromMilliseconds(150);

        TextResourceDetailsBuilder builder = new(url, response, responseTime);

        // Act
        TextResourceDetails textResourceDetails = (TextResourceDetails)builder.Build();

        // Assert
        Assert.Equal("text/plain", textResourceDetails.ContentType); // default value
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

        TextResourceDetailsBuilder builder = new(url, response, responseTime);

        // Act
        TextResourceDetails textResourceDetails = (TextResourceDetails)builder.Build();

        // Assert
        textResourceDetails.RedirectLocation.Should().Be(new Uri("https://example.com/redirect"));
    }

    [Fact]
    public void Build_should_handle_missing_cookies()
    {
        // Arrange
        Uri url = new("https://example.com");
        HttpResponseMessage response = CreateHttpResponseMessage(
            HttpStatusCode.OK,
            [], // No Set-Cookie header
            "Sample text content"
        );
        TimeSpan responseTime = TimeSpan.FromMilliseconds(150);

        TextResourceDetailsBuilder builder = new(url, response, responseTime);

        // Act
        TextResourceDetails textResourceDetails = (TextResourceDetails)builder.Build();

        // Assert
        textResourceDetails.Cookies.Should().BeEmpty();
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
