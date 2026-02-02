using System.Text.Json.Serialization;

namespace SpotifyPlaylists.Model;

/// <summary>Request body for obtaining a Spotify OAuth token (client credentials).</summary>
public class TokenRequest
{
    [JsonPropertyName("client_id")]
    public string ClientId { get; set; } = string.Empty;

    [JsonPropertyName("client_secret")]
    public string ClientSecret { get; set; } = string.Empty;
}
