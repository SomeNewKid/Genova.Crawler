// This file is part of the Genova project licensed under the GNU General Public License v3.0.
// See the LICENSE file in the project root for more information.

using Genova.Common.Attributes;

namespace Genova.Crawler.Resources;

/// <summary>
/// Represents the details of a text resource discovered during website crawling.
/// </summary>
[CodeQuality(Public = true, Unsealed = true, Justification = "Intended for use by the Scanner.")]
public class CompressedResourceDetails : ResourceDetails
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CompressedResourceDetails"/> class.
    /// </summary>
    /// <param name="parameters">The parameters required to initialize the resource details.</param>
    /// <param name="encoding">The encoding of the resource.</param>
    internal CompressedResourceDetails(ResourceDetailsParameters parameters, string? encoding)
        : base(parameters)
    {
        Encoding = encoding;
    }

    /// <summary>
    /// Gets the encoding of the resource.
    /// </summary>
    public string? Encoding { get; }
}
