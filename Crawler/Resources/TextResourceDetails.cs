// This file is part of the Genova project licensed under the GNU General Public License v3.0.
// See the LICENSE file in the project root for more information.

using Genova.Common.Attributes;

namespace Genova.Crawler.Resources;

/// <summary>
/// Represents the details of a text resource discovered during website crawling.
/// </summary>
[CodeQuality(Public = true, Unsealed = true, Justification = "Intended for use by the Scanner.")]
public class TextResourceDetails : ResourceDetails
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TextResourceDetails"/> class.
    /// </summary>
    /// <param name="parameters">The parameters required to initialize the resource details.</param>
    /// <param name="body">The body content of the text resource.</param>
    internal TextResourceDetails(ResourceDetailsParameters parameters, string? body)
        : base(parameters)
    {
        Body = body;
    }

    /// <summary>
    /// Gets the body content of the text resource.
    /// </summary>
    /// <remarks>
    /// This property contains the raw text content of the resource. It will be <c>null</c> if the resource has no body.
    /// </remarks>
    public string? Body { get; }
}
