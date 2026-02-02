using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using SpotifyPlaylists.Model;

namespace SpotifyPlaylists.Services;

public class SpotifyTokenService : ISpotifyTokenService
{
    private const string CacheKey = "spotify_oauth_token";
    private const int ExpiryBufferSeconds = 60; // Refresh 60 seconds before expiry

    private readonly HttpClient _httpClient;
    private readonly IMemoryCache _cache;
    private readonly OAuthConfig _config;
    private readonly ILogger<SpotifyTokenService> _logger;

    public SpotifyTokenService(
        HttpClient httpClient,
        IOptions<OAuthConfig> config,
        IMemoryCache cache,
        ILogger<SpotifyTokenService> logger)
    {
        _httpClient = httpClient;
        _cache = cache;
        _config = config.Value;
        _logger = logger;
    }

    public async Task<string?> GetTokenAsync(CancellationToken cancellationToken = default)
    {
        if (_cache.TryGetValue(CacheKey, out string? cached) && !string.IsNullOrEmpty(cached))
            return cached;

        if (string.IsNullOrEmpty(_config.ClientId) || string.IsNullOrEmpty(_config.ClientSecret))
        {
            _logger.LogWarning("Spotify OAuth config is missing ClientId or ClientSecret.");
            return null;
        }

        var requestBody = new StringContent(
            $"grant_type=client_credentials&client_id={_config.ClientId}&client_secret={_config.ClientSecret}",
            Encoding.UTF8,
            "application/x-www-form-urlencoded");

        var response = await _httpClient.PostAsync(_config.TokenUrl, requestBody, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("Failed to obtain Spotify token: {StatusCode}", response.StatusCode);
            return null;
        }

        var json = await response.Content.ReadAsStringAsync(cancellationToken);
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;
        var accessToken = root.TryGetProperty("access_token", out var tokenProp) ? tokenProp.GetString() : null;
        var expiresIn = root.TryGetProperty("expires_in", out var expiresProp) ? expiresProp.GetInt32() : 3600;

        if (string.IsNullOrEmpty(accessToken))
        {
            _logger.LogError("Spotify token response did not contain access_token.");
            return null;
        }

        var cacheExpiry = TimeSpan.FromSeconds(Math.Max(0, expiresIn - ExpiryBufferSeconds));
        _cache.Set(CacheKey, accessToken, cacheExpiry);
        _logger.LogDebug("Cached new Spotify token for {Seconds}s", cacheExpiry.TotalSeconds);

        return accessToken;
    }
}
