// This file is part of the Genova project licensed under the GNU General Public License v3.0.
// See the LICENSE file in the project root for more information.

using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Text;
using Genova.Common.Attributes;
using Genova.Crawler.Resources;

namespace Genova.Crawler.Utilities;

/// <summary>
/// Provides utility methods for working with resource details.
/// </summary>
[CodeQuality(Public = true, Justification = "Intended for use by the Scanner.")]
public static class ResourceHelper
{
    /// <summary>
    /// Serializes the details of a resource into a multi-line, formatted string.
    /// </summary>
    /// <param name="resource">The <see cref="ResourceDetails"/> to serialize.</param>
    /// <returns>A multi-line string summarizing the resource details.</returns>
    [ExcludeFromCodeCoverage]
    public static string SerializeResource(ResourceDetails resource)
    {
        ArgumentNullException.ThrowIfNull(resource);

        StringBuilder builder = new();

        // Add basic resource details
        SerializeBasicDetails(builder, resource.Url, resource.StatusCode, resource.ContentLength, resource.ContentType, resource.RedirectLocation, resource.ResponseTime);

        // Add headers and cookies
        SerializeHeaders(builder, resource.Headers);
        SerializeCookies(builder, resource.Cookies);

        // Check if the resource is a TextResourceDetails and append the body
        if (resource is TextResourceDetails textResource)
        {
            builder.AppendLine("Body:");
            builder.AppendLine(textResource.Body ?? "N/A");
        }

        return builder.ToString();
    }

    /// <summary>
    /// Serializes the parameters of a fetch operation into a multi-line, formatted string.
    /// </summary>
    /// <param name="url">The URL of the resource to fetch.</param>
    /// <param name="httpMethod">The HTTP method to use for the request.</param>
    /// <param name="followRedirects">Indicates whether to follow redirects during the fetch operation.</param>
    /// <param name="parameters">The <see cref="FetchParameters"/> containing headers, body, and form data.</param>
    /// <param name="fetchOptions">The <see cref="FetchOptions"/> containing additional fetch options.</param>
    /// <returns>A multi-line string summarizing the fetch parameters.</returns>
    [ExcludeFromCodeCoverage]
    public static string SerializeFetch(
        Uri url,
        HttpMethod httpMethod,
        bool followRedirects = false,
        FetchParameters? parameters = null,
        FetchOptions? fetchOptions = null)
    {
        StringBuilder builder = new();

        // Add basic fetch details
        SerializeBasicDetails(builder, url, null, null, null, null, null);
        builder.AppendLine($"HTTP Method: {httpMethod.Method}");
        builder.AppendLine($"Follow Redirects: {followRedirects}");

        // Add FetchParameters details
        if (parameters != null)
        {
            builder.AppendLine("Fetch Parameters:");
            SerializeHeaders(builder, parameters.Headers);
            SerializeBodyOrForm(builder, parameters.Body, parameters.Form);
        }
        else
        {
            builder.AppendLine("Fetch Parameters: None");
        }

        // Add FetchOptions details
        if (fetchOptions != null)
        {
            builder.AppendLine("Fetch Options:");
            builder.AppendLine($"  DefaultRequestHeadersHost: {fetchOptions.DefaultRequestHeadersHost ?? "N/A"}");
        }
        else
        {
            builder.AppendLine("Fetch Options: None");
        }

        return builder.ToString();
    }

    [ExcludeFromCodeCoverage]
    private static void SerializeBasicDetails(
        StringBuilder builder,
        Uri url,
        HttpStatusCode? statusCode,
        long? contentLength,
        string? contentType,
        Uri? redirectLocation,
        TimeSpan? responseTime)
    {
        builder.AppendLine($"URL: {url}");
        if (statusCode.HasValue)
        {
            builder.AppendLine($"Status Code: {statusCode}");
        }

        if (contentLength.HasValue)
        {
            builder.AppendLine($"Content Length: {contentLength}");
        }

        builder.AppendLine($"Content Type: {contentType ?? "N/A"}");
        builder.AppendLine($"Redirect Location: {redirectLocation?.ToString() ?? "N/A"}");
        if (responseTime.HasValue)
        {
            builder.AppendLine($"Response Time: {responseTime.Value.TotalMilliseconds} ms");
        }
    }

    [ExcludeFromCodeCoverage]
    private static void SerializeHeaders(StringBuilder builder, Dictionary<string, string>? headers)
    {
        builder.AppendLine("Headers:");
        if (headers != null && headers.Count > 0)
        {
            foreach (KeyValuePair<string, string> header in headers)
            {
                builder.AppendLine($"  {header.Key}: {header.Value}");
            }
        }
        else
        {
            builder.AppendLine("  None");
        }
    }

    [ExcludeFromCodeCoverage]
    private static void SerializeCookies(StringBuilder builder, IEnumerable<string> cookies)
    {
        builder.AppendLine("Cookies:");
        if (cookies.Any())
        {
            foreach (string cookie in cookies)
            {
                builder.AppendLine($"  {cookie}");
            }
        }
        else
        {
            builder.AppendLine("  None");
        }
    }

    [ExcludeFromCodeCoverage]
    private static void SerializeBodyOrForm(StringBuilder builder, string? body, Dictionary<string, string>? form)
    {
        if (!string.IsNullOrWhiteSpace(body))
        {
            builder.AppendLine($"  Body: {body}");
        }
        else
        {
            builder.AppendLine("  Body: None");
        }

        if (form != null && form.Count > 0)
        {
            builder.AppendLine("  Form Data:");
            foreach (KeyValuePair<string, string> formField in form)
            {
                builder.AppendLine($"    {formField.Key}: {formField.Value}");
            }
        }
        else
        {
            builder.AppendLine("  Form Data: None");
        }
    }
}
