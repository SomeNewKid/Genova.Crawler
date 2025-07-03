// This file is part of the Genova project licensed under the GNU General Public License v3.0.
// See the LICENSE file in the project root for more information.

using Genova.Common.Attributes;
using Genova.Common.Utilities;

namespace Genova.Crawler.Resources;

/// <summary>
/// Provides functionality to build <see cref="ResourceDetails"/> instances.
/// </summary>
[CodeQuality(Unsealed = true, Justification = "Intended for override by deriving classes.")]
internal class ResourceDetailsBuilder
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ResourceDetailsBuilder"/> class.
    /// </summary>
    /// <param name="url">The URL of the resource.</param>
    /// <param name="response">The HTTP response when requesting the resource.</param>
    /// <param name="responseTime">The time taken to receive the response.</param>
    internal ResourceDetailsBuilder(Uri url, HttpResponseMessage response, TimeSpan responseTime)
    {
        Url = url;
        Response = response;
        ResponseTime = responseTime;
    }

    /// <summary>
    /// Gets the URL of the resource.
    /// </summary>
    protected Uri Url { get; }

    /// <summary>
    /// Gets the HTTP response when requesting the resource.
    /// </summary>
    protected HttpResponseMessage Response { get; }

    /// <summary>
    /// Gets the time taken to receive the response.
    /// </summary>
    protected TimeSpan ResponseTime { get; }

    /// <summary>
    /// Extracts the content encoding from the HTTP response.
    /// </summary>
    /// <param name="response">The HTTP response.</param>
    /// <returns>The value of the "Content-Encoding" response header.</returns>
    internal static string? GetResponseEncoding(HttpResponseMessage response)
    {
        if (response.Content.Headers.ContentEncoding.Count > 0)
        {
            return response.Content.Headers.ContentEncoding.ToString();
        }

        return null;
    }

    /// <summary>
    /// Builds a <see cref="ResourceDetails"/> instance.
    /// </summary>
    /// <returns>A <see cref="ResourceDetails"/> instance.</returns>
    internal virtual ResourceDetails Build()
    {
        ResourceDetailsParameters parameters = new()
        {
            Url = Url,
            Headers = ExtractHeaders(),
            Cookies = ExtractCookies(),
            StatusCode = Response.StatusCode,
            ContentLength = ExtractContentLength(),
            ContentType = ExtractContentType(),
            RedirectLocation = ExtractRedirectLocation(),
            ResponseTime = ResponseTime,
        };

        return new ResourceDetails(parameters);
    }

    /// <summary>
    /// Extracts headers from the HTTP response.
    /// </summary>
    /// <returns>A dictionary of headers.</returns>
    protected virtual Dictionary<string, string> ExtractHeaders()
    {
        Dictionary<string, string> headers = Response.Headers.ToDictionary(h => h.Key, h => string.Join(", ", h.Value));
        string? compression = GetResponseEncoding(Response);
        if (!string.IsNullOrEmpty(compression))
        {
            headers["Content-Encoding"] = compression;
        }

        return headers;
    }

    /// <summary>
    /// Extracts cookies from the HTTP response.
    /// </summary>
    /// <returns>A list of cookies.</returns>
    protected virtual List<string> ExtractCookies()
    {
        return Response.Headers.TryGetValues("Set-Cookie", out IEnumerable<string>? cookieValues)
            ? cookieValues.ToList()
            : [];
    }

    /// <summary>
    /// Extracts the content length from the HTTP response.
    /// </summary>
    /// <returns>The content length, or 0 if not specified.</returns>
    protected virtual long ExtractContentLength()
    {
        return Response.Content.Headers.ContentLength ?? 0;
    }

    /// <summary>
    /// Extracts the content type from the HTTP response.
    /// </summary>
    /// <returns>The content type, or <c>null</c> if not specified.</returns>
    protected virtual string? ExtractContentType()
    {
        return HttpHelper.GetContentType(Response);
    }

    /// <summary>
    /// Extracts the redirect location from the HTTP response.
    /// </summary>
    /// <returns>The redirect location, or <c>null</c> if not applicable.</returns>
    protected virtual Uri? ExtractRedirectLocation()
    {
        if (Response.Headers.Location != null)
        {
            return new Uri(Url, Response.Headers.Location);
        }

        return null;
    }
}
