using Wallsh.Models;
using Wallsh.Services.Bing;

namespace Wallsh.Handlers;

public class BingHandler : IWpService
{
    private BingResponse? _latestResponse;

    public async Task OnChange(WallpaperChanger changer)
    {
        var bingRequest = new BingRequest();

        // Create downloads folder if it does not already exist.
        var folder = Path.Combine(changer.Config.WallpapersFolder, "bing");
        Directory.CreateDirectory(folder);

        var timeDiff = DateTime.Now - changer.Config.Bing.LastFetchTime;

        // It's first time requesting OR it has been less than
        // 12 hours since the last request.
        if (_latestResponse is null || timeDiff.TotalHours >= 12)
        {
            Console.WriteLine($"Time diff: {timeDiff.TotalHours}");
            var response = await bingRequest.RequestWallpapersAsync<BingResponse>(changer.Config.Bing);

            if (response is null)
            {
                changer.Stop();
                Console.WriteLine("[BingHandler]: Could not be request wallpapers. (response is null)");
                return;
            }

            _latestResponse = response;
            changer.Config.Bing.LastFetchTime = DateTime.Now;
        }

        var notOnDisk = new List<BingWallpaperImage>();
        foreach (var wp in _latestResponse.Images)
        {
            if (wp.Urlbase is null)
                continue;

            var filePath = Path.Combine(folder, GetWallpaperName(wp.Urlbase));

            if (!File.Exists(filePath))
                notOnDisk.Add(wp);
        }

        // All wallpapers from _latestResponse has been downloaded already,
        // so let's set random wallpaper from disk that is not current one.
        if (notOnDisk.Count == 0)
        {
            Console.WriteLine("[BingHandler]: All queried wallpapers already exists on disk.");

            var wp = changer.GetRandomWallpaperFromDisk(folder);

            Console.WriteLine("[BingHandler][DISK]: Setting wallpaper.");
            changer.WpEnvironment.SetWallpaperFromPath(wp);

            return;
        }

        var randomWallpaper = notOnDisk[Random.Shared.Next(notOnDisk.Count)];

        var queryUri = string.Join("",
            "https://www.bing.com",
            randomWallpaper.Urlbase,
            $"_{changer.Config.Bing.Resolution}.jpg");

        var wpName = GetWallpaperName(randomWallpaper.Urlbase!);
        var wallpaperPath = await bingRequest.DownloadWallpaperAsync(folder, wpName, queryUri);
        if (wallpaperPath is null)
        {
            changer.Stop();
            Console.WriteLine("[BingHandler]: Wallpaper could not be downloaded. (wallpaperPath is null)");
            return;
        }

        Console.WriteLine("[BingHandler][DOWNLOAD]: Setting wallpaper.");
        changer.WpEnvironment.SetWallpaperFromPath(wallpaperPath);
    }

    public void Reset(WallpaperChanger changer) => _latestResponse = null;

    private static string GetWallpaperName(string urlBase) =>
        urlBase.Split("?id=OHR.").Last().Split('_').First() + ".jpg";
}
