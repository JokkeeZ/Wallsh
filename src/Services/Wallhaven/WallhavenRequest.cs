using System.Net.Http.Json;

namespace Wallsh.Services.Wallhaven;

public static class WallhavenRequest
{
    private static Uri BuildRequestUri(WallhavenConfiguration cfg)
    {
        var qParams = new Dictionary<string, string>
        {
            ["categories"] = cfg.CategoriesToString(),
            ["purity"] = cfg.PurityToString(),
            ["sorting"] = cfg.SortingToString(),
            ["atleast"] = cfg.Resolution,
            ["page"] = cfg.Page.ToString(),
            ["ratios"] = cfg.RatioToString()
        };

        if (!string.IsNullOrWhiteSpace(cfg.ApiKey))
            qParams.Add("apikey", cfg.ApiKey);

        var uri = new UriBuilder
        {
            Scheme = "http",
            Host = "wallhaven.cc",
            Path = "api/v1/search",
            Query = string.Join("&", qParams.Select(kv => $"{kv.Key}={kv.Value}"))
        };

        return uri.Uri;
    }

    public static async Task<WallhavenApiResponse?> RequestWallpapersAsync(WallhavenConfiguration cfg)
    {
        var uri = BuildRequestUri(cfg);
        Console.WriteLine($"[WallhavenRequest] - {uri.AbsoluteUri}");

        using var http = new HttpClient();
        var response = await http.GetAsync(uri.AbsoluteUri);

        if (!response.IsSuccessStatusCode)
        {
            Console.WriteLine($"[WallhavenRequest] - {response.StatusCode}");
            return null;
        }

        if (response.Content.Headers.ContentType?.MediaType == "application/json")
            return await response.Content.ReadFromJsonAsync<WallhavenApiResponse>();

        Console.WriteLine("[WallhavenRequest] - Invalid content type");
        return null;
    }

    public static async Task<string?> DownloadWallPaperAsync(string folder, WallhavenWallpaperInfo wpInfo)
    {
        var fileName = wpInfo.Path?.Split('/').Last();

        if (string.IsNullOrEmpty(fileName))
        {
            Console.WriteLine("[WallhavenRequest][DownloadWallPaperAsync] - FileName is null or empty");
            return null;
        }

        // Create downloads folder if it does not already exist.
        var downloadsFolder = Path.Combine(folder, "wallhaven");
        Directory.CreateDirectory(downloadsFolder);

        using var client = new HttpClient();
        await using var s = await client.GetStreamAsync(wpInfo.Path);
        await using var fs = new FileStream(Path.Combine(downloadsFolder, fileName), FileMode.OpenOrCreate);
        await s.CopyToAsync(fs);

        return fs.Name;
    }
}
