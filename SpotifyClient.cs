using System.Net;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace SpotifyPlaylists;

public class SpotifyClient
{
    public HttpClient _spotifyClient = new HttpClient();
    public SpotifyClient(HttpClient spotifyClient)
    {
        _spotifyClient = spotifyClient;
    }

    public async Task<string?> GetOAuthToken(string clientId, string clientSecret, string tokenUrl)
    {
        var requestBody = new StringContent(
            $"grant_type=client_credentials&client_id={clientId}&client_secret={clientSecret}",
            Encoding.UTF8,
            "application/x-www-form-urlencoded"
        );

        var response = await _spotifyClient.PostAsync(tokenUrl, requestBody);

        if (response.IsSuccessStatusCode)
        {
            var responseContent = await response.Content.ReadAsStringAsync();
            using (var doc = JsonDocument.Parse(responseContent))
            {
                return doc.RootElement.GetProperty("access_token").GetString();
            }
        }
        else
        {
            Console.WriteLine("Error fetching token: " + response.StatusCode);
            return null;
        }
    }

    public void AttachBearerToken(string token)
    {
        _spotifyClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
    }

    public async Task<HttpResponseMessage> MakeAuthenticatedRequest(string url)
    {
        var response = await _spotifyClient.GetAsync(url);
        if (!response.IsSuccessStatusCode) Console.WriteLine($"Error making authenticated request to {url}: " + response.StatusCode);
        return response;
    }
}
