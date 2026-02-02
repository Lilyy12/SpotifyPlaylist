using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using SpotifyPlaylists.Services;

namespace SpotifyPlaylists;

public class SpotifyClient
{
    private readonly HttpClient _httpClient;
    private readonly ISpotifyTokenService _tokenService;
    private readonly ILogger<SpotifyClient> _logger;

    public SpotifyClient(HttpClient httpClient, ISpotifyTokenService tokenService, ILogger<SpotifyClient> logger)
    {
        _httpClient = httpClient;
        _tokenService = tokenService;
        _logger = logger;
    }

    /// <summary>Gets an OAuth token using the provided credentials (e.g. for the token API endpoint).</summary>
    public async Task<string?> GetOAuthToken(string clientId, string clientSecret, string tokenUrl, CancellationToken cancellationToken = default)
    {
        var requestBody = new StringContent(
            $"grant_type=client_credentials&client_id={clientId}&client_secret={clientSecret}",
            Encoding.UTF8,
            "application/x-www-form-urlencoded");

        var response = await _httpClient.PostAsync(tokenUrl, requestBody, cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
            using var doc = JsonDocument.Parse(responseContent);
            return doc.RootElement.TryGetProperty("access_token", out var prop) ? prop.GetString() : null;
        }

        _logger.LogError("Error fetching token: {StatusCode}", response.StatusCode);
        return null;
    }

    /// <summary>Performs an authenticated GET to the Spotify API using the cached token.</summary>
    public async Task<HttpResponseMessage> MakeAuthenticatedRequest(string url, CancellationToken cancellationToken = default)
    {
        var token = await _tokenService.GetTokenAsync(cancellationToken);
        if (string.IsNullOrEmpty(token))
        {
            _logger.LogError("No token available for authenticated request.");
            return new HttpResponseMessage(System.Net.HttpStatusCode.Unauthorized);
        }

        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var response = await _httpClient.SendAsync(request, cancellationToken);

        if (!response.IsSuccessStatusCode)
            _logger.LogWarning("Spotify API request failed: {Url} {StatusCode}", url, response.StatusCode);

        return response;
    }
}
