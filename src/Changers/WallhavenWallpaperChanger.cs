using Microsoft.Extensions.Logging;
using Wallsh.Models;
using Wallsh.Models.Environments;
using Wallsh.Services.Wallhaven;

namespace Wallsh.Changers;

public class WallhavenWallpaperChanger(IWpEnvironment env) : IWallpaperChanger
{
    private readonly ILogger<WallhavenWallpaperChanger> _log = App.CreateLogger<WallhavenWallpaperChanger>();
    private WallhavenApiResponse? _latestResponse;

    public async Task OnChange(WallpaperManager manager)
    {
        if (ShouldFetchNewWallpapers(manager))
            _latestResponse = await FetchWallpapersAsync(manager);

        if (_latestResponse is null)
        {
            _log.LogError("_latestResponse is null after attempting to fetch wallpapers. Requesting stop");
            manager.RequestStop();
            return;
        }

        var folder = manager.GetChangerDownloadFolderPath();
        var notOnDisk = _latestResponse.Data.Where(wp =>
                !manager.FileExistsInChangerDownloadFolder(GetWallpaperNameFromUrl(wp.Path!)))
            .ToList();

        if (notOnDisk.Count == 0)
        {
            SetRandomWallpaperFromDisk(manager, folder);
            return;
        }

        await DownloadAndSetWallpaper(manager, folder, notOnDisk);
    }

    public bool ShouldFetchNewWallpapers(WallpaperManager manager) => _latestResponse is null;

    public void Reset(WallpaperManager manager)
    {
        manager.Config.Wallhaven.Page = 1;
        _latestResponse = null;
    }

    public string GetWallpaperNameFromUrl(string url) => url.Split('/').Last();

    private void SetRandomWallpaperFromDisk(WallpaperManager manager, string folder)
    {
        var wpPath = manager.GetRandomWallpaperFromDisk(folder);
        if (wpPath is null)
        {
            _log.LogError("No wallpapers found on disk to set. Requesting stop");
            manager.RequestStop();
            return;
        }

        _log.LogDebug("Setting a random wallpaper from disk.");
        env.SetWallpaperFromPath(wpPath);
    }

    private async Task<WallhavenApiResponse?> FetchWallpapersAsync(WallpaperManager manager)
    {
        var response =
            await new WallhavenRequest().RequestWallpapersAsync<WallhavenApiResponse>(manager.Config.Wallhaven);
        if (response is null)
        {
            _log.LogError("Failed to fetch wallpapers (response is null). Requesting stop");
            manager.RequestStop();
        }

        return response;
    }

    private async Task DownloadAndSetWallpaper(WallpaperManager manager, string folder,
        List<WallhavenWallpaperInfo> wallpapers)
    {
        var randomWp = wallpapers[Random.Shared.Next(wallpapers.Count)];
        var wpName = GetWallpaperNameFromUrl(randomWp.Path!);
        var wpPath = await new WallhavenRequest().DownloadWallpaperAsync(folder, wpName, randomWp.Path!);

        if (wpPath is null)
        {
            _log.LogError("Failed to download wallpaper. Requesting stop");
            manager.RequestStop();
            return;
        }

        _log.LogDebug("Download: Setting wallpaper.");
        env.SetWallpaperFromPath(wpPath);
    }
}
