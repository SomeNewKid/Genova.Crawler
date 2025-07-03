// This file is part of the Genova project licensed under the GNU General Public License v3.0.
// See the LICENSE file in the project root for more information.

using Genova.Crawler.Resources;

namespace Genova.Crawler;

/// <summary>
/// Defines the contract for a crawler that discovers and fetches resources from a website.
/// </summary>
public interface ICrawler
{
    /// <summary>
    /// Asynchronously discovers resources from the specified base URL.
    /// </summary>
    /// <param name="cancellationToken">
    /// A <see cref="CancellationToken"/> that can be used to cancel the operation.
    /// </param>
    /// <returns>
    /// An asynchronous stream of <see cref="ResourceDetails"/> objects representing the discovered resources.
    /// </returns>
    IAsyncEnumerable<ResourceDetails> DiscoverResourcesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Fetches the resource details for a custom HTTP request.
    /// </summary>
    /// <param name="url">The URL of the resource to fetch.</param>
    /// <param name="httpMethod">The HTTP method to use (e.g., GET, POST, PUT, DELETE).</param>
    /// <param name="followRedirects">Indicates whether to follow redirects.</param>
    /// <param name="parameters">Optional parameters for the request, including headers, body, and form data.</param>
    /// <param name="fetchOptions">Options for fetching a resource.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to cancel the operation.</param>
    /// <returns>A <see cref="ResourceDetails"/> object representing the fetched resource.</returns>
    Task<ResourceDetails> FetchResourceAsync(
        Uri url,
        HttpMethod httpMethod,
        bool followRedirects = false,
        FetchParameters? parameters = null,
        FetchOptions? fetchOptions = null,
        CancellationToken cancellationToken = default);
}
