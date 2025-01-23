using Microsoft.Extensions.Logging;
using Wallsh.Models;
using Wallsh.Services.Bing;

namespace Wallsh.Changers;

public class BingWallpaperChanger : IWallpaperChanger
{
    private readonly ILogger<BingWallpaperChanger> _log = App.CreateLogger<BingWallpaperChanger>();
    private BingResponse? _latestResponse;

    public async Task OnChange(WallpaperChanger changer)
    {
        var bingRequest = new BingRequest();

        var timeDiff = DateTime.Now - changer.Config.Bing.LastFetchTime;

        // It's first time requesting OR it has been less than
        // 12 hours since the last request.
        if (_latestResponse is null || timeDiff.TotalHours >= 12)
        {
            var response = await bingRequest.RequestWallpapersAsync<BingResponse>(changer.Config.Bing);

            if (response is null)
            {
                changer.Stop();
                _log.LogError("Could not be request wallpapers. (response is null)");
                return;
            }

            _latestResponse = response;
            changer.Config.Bing.LastFetchTime = DateTime.Now;
        }

        var folder = changer.GetChangerDownloadFolderPath();

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
            _log.LogDebug("All queried wallpapers already exists on disk.");

            var wp = changer.GetRandomWallpaperFromDisk(folder);

            if (wp is null)
            {
                changer.Stop();
                _log.LogError("Could not set random wallpaper from disk. (wp is null)");
                return;
            }

            _log.LogDebug("Disk: Setting wallpaper.");
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
            _log.LogError("Wallpaper could not be downloaded. (wallpaperPath is null)");
            return;
        }

        _log.LogDebug("Download: Setting wallpaper.");
        changer.WpEnvironment.SetWallpaperFromPath(wallpaperPath);
    }

    public void Reset(WallpaperChanger changer) => _latestResponse = null;

    private static string GetWallpaperName(string urlBase) =>
        urlBase.Split("?id=OHR.").Last().Split('_').First() + ".jpg";
}
