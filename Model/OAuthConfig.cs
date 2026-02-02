namespace SpotifyPlaylists.Model;

using System.Text.Json.Serialization;
using Microsoft.Extensions.Configuration;

public class OAuthConfig
{
    public const string SectionName = "OAuth";

    [JsonPropertyName("client_id")]
    [ConfigurationKeyName("client_id")]
    public string ClientId { get; set; } = string.Empty;

    [JsonPropertyName("client_secret")]
    [ConfigurationKeyName("client_secret")]
    public string ClientSecret { get; set; } = string.Empty;

    [JsonPropertyName("token_url")]
    [ConfigurationKeyName("token_url")]
    public string TokenUrl { get; set; } = "https://accounts.spotify.com/api/token";
}
