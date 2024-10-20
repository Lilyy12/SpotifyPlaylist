using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using SpotifyPlaylists.Model;

namespace SpotifyPlaylists.Controller;

    [ApiController]
    [Route("v1/artists/")]
    public class ArtistController : ControllerBase
    {
        public ArtistController()
        {
        }

        [HttpGet]
        [Route("{id}")]
        public async Task<IActionResult> GetArtistFromId(string id)
        {
            var config = LoadConfig("config.json");
            using (var httpClient = new HttpClient())
            {
                var spotifyClient = new SpotifyClient(httpClient);

                var token = await spotifyClient.GetOAuthToken(config.ClientId, config.ClientSecret, config.TokenUrl);

                if (!string.IsNullOrEmpty(token))
                {
                    spotifyClient.AttachBearerToken(token);

                    string apiResponse = await spotifyClient.MakeAuthenticatedRequest
                    ($"https://api.spotify.com/v1/artists/{id}") ?? "no response";

                    Console.WriteLine("API Response: " + apiResponse);
                    return Ok(apiResponse);
                }
                else
                {
                    Console.WriteLine("Failed to retrieve token from config.");
                    return NotFound("Token is invalid");
                }
            }
        }

        public static OAuthConfig LoadConfig(string filePath)
        {
            var jsonString = System.IO.File.ReadAllText(filePath);
            return JsonSerializer.Deserialize<OAuthConfig>(jsonString);
        }
    }
