using Microsoft.AspNetCore.Mvc;

namespace SpotifyPlaylists.Controller;

[ApiController]
[Route("api/v1/albums")]
public class AlbumsController : ControllerBase
{
    private readonly SpotifyClient _spotifyClient;
    private readonly ILogger<AlbumsController> _logger;

    public AlbumsController(SpotifyClient spotifyClient, ILogger<AlbumsController> logger)
    {
        _spotifyClient = spotifyClient;
        _logger = logger;
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetAlbum(string id, [FromQuery] string? market, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            _logger.LogWarning("GetAlbum called with empty id.");
            return BadRequest(new { error = "Album id is required." });
        }

        var query = new Dictionary<string, string?>();
        if (!string.IsNullOrWhiteSpace(market)) query["market"] = market;
        var path = $"/v1/albums/{id}{ToQueryString(query)}";

        using var apiResponse = await _spotifyClient.MakeAuthenticatedRequest(path, cancellationToken);
        var content = await apiResponse.Content.ReadAsStringAsync(cancellationToken);

        if (apiResponse.IsSuccessStatusCode)
        {
            _logger.LogDebug("Album retrieved: {Id}", id);
            return Content(content, "application/json");
        }

        _logger.LogWarning("Spotify API error for album {Id}: {StatusCode}", id, apiResponse.StatusCode);
        Response.StatusCode = (int)apiResponse.StatusCode;
        return Content(content, "application/json");
    }

    [HttpGet("{id}/tracks")]
    public async Task<IActionResult> GetAlbumTracks(string id, [FromQuery] string? market, [FromQuery] int? limit, [FromQuery] int? offset, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            _logger.LogWarning("GetAlbumTracks called with empty id.");
            return BadRequest(new { error = "Album id is required." });
        }

        var query = new Dictionary<string, string?>();
        if (!string.IsNullOrWhiteSpace(market)) query["market"] = market;
        if (limit.HasValue) query["limit"] = limit.Value.ToString();
        if (offset.HasValue) query["offset"] = offset.Value.ToString();
        var path = $"/v1/albums/{id}/tracks{ToQueryString(query)}";

        using var apiResponse = await _spotifyClient.MakeAuthenticatedRequest(path, cancellationToken);
        var content = await apiResponse.Content.ReadAsStringAsync(cancellationToken);

        if (apiResponse.IsSuccessStatusCode)
        {
            _logger.LogDebug("Album tracks retrieved: {Id}", id);
            return Content(content, "application/json");
        }

        _logger.LogWarning("Spotify API error for album tracks {Id}: {StatusCode}", id, apiResponse.StatusCode);
        Response.StatusCode = (int)apiResponse.StatusCode;
        return Content(content, "application/json");
    }

    private static string ToQueryString(Dictionary<string, string?> query)
    {
        var pairs = query.Where(kv => !string.IsNullOrEmpty(kv.Value)).Select(kv => $"{Uri.EscapeDataString(kv.Key)}={Uri.EscapeDataString(kv.Value!)}");
        return pairs.Any() ? "?" + string.Join("&", pairs) : string.Empty;
    }
}
