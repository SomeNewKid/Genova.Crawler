// This file is part of the Genova project licensed under the GNU General Public License v3.0.
// See the LICENSE file in the project root for more information.

using System.Collections.Concurrent;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using Genova.Common.Attributes;
using Genova.Crawler.Resources;

namespace Genova.Crawler;

/// <summary>
/// Provides functionality to crawl a website and discover its resources.
/// </summary>
[CodeQuality(Public = true, Justification = "This class is intended for use by the Scanner.")]
public sealed class Crawler : ICrawler
{
    private const char Slash = '/';
    private readonly HttpClient _followingClient;
    private readonly HttpClient _nonFollowingClient;
    private readonly CrawlOptions _crawlOptions;
    private readonly ConcurrentDictionary<Uri, bool> _processedUrls = new();
    private readonly string _host;

    /// <summary>
    /// Initializes a new instance of the <see cref="Crawler"/> class.
    /// </summary>
    /// <param name="followingClient">The <see cref="HttpClient"/> used to make HTTP requests during crawling,
    /// following any redirections.</param>
    /// <param name="nonFollowingClient">The <see cref="HttpClient"/> used to make HTTP requests during crawling,
    /// not following any redirections.</param>
    /// <param name="crawlOptions">The <see cref="CrawlOptions"/> used during crawling.</param>
    public Crawler(HttpClient followingClient, HttpClient nonFollowingClient, CrawlOptions crawlOptions)
    {
        _followingClient = followingClient;
        _nonFollowingClient = nonFollowingClient;
        _crawlOptions = crawlOptions;
        _host = new Uri(crawlOptions.BaseUrl).Host;
    }

    /// <inheritdoc/>
    public async IAsyncEnumerable<ResourceDetails> DiscoverResourcesAsync(
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        IEnumerable<string> startingPaths = GetStartingPaths(_crawlOptions);

        foreach (string startingPath in startingPaths)
        {
            string startingUrl = _crawlOptions.BaseUrl.Trim(Slash) + Slash + startingPath.Trim(Slash);
            Uri startingUri = new(startingUrl);

            await foreach (ResourceDetails resource in CrawlAsync(startingUri, cancellationToken))
            {
                yield return resource;
            }
        }
    }

    /// <inheritdoc/>
    public async Task<ResourceDetails> FetchResourceAsync(
        Uri url,
        HttpMethod httpMethod,
        bool followRedirects = false,
        FetchParameters? parameters = null,
        FetchOptions? fetchOptions = null,
        CancellationToken cancellationToken = default)
    {
        // Pause between requests if specified in CrawlOptions
        await PauseIfNeededAsync(cancellationToken);

        using HttpRequestMessage request = new(httpMethod, url);

        // Handle form data or body
        PrepareRequestContent(request, parameters);

        // Add headers to the request
        AddHeadersToRequest(request, parameters?.Headers);

        HttpClient client = followRedirects ? _followingClient : _nonFollowingClient;

        // Temporarily set the Host header if specified in FetchOptions
        string? originalHost = ConfigureHostHeader(client, fetchOptions);

        // Use the client to send the request and get the response
        using HttpResponseMessage response = await client.SendAsync(request, cancellationToken);

        // Create and return the ResourceDetails object
        ResourceDetails resourceDetails = ResourceDetailsFactory.Create(url, response);

        // Reset the Host header to its original value
        ResetHostHeader(client, fetchOptions, originalHost);

        return resourceDetails;
    }

    /// <summary>
    /// Gets the starting paths for crawling based on the provided <see cref="CrawlOptions"/>.
    /// </summary>
    /// <param name="crawlOptions">The <see cref="CrawlOptions"/> used during crawling.</param>
    /// <returns>The collection of starting paths, with a minimum of the home page.</returns>
    internal static IEnumerable<string> GetStartingPaths(CrawlOptions crawlOptions)
    {
        // Filter out empty or whitespace-only paths and trim valid paths
        IEnumerable<string> filteredPaths = crawlOptions.StartingPaths
            .Where(path => !string.IsNullOrWhiteSpace(path))
            .Select(path => path.Trim());

        // If no valid paths remain, default to the root path "/"
        return filteredPaths.Any() ? filteredPaths : ["/"];
    }

    /// <summary>
    /// Determines whether a resource should yield a <see cref="ResourceDetails"/> object based on its
    /// filename extension and a set of rules.
    /// </summary>
    /// <param name="extension">The file extension of the resource.</param>
    /// <param name="filenameExtensionRules">The list of filename extension rules.</param>
    /// <returns><c>true</c> if the resource should yield details, otherwise <c>false</c>.</returns>
    internal static bool ShouldYieldResource(string extension, List<string> filenameExtensionRules)
    {
        if (string.IsNullOrEmpty(extension))
        {
            extension = ".html"; // Default to .html for resources without an extension
        }

        // Normalize the extension to treat .htm and .html as interchangeable
        extension = NormalizeExtension(extension);

        foreach (string rule in filenameExtensionRules)
        {
            // Normalize the rule to treat .htm and .html as interchangeable
            string normalizedRule = NormalizeExtension(rule);

            if (normalizedRule == "*")
            {
                return true; // Allow anything else
            }

            if (normalizedRule.StartsWith('!'))
            {
                string excludedExtension = normalizedRule.Substring(1).ToLowerInvariant();
                if (extension == excludedExtension)
                {
                    return false; // Exclude this extension
                }
            }
            else
            {
                string includedExtension = normalizedRule.ToLowerInvariant();
                if (extension == includedExtension)
                {
                    return true; // Include this extension
                }
            }
        }

        return false; // Default to not yielding if no rules match
    }

    private static void AddHeadersToRequest(HttpRequestMessage request, Dictionary<string, string>? headers)
    {
        if (headers == null)
        {
            return;
        }

        foreach (KeyValuePair<string, string> header in headers)
        {
            if (header.Key.Equals("Content-Type", StringComparison.OrdinalIgnoreCase) && request.Content != null)
            {
                request.Content.Headers.ContentType = new MediaTypeHeaderValue(header.Value);
            }
            else
            {
                request.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }
        }
    }

    private static string? ConfigureHostHeader(HttpClient client, FetchOptions? fetchOptions)
    {
        if (fetchOptions?.DefaultRequestHeadersHost == null)
        {
            return null;
        }

        string? originalHost = client.DefaultRequestHeaders.Host;
        client.DefaultRequestHeaders.Host = fetchOptions.DefaultRequestHeadersHost;
        return originalHost;
    }

    private static string NormalizeExtension(string extension)
    {
        if (extension.Equals(".htm", StringComparison.OrdinalIgnoreCase))
        {
            return ".html";
        }

        return extension.ToLowerInvariant();
    }

    private static void PrepareRequestContent(HttpRequestMessage request, FetchParameters? parameters)
    {
        if (parameters?.Form != null && parameters.Form.Count != 0)
        {
            string formData = string.Join("&", parameters.Form.Select(kvp =>
                $"{Uri.EscapeDataString(kvp.Key)}={Uri.EscapeDataString(kvp.Value)}"));
            request.Content = new StringContent(formData);

            parameters.Headers ??= [];

            if (!parameters.Headers.ContainsKey("Content-Type"))
            {
                parameters.Headers["Content-Type"] = "application/x-www-form-urlencoded";
            }
        }
        else if (parameters?.Body != null)
        {
            request.Content = new StringContent(parameters.Body);
        }
    }

    private static void ResetHostHeader(HttpClient client, FetchOptions? fetchOptions, string? originalHost)
    {
        if (fetchOptions?.DefaultRequestHeadersHost != null)
        {
            client.DefaultRequestHeaders.Host = originalHost;
        }
    }

    private async IAsyncEnumerable<ResourceDetails> CrawlAsync(
        Uri url,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        if (!_processedUrls.TryAdd(url, true))
        {
            yield break; // Skip already processed URLs
        }

        // Check if the URL matches the filename extension rules
        if (!ShouldYieldResource(url))
        {
            yield break;
        }

        // Fetch the resource using FetchResourceAsync
        ResourceDetails resource = await FetchResourceAsync(
            url,
            HttpMethod.Get,
            followRedirects: true,
            cancellationToken: cancellationToken);
        yield return resource;

        // Recursively process linked resources
        IEnumerable<Uri> linkedUrls = ResourceLinksFactory.ExtractLinks(resource);
        foreach (Uri linkedUrl in linkedUrls)
        {
            await foreach (ResourceDetails linkedResource in CrawlAsync(linkedUrl, cancellationToken))
            {
                yield return linkedResource;
            }
        }
    }

    private async Task PauseIfNeededAsync(CancellationToken cancellationToken)
    {
        if (_crawlOptions.PauseBetweenRequests > 0)
        {
            await Task.Delay(_crawlOptions.PauseBetweenRequests, cancellationToken);
        }
    }

    private bool ShouldYieldResource(Uri url)
    {
        if (!url.Host.Equals(_host, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        string extension = Path.GetExtension(url.AbsolutePath).ToLowerInvariant();
        return ShouldYieldResource(extension, _crawlOptions.FilenameExtensionRules);
    }
}
