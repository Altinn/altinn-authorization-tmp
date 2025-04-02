using System.IdentityModel.Tokens.Jwt;
using System.Runtime.CompilerServices;
using System.Text;
using System.Web;
using Altinn.Common.AccessTokenClient.Services;
using CommunityToolkit.Diagnostics;

namespace Altinn.Authorization.Integration.Platform;

/// <summary>
/// Provides utility methods to create and modify HTTP requests.
/// </summary>
internal static class RequestComposer
{
    /// <summary>
    /// Creates a new HTTP request message and applies the provided actions to it.
    /// </summary>
    /// <param name="actions">A set of actions to configure the HTTP request.</param>
    /// <returns>A configured <see cref="HttpRequestMessage"/>.</returns>
    public static HttpRequestMessage New(params Action<HttpRequestMessage>[] actions)
    {
        var request = new HttpRequestMessage();
        foreach (var action in actions)
        {
            action(request);
        }

        return request;
    }

    /// <summary>
    /// Sets the HTTP method for the request.
    /// </summary>
    /// <param name="method">The HTTP method to set.</param>
    /// <returns>An action to configure the request method.</returns>
    public static Action<HttpRequestMessage> WithHttpVerb(HttpMethod method) => request =>
    {
        request.Method = method;
    };

    /// <summary>
    /// Sets the URI of the request.
    /// </summary>
    /// <remarks>
    /// ignores uri if empty string or null.
    /// </remarks>
    /// <param name="uri">The URI to set.</param>
    /// <returns>An action to configure the request URI.</returns>
    public static Action<HttpRequestMessage> WithSetUri(string uri) => request =>
    {
        if (!string.IsNullOrEmpty(uri))
        {
            request.RequestUri = new Uri(uri);
        }
    };

    /// <summary>
    /// Sets the URI of the request with additional segments.
    /// </summary>
    /// <remarks>
    /// ignores uri if null.
    /// </remarks>
    /// <param name="uri">The base URI.</param>
    /// <param name="segments">Additional path segments to append.</param>
    /// <returns>An action to configure the request URI.</returns>
    public static Action<HttpRequestMessage> WithSetUri(Uri uri, params string[] segments) => request =>
    {
        if (uri != null)
        {
            request.RequestUri = new Uri(uri, string.Join("/", segments));
        }
    };

    /// <summary>
    /// Appends a query parameter with multiple values to the request URI.
    /// </summary>
    /// <typeparam name="T">The type of values.</typeparam>
    /// <param name="param">The query parameter name.</param>
    /// <param name="values">The values to associate with the parameter.</param>
    /// <returns>An action to modify the request URI.</returns>
    public static Action<HttpRequestMessage> WithAppendQueryParam<T>(string param, IEnumerable<T> values) => request =>
    {
        values ??= [];
        if (request == null || string.IsNullOrEmpty(param) || !values.Any())
        {
            return;
        }

        var query = HttpUtility.ParseQueryString(request.RequestUri?.Query ?? string.Empty);
        query[param] = string.Join(",", values);
        request.RequestUri = new UriBuilder(request.RequestUri)
        {
            Query = query.ToString()
        }.Uri;
    };

    /// <summary>
    /// Appends a query parameter with a single value to the request URI.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="param">The query parameter name.</param>
    /// <param name="value">The value to associate with the parameter.</param>
    /// <returns>An action to modify the request URI.</returns>
    public static Action<HttpRequestMessage> WithAppendQueryParam<T>(string param, T value) => request =>
    {
        if (request == null || string.IsNullOrEmpty(param) || EqualityComparer<T>.Default.Equals(value, default))
        {
            return;
        }

        var query = HttpUtility.ParseQueryString(request.RequestUri?.Query ?? string.Empty);
        query[param] = value.ToString();
        request.RequestUri = new UriBuilder(request.RequestUri)
        {
            Query = query.ToString()
        }.Uri;
    };

    public static Action<HttpRequestMessage> WithBasicAuth(string username, string password) => request =>
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(nameof(username));
        ArgumentException.ThrowIfNullOrWhiteSpace(nameof(password));

        var cred = $"{username}:{password}";
        request.Headers.Authorization = new("Basic", Convert.ToBase64String(Encoding.UTF8.GetBytes(cred)));
    };

    /// <summary>
    /// Adds a platform access token to the request headers.
    /// </summary>
    /// <returns>An action to add the access token to the request headers.</returns>
    public static Action<HttpRequestMessage> WithPlatformAccessToken(string token) => request =>
    {
        var token = accessTokenGenerator.GenerateAccessToken(issuer, app);

        token = "eyJhbGciOiJSUzI1NiIsImtpZCI6IkJCQjA2MkM5ODI4NEZBRTYxOUNCMjlGRkYyQ0FBMDFGNUE3QzU2RjIiLCJ0eXAiOiJKV1QiLCJ4NWMiOiJCQkIwNjJDOTgyODRGQUU2MTlDQjI5RkZGMkNBQTAxRjVBN0M1NkYyIn0.eyJ1cm46YWx0aW5uOmFwcCI6ImFjY2Vzcy1tYW5hZ2VtZW50IiwiZXhwIjoyMTAxNjgyOTY2LCJpYXQiOjE3NDE2ODI5NjYsImlzcyI6InBsYXRmb3JtIiwiYWN0dWFsX2lzcyI6ImFsdGlubi10ZXN0LXRvb2xzIiwibmJmIjoxNzQxNjgyOTY2fQ.DEh5SKTiGFCnIzBKBTaNyGCecCmR3tBdDC_Sg9wGxvr_D60cRcOOp9FPJGVofnRT3Da_aLFgGEJXCERgteqe72Jr-EANO8Lh7PSJpnYKmt8skwNiCQY76S_cx4gG8c55wbg1PPqv4vylmS0DhywYJ2CBkcajwdt8frfjNCyxxp6VIPi6hM9pzJhOLPLh5kX5lGcWpXaxselXbzyEg0fD6vgl6dRfrkWdjNxzPxg8Uy0rPnvs5RHiFIotzfwTIqo7NASa0SH8vh-Hcw9tfv_s2lv4KS4jpqSHIgDN9uqZgfoIdjGXnbsI6FFUPUsh9ITsJKkBT3D_s7qASStT55o1Lw";

        if (!string.IsNullOrEmpty(token))
        {
            request.Headers.Add("PlatformAccessToken", token);
        }
    };

    /// <summary>
    /// Adds a platform access token to the request headers.
    /// </summary>
    /// <returns>An action to add the access token to the request headers.</returns>
    public static Action<HttpRequestMessage> WithJWTToken(string token) => request =>
    {
        if (!string.IsNullOrEmpty(token))
        {
            request.Headers.Add("Authorization", $"Bearer {token}");
        }
    };
}
