// This file is part of the Genova project licensed under the GNU General Public License v3.0.
// See the LICENSE file in the project root for more information.

using System.Net;
using System.Xml;
using FluentAssertions;
using Genova.Crawler.Resources;

namespace Genova.Crawler.UnitTests.Resources;

/// <summary>
/// Unit tests for the <see cref="XmlResourceLinksExtractor"/> class.
/// </summary>
public class XmlResourceLinksExtractor_Tests
{
    [Fact]
    public void ExtractLinks_should_return_links_from_valid_sitemap()
    {
        // Arrange
        string body = """
            <urlset xmlns="http://www.sitemaps.org/schemas/sitemap/0.9">
                <url>
                    <loc>http://example.com/page1</loc>
                </url>
                <url>
                    <loc>http://example.com/page2</loc>
                </url>
            </urlset>
            """;
        XmlDocument xmlDocument = new();
        xmlDocument.LoadXml(body);
        XmlResourceDetails xmlResource = CreateXmlResourceDetails(body, xmlDocument);

        // Act
        IEnumerable<Uri> links = XmlResourceLinksExtractor.ExtractLinks(xmlResource);

        // Assert
        links.Should().BeEquivalentTo(new[]
        {
            new Uri("http://example.com/page1"),
            new Uri("http://example.com/page2")
        });
    }

    [Fact]
    public void ExtractLinks_should_return_links_from_sitemap_index()
    {
        // Arrange
        string body = """
            <sitemapindex xmlns="http://www.sitemaps.org/schemas/sitemap/0.9">
                <sitemap>
                    <loc>http://example.com/sitemap1.xml</loc>
                </sitemap>
                <sitemap>
                    <loc>http://example.com/sitemap2.xml</loc>
                </sitemap>
            </sitemapindex>
            """;
        XmlDocument xmlDocument = new();
        xmlDocument.LoadXml(body);
        XmlResourceDetails xmlResource = CreateXmlResourceDetails(body, xmlDocument);

        // Act
        IEnumerable<Uri> links = XmlResourceLinksExtractor.ExtractLinks(xmlResource);

        // Assert
        links.Should().BeEquivalentTo(new[]
        {
            new Uri("http://example.com/sitemap1.xml"),
            new Uri("http://example.com/sitemap2.xml")
        });
    }

    [Fact]
    public void ExtractLinks_should_return_empty_for_non_sitemap_xml()
    {
        // Arrange
        string body = """
            <root>
                <item>http://example.com/page1</item>
                <item>http://example.com/page2</item>
            </root>
            """;
        XmlDocument xmlDocument = new();
        xmlDocument.LoadXml(body);
        XmlResourceDetails xmlResource = CreateXmlResourceDetails(body, xmlDocument);

        // Act
        IEnumerable<Uri> links = XmlResourceLinksExtractor.ExtractLinks(xmlResource);

        // Assert
        links.Should().BeEmpty();
    }

    [Fact]
    public void ExtractLinks_should_return_empty_for_null_document()
    {
        // Arrange
        XmlResourceDetails xmlResource = CreateXmlResourceDetails(null, null);

        // Act
        Action act = () => XmlResourceLinksExtractor.ExtractLinks(xmlResource);

        // Assert
        act.Should().Throw<InvalidOperationException>().WithMessage("XML document cannot be null.");
    }

    [Fact]
    public void ExtractLinks_should_ignore_empty_loc_elements()
    {
        // Arrange
        string body = """
            <urlset xmlns="http://www.sitemaps.org/schemas/sitemap/0.9">
                <url>
                    <loc>http://example.com/page1</loc>
                </url>
                <url>
                    <loc></loc>
                </url>
                <url>
                    <loc>   </loc>
                </url>
            </urlset>
            """;
        XmlDocument xmlDocument = new();
        xmlDocument.LoadXml(body);
        XmlResourceDetails xmlResource = CreateXmlResourceDetails(body, xmlDocument);

        // Act
        IEnumerable<Uri> links = XmlResourceLinksExtractor.ExtractLinks(xmlResource);

        // Assert
        links.Should().BeEquivalentTo(new[]
        {
            new Uri("http://example.com/page1")
        });
    }

    [Fact]
    public void ExtractLinks_should_resolve_relative_links()
    {
        // Arrange
        string body = """
            <urlset xmlns="http://www.sitemaps.org/schemas/sitemap/0.9">
                <url>
                    <loc>/page1</loc>
                </url>
                <url>
                    <loc>/page2</loc>
                </url>
            </urlset>
            """;
        XmlDocument xmlDocument = new();
        xmlDocument.LoadXml(body);
        XmlResourceDetails xmlResource = CreateXmlResourceDetails(body, xmlDocument);

        // Act
        IEnumerable<Uri> links = XmlResourceLinksExtractor.ExtractLinks(xmlResource);

        // Assert
        links.Should().BeEquivalentTo(new[]
        {
            new Uri("http://example.com/page1"),
            new Uri("http://example.com/page2")
        });
    }


    [Fact]
    public void IsSitemap_should_return_true_for_urlset_root()
    {
        // Arrange
        string body = """
            <urlset xmlns="http://www.sitemaps.org/schemas/sitemap/0.9">
                <url>
                    <loc>http://example.com/page1</loc>
                </url>
            </urlset>
            """;
        XmlDocument xmlDocument = new();
        xmlDocument.LoadXml(body);

        // Act
        bool result = XmlResourceLinksExtractor.IsSitemap(xmlDocument);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsSitemap_should_return_true_for_sitemapindex_root()
    {
        // Arrange
        string body = """
            <sitemapindex xmlns="http://www.sitemaps.org/schemas/sitemap/0.9">
                <sitemap>
                    <loc>http://example.com/sitemap1.xml</loc>
                </sitemap>
            </sitemapindex>
            """;
        XmlDocument xmlDocument = new();
        xmlDocument.LoadXml(body);

        // Act
        bool result = XmlResourceLinksExtractor.IsSitemap(xmlDocument);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsSitemap_should_return_false_for_non_sitemap_root()
    {
        // Arrange
        string body = """
            <root>
                <item>http://example.com/page1</item>
            </root>
            """;
        XmlDocument xmlDocument = new();
        xmlDocument.LoadXml(body);

        // Act
        bool result = XmlResourceLinksExtractor.IsSitemap(xmlDocument);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsSitemap_should_return_false_when_DocumentElement_is_null()
    {
        // Arrange
        XmlDocument xmlDocument = new(); // Empty XmlDocument with no content

        // Act
        bool result = XmlResourceLinksExtractor.IsSitemap(xmlDocument);

        // Assert
        result.Should().BeFalse();
    }

    private static XmlResourceDetails CreateXmlResourceDetails(string? body, XmlDocument? xmlDocument)
    {
        ResourceDetailsParameters parameters = new()
        {
            Url = new Uri("http://example.com"),
            Headers = new Dictionary<string, string> { { "Content-Type", "application/xml" } },
            Cookies = ["SessionId=abc123"],
            StatusCode = HttpStatusCode.OK,
            ContentLength = 2048,
            ContentType = "application/xml",
            RedirectLocation = new Uri("http://example.com/redirect"),
            ResponseTime = TimeSpan.FromMilliseconds(200)
        };

        return new XmlResourceDetails(parameters, body, xmlDocument);
    }
}
