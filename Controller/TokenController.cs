using Microsoft.AspNetCore.Mvc;
using SpotifyPlaylists.Model;

namespace SpotifyPlaylists.Controller
{
    [ApiController]
    [Route("api")]
    public class TokenController : ControllerBase
    {
        private readonly HttpClient _httpClient;

        public TokenController()
        {
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri("https://accounts.spotify.com")
            };
        }
        
        [HttpPost]
        [Route("token")]
        public async Task<IActionResult> GetTokenKey([FromQuery]string? clientId, [FromQuery]string? clientSecret)
        {
            Dictionary<string, string> data = new Dictionary<string, string>
            {
                {"grant_type", "client_credentials"},
                {"client_id", $"{clientId}"},
                {"client_secret", $"{clientSecret}"}
            };
            
            var content = new FormUrlEncodedContent(data);
            
            var response = await _httpClient.PostAsync("/api/token", content);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<TokenResult>();
                return Ok(result);
            }
            else 
            {
                throw new Exception($"Invalid request: {response.StatusCode}");
            }
        }
    }
}