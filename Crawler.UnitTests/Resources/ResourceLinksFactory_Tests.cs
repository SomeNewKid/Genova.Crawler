// This file is part of the Genova project licensed under the GNU General Public License v3.0.
// See the LICENSE file in the project root for more information.

using System.Xml;
using FluentAssertions;
using Genova.Crawler.Resources;

namespace Genova.Crawler.UnitTests.Resources;

public class ResourceLinksFactory_Tests
{
    [Fact]
    public void ExtractLinks_should_return_links_for_HtmlResourceDetails()
    {
        // Arrange
        ResourceDetailsParameters parameters = new()
        {
            Url = new Uri("https://example.com"),
            Headers = new Dictionary<string, string> { { "Content-Type", "text/html" } },
            Cookies = [],
            StatusCode = System.Net.HttpStatusCode.OK,
            ContentLength = 1024,
            ContentType = "text/html",
            RedirectLocation = null,
            ResponseTime = TimeSpan.FromMilliseconds(150)
        };

        string body = """
            <html>
                <body>
                    <a href="https://example.com/page1">Page 1</a>
                    <a href="/page2">Page 2</a>
                </body>
            </html>
            """;

        AngleSharp.Html.Parser.HtmlParser parser = new();
        AngleSharp.Dom.IDocument htmlDocument = parser.ParseDocument(body);

        HtmlResourceDetails htmlResource = new(parameters, body, htmlDocument);

        // Act
        IEnumerable<Uri> links = ResourceLinksFactory.ExtractLinks(htmlResource);

        // Assert
        links.Should().BeEquivalentTo(new[]
        {
            new Uri("https://example.com/page1"),
            new Uri("https://example.com/page2")
        });
    }

    [Fact]
    public void ExtractLinks_should_return_links_for_XmlResourceDetails()
    {
        // Arrange
        ResourceDetailsParameters parameters = new()
        {
            Url = new Uri("https://example.com"),
            Headers = new Dictionary<string, string> { { "Content-Type", "application/xml" } },
            Cookies = [],
            StatusCode = System.Net.HttpStatusCode.OK,
            ContentLength = 1024,
            ContentType = "application/xml",
            RedirectLocation = null,
            ResponseTime = TimeSpan.FromMilliseconds(150)
        };

        string body = """
            <urlset xmlns="http://www.sitemaps.org/schemas/sitemap/0.9">
                <url>
                    <loc>https://example.com/page1</loc>
                </url>
                <url>
                    <loc>/page2</loc>
                </url>
            </urlset>
            """;

        XmlDocument xmlDocument = new();
        xmlDocument.LoadXml(body);

        XmlResourceDetails xmlResource = new(parameters, body, xmlDocument);

        // Act
        IEnumerable<Uri> links = ResourceLinksFactory.ExtractLinks(xmlResource);

        // Assert
        links.Should().BeEquivalentTo(new[]
        {
            new Uri("https://example.com/page1"),
            new Uri("https://example.com/page2")
        });
    }

    [Fact]
    public void ExtractLinks_should_return_empty_for_unsupported_resource_type()
    {
        // Arrange
        ResourceDetailsParameters parameters = new()
        {
            Url = new Uri("https://example.com"),
            Headers = [],
            Cookies = [],
            StatusCode = System.Net.HttpStatusCode.OK,
            ContentLength = 0,
            ContentType = "application/json",
            RedirectLocation = null,
            ResponseTime = TimeSpan.FromMilliseconds(150)
        };

        TextResourceDetails unsupportedResource = new(parameters, null);

        // Act
        IEnumerable<Uri> links = ResourceLinksFactory.ExtractLinks(unsupportedResource);

        // Assert
        links.Should().BeEmpty();
    }
}
