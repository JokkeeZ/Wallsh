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
        if (ShouldFetchNewWallpapers(changer))
            _latestResponse = await FetchWallpapersAsync(changer);

        if (_latestResponse is null)
        {
            _log.LogError("_latestResponse is null after attempting to fetch wallpapers. Stopping changer.");
            changer.RequestStop();
            return;
        }

        var folder = changer.GetChangerDownloadFolderPath();
        var notOnDisk = _latestResponse.Images
            .Where(wp => wp.Urlbase != null)
            .Where(wp => !changer.FileExistsInChangerDownloadFolder(GetWallpaperNameFromUrl(wp.Urlbase!)))
            .ToList();

        if (notOnDisk.Count == 0)
        {
            SetRandomWallpaperFromDisk(changer, folder);
            return;
        }

        await DownloadAndSetWallpaper(changer, folder, notOnDisk);
    }

    public bool ShouldFetchNewWallpapers(WallpaperChanger changer)
    {
        var timeDiff = DateTime.Now - changer.Config.Bing.LastFetchTime;
        return _latestResponse is null || timeDiff.TotalHours >= 12;
    }

    public void Reset(WallpaperChanger changer) => _latestResponse = null;

    public string GetWallpaperNameFromUrl(string url)
    {
        var idPart = url.Split("?id=OHR.").LastOrDefault() ?? string.Empty;
        var wpName = idPart.Split('_').FirstOrDefault() ?? string.Empty;
        return $"{wpName}.jpg";
    }

    private void SetRandomWallpaperFromDisk(WallpaperChanger changer, string folder)
    {
        var wpPath = changer.GetRandomWallpaperFromDisk(folder);
        if (wpPath is null)
        {
            _log.LogError("No wallpapers found on disk to set. Stopping changer.");
            changer.RequestStop();
            return;
        }

        _log.LogDebug("Setting a random wallpaper from disk.");
        changer.WpEnvironment.SetWallpaperFromPath(wpPath);
    }
    
    private async Task<BingResponse?> FetchWallpapersAsync(WallpaperChanger changer)
    {
        var response = await new BingRequest().RequestWallpapersAsync<BingResponse>(changer.Config.Bing);
        if (response is null)
        {
            _log.LogError("Failed to fetch wallpapers (response is null).");
            return null;
        }

        changer.Config.Bing.LastFetchTime = DateTime.Now;
        return response;
    }

    private async Task DownloadAndSetWallpaper(WallpaperChanger changer, string folder,
        List<BingWallpaperImage> wallpapers)
    {
        var randomWp = wallpapers[Random.Shared.Next(wallpapers.Count)];
        var wpName = GetWallpaperNameFromUrl(randomWp.Urlbase!);
        var queryUri = $"https://www.bing.com{randomWp.Urlbase}_{changer.Config.Bing.Resolution}.jpg";
        var wpPath = await new BingRequest().DownloadWallpaperAsync(folder, wpName, queryUri);

        if (wpPath is null)
        {
            _log.LogError("Failed to download wallpaper. Stopping changer.");
            changer.RequestStop();
            return;
        }

        _log.LogDebug("Setting the downloaded wallpaper.");
        changer.WpEnvironment.SetWallpaperFromPath(wpPath);
    }
}
