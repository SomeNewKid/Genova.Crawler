// This file is part of the Genova project licensed under the GNU General Public License v3.0.
// See the LICENSE file in the project root for more information.

using System.Net;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;
using FluentAssertions;
using Genova.Crawler.Resources;

namespace Genova.Crawler.UnitTests.Resources;

/// <summary>
/// Unit tests for the <see cref="HtmlResourceLinksExtractor"/> class.
/// </summary>
public class HtmlResourceLinksExtractor_Tests
{
    [Theory]
    [InlineData("http://example.com/resource")]
    [InlineData("/relative/path")]
    [InlineData("https://example.com/resource")]
    [InlineData("ftp://example.com/resource")]
    public void IsResourceLink_should_return_true_for_valid_links(string link)
    {
        // Act
        bool result = HtmlResourceLinksExtractor.IsResourceLink(link);

        // Assert
        Assert.True(result, $"Expected true for link: {link}");
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData("#fragment")]
    [InlineData("tel:+123456789")]
    [InlineData("mailto:example@example.com")]
    [InlineData("javascript:void(0)")]
    [InlineData("data:image/png;base64,...")]
    [InlineData("http://")]
    public void IsResourceLink_should_return_false_for_invalid_links(string link)
    {
        // Act
        bool result = HtmlResourceLinksExtractor.IsResourceLink(link);

        // Assert
        Assert.False(result, $"Expected false for link: {link}");
    }

    [Fact]
    public void ExtractLinks_should_throw_InvalidOperationException_when_HtmlDocument_is_null()
    {
        // Arrange
        HtmlResourceDetails htmlResource = CreateHtmlResourceDetails(null, null);

        // Act
        Action act = () => HtmlResourceLinksExtractor.ExtractLinks(htmlResource);

        // Assert
        act.Should().Throw<InvalidOperationException>().WithMessage("HTML document cannot be null.");
    }

    [Fact]
    public void ExtractLinks_should_return_links_from_meta_refresh_elements()
    {
        // Arrange
        string body = """
            <html>
                <head>
                    <meta http-equiv="refresh" content="0;url=http://example.com/redirect1" />
                    <meta http-equiv="refresh" content="5;url=/relative/redirect2" />
                    <meta name="description" content="This is a test page." />
                </head>
            </html>
            """;
        HtmlParser parser = new();
        IHtmlDocument document = parser.ParseDocument(body);
        HtmlResourceDetails htmlResource = CreateHtmlResourceDetails(body, document);

        // Act
        IEnumerable<Uri> links = HtmlResourceLinksExtractor.ExtractLinks(htmlResource);

        // Assert
        links.Should().BeEquivalentTo(new[]
        {
            new Uri("http://example.com/redirect1"),
            new Uri("http://example.com/relative/redirect2")
        });
    }

    [Fact]
    public void ExtractLinks_should_return_links_from_form_elements()
    {
        // Arrange
        string body = """
            <html>
                <body>
                    <form action="http://example.com/form1"></form>
                    <form action="/relative/form2"></form>
                    <form action="#fragment"></form>
                </body>
            </html>
            """;
        HtmlParser parser = new();
        IHtmlDocument document = parser.ParseDocument(body);
        HtmlResourceDetails htmlResource = CreateHtmlResourceDetails(body, document);

        // Act
        IEnumerable<Uri> links = HtmlResourceLinksExtractor.ExtractLinks(htmlResource);

        // Assert
        links.Should().BeEquivalentTo(new[]
        {
            new Uri("http://example.com/form1"),
            new Uri("http://example.com/relative/form2")
        });
    }

    [Fact]
    public void ExtractLinks_should_return_links_from_button_elements()
    {
        // Arrange
        string body = """
            <html>
                <body>
                    <button formaction="http://example.com/action1">Submit</button>
                    <button formaction="/relative/action2">Submit</button>
                    <button formaction="#fragment">Submit</button>
                </body>
            </html>
            """;
        HtmlParser parser = new();
        IHtmlDocument document = parser.ParseDocument(body);
        HtmlResourceDetails htmlResource = CreateHtmlResourceDetails(body, document);

        // Act
        IEnumerable<Uri> links = HtmlResourceLinksExtractor.ExtractLinks(htmlResource);

        // Assert
        links.Should().BeEquivalentTo(new[]
        {
            new Uri("http://example.com/action1"),
            new Uri("http://example.com/relative/action2")
        });
    }

    [Fact]
    public void ExtractLinks_should_return_links_from_source_elements()
    {
        // Arrange
        string body = """
            <html>
                <body>
                    <video>
                        <source src="http://example.com/video1.mp4" />
                        <source src="/relative/video2.mp4" />
                        <source src="data:video/mp4;base64,..." />
                    </video>
                    <picture>
                        <source srcset="http://example.com/image1.jpg" />
                        <source srcset="/relative/image2.jpg" />
                        <source srcset="data:image/jpeg;base64,..." />
                    </picture>
                </body>
            </html>
            """;
        HtmlParser parser = new();
        IHtmlDocument document = parser.ParseDocument(body);
        HtmlResourceDetails htmlResource = CreateHtmlResourceDetails(body, document);

        // Act
        IEnumerable<Uri> links = HtmlResourceLinksExtractor.ExtractLinks(htmlResource);

        // Assert
        links.Should().BeEquivalentTo(new[]
        {
            new Uri("http://example.com/video1.mp4"),
            new Uri("http://example.com/relative/video2.mp4"),
            new Uri("http://example.com/image1.jpg"),
            new Uri("http://example.com/relative/image2.jpg")
        });
    }

    [Fact]
    public void ExtractLinks_should_return_links_from_object_elements()
    {
        // Arrange
        string body = """
            <html>
                <body>
                    <object data="http://example.com/object1.pdf"></object>
                    <object data="/relative/object2.swf"></object>
                    <object data="data:application/pdf;base64,..."></object>
                </body>
            </html>
            """;
        HtmlParser parser = new();
        IHtmlDocument document = parser.ParseDocument(body);
        HtmlResourceDetails htmlResource = CreateHtmlResourceDetails(body, document);

        // Act
        IEnumerable<Uri> links = HtmlResourceLinksExtractor.ExtractLinks(htmlResource);

        // Assert
        links.Should().BeEquivalentTo(new[]
        {
            new Uri("http://example.com/object1.pdf"),
            new Uri("http://example.com/relative/object2.swf")
        });
    }

    private static HtmlResourceDetails CreateHtmlResourceDetails(string? body, IHtmlDocument? htmlDocument)
    {
        ResourceDetailsParameters parameters = new()
        {
            Url = new Uri("http://example.com"),
            Headers = new Dictionary<string, string> { { "Content-Type", "text/html" } },
            Cookies = ["SessionId=abc123"],
            StatusCode = HttpStatusCode.OK,
            ContentLength = 1024,
            ContentType = "text/html",
            RedirectLocation = new Uri("http://example.com/redirect"),
            ResponseTime = TimeSpan.FromMilliseconds(150)
        };

        return new HtmlResourceDetails(parameters, body, htmlDocument);
    }
    [Fact]
    public void ExtractLinksFromElement_should_return_empty_when_element_is_null()
    {
        // Act
        IEnumerable<string> result = HtmlResourceLinksExtractor.ExtractLinksFromElement(null!);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void ExtractLinksFromElement_should_exclude_base_elements()
    {
        // Arrange
        string body = """
            <html>
                <head>
                    <base href="http://example.com/" />
                </head>
            </html>
            """;
        IElement? baseElement = ParseHtml(body).QuerySelector("base");
        Assert.NotNull(baseElement);

        // Act
        IEnumerable<string> result = HtmlResourceLinksExtractor.ExtractLinksFromElement(baseElement);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void ExtractLinksFromElement_should_return_links_from_specific_elements()
    {
        // Arrange
        string body = """
            <html>
                <body>
                    <form action="http://example.com/form"></form>
                    <button formaction="/relative/action"></button>
                    <source srcset="http://example.com/image.jpg" />
                    <object data="/relative/object"></object>
                    <meta http-equiv="refresh" content="0;url=http://example.com/redirect" />
                </body>
            </html>
            """;
        IElement bodyElement = ParseHtml(body).Body!;

        // Act
        IEnumerable<string> result = HtmlResourceLinksExtractor.ExtractLinksFromElement(bodyElement);

        // Assert
        result.Should().BeEquivalentTo(
        [
            "http://example.com/form",
            "/relative/action",
            "http://example.com/image.jpg",
            "/relative/object",
            "http://example.com/redirect",
        ]);
    }

    [Fact]
    public void ExtractLinksFromElement_should_return_links_from_generic_href_and_src_attributes()
    {
        // Arrange
        string body = """
            <html>
                <body>
                    <a href="http://example.com/link"></a>
                    <img src="/relative/image.jpg" />
                </body>
            </html>
            """;
        IElement bodyElement = ParseHtml(body).Body!;

        // Act
        IEnumerable<string> result = HtmlResourceLinksExtractor.ExtractLinksFromElement(bodyElement);

        // Assert
        result.Should().BeEquivalentTo(
        [
            "http://example.com/link",
            "/relative/image.jpg",
        ]);
    }

    [Fact]
    public void ExtractLinksFromElement_should_return_links_from_child_elements()
    {
        // Arrange
        string body = """
            <html>
                <body>
                    <div>
                        <a href="http://example.com/link1"></a>
                        <img src="/relative/image1.jpg" />
                        <div>
                            <a href="http://example.com/link2"></a>
                            <img src="/relative/image2.jpg" />
                        </div>
                    </div>
                </body>
            </html>
            """;
        IElement bodyElement = ParseHtml(body).Body!;

        // Act
        IEnumerable<string> result = HtmlResourceLinksExtractor.ExtractLinksFromElement(bodyElement);

        // Assert
        result.Should().BeEquivalentTo(
        [
            "http://example.com/link1",
            "/relative/image1.jpg",
            "http://example.com/link2",
            "/relative/image2.jpg",
        ]);
    }

    private static IHtmlDocument ParseHtml(string html)
    {
        HtmlParser parser = new();
        return parser.ParseDocument(html);
    }
}
