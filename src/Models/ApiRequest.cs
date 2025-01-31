using System.Net.Http.Json;
using Microsoft.Extensions.Logging;

namespace Wallsh.Models;

public abstract class ApiRequest<TConfig> where TConfig : class, new()
{
    private readonly ILogger<ApiRequest<TConfig>> _log = App.CreateLogger<ApiRequest<TConfig>>();

    protected abstract UriBuilder BuildRequestUri(TConfig cfg);

    public async Task<string?> DownloadWallpaperAsync(string folder, string fileName, string fileUri)
    {
        using var client = new HttpClient();

        try
        {
            await using var stream = await client.GetStreamAsync(fileUri);
            await using var fileStream = new FileStream(Path.Combine(folder, fileName), FileMode.OpenOrCreate);
            await stream.CopyToAsync(fileStream);

            return fileStream.Name;
        }
        catch (Exception ex) when (ex is HttpRequestException or InvalidOperationException or UriFormatException)
        {
            _log.LogError("Wallpaper download request failed: {Error}", ex.Message);
            return null;
        }
        catch (Exception e)
        {
            _log.LogError("Exception was thrown: {Error}", e.ToString());
            return null;
        }
    }

    public async Task<T?> RequestWallpapersAsync<T>(TConfig cfg) where T : class, IApiResponse
    {
        var uri = BuildRequestUri(cfg);
        _log.LogDebug("Requesting wallpaper: {Uri}", uri.Uri.AbsoluteUri);

        using var http = new HttpClient();

        try
        {
            var response = await http.GetAsync(uri.Uri.AbsoluteUri);

            if (!response.IsSuccessStatusCode)
            {
                _log.LogError("Received StatusCode: {StatusCode}", response.StatusCode);
                return null;
            }

            if (response.Content.Headers.ContentType?.MediaType != "application/json")
            {
                _log.LogError("Received Content-Type: {ContentType}", response.Content.Headers.ContentType?.MediaType);
                return null;
            }

            return await response.Content.ReadFromJsonAsync<T>();
        }
        catch (Exception ex) when (ex is HttpRequestException or InvalidOperationException or UriFormatException)
        {
            _log.LogError("Wallpaper fetch request failed: {Error}", ex.Message);
            return null;
        }
        catch (Exception e)
        {
            _log.LogError("Exception was thrown: {Error}", e.ToString());
            return null;
        }
    }
}
