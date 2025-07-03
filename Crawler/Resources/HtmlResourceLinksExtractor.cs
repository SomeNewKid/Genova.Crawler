// This file is part of the Genova project licensed under the GNU General Public License v3.0.
// See the LICENSE file in the project root for more information.

using AngleSharp.Dom;

namespace Genova.Crawler.Resources;

/// <summary>
/// Extracts links to resources from an <see cref="HtmlResourceDetails"/> instance.
/// </summary>
internal static class HtmlResourceLinksExtractor
{
    /// <summary>
    /// Extracts all resource links from the given <see cref="HtmlResourceDetails"/> instance.
    /// </summary>
    /// <param name="htmlResource">The HTML resource from which to extract links.</param>
    /// <returns>An <see cref="IEnumerable{Uri}"/> collection of resource links.</returns>
    internal static IEnumerable<Uri> ExtractLinks(HtmlResourceDetails htmlResource)
    {
        if (htmlResource.HtmlDocument == null)
        {
            throw new InvalidOperationException("HTML document cannot be null.");
        }

        Uri baseUri = htmlResource.Url;

        // Recursively traverse the DOM and extract resource links
        IEnumerable<string> rawLinks = ExtractLinksFromElement(htmlResource.HtmlDocument.DocumentElement);

        // Filter and resolve links to absolute URIs
        IEnumerable<Uri> resourceLinks = rawLinks
            .Where(IsResourceLink) // Filter out non-resource links
            .Select(href => new Uri(baseUri, href)) // Resolve relative URLs
            .Distinct();

        return resourceLinks;
    }

    /// <summary>
    /// Determines whether a given link is a valid resource link.
    /// </summary>
    /// <param name="link">The link to evaluate.</param>
    /// <returns><c>true</c> if the link is a valid resource link; otherwise, <c>false</c>.</returns>
    internal static bool IsResourceLink(string link)
    {
        if (string.IsNullOrWhiteSpace(link))
        {
            return false;
        }

        // Exclude links with unsupported schemes or fragments
        if (link.StartsWith('#'))
        {
            return false;
        }

        // Exclude links with a colon but no protocol marker ("://")
        if (link.Contains(':') && !link.Contains("://"))
        {
            return false;
        }

        // Include links with supported schemes (e.g., http, https, ftp)
        if (Uri.TryCreate(link, UriKind.RelativeOrAbsolute, out Uri? uri))
        {
            return !uri.IsAbsoluteUri ||
                    uri.Scheme == Uri.UriSchemeHttp ||
                    uri.Scheme == Uri.UriSchemeHttps ||
                    uri.Scheme == Uri.UriSchemeFtp; // Allow relative URIs
        }

        return false;
    }

    /// <summary>
    /// Recursively extracts all `href` and `src` attribute values from the given DOM element and its children.
    /// </summary>
    /// <param name="element">The DOM element to traverse.</param>
    /// <returns>An <see cref="IEnumerable{String}"/> collection of raw link values.</returns>
    internal static IEnumerable<string> ExtractLinksFromElement(IElement element)
    {
        if (element == null)
        {
            yield break;
        }

        if (ShouldExcludeElement(element))
        {
            yield break;
        }

        foreach (string link in ExtractAttributes(element, ["action", "formaction", "srcset", "data"]))
        {
            yield return link;
        }

        foreach (string link in ExtractMetaRefreshLinks(element))
        {
            yield return link;
        }

        foreach (string link in ExtractAttributes(element, ["href", "src"]))
        {
            yield return link;
        }

        foreach (string link in ExtractChildLinks(element))
        {
            yield return link;
        }
    }

    /// <summary>
    /// Determines whether the element should be excluded from link extraction.
    /// </summary>
    /// <param name="element">The DOM element to evaluate.</param>
    /// <returns><c>true</c> if the element should be excluded; otherwise, <c>false</c>.</returns>
    private static bool ShouldExcludeElement(IElement element)
    {
        return element.TagName.Equals("base", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Extracts attributes from an element based on the provided attribute names.
    /// </summary>
    /// <param name="element">The DOM element to evaluate.</param>
    /// <param name="attributeNames">The attribute names to extract.</param>
    /// <returns>An <see cref="IEnumerable{String}"/> collection of attribute values.</returns>
    private static IEnumerable<string> ExtractAttributes(IElement element, IEnumerable<string> attributeNames)
    {
        return from string attributeName in attributeNames
               where element.HasAttribute(attributeName)
               select element.GetAttribute(attributeName)!;
    }

    /// <summary>
    /// Extracts links from a `&lt;meta http-equiv="refresh"&gt;` element.
    /// </summary>
    /// <param name="element">The `&lt;meta&gt;` element to evaluate.</param>
    /// <returns>An <see cref="IEnumerable{String}"/> collection of links.</returns>
    private static IEnumerable<string> ExtractMetaRefreshLinks(IElement element)
    {
        if (element.TagName.Equals("meta", StringComparison.OrdinalIgnoreCase) &&
            element.HasAttribute("http-equiv") &&
            element.GetAttribute("http-equiv")!.Equals("refresh", StringComparison.OrdinalIgnoreCase) &&
            element.HasAttribute("content"))
        {
            string content = element.GetAttribute("content")!;
            int urlIndex = content.IndexOf("url=", StringComparison.OrdinalIgnoreCase);
            if (urlIndex >= 0)
            {
                string url = content[(urlIndex + 4)..].Trim();
                yield return url;
            }
        }
    }

    /// <summary>
    /// Recursively extracts links from the child elements of the given element.
    /// </summary>
    /// <param name="element">The DOM element whose children to evaluate.</param>
    /// <returns>An <see cref="IEnumerable{String}"/> collection of links.</returns>
    private static IEnumerable<string> ExtractChildLinks(IElement element)
    {
        foreach (IElement child in element.Children)
        {
            foreach (string link in ExtractLinksFromElement(child))
            {
                yield return link;
            }
        }
    }
}
