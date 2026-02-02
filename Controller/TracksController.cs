using Microsoft.AspNetCore.Mvc;

namespace SpotifyPlaylists.Controller;

[ApiController]
[Route("api/v1/tracks")]
public class TracksController : ControllerBase
{
    private readonly SpotifyClient _spotifyClient;
    private readonly ILogger<TracksController> _logger;

    public TracksController(SpotifyClient spotifyClient, ILogger<TracksController> logger)
    {
        _spotifyClient = spotifyClient;
        _logger = logger;
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetTrack(string id, [FromQuery] string? market, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            _logger.LogWarning("GetTrack called with empty id.");
            return BadRequest(new { error = "Track id is required." });
        }

        var query = new Dictionary<string, string?>();
        if (!string.IsNullOrWhiteSpace(market)) query["market"] = market;
        var path = $"/v1/tracks/{id}{ToQueryString(query)}";

        using var apiResponse = await _spotifyClient.MakeAuthenticatedRequest(path, cancellationToken);
        var content = await apiResponse.Content.ReadAsStringAsync(cancellationToken);

        if (apiResponse.IsSuccessStatusCode)
        {
            _logger.LogDebug("Track retrieved: {Id}", id);
            return Content(content, "application/json");
        }

        _logger.LogWarning("Spotify API error for track {Id}: {StatusCode}", id, apiResponse.StatusCode);
        Response.StatusCode = (int)apiResponse.StatusCode;
        return Content(content, "application/json");
    }

    private static string ToQueryString(Dictionary<string, string?> query)
    {
        var pairs = query.Where(kv => !string.IsNullOrEmpty(kv.Value)).Select(kv => $"{Uri.EscapeDataString(kv.Key)}={Uri.EscapeDataString(kv.Value!)}");
        return pairs.Any() ? "?" + string.Join("&", pairs) : string.Empty;
    }
}
