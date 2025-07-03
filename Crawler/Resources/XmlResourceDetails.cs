// This file is part of the Genova project licensed under the GNU General Public License v3.0.
// See the LICENSE file in the project root for more information.

using System.Xml;
using Genova.Common.Attributes;

namespace Genova.Crawler.Resources;

/// <summary>
/// Represents the details of an XML resource discovered during website crawling.
/// </summary>
[CodeQuality(Public = true, Justification = "Intended for use by the Scanner.")]
public sealed class XmlResourceDetails : TextResourceDetails
{
    /// <summary>
    /// Initializes a new instance of the <see cref="XmlResourceDetails"/> class.
    /// </summary>
    /// <param name="parameters">The parameters required to initialize the resource details.</param>
    /// <param name="body">The raw text content of the XML resource.</param>
    /// <param name="xmlDocument">The parsed XML document of the resource, if applicable.</param>
    internal XmlResourceDetails(ResourceDetailsParameters parameters, string? body, XmlDocument? xmlDocument)
        : base(parameters, body)
    {
        XmlDocument = xmlDocument;
    }

    /// <summary>
    /// Gets the parsed XML document of the resource, if applicable.
    /// </summary>
    /// <remarks>
    /// This property is populated when the resource is an XML document and is parsed
    /// using the <see cref="XmlDocument"/> class. It will be <c>null</c> for non-XML resources.
    /// </remarks>
    public XmlDocument? XmlDocument { get; }
}
