// This file is part of the Genova project licensed under the GNU General Public License v3.0.
// See the LICENSE file in the project root for more information.

namespace Genova.Crawler.Resources;

/// <summary>
/// Provides functionality to extract links from a <see cref="ResourceDetails"/> instance.
/// </summary>
internal static class ResourceLinksFactory
{
    /// <summary>
    /// Extracts links from the given <see cref="ResourceDetails"/> instance.
    /// </summary>
    /// <param name="resource">The resource from which to extract links.</param>
    /// <returns>An <see cref="IEnumerable{Uri}"/> collection of links.</returns>
    internal static IEnumerable<Uri> ExtractLinks(ResourceDetails resource)
    {
        if (resource is HtmlResourceDetails htmlResource)
        {
            return HtmlResourceLinksExtractor.ExtractLinks(htmlResource);
        }

        if (resource is XmlResourceDetails xmlResource)
        {
            return XmlResourceLinksExtractor.ExtractLinks(xmlResource);
        }

        return Array.Empty<Uri>();
    }
}
