namespace SpotifyPlaylists.Services;

/// <summary>Provides cached Spotify OAuth tokens (client credentials flow).</summary>
public interface ISpotifyTokenService
{
    /// <summary>Returns a valid access token, from cache or by requesting a new one.</summary>
    Task<string?> GetTokenAsync(CancellationToken cancellationToken = default);
}
