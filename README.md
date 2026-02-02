# LilyPlaylists

A .NET 8 ASP.NET Core Web API that proxies read-only requests to the [Spotify Web API](https://developer.spotify.com/documentation/web-api). It handles OAuth (client credentials), token caching, and exposes search, artists, albums, tracks, and public playlists.

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- A [Spotify app](https://developer.spotify.com/dashboard) (Client ID and Client Secret)

## Setup

1. **Clone and restore**

   ```bash
   cd LilyPlaylists
   dotnet restore
   ```

2. **Configure Spotify credentials**

   Create a `config.json` in the project root (this file is gitignored):

   ```json
   {
     "client_id": "YOUR_SPOTIFY_CLIENT_ID",
     "client_secret": "YOUR_SPOTIFY_CLIENT_SECRET",
     "token_url": "https://accounts.spotify.com/api/token"
   }
   ```

   Or use [User Secrets](https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets) / environment variables and set `client_id`, `client_secret`, and optionally `token_url` at the configuration root.

3. **Run**

   ```bash
   dotnet run --project SpotifyPlaylists.csproj
   ```

   In Development, Swagger UI is available at `/swagger`.

## API Endpoints

Base path: **`api/v1`**. All proxy endpoints return Spotify’s JSON and mirror Spotify’s status codes.

### Token

| Method | Path | Description |
|--------|------|-------------|
| `POST` | `/api/v1/token` | Get an access token. Body: `{ "client_id": "...", "client_secret": "..." }`. |

### Search

| Method | Path | Description |
|--------|------|-------------|
| `GET` | `/api/v1/search` | Proxy to [Spotify Search](https://developer.spotify.com/documentation/web-api/reference/search). |

**Query parameters**

- **`q`** (required) – Search query.
- **`type`** (required) – Comma-separated: `artist`, `track`, `album`, `playlist`.
- **`market`** (optional)
- **`limit`**, **`offset`** (optional)

**Example:** `GET /api/v1/search?q=foo&type=artist,track`

### Artists

| Method | Path | Description |
|--------|------|-------------|
| `GET` | `/api/v1/artists/{id}` | Get artist by ID. |
| `GET` | `/api/v1/artists/{id}/albums` | Get artist’s albums. Optional: `market`, `include_groups`, `limit`, `offset`. |
| `GET` | `/api/v1/artists/{id}/top-tracks` | Get artist’s top tracks. Optional: `market`. |
| `GET` | `/api/v1/artists/{id}/related-artists` | Get related artists. |

### Albums

| Method | Path | Description |
|--------|------|-------------|
| `GET` | `/api/v1/albums/{id}` | Get album by ID. Optional: `market`. |
| `GET` | `/api/v1/albums/{id}/tracks` | Get album tracks. Optional: `market`, `limit`, `offset`. |

### Tracks

| Method | Path | Description |
|--------|------|-------------|
| `GET` | `/api/v1/tracks/{id}` | Get track by ID. Optional: `market`. |

### Playlists (public only)

| Method | Path | Description |
|--------|------|-------------|
| `GET` | `/api/v1/playlists/{id}` | Get playlist by ID. Optional: `market`, `fields`. |
| `GET` | `/api/v1/playlists/{id}/tracks` | Get playlist tracks. Optional: `market`, `fields`, `limit`, `offset`. |

## Example requests

```bash
# Get token (body with your app credentials)
curl -X POST http://localhost:5000/api/v1/token \
  -H "Content-Type: application/json" \
  -d '{"client_id":"YOUR_ID","client_secret":"YOUR_SECRET"}'

# Search
curl "http://localhost:5000/api/v1/search?q=beatles&type=artist,track"

# Artist
curl "http://localhost:5000/api/v1/artists/0OdUWJ0sBjDrqHygGUXeCF"

# Artist top tracks
curl "http://localhost:5000/api/v1/artists/0OdUWJ0sBjDrqHygGUXeCF/top-tracks?market=US"

# Album and tracks
curl "http://localhost:5000/api/v1/albums/4iV5W9uYEdYUVa79Axb7Rh"
curl "http://localhost:5000/api/v1/albums/4iV5W9uYEdYUVa79Axb7Rh/tracks"

# Track
curl "http://localhost:5000/api/v1/tracks/3n3Ppam7vgaVa1iaRUc9Lp"

# Playlist and tracks
curl "http://localhost:5000/api/v1/playlists/37i9dQZEVXbMDoHDwVN2tF"
curl "http://localhost:5000/api/v1/playlists/37i9dQZEVXbMDoHDwVN2tF/tracks"
```

(Replace host/port with your `launchSettings.json` or environment values.)

## Project structure

```
LilyPlaylists/
├── Controller/          # API controllers (artists, search, albums, tracks, playlists, token)
├── Model/               # DTOs and config (OAuthConfig, TokenRequest, TokenResult)
├── Services/            # Token caching (ISpotifyTokenService, SpotifyTokenService)
├── SpotifyClient.cs     # Spotify API client (token + authenticated requests)
├── Program.cs            # App setup, DI, config
├── config.json          # Local OAuth config (gitignored)
└── SpotifyPlaylists.csproj
```

## License

Unlicense or your choice.
