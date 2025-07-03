// This file is part of the Genova project licensed under the GNU General Public License v3.0.
// See the LICENSE file in the project root for more information.

using System.Diagnostics;
using System.Net;
using FluentAssertions;
using Genova.Crawler.Resources;

namespace Genova.Crawler.UnitTests.Resources;

public class ResourceDetailsFactory_Tests
{
    [Fact]
    public void Create_should_return_HtmlResourceDetails_for_html_content_type()
    {
        // Arrange
        Uri url = new("https://example.com");
        HttpResponseMessage response = CreateHttpResponseMessage(
            HttpStatusCode.OK,
            "text/html",
            "<html><body>Sample HTML</body></html>"
        );

        // Act
        ResourceDetails result = ResourceDetailsFactory.Create(url, response);

        // Assert
        result.Should().BeOfType<HtmlResourceDetails>();
    }

    [Fact]
    public void Create_should_return_XmlResourceDetails_for_xml_content_type()
    {
        // Arrange
        Uri url = new("https://example.com");
        HttpResponseMessage response = CreateHttpResponseMessage(
            HttpStatusCode.OK,
            "application/xml",
            "<root><child>Sample XML</child></root>"
        );

        // Act
        ResourceDetails result = ResourceDetailsFactory.Create(url, response);

        // Assert
        result.Should().BeOfType<XmlResourceDetails>();
    }

    [Fact]
    public void Create_should_return_TextResourceDetails_for_text_content_type()
    {
        // Arrange
        Uri url = new("https://example.com");
        HttpResponseMessage response = CreateHttpResponseMessage(
            HttpStatusCode.OK,
            "text/plain",
            "Sample plain text"
        );

        // Act
        ResourceDetails result = ResourceDetailsFactory.Create(url, response);

        // Assert
        result.Should().BeOfType<TextResourceDetails>();
    }

    [Fact]
    public void Create_should_return_ResourceDetails_for_unknown_content_type()
    {
        // Arrange
        Uri url = new("https://example.com");
        HttpResponseMessage response = CreateHttpResponseMessage(
            HttpStatusCode.OK,
            "application/octet-stream",
            "Binary content"
        );

        // Act
        ResourceDetails result = ResourceDetailsFactory.Create(url, response);

        // Assert
        result.Should().BeOfType<ResourceDetails>();
    }

    [Fact]
    public void Create_should_handle_null_content_type()
    {
        // Arrange
        Uri url = new("https://example.com");
        HttpResponseMessage response = CreateHttpResponseMessage(
            HttpStatusCode.OK,
            null,
            "Sample content"
        );

        // Act
        ResourceDetails result = ResourceDetailsFactory.Create(url, response);

        // Assert
        result.Should().BeOfType<TextResourceDetails>(); // default conent type is "text/plain"
    }

    [Fact]
    public void Create_should_measure_response_time_if_available()
    {
        // Arrange
        Uri url = new("https://example.com");
        HttpResponseMessage response = CreateHttpResponseMessage(
            HttpStatusCode.OK,
            "text/html",
            "<html><body>Sample HTML</body></html>"
        );

        // Simulate response time
        Stopwatch stopwatch = Stopwatch.StartNew();
        response.RequestMessage = new HttpRequestMessage();
        response.RequestMessage.Options.Set(new HttpRequestOptionsKey<Stopwatch>("ResponseTime"), stopwatch);
        stopwatch.Stop();

        // Act
        ResourceDetails result = ResourceDetailsFactory.Create(url, response);

        // Assert
        result.ResponseTime.Should().Be(stopwatch.Elapsed);
    }

    private static HttpResponseMessage CreateHttpResponseMessage(
        HttpStatusCode statusCode,
        string? contentType,
        string content)
    {
        HttpResponseMessage response = new(statusCode)
        {
            Content = new StringContent(content)
        };

        if (!string.IsNullOrWhiteSpace(contentType))
        {
            response.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(contentType);
        }

        return response;
    }
}
