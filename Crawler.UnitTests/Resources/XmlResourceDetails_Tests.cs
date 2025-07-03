// This file is part of the Genova project licensed under the GNU General Public License v3.0.
// See the LICENSE file in the project root for more information.

using System.Net;
using System.Xml;
using FluentAssertions;
using Genova.Crawler.Resources;

namespace Genova.Crawler.UnitTests.Resources;

public class XmlResourceDetails_Tests
{
    [Fact]
    public void Constructor_should_initialize_properties_correctly()
    {
        // Arrange
        ResourceDetailsParameters parameters = new()
        {
            Url = new Uri("https://example.com"),
            Headers = new Dictionary<string, string> { { "Content-Type", "application/xml" } },
            Cookies = ["SessionId=abc123"],
            StatusCode = HttpStatusCode.OK,
            ContentLength = 2048,
            ContentType = "application/xml",
            RedirectLocation = new Uri("https://example.com/redirect"),
            ResponseTime = TimeSpan.FromMilliseconds(200)
        };
        string? body = """
            <root>
                <child>Example Content</child>
            </root>
            """;

        // Create a real XmlDocument by parsing the body content
        XmlDocument xmlDocument = new();
        xmlDocument.LoadXml(body);

        // Act
        XmlResourceDetails xmlResourceDetails = new(parameters, body, xmlDocument);

        // Assert
        xmlResourceDetails.Url.Should().Be(parameters.Url);
        xmlResourceDetails.Headers.Should().BeEquivalentTo(parameters.Headers);
        xmlResourceDetails.Cookies.Should().BeEquivalentTo(parameters.Cookies);
        xmlResourceDetails.StatusCode.Should().Be(parameters.StatusCode);
        xmlResourceDetails.ContentLength.Should().Be(parameters.ContentLength);
        xmlResourceDetails.ContentType.Should().Be(parameters.ContentType);
        xmlResourceDetails.RedirectLocation.Should().Be(parameters.RedirectLocation);
        xmlResourceDetails.ResponseTime.Should().Be(parameters.ResponseTime);
        xmlResourceDetails.Body.Should().Be(body);
        xmlResourceDetails.XmlDocument.Should().NotBeNull();
        xmlResourceDetails.XmlDocument.DocumentElement!.Name.Should().Be("root");
    }

    [Fact]
    public void Constructor_should_initialize_properties_with_null_XmlDocument()
    {
        // Arrange
        ResourceDetailsParameters parameters = new()
        {
            Url = new Uri("https://example.com"),
            Headers = new Dictionary<string, string> { { "Content-Type", "application/xml" } },
            Cookies = ["SessionId=abc123"],
            StatusCode = HttpStatusCode.OK,
            ContentLength = 2048,
            ContentType = "application/xml",
            RedirectLocation = new Uri("https://example.com/redirect"),
            ResponseTime = TimeSpan.FromMilliseconds(200)
        };
        string? body = """
            <root>
                <child>Example Content</child>
            </root>
            """;
        XmlDocument? xmlDocument = null;

        // Act
        XmlResourceDetails xmlResourceDetails = new(parameters, body, xmlDocument);

        // Assert
        xmlResourceDetails.Url.Should().Be(parameters.Url);
        xmlResourceDetails.Headers.Should().BeEquivalentTo(parameters.Headers);
        xmlResourceDetails.Cookies.Should().BeEquivalentTo(parameters.Cookies);
        xmlResourceDetails.StatusCode.Should().Be(parameters.StatusCode);
        xmlResourceDetails.ContentLength.Should().Be(parameters.ContentLength);
        xmlResourceDetails.ContentType.Should().Be(parameters.ContentType);
        xmlResourceDetails.RedirectLocation.Should().Be(parameters.RedirectLocation);
        xmlResourceDetails.ResponseTime.Should().Be(parameters.ResponseTime);
        xmlResourceDetails.Body.Should().Be(body);
        xmlResourceDetails.XmlDocument.Should().BeNull();
    }

    [Fact]
    public void Constructor_should_throw_ArgumentNullException_when_parameters_is_null()
    {
        // Arrange
        string? body = """
            <root>
                <child>Example Content</child>
            </root>
            """;
        XmlDocument? xmlDocument = null;

        // Act
        Action act = () => _ = new XmlResourceDetails(null!, body, xmlDocument);

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
            Headers = new Dictionary<string, string> { { "Content-Type", "application/xml" } },
            Cookies = ["SessionId=abc123"],
            StatusCode = HttpStatusCode.OK,
            ContentLength = -1, // Invalid value
            ContentType = "application/xml",
            RedirectLocation = new Uri("https://example.com/redirect"),
            ResponseTime = TimeSpan.FromMilliseconds(200)
        };
        string? body = """
            <root>
                <child>Example Content</child>
            </root>
            """;
        XmlDocument? xmlDocument = null;

        // Act
        Action act = () => _ = new XmlResourceDetails(parameters, body, xmlDocument);

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
            Headers = new Dictionary<string, string> { { "Content-Type", "application/xml" } },
            Cookies = ["SessionId=abc123"],
            StatusCode = HttpStatusCode.OK,
            ContentLength = 2048,
            ContentType = "application/xml",
            RedirectLocation = new Uri("https://example.com/redirect"),
            ResponseTime = TimeSpan.FromMilliseconds(-1) // Invalid value
        };
        string? body = """
            <root>
                <child>Example Content</child>
            </root>
            """;
        XmlDocument? xmlDocument = null;

        // Act
        Action act = () => _ = new XmlResourceDetails(parameters, body, xmlDocument);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>().WithParameterName("parameters.ResponseTime");
    }
}
