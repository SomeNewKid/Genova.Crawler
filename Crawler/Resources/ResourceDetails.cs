// This file is part of the Genova project licensed under the GNU General Public License v3.0.
// See the LICENSE file in the project root for more information.

using System.Net;
using Genova.Common.Attributes;

namespace Genova.Crawler.Resources;

/// <summary>
/// Represents the details of a resource discovered during website crawling.
/// </summary>
[CodeQuality(Public = true, Unsealed = true, Justification = "Intended for use by the Scanner.")]
public class ResourceDetails
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ResourceDetails"/> class.
    /// </summary>
    /// <param name="parameters">The parameters required to initialize the resource details.</param>
    internal ResourceDetails(ResourceDetailsParameters parameters)
    {
        ValidateParameters(parameters);

        Url = parameters.Url;
        Headers = parameters.Headers;
        Cookies = parameters.Cookies;
        StatusCode = parameters.StatusCode;
        ContentLength = parameters.ContentLength;
        ContentType = parameters.ContentType;
        RedirectLocation = parameters.RedirectLocation;
        ResponseTime = parameters.ResponseTime;
    }

    /// <summary>
    /// Gets the URL of the resource.
    /// </summary>
    public Uri Url { get; }

    /// <summary>
    /// Gets the headers associated with the resource.
    /// </summary>
    /// <remarks>
    /// The headers are represented as a dictionary where the key is the header name
    /// and the value is the header value.
    /// </remarks>
    public Dictionary<string, string> Headers { get; }

    /// <summary>
    /// Gets the cookies associated with the resource.
    /// </summary>
    /// <remarks>
    /// The cookies are represented as a list of strings, where each string contains
    /// the cookie name and value in the format "Name=Value".
    /// </remarks>
    public List<string> Cookies { get; }

    /// <summary>
    /// Gets the HTTP status code of the response.
    /// </summary>
    public HttpStatusCode StatusCode { get; }

    /// <summary>
    /// Gets the length of the response content.
    /// </summary>
    /// <remarks>
    /// This value represents the size of the response body in bytes.
    /// </remarks>
    public long ContentLength { get; }

    /// <summary>
    /// Gets the content type of the response.
    /// </summary>
    /// <remarks>
    /// This value is derived from the "Content-Type" header in the HTTP response.
    /// </remarks>
    public string? ContentType { get; }

    /// <summary>
    /// Gets the redirect location, if applicable.
    /// </summary>
    /// <remarks>
    /// This value is derived from the "Location" header in the HTTP response for 3xx status codes.
    /// </remarks>
    public Uri? RedirectLocation { get; }

    /// <summary>
    /// Gets the time taken to receive the response.
    /// </summary>
    /// <remarks>
    /// This value represents the duration between sending the request and receiving the response.
    /// </remarks>
    public TimeSpan ResponseTime { get; }

    /// <summary>
    /// Validates the parameters required to initialize the resource details.
    /// </summary>
    /// <param name="parameters">The parameters to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown if any required parameter is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if any parameter has an invalid value.</exception>
    private static void ValidateParameters(ResourceDetailsParameters parameters)
    {
        ArgumentNullException.ThrowIfNull(parameters);
        ArgumentNullException.ThrowIfNull(parameters.Url);
        ArgumentNullException.ThrowIfNull(parameters.Headers);
        ArgumentNullException.ThrowIfNull(parameters.Cookies);

        if (parameters.ContentLength < 0)
        {
            throw new ArgumentOutOfRangeException(
                $"{nameof(parameters)}.{nameof(parameters.ContentLength)}",
                "Content length cannot be negative.");
        }

        if (parameters.ResponseTime < TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(
                $"{nameof(parameters)}.{nameof(parameters.ResponseTime)}",
                "Response time cannot be negative.");
        }
    }
}
