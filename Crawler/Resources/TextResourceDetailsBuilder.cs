// This file is part of the Genova project licensed under the GNU General Public License v3.0.
// See the LICENSE file in the project root for more information.

using Genova.Common.Attributes;

namespace Genova.Crawler.Resources;

/// <summary>
/// Provides functionality to build <see cref="TextResourceDetails"/> instances.
/// </summary>
[CodeQuality(Unsealed = true, Justification = "Intended for extension by more specific builders.")]
internal class TextResourceDetailsBuilder : ResourceDetailsBuilder
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TextResourceDetailsBuilder"/> class.
    /// </summary>
    /// <param name="url">The URL of the resource.</param>
    /// <param name="response">The HTTP response when requesting the resource.</param>
    /// <param name="responseTime">The time taken to receive the response.</param>
    internal TextResourceDetailsBuilder(Uri url, HttpResponseMessage response, TimeSpan responseTime)
        : base(url, response, responseTime)
    {
    }

    /// <summary>
    /// Builds a <see cref="TextResourceDetails"/> instance.
    /// </summary>
    /// <returns>A <see cref="TextResourceDetails"/> instance.</returns>
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

        // Extract the body content specific to TextResourceDetails
        string? body = ExtractBodyContent();

        // Return a new TextResourceDetails instance
        return new TextResourceDetails(parameters, body);
    }

    /// <summary>
    /// Extracts the body content from the HTTP response.
    /// </summary>
    /// <returns>The body content as a string, or <c>null</c> if the response has no content.</returns>
    protected string? ExtractBodyContent()
    {
        return Response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
    }
}
