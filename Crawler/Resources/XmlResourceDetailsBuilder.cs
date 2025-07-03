// This file is part of the Genova project licensed under the GNU General Public License v3.0.
// See the LICENSE file in the project root for more information.

using System.Xml;
using Genova.Common.Utilities;

namespace Genova.Crawler.Resources;

/// <summary>
/// Provides functionality to build <see cref="XmlResourceDetails"/> instances.
/// </summary>
internal sealed class XmlResourceDetailsBuilder : TextResourceDetailsBuilder
{
    /// <summary>
    /// Initializes a new instance of the <see cref="XmlResourceDetailsBuilder"/> class.
    /// </summary>
    /// <param name="url">The URL of the resource.</param>
    /// <param name="response">The HTTP response when requesting the resource.</param>
    /// <param name="responseTime">The time taken to receive the response.</param>
    internal XmlResourceDetailsBuilder(Uri url, HttpResponseMessage response, TimeSpan responseTime)
        : base(url, response, responseTime)
    {
    }

    /// <summary>
    /// Builds an <see cref="XmlResourceDetails"/> instance.
    /// </summary>
    /// <returns>An <see cref="XmlResourceDetails"/> instance.</returns>
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

        // Parse the XML document from the body content
        XmlDocument? xmlDocument = XmlHelper.ParseXmlDocument(body);

        // Return a new XmlResourceDetails instance
        return new XmlResourceDetails(parameters, body, xmlDocument);
    }
}
