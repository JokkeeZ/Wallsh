using System.Net.Http.Json;

namespace Wallsh.Models;

public abstract class ApiRequest<TConfig> where TConfig : class, new()
{
    public abstract UriBuilder BuildRequestUri(TConfig cfg);

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
        catch (Exception e)
        {
            Console.WriteLine($"[ApiRequest<{typeof(TConfig).Name}>]: Error downloading wallpaper: {e.Message}");
            return null;
        }
    }

    public async Task<T?> RequestWallpapersAsync<T>(TConfig cfg) where T : class, IApiResponse
    {
        var uri = BuildRequestUri(cfg);
        Console.WriteLine($"[ApiRequest<{typeof(TConfig).Name}>]: {uri.Uri.AbsoluteUri}");

        using var http = new HttpClient();
        var response = await http.GetAsync(uri.Uri.AbsoluteUri);

        if (!response.IsSuccessStatusCode)
        {
            Console.WriteLine($"[ApiRequest<{typeof(TConfig).Name}>]: {response.StatusCode}");
            return null;
        }

        if (response.Content.Headers.ContentType?.MediaType == "application/json")
            return await response.Content.ReadFromJsonAsync<T>();

        Console.WriteLine($"[ApiRequest<{typeof(TConfig).Name}>]: Invalid content type");
        return null;
    }
}
