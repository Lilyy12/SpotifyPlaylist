using SpotifyPlaylists;
using SpotifyPlaylists.Model;
using SpotifyPlaylists.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile("config.json", optional: true);
builder.Services.Configure<OAuthConfig>(builder.Configuration);
builder.Services.AddMemoryCache();
builder.Services.AddHttpClient<SpotifyTokenService>()
    .ConfigureHttpClient(c => c.BaseAddress = new Uri("https://accounts.spotify.com"));
builder.Services.AddHttpClient<SpotifyClient>()
    .ConfigureHttpClient(c => c.BaseAddress = new Uri("https://api.spotify.com"));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();   

app.UseRouting();

app.UseAuthorization();

app.MapControllers();

app.Run();
