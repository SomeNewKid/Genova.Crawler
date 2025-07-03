// This file is part of the Genova project licensed under the GNU General Public License v3.0.
// See the LICENSE file in the project root for more information.

using System.Net;

namespace Genova.Crawler.Resources;

/// <summary>
/// Encapsulates the parameters required to initialize a <see cref="ResourceDetails"/> instance.
/// </summary>
internal sealed class ResourceDetailsParameters
{
    /// <summary>
    /// Gets or sets the URL of the resource.
    /// </summary>
    public Uri Url { get; set; } = null!;

    /// <summary>
    /// Gets or sets the headers associated with the resource.
    /// </summary>
    public Dictionary<string, string> Headers { get; set; } = [];

    /// <summary>
    /// Gets or sets the cookies associated with the resource.
    /// </summary>
    public List<string> Cookies { get; set; } = [];

    /// <summary>
    /// Gets or sets the HTTP status code of the response.
    /// </summary>
    public HttpStatusCode StatusCode { get; set; }

    /// <summary>
    /// Gets or sets the length of the response content.
    /// </summary>
    public long ContentLength { get; set; }

    /// <summary>
    /// Gets or sets the content type of the response.
    /// </summary>
    public string? ContentType { get; set; }

    /// <summary>
    /// Gets or sets the redirect location, if applicable.
    /// </summary>
    public Uri? RedirectLocation { get; set; }

    /// <summary>
    /// Gets or sets the time taken to receive the response.
    /// </summary>
    public TimeSpan ResponseTime { get; set; }
}
