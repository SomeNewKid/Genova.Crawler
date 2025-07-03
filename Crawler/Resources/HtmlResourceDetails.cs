// This file is part of the Genova project licensed under the GNU General Public License v3.0.
// See the LICENSE file in the project root for more information.

using AngleSharp.Dom;
using Genova.Common.Attributes;

namespace Genova.Crawler.Resources;

/// <summary>
/// Represents the details of an HTML resource discovered during website crawling.
/// </summary>
[CodeQuality(Public = true, Justification = "Intended for use by the Scanner.")]
public sealed class HtmlResourceDetails : TextResourceDetails
{
    /// <summary>
    /// Initializes a new instance of the <see cref="HtmlResourceDetails"/> class.
    /// </summary>
    /// <param name="parameters">The parameters required to initialize the resource details.</param>
    /// <param name="body">The raw text content of the HTML resource.</param>
    /// <param name="htmlDocument">The parsed HTML document of the resource, if applicable.</param>
    internal HtmlResourceDetails(ResourceDetailsParameters parameters, string? body, IDocument? htmlDocument)
        : base(parameters, body)
    {
        HtmlDocument = htmlDocument;
    }

    /// <summary>
    /// Gets the parsed HTML document of the resource, if applicable.
    /// </summary>
    /// <remarks>
    /// This property is populated when the resource is an HTML document and is parsed
    /// using AngleSharp. It will be <c>null</c> for non-HTML resources.
    /// </remarks>
    public IDocument? HtmlDocument { get; }
}
