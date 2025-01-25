using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Wallsh.Models;
using Wallsh.Models.Environments;
using Wallsh.Services.Bing;

namespace Wallsh.Changers;

public class BingWallpaperChanger(IWpEnvironment env) : IWallpaperChanger
{
    private readonly ILogger<BingWallpaperChanger> _log = App.CreateLogger<BingWallpaperChanger>();
    private BingResponse? _latestResponse;
    
    public async Task OnChange(WallpaperManager manager)
    {
        if (ShouldFetchNewWallpapers(manager))
            _latestResponse = await FetchWallpapersAsync(manager);

        if (_latestResponse is null)
        {
            _log.LogError("_latestResponse is null after attempting to fetch wallpapers. Requesting stop.");
            manager.RequestStop();
            return;
        }

        var folder = manager.GetChangerDownloadFolderPath();
        var notOnDisk = _latestResponse.Images
            .Where(wp => wp.Urlbase != null)
            .Where(wp => !manager.FileExistsInChangerDownloadFolder(GetWallpaperNameFromUrl(wp.Urlbase!)))
            .ToList();

        if (notOnDisk.Count == 0)
        {
            SetRandomWallpaperFromDisk(manager, folder);
            return;
        }

        await DownloadAndSetWallpaper(manager, folder, notOnDisk.First());
    }

    public bool ShouldFetchNewWallpapers(WallpaperManager manager)
    {
        var timeDiff = DateTime.Now - manager.Config.Bing.LastFetchTime;
        return _latestResponse is null || timeDiff.TotalHours >= 12;
    }

    public void Reset(WallpaperManager manager) => _latestResponse = null;

    public string GetWallpaperNameFromUrl(string url)
    {
        var idPart = url.Split("?id=OHR.").LastOrDefault() ?? string.Empty;
        var wpName = idPart.Split('_').FirstOrDefault() ?? string.Empty;
        return $"{wpName}.jpg";
    }

    private void SetRandomWallpaperFromDisk(WallpaperManager manager, string folder)
    {
        var wpPath = manager.GetRandomWallpaperFromDisk(folder);
        if (wpPath is null)
        {
            _log.LogError("No wallpapers found on disk to set. Requesting stop.");
            manager.RequestStop();
            return;
        }

        _log.LogDebug("Setting a random wallpaper from disk.");
        env.SetWallpaperFromPath(wpPath);
    }

    private async Task<BingResponse?> FetchWallpapersAsync(WallpaperManager manager)
    {
        var response = await new BingRequest().RequestWallpapersAsync<BingResponse>(manager.Config.Bing);
        if (response is null)
        {
            _log.LogError("Failed to fetch wallpapers (response is null).");
            return null;
        }

        manager.Config.Bing.LastFetchTime = DateTime.Now;
        return response;
    }

    private async Task DownloadAndSetWallpaper(WallpaperManager manager, string folder,
        BingWallpaperImage bingWallpaperImage)
    {
        var wpName = GetWallpaperNameFromUrl(bingWallpaperImage.Urlbase!);
        var queryUri = $"https://www.bing.com{bingWallpaperImage.Urlbase}_{manager.Config.Bing.Resolution}.jpg";
        var wpPath = await new BingRequest().DownloadWallpaperAsync(folder, wpName, queryUri);

        if (wpPath is null)
        {
            _log.LogError("Failed to download wallpaper. Requesting stop.");
            manager.RequestStop();
            return;
        }

        _log.LogDebug("Setting the downloaded wallpaper.");
        env.SetWallpaperFromPath(wpPath);
    }
}
