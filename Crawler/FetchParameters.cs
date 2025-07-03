// This file is part of the Genova project licensed under the GNU General Public License v3.0.
// See the LICENSE file in the project root for more information.

using Genova.Common.Attributes;

namespace Genova.Crawler;

/// <summary>
/// Represents parameters for fetching a resource.
/// </summary>
[CodeQuality(Public = true, Justification = "Intended for use by the Scanner.")]
public sealed class FetchParameters
{
    /// <summary>
    /// Gets or sets the headers to include in the request.
    /// </summary>
    public Dictionary<string, string>? Headers { get; set; }

    /// <summary>
    /// Gets or sets the raw body content of the request.
    /// </summary>
    public string? Body { get; set; }

    /// <summary>
    /// Gets or sets the form data to include in the request.
    /// </summary>
    public Dictionary<string, string>? Form { get; set; }
}
