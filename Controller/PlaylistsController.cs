using Microsoft.AspNetCore.Mvc;

namespace SpotifyPlaylists.Controller;

[ApiController]
[Route("api/v1/playlists")]
public class PlaylistsController : ControllerBase
{
    private readonly SpotifyClient _spotifyClient;
    private readonly ILogger<PlaylistsController> _logger;

    public PlaylistsController(SpotifyClient spotifyClient, ILogger<PlaylistsController> logger)
    {
        _spotifyClient = spotifyClient;
        _logger = logger;
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetPlaylist(string id, [FromQuery] string? market, [FromQuery] string? fields, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            _logger.LogWarning("GetPlaylist called with empty id.");
            return BadRequest(new { error = "Playlist id is required." });
        }

        var query = new Dictionary<string, string?>();
        if (!string.IsNullOrWhiteSpace(market)) query["market"] = market;
        if (!string.IsNullOrWhiteSpace(fields)) query["fields"] = fields;
        var path = $"/v1/playlists/{id}{ToQueryString(query)}";

        using var apiResponse = await _spotifyClient.MakeAuthenticatedRequest(path, cancellationToken);
        var content = await apiResponse.Content.ReadAsStringAsync(cancellationToken);

        if (apiResponse.IsSuccessStatusCode)
        {
            _logger.LogDebug("Playlist retrieved: {Id}", id);
            return Content(content, "application/json");
        }

        _logger.LogWarning("Spotify API error for playlist {Id}: {StatusCode}", id, apiResponse.StatusCode);
        Response.StatusCode = (int)apiResponse.StatusCode;
        return Content(content, "application/json");
    }

    [HttpGet("{id}/tracks")]
    public async Task<IActionResult> GetPlaylistTracks(string id, [FromQuery] string? market, [FromQuery] string? fields, [FromQuery] int? limit, [FromQuery] int? offset, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            _logger.LogWarning("GetPlaylistTracks called with empty id.");
            return BadRequest(new { error = "Playlist id is required." });
        }

        var query = new Dictionary<string, string?>();
        if (!string.IsNullOrWhiteSpace(market)) query["market"] = market;
        if (!string.IsNullOrWhiteSpace(fields)) query["fields"] = fields;
        if (limit.HasValue) query["limit"] = limit.Value.ToString();
        if (offset.HasValue) query["offset"] = offset.Value.ToString();
        var path = $"/v1/playlists/{id}/tracks{ToQueryString(query)}";

        using var apiResponse = await _spotifyClient.MakeAuthenticatedRequest(path, cancellationToken);
        var content = await apiResponse.Content.ReadAsStringAsync(cancellationToken);

        if (apiResponse.IsSuccessStatusCode)
        {
            _logger.LogDebug("Playlist tracks retrieved: {Id}", id);
            return Content(content, "application/json");
        }

        _logger.LogWarning("Spotify API error for playlist tracks {Id}: {StatusCode}", id, apiResponse.StatusCode);
        Response.StatusCode = (int)apiResponse.StatusCode;
        return Content(content, "application/json");
    }

    private static string ToQueryString(Dictionary<string, string?> query)
    {
        var pairs = query.Where(kv => !string.IsNullOrEmpty(kv.Value)).Select(kv => $"{Uri.EscapeDataString(kv.Key)}={Uri.EscapeDataString(kv.Value!)}");
        return pairs.Any() ? "?" + string.Join("&", pairs) : string.Empty;
    }
}
