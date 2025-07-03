// This file is part of the Genova project licensed under the GNU General Public License v3.0.
// See the LICENSE file in the project root for more information.

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Genova.Common.Utilities;

namespace Genova.Crawler.Resources;

/// <summary>
/// Provides functionality to create <see cref="ResourceDetails"/> instances.
/// </summary>
internal static class ResourceDetailsFactory
{
    [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Conflicting naming rules.")]
    private static readonly HttpRequestOptionsKey<Stopwatch> ResponseTimeKey = new("ResponseTime");

    /// <summary>
    /// Creates a <see cref="ResourceDetails"/> instance based on the HTTP response.
    /// </summary>
    /// <param name="url">The URL of the resource.</param>
    /// <param name="response">The HTTP response message.</param>
    /// <returns>A <see cref="ResourceDetails"/> instance.</returns>
    internal static ResourceDetails Create(Uri url, HttpResponseMessage response)
    {
        // Measure the response time (if available)
        TimeSpan responseTime = GetResponseTime(response);

        string? contentEncoding = ResourceDetailsBuilder.GetResponseEncoding(response);

        if (!string.IsNullOrWhiteSpace(contentEncoding))
        {
            // If the content was encoded, don't attempt to decode it
            CompressedResourceDetailsBuilder textBuilder = new(url, response, responseTime);
            return textBuilder.Build();
        }

        string? contentType = HttpHelper.GetContentType(response);
        string contentSubType = HttpHelper.GetContentSubType(contentType);

        if (contentSubType.Equals("html", StringComparison.OrdinalIgnoreCase))
        {
            HtmlResourceDetailsBuilder htmlBuilder = new(url, response, responseTime);
            return htmlBuilder.Build();
        }

        if (contentSubType.Equals("xml", StringComparison.OrdinalIgnoreCase))
        {
            XmlResourceDetailsBuilder xmlBuilder = new(url, response, responseTime);
            return xmlBuilder.Build();
        }

        if (IsTextBasedContent(contentSubType))
        {
            TextResourceDetailsBuilder textBuilder = new(url, response, responseTime);
            return textBuilder.Build();
        }

        ResourceDetailsBuilder builderBase = new(url, response, responseTime);
        return builderBase.Build();
    }

    /// <summary>
    /// Determines if the content type is text-based.
    /// </summary>
    /// <param name="contentSubType">The content subtype extracted from the Content-Type header.</param>
    /// <returns><c>true</c> if the content is text-based; otherwise, <c>false</c>.</returns>
    private static bool IsTextBasedContent(string contentSubType)
    {
        return contentSubType.Equals("plain", StringComparison.OrdinalIgnoreCase) || // Plain text
               contentSubType.Equals("css", StringComparison.OrdinalIgnoreCase) || // Stylesheets
               contentSubType.Equals("javascript", StringComparison.OrdinalIgnoreCase) || // JavaScript files
               contentSubType.Equals("json", StringComparison.OrdinalIgnoreCase) || // JSON files
               contentSubType.Equals("csv", StringComparison.OrdinalIgnoreCase) || // CSV files
               contentSubType.Equals("markdown", StringComparison.OrdinalIgnoreCase) || // Markdown files
               contentSubType.Equals("html", StringComparison.OrdinalIgnoreCase) || // HTML files (fallback)
               contentSubType.Equals("xml", StringComparison.OrdinalIgnoreCase); // XML files (fallback)
    }

    /// <summary>
    /// Extracts the response time from the HTTP response, if available.
    /// </summary>
    /// <param name="response">The HTTP response message.</param>
    /// <returns>The time taken to receive the response.</returns>
    private static TimeSpan GetResponseTime(HttpResponseMessage response)
    {
        if (response.RequestMessage?.Options.TryGetValue(ResponseTimeKey, out Stopwatch? stopwatch) == true)
        {
            return stopwatch.Elapsed;
        }

        return TimeSpan.Zero; // Default to zero if no response time is available
    }
}
