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

    [HttpGet("{id}/albums")]
    public async Task<IActionResult> GetArtistAlbums(string id, [FromQuery] string? market, [FromQuery] string? includeGroups, [FromQuery] int? limit, [FromQuery] int? offset, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            _logger.LogWarning("GetArtistAlbums called with empty id.");
            return BadRequest(new { error = "Artist id is required." });
        }

        var query = BuildQuery(market, limit, offset);
        if (!string.IsNullOrEmpty(includeGroups)) query["include_groups"] = includeGroups;
        var path = $"/v1/artists/{id}/albums{ToQueryString(query)}";

        using var apiResponse = await _spotifyClient.MakeAuthenticatedRequest(path, cancellationToken);
        var content = await apiResponse.Content.ReadAsStringAsync(cancellationToken);

        if (apiResponse.IsSuccessStatusCode)
        {
            _logger.LogDebug("Artist albums retrieved: {Id}", id);
            return Content(content, "application/json");
        }

        _logger.LogWarning("Spotify API error for artist albums {Id}: {StatusCode}", id, apiResponse.StatusCode);
        Response.StatusCode = (int)apiResponse.StatusCode;
        return Content(content, "application/json");
    }

    [HttpGet("{id}/top-tracks")]
    public async Task<IActionResult> GetArtistTopTracks(string id, [FromQuery] string? market, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            _logger.LogWarning("GetArtistTopTracks called with empty id.");
            return BadRequest(new { error = "Artist id is required." });
        }

        var query = new Dictionary<string, string?>();
        if (!string.IsNullOrWhiteSpace(market)) query["market"] = market;
        var path = $"/v1/artists/{id}/top-tracks{ToQueryString(query)}";

        using var apiResponse = await _spotifyClient.MakeAuthenticatedRequest(path, cancellationToken);
        var content = await apiResponse.Content.ReadAsStringAsync(cancellationToken);

        if (apiResponse.IsSuccessStatusCode)
        {
            _logger.LogDebug("Artist top tracks retrieved: {Id}", id);
            return Content(content, "application/json");
        }

        _logger.LogWarning("Spotify API error for artist top tracks {Id}: {StatusCode}", id, apiResponse.StatusCode);
        Response.StatusCode = (int)apiResponse.StatusCode;
        return Content(content, "application/json");
    }

    [HttpGet("{id}/related-artists")]
    public async Task<IActionResult> GetRelatedArtists(string id, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            _logger.LogWarning("GetRelatedArtists called with empty id.");
            return BadRequest(new { error = "Artist id is required." });
        }

        using var apiResponse = await _spotifyClient.MakeAuthenticatedRequest($"/v1/artists/{id}/related-artists", cancellationToken);
        var content = await apiResponse.Content.ReadAsStringAsync(cancellationToken);

        if (apiResponse.IsSuccessStatusCode)
        {
            _logger.LogDebug("Related artists retrieved: {Id}", id);
            return Content(content, "application/json");
        }

        _logger.LogWarning("Spotify API error for related artists {Id}: {StatusCode}", id, apiResponse.StatusCode);
        Response.StatusCode = (int)apiResponse.StatusCode;
        return Content(content, "application/json");
    }

    private static Dictionary<string, string?> BuildQuery(string? market, int? limit, int? offset)
    {
        var q = new Dictionary<string, string?>();
        if (!string.IsNullOrWhiteSpace(market)) q["market"] = market;
        if (limit.HasValue) q["limit"] = limit.Value.ToString();
        if (offset.HasValue) q["offset"] = offset.Value.ToString();
        return q;
    }

    private static string ToQueryString(Dictionary<string, string?> query)
    {
        if (query.Count == 0) return string.Empty;
        var pairs = query.Where(kv => !string.IsNullOrEmpty(kv.Value)).Select(kv => $"{Uri.EscapeDataString(kv.Key)}={Uri.EscapeDataString(kv.Value!)}");
        return "?" + string.Join("&", pairs);
    }
}
