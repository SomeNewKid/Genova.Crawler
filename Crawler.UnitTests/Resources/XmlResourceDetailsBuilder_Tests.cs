// This file is part of the Genova project licensed under the GNU General Public License v3.0.
// See the LICENSE file in the project root for more information.

using System.Net;
using System.Net.Http.Headers;
using FluentAssertions;
using Genova.Crawler.Resources;

namespace Genova.Crawler.UnitTests.Resources;

public class XmlResourceDetailsBuilder_Tests
{
    [Fact]
    public void Build_should_initialize_XmlResourceDetails_correctly()
    {
        // Arrange
        Uri url = new("https://example.com");
        HttpResponseMessage response = CreateHttpResponseMessage(
            HttpStatusCode.OK,
            new Dictionary<string, string>
            {
                { "Content-Type", "application/xml" },
                { "Set-Cookie", "SessionId=abc123" }
            },
            """
            <root>
                <child>Example Content</child>
            </root>
            """
        );
        TimeSpan responseTime = TimeSpan.FromMilliseconds(150);

        XmlResourceDetailsBuilder builder = new(url, response, responseTime);

        // Act
        XmlResourceDetails xmlResourceDetails = (XmlResourceDetails)builder.Build();

        // Assert
        xmlResourceDetails.Url.Should().Be(url);
        xmlResourceDetails.Cookies.Should().ContainSingle().Which.Should().Be("SessionId=abc123");
        xmlResourceDetails.StatusCode.Should().Be(HttpStatusCode.OK);
        xmlResourceDetails.ContentType.Should().Be("application/xml");
        xmlResourceDetails.RedirectLocation.Should().BeNull();
        xmlResourceDetails.ResponseTime.Should().Be(responseTime);
        xmlResourceDetails.Body.Should().Be("""
            <root>
                <child>Example Content</child>
            </root>
            """);
        xmlResourceDetails.XmlDocument.Should().NotBeNull();
        xmlResourceDetails.XmlDocument!.DocumentElement!.Name.Should().Be("root");
    }

    [Fact]
    public void Build_should_handle_null_body_content()
    {
        // Arrange
        Uri url = new("https://example.com");
        HttpResponseMessage response = CreateHttpResponseMessage(
            HttpStatusCode.OK,
            new Dictionary<string, string> { { "Content-Type", "application/xml" } },
            null // No content
        );
        TimeSpan responseTime = TimeSpan.FromMilliseconds(150);

        XmlResourceDetailsBuilder builder = new(url, response, responseTime);

        // Act
        XmlResourceDetails xmlResourceDetails = (XmlResourceDetails)builder.Build();

        // Assert
        xmlResourceDetails.Body.Should().BeEmpty();
        xmlResourceDetails.XmlDocument.Should().BeNull();
    }

    [Fact]
    public void Build_should_handle_invalid_xml_body()
    {
        // Arrange
        Uri url = new("https://example.com");
        HttpResponseMessage response = CreateHttpResponseMessage(
            HttpStatusCode.OK,
            new Dictionary<string, string> { { "Content-Type", "application/xml" } },
            "<root><child>Invalid XML"
        );
        TimeSpan responseTime = TimeSpan.FromMilliseconds(150);

        XmlResourceDetailsBuilder builder = new(url, response, responseTime);

        // Act
        XmlResourceDetails xmlResourceDetails = (XmlResourceDetails)builder.Build();

        // Assert
        xmlResourceDetails.Body.Should().Be("<root><child>Invalid XML");
        xmlResourceDetails.XmlDocument.Should().BeNull();
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

        XmlResourceDetailsBuilder builder = new(url, response, responseTime);

        // Act
        XmlResourceDetails xmlResourceDetails = (XmlResourceDetails)builder.Build();

        // Assert
        xmlResourceDetails.RedirectLocation.Should().Be(new Uri("https://example.com/redirect"));
    }

    [Fact]
    public void Build_should_handle_missing_cookies()
    {
        // Arrange
        Uri url = new("https://example.com");
        HttpResponseMessage response = CreateHttpResponseMessage(
            HttpStatusCode.OK,
            [], // No Set-Cookie header
            """
            <root>
                <child>Example Content</child>
            </root>
            """
        );
        TimeSpan responseTime = TimeSpan.FromMilliseconds(150);

        XmlResourceDetailsBuilder builder = new(url, response, responseTime);

        // Act
        XmlResourceDetails xmlResourceDetails = (XmlResourceDetails)builder.Build();

        // Assert
        xmlResourceDetails.Cookies.Should().BeEmpty();
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
