// This file is part of the Genova project licensed under the GNU General Public License v3.0.
// See the LICENSE file in the project root for more information.

using System.Xml;

namespace Genova.Crawler.Resources;

/// <summary>
/// Extracts links from an <see cref="XmlResourceDetails"/> instance.
/// </summary>
internal static class XmlResourceLinksExtractor
{
    /// <summary>
    /// Extracts links from the given <see cref="XmlResourceDetails"/> instance.
    /// </summary>
    /// <param name="xmlResource">The XML resource from which to extract links.</param>
    /// <returns>An <see cref="IEnumerable{Uri}"/> collection of resource links, or an empty collection if the XML is not a sitemap.</returns>
    internal static IEnumerable<Uri> ExtractLinks(XmlResourceDetails xmlResource)
    {
        if (xmlResource.XmlDocument == null)
        {
            throw new InvalidOperationException("XML document cannot be null.");
        }

        // Check if the XML document is a sitemap
        if (!IsSitemap(xmlResource.XmlDocument))
        {
            return Array.Empty<Uri>();
        }

        // Extract <loc> elements from the sitemap
        return ExtractSitemapLinks(xmlResource.XmlDocument, xmlResource.Url);
    }

    /// <summary>
    /// Determines whether the given XML document is a sitemap.
    /// </summary>
    /// <param name="xmlDocument">The XML document to evaluate.</param>
    /// <returns><c>true</c> if the document is a sitemap; otherwise, <c>false</c>.</returns>
    internal static bool IsSitemap(XmlDocument xmlDocument)
    {
        XmlElement? root = xmlDocument.DocumentElement;
        return root != null &&
               (root.Name.Equals("urlset", StringComparison.OrdinalIgnoreCase) ||
                root.Name.Equals("sitemapindex", StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Extracts &lt;loc&gt; elements from a sitemap XML document.
    /// </summary>
    /// <param name="xmlDocument">The XML document to process.</param>
    /// <param name="baseUri">The base URI to resolve relative links.</param>
    /// <returns>An <see cref="IEnumerable{Uri}"/> collection of resource links.</returns>
    private static IEnumerable<Uri> ExtractSitemapLinks(XmlDocument xmlDocument, Uri baseUri)
    {
        XmlNodeList? locNodes = xmlDocument.GetElementsByTagName("loc");
        foreach (XmlNode locNode in locNodes)
        {
            if (!string.IsNullOrWhiteSpace(locNode.InnerText))
            {
                string link = locNode.InnerText.Trim();
                if (Uri.TryCreate(baseUri, link, out Uri? resolvedUri))
                {
                    yield return resolvedUri;
                }
            }
        }
    }
}
