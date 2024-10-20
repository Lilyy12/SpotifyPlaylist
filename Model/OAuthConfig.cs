namespace SpotifyPlaylists.Model;
using System.Text.Json.Serialization;

public struct OAuthConfig
{
    [JsonPropertyName("client_id")]
    public string ClientId { get; set; }

    [JsonPropertyName("client_secret")]
    public string ClientSecret { get; set; }

    [JsonPropertyName("token_url")]
    public string TokenUrl { get; set; }
}
