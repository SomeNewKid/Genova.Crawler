// This file is part of the Genova project licensed under the GNU General Public License v3.0.
// See the LICENSE file in the project root for more information.

using Genova.Common.Attributes;

namespace Genova.Crawler.Resources;

/// <summary>
/// Provides functionality to build <see cref="CompressedResourceDetails"/> instances.
/// </summary>
[CodeQuality(Unsealed = true, Justification = "Intended for extension by more specific builders.")]
internal class CompressedResourceDetailsBuilder : ResourceDetailsBuilder
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CompressedResourceDetailsBuilder"/> class.
    /// </summary>
    /// <param name="url">The URL of the resource.</param>
    /// <param name="response">The HTTP response when requesting the resource.</param>
    /// <param name="responseTime">The time taken to receive the response.</param>
    internal CompressedResourceDetailsBuilder(Uri url, HttpResponseMessage response, TimeSpan responseTime)
        : base(url, response, responseTime)
    {
    }

    /// <summary>
    /// Builds a <see cref="CompressedResourceDetails"/> instance.
    /// </summary>
    /// <returns>A <see cref="CompressedResourceDetails"/> instance.</returns>
    internal override ResourceDetails Build()
    {
        // Create the parameters object for the base ResourceDetails
        ResourceDetailsParameters parameters = new()
        {
            Url = Url,
            Headers = ExtractHeaders(),
            Cookies = ExtractCookies(),
            StatusCode = Response.StatusCode,
            ContentLength = ExtractContentLength(),
            ContentType = ExtractContentType(),
            RedirectLocation = ExtractRedirectLocation(),
            ResponseTime = ResponseTime,
        };

        string? encoding = ResourceDetailsBuilder.GetResponseEncoding(Response);

        return new CompressedResourceDetails(parameters, encoding);
    }
}
