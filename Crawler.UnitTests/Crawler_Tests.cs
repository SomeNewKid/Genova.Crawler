// This file is part of the Genova project licensed under the GNU General Public License v3.0.
// See the LICENSE file in the project root for more information.

using FluentAssertions;

namespace Genova.Crawler.UnitTests;

/// <summary>
/// Unit tests for the <see cref="Crawler"/> class.
/// </summary>
public class Crawler_Tests
{
    [Theory]
    [InlineData("http://example.com/file", true)]
    [InlineData("http://example.com/file.htm", true)]
    [InlineData("http://example.com/file.html", true)]
    [InlineData("http://example.com/file.xml", true)]
    [InlineData("http://example.com/file.gif", false)]
    [InlineData("http://example.com/file.jpg", false)]
    [InlineData("http://example.com/file.txt", false)]
    public void ShouldYieldResource_should_respect_whitelist_rules_using_htm_rule(string url, bool expected)
    {
        // Arrange
        List<string> filenameExtensionRules = [".htm", ".xml", "!*"];
        string extension = Path.GetExtension(new Uri(url).AbsolutePath).ToLowerInvariant();

        // Act
        bool result = Crawler.ShouldYieldResource(extension, filenameExtensionRules);

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("http://example.com/file", true)]
    [InlineData("http://example.com/file.htm", true)]
    [InlineData("http://example.com/file.html", true)]
    [InlineData("http://example.com/file.xml", true)]
    [InlineData("http://example.com/file.gif", false)]
    [InlineData("http://example.com/file.jpg", false)]
    [InlineData("http://example.com/file.txt", false)]
    public void ShouldYieldResource_should_respect_whitelist_rules_using_html_rule(string url, bool expected)
    {
        // Arrange
        List<string> filenameExtensionRules = [".html", ".xml", "!*"];
        string extension = Path.GetExtension(new Uri(url).AbsolutePath).ToLowerInvariant();

        // Act
        bool result = Crawler.ShouldYieldResource(extension, filenameExtensionRules);

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("http://example.com/file", true)]
    [InlineData("http://example.com/file.htm", true)]
    [InlineData("http://example.com/file.html", true)]
    [InlineData("http://example.com/file.xml", true)]
    [InlineData("http://example.com/file.gif", false)]
    [InlineData("http://example.com/file.jpg", false)]
    [InlineData("http://example.com/file.txt", true)]
    public void ShouldYieldResource_should_respect_blacklist_rules(string url, bool expected)
    {
        // Arrange
        List<string> filenameExtensionRules = ["!.gif", "!.jpg", "*"];
        string extension = Path.GetExtension(new Uri(url).AbsolutePath).ToLowerInvariant();

        // Act
        bool result = Crawler.ShouldYieldResource(extension, filenameExtensionRules);

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("http://example.com/file.html", false)]
    [InlineData("http://example.com/file.xml", false)]
    [InlineData("http://example.com/file.gif", false)]
    [InlineData("http://example.com/file.jpg", false)]
    [InlineData("http://example.com/file.txt", false)]
    public void ShouldYieldResource_should_return_false_when_no_rules_match(string url, bool expected)
    {
        // Arrange
        List<string> filenameExtensionRules = [];
        string extension = Path.GetExtension(new Uri(url).AbsolutePath).ToLowerInvariant();

        // Act
        bool result = Crawler.ShouldYieldResource(extension, filenameExtensionRules);

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("http://example.com/file.html", true)]
    [InlineData("http://example.com/file.xml", false)]
    [InlineData("http://example.com/file.gif", false)]
    [InlineData("http://example.com/file.jpg", false)]
    [InlineData("http://example.com/file.txt", false)]
    public void ShouldYieldResource_should_respect_specific_inclusion_rules(string url, bool expected)
    {
        // Arrange
        List<string> filenameExtensionRules = [".html"];
        string extension = Path.GetExtension(new Uri(url).AbsolutePath).ToLowerInvariant();

        // Act
        bool result = Crawler.ShouldYieldResource(extension, filenameExtensionRules);

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public async Task FetchResourceAsync_should_add_headers_to_request()
    {
        // Arrange
        HttpClient followingClient = new();
        HttpClient nonFollowingClient = new();
        CrawlOptions crawlOptions = new()
        {
            BaseUrl = "https://example.com",
            PauseBetweenRequests = 0,
            StartingPaths = [],
            FilenameExtensionRules = [".html"]
        };
        Crawler crawler = new(followingClient, nonFollowingClient, crawlOptions);

        Uri url = new("https://example.com/resource");
        HttpMethod httpMethod = HttpMethod.Get;
        Dictionary<string, string> headers = new()
        {
            { "Authorization", "Bearer token" },
            { "Custom-Header", "CustomValue" }
        };

        // Act
        Func<Task> act = async () =>
        {
            await crawler.FetchResourceAsync(
                url,
                httpMethod,
                followRedirects: false,
                new FetchParameters
                {
                    Headers = headers
                },
                cancellationToken: CancellationToken.None);
        };

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task FetchResourceAsync_should_set_request_content_when_body_is_provided()
    {
        // Arrange
        HttpClient followingClient = new();
        HttpClient nonFollowingClient = new();
        CrawlOptions crawlOptions = new()
        {
            BaseUrl = "https://example.com",
            PauseBetweenRequests = 0,
            StartingPaths = [],
            FilenameExtensionRules = [".html"]
        };
        Crawler crawler = new(followingClient, nonFollowingClient, crawlOptions);

        Uri url = new("https://example.com/resource");
        HttpMethod httpMethod = HttpMethod.Post;
        string body = "{ \"key\": \"value\" }";

        // Act
        Func<Task> act = async () =>
        {
            await crawler.FetchResourceAsync(
                url,
                httpMethod,
                followRedirects: false,
                new FetchParameters
                {
                    Body = body
                },
                cancellationToken: CancellationToken.None);
        };

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public void GetStartingPaths_should_return_root_path_when_StartingPaths_is_empty()
    {
        // Arrange
        CrawlOptions crawlOptions = new()
        {
            StartingPaths = []
        };

        // Act
        IEnumerable<string> startingPaths = Crawler.GetStartingPaths(crawlOptions);

        // Assert
        startingPaths.Should().ContainSingle()
            .Which.Should().Be("/");
    }

    [Fact]
    public void GetStartingPaths_should_return_root_path_when_StartingPaths_has_a_single_empty_path()
    {
        // Arrange
        CrawlOptions crawlOptions = new()
        {
            StartingPaths = [""]
        };

        // Act
        IEnumerable<string> startingPaths = Crawler.GetStartingPaths(crawlOptions);

        // Assert
        startingPaths.Should().ContainSingle()
            .Which.Should().Be("/");
    }

    [Fact]
    public void GetStartingPaths_should_return_single_path_when_StartingPaths_contains_one_path()
    {
        // Arrange
        CrawlOptions crawlOptions = new()
        {
            StartingPaths = new List<string> { "/about" }
        };

        // Act
        IEnumerable<string> startingPaths = Crawler.GetStartingPaths(crawlOptions);

        // Assert
        startingPaths.Should().ContainSingle()
            .Which.Should().Be("/about");
    }

    [Fact]
    public void GetStartingPaths_should_return_multiple_paths_when_StartingPaths_contains_multiple_paths()
    {
        // Arrange
        CrawlOptions crawlOptions = new()
        {
            StartingPaths = new List<string> { "/about", "/contact", "/products" }
        };

        // Act
        IEnumerable<string> startingPaths = Crawler.GetStartingPaths(crawlOptions);

        // Assert
        startingPaths.Should().BeEquivalentTo(["/about", "/contact", "/products"]);
    }

    [Fact]
    public void GetStartingPaths_should_trim_whitespace_from_paths()
    {
        // Arrange
        CrawlOptions crawlOptions = new()
        {
            StartingPaths = new List<string> { " /about ", " /contact ", " /products " }
        };

        // Act
        IEnumerable<string> startingPaths = Crawler.GetStartingPaths(crawlOptions);

        // Assert
        startingPaths.Should().BeEquivalentTo(["/about", "/contact", "/products"]);
    }
}
