using Microsoft.AspNetCore.Mvc;

namespace SpotifyPlaylists.Controller;

[ApiController]
[Route("api/v1/artists")]
public class ArtistController : ControllerBase
{
    private readonly SpotifyClient _spotifyClient;
    private readonly ILogger<ArtistController> _logger;

    public ArtistController(SpotifyClient spotifyClient, ILogger<ArtistController> logger)
    {
        _spotifyClient = spotifyClient;
        _logger = logger;
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetArtistFromId(string id, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            _logger.LogWarning("GetArtistFromId called with empty id.");
            return BadRequest(new { error = "Artist id is required." });
        }

        using var apiResponse = await _spotifyClient.MakeAuthenticatedRequest($"/v1/artists/{id}", cancellationToken);
        var content = await apiResponse.Content.ReadAsStringAsync(cancellationToken);

        if (apiResponse.IsSuccessStatusCode)
        {
            _logger.LogDebug("Artist retrieved: {Id}", id);
            return Content(content, "application/json");
        }

        _logger.LogWarning("Spotify API error for artist {Id}: {StatusCode}", id, apiResponse.StatusCode);
        Response.StatusCode = (int)apiResponse.StatusCode;
        return Content(content, "application/json");
    }
}
