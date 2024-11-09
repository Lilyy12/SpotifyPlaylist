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
            var spotifyClient = new SpotifyClient(new HttpClient());
            var token = await spotifyClient.GetOAuthToken(config.ClientId, config.ClientSecret, config.TokenUrl);

            if (!string.IsNullOrEmpty(token))
            {
                spotifyClient.AttachBearerToken(token);
                var apiResponse = await spotifyClient.MakeAuthenticatedRequest
                    ($"https://api.spotify.com/v1/artists/{id}");
                if (apiResponse.IsSuccessStatusCode)
                {
                    Console.WriteLine("API Response: " + apiResponse);
                    return Ok(apiResponse);
                }
                else
                {
                    Console.WriteLine("API Response: " + apiResponse);
                    return BadRequest(apiResponse);
                }
                    
            }
            else
            {
                Console.WriteLine("Failed to retrieve token from config.");
                return NotFound("Token is invalid");
            }
        }

        private static OAuthConfig LoadConfig(string filePath)
        {
            var jsonString = System.IO.File.ReadAllText(filePath);
            return JsonSerializer.Deserialize<OAuthConfig>(jsonString);
        }
    }
