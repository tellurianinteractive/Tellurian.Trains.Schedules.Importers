using System.Net.Http.Headers;
using System.Text;

namespace Tellurian.Trains.Schedules.Planning.App.Services;

/// <summary>
/// Sends an exported plan (JSON) to the ModuleRegistry web API, which converts and distributes it
/// as a SQLite database for on-premise dispatch applications (see Requirements Specification §5.5).
/// The endpoint contract is provisional until the service exists: the JSON is POSTed to the
/// configured URL with the API key as a bearer token.
/// </summary>
public sealed class ModuleRegistryUploadService(HttpClient http)
{
    /// <summary>The outcome of an upload attempt.</summary>
    /// <param name="Success">Whether the service accepted the plan.</param>
    /// <param name="Message">A human-readable result or error message.</param>
    public readonly record struct UploadResult(bool Success, string Message);

    /// <summary>
    /// POSTs the plan JSON to the configured ModuleRegistry URL. Never throws — network and
    /// non-success responses are returned as a failed <see cref="UploadResult"/>.
    /// </summary>
    /// <param name="url">The ModuleRegistry endpoint URL (from Integration settings).</param>
    /// <param name="apiKey">The API key, sent as a bearer token when present.</param>
    /// <param name="json">The serialised plan.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public async Task<UploadResult> UploadJsonAsync(string? url, string? apiKey, string json, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(url) || !Uri.TryCreate(url, UriKind.Absolute, out var endpoint))
            return new(false, "MissingUrl");

        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, endpoint)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };
            if (!string.IsNullOrWhiteSpace(apiKey))
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

            using var response = await http.SendAsync(request, cancellationToken);
            return response.IsSuccessStatusCode
                ? new(true, "UploadSucceeded")
                : new(false, $"{(int)response.StatusCode} {response.ReasonPhrase}");
        }
        catch (Exception ex)
        {
            return new(false, ex.Message);
        }
    }
}
