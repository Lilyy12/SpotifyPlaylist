using Microsoft.AspNetCore.Mvc;

namespace SpotifyPlaylists.Controller;

[ApiController]
[Route("api/v1")]
public class SearchController : ControllerBase
{
    private readonly SpotifyClient _spotifyClient;
    private readonly ILogger<SearchController> _logger;

    public SearchController(SpotifyClient spotifyClient, ILogger<SearchController> logger)
    {
        _spotifyClient = spotifyClient;
        _logger = logger;
    }

    [HttpGet("search")]
    public async Task<IActionResult> Search(
        [FromQuery] string? q,
        [FromQuery] string? type,
        [FromQuery] string? market,
        [FromQuery] int? limit,
        [FromQuery] int? offset,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(q))
        {
            _logger.LogWarning("Search called with empty q.");
            return BadRequest(new { error = "Query parameter 'q' is required." });
        }

        if (string.IsNullOrWhiteSpace(type))
        {
            _logger.LogWarning("Search called with empty type.");
            return BadRequest(new { error = "Query parameter 'type' is required (e.g. artist,track,album,playlist)." });
        }

        var query = new Dictionary<string, string?>
        {
            ["q"] = q,
            ["type"] = type
        };
        if (!string.IsNullOrWhiteSpace(market)) query["market"] = market;
        if (limit.HasValue) query["limit"] = limit.Value.ToString();
        if (offset.HasValue) query["offset"] = offset.Value.ToString();

        var path = "/v1/search" + ToQueryString(query);

        using var apiResponse = await _spotifyClient.MakeAuthenticatedRequest(path, cancellationToken);
        var content = await apiResponse.Content.ReadAsStringAsync(cancellationToken);

        if (apiResponse.IsSuccessStatusCode)
        {
            _logger.LogDebug("Search completed: {Q}", q);
            return Content(content, "application/json");
        }

        _logger.LogWarning("Spotify API search error: {StatusCode}", apiResponse.StatusCode);
        Response.StatusCode = (int)apiResponse.StatusCode;
        return Content(content, "application/json");
    }

    private static string ToQueryString(Dictionary<string, string?> query)
    {
        var pairs = query.Where(kv => !string.IsNullOrEmpty(kv.Value)).Select(kv => $"{Uri.EscapeDataString(kv.Key)}={Uri.EscapeDataString(kv.Value!)}");
        return pairs.Any() ? "?" + string.Join("&", pairs) : string.Empty;
    }
}
