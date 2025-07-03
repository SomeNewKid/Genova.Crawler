// This file is part of the Genova project licensed under the GNU General Public License v3.0.
// See the LICENSE file in the project root for more information.

using Genova.Common.Attributes;

namespace Genova.Crawler;

/// <summary>
/// Represents additional options for fetching a resource.
/// </summary>
[CodeQuality(Public = true, Justification = "Intended for use by the Scanner.")]
public sealed class FetchOptions
{
    /// <summary>
    /// Gets or sets the value for the Host header in the request.
    /// </summary>
    /// <remarks>
    /// If set, this value will override the default Host header for the request.
    /// </remarks>
    public string? DefaultRequestHeadersHost { get; set; }
}
