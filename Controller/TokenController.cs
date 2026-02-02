using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using SpotifyPlaylists.Model;

namespace SpotifyPlaylists.Controller;

[ApiController]
[Route("api/v1")]
public class TokenController : ControllerBase
{
    private readonly SpotifyClient _spotifyClient;
    private readonly OAuthConfig _oauthConfig;
    private readonly ILogger<TokenController> _logger;

    public TokenController(SpotifyClient spotifyClient, IOptions<OAuthConfig> oauthConfig, ILogger<TokenController> logger)
    {
        _spotifyClient = spotifyClient;
        _oauthConfig = oauthConfig.Value;
        _logger = logger;
    }

    [HttpPost("token")]
    public async Task<IActionResult> GetToken([FromBody] TokenRequest? request, CancellationToken cancellationToken)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.ClientId) || string.IsNullOrWhiteSpace(request.ClientSecret))
        {
            _logger.LogWarning("Token request missing client_id or client_secret.");
            return BadRequest(new { error = "client_id and client_secret are required in the request body." });
        }

        var tokenUrl = string.IsNullOrWhiteSpace(_oauthConfig.TokenUrl)
            ? "https://accounts.spotify.com/api/token"
            : _oauthConfig.TokenUrl;

        var token = await _spotifyClient.GetOAuthToken(request.ClientId, request.ClientSecret, tokenUrl, cancellationToken);

        if (string.IsNullOrEmpty(token))
        {
            _logger.LogWarning("Failed to obtain token from Spotify.");
            return StatusCode(502, new { error = "Failed to obtain token from Spotify." });
        }

        return Ok(new TokenResult
        {
            AccessToken = token,
            TokenType = "Bearer",
            ExpiresIn = 3600
        });
    }
}
