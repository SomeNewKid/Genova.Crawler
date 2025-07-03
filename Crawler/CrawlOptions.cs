// This file is part of the Genova project licensed under the GNU General Public License v3.0.
// See the LICENSE file in the project root for more information.

using Genova.Common.Attributes;
using Genova.Crawler.Resources;

namespace Genova.Crawler;

/// <summary>
/// Represents the options for crawling a website.
/// </summary>
[CodeQuality(Public = true, Justification = "This class is intended for used by the Scanner.")]
public sealed class CrawlOptions
{
    /// <summary>
    /// Gets or sets the base URL of the website to crawl.
    /// </summary>
    public string BaseUrl { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the milliseconds to wait between requests.
    /// </summary>
    /// <remarks>
    /// This pause is used to avoid overwhelming the server with requests, and potentiatlly triggering a
    /// Web Application Firewall.
    /// </remarks>
    public int PauseBetweenRequests { get; set; } = 0;

    /// <summary>
    /// Gets or sets the paths at which to start the crawling.
    /// </summary>
    /// <remarks>
    /// If no starting paths are provided, the home page will be used to start the crawling.
    /// </remarks>
    public IEnumerable<string> StartingPaths { get; set; } = [];

    /// <summary>
    /// Gets or sets the list of filename extensions that determine whether a resource will yield
    /// a <see cref="ResourceDetails"/> object.
    /// </summary>
    /// <remarks>
    /// The list is evaluated in order. A leading '!' indicates an exclusion,
    /// while the absence of '!' indicates an inclusion.
    /// The '*' character means "anything else".
    /// </remarks>
    public List<string> FilenameExtensionRules { get; set; } = ["*"];
}
