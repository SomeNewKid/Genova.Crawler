// This file is part of the Genova project licensed under the GNU General Public License v3.0.
// See the LICENSE file in the project root for more information.

using AngleSharp.Dom;
using Genova.Common.Utilities;

namespace Genova.Crawler.Resources;

/// <summary>
/// Provides functionality to build <see cref="HtmlResourceDetails"/> instances.
/// </summary>
internal sealed class HtmlResourceDetailsBuilder : TextResourceDetailsBuilder
{
    /// <summary>
    /// Initializes a new instance of the <see cref="HtmlResourceDetailsBuilder"/> class.
    /// </summary>
    /// <param name="url">The URL of the resource.</param>
    /// <param name="response">The HTTP response when requesting the resource.</param>
    /// <param name="responseTime">The time taken to receive the response.</param>
    internal HtmlResourceDetailsBuilder(Uri url, HttpResponseMessage response, TimeSpan responseTime)
        : base(url, response, responseTime)
    {
    }

    /// <summary>
    /// Builds an <see cref="HtmlResourceDetails"/> instance.
    /// </summary>
    /// <returns>An <see cref="HtmlResourceDetails"/> instance.</returns>
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

        // Parse the HTML document from the body content
        IDocument? htmlDocument = HtmlHelper.ParseHtmlDocument(body);

        // Return a new HtmlResourceDetails instance
        return new HtmlResourceDetails(parameters, body, htmlDocument);
    }
}
