using Microsoft.Extensions.Logging;
using Wallsh.Models;
using Wallsh.Services.Wallhaven;

namespace Wallsh.Changers;

public class WallhavenWallpaperChanger : IWallpaperChanger
{
    private readonly ILogger<WallhavenWallpaperChanger> _log = App.CreateLogger<WallhavenWallpaperChanger>();
    private WallhavenApiResponse? _latestResponse;

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
        var notOnDisk = _latestResponse.Data.Where(wp =>
                !changer.FileExistsInChangerDownloadFolder(GetWallpaperNameFromUrl(wp.Path!)))
            .ToList();

        if (notOnDisk.Count == 0)
        {
            SetRandomWallpaperFromDisk(changer, folder);
            return;
        }

        await DownloadAndSetWallpaper(changer, folder, notOnDisk);
    }

    public bool ShouldFetchNewWallpapers(WallpaperChanger changer) => _latestResponse is null;

    public void Reset(WallpaperChanger changer)
    {
        changer.Config.Wallhaven.Page = 1;
        _latestResponse = null;
    }

    public string GetWallpaperNameFromUrl(string url) => url.Split('/').Last();

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

    private async Task<WallhavenApiResponse?> FetchWallpapersAsync(WallpaperChanger changer)
    {
        var response =
            await new WallhavenRequest().RequestWallpapersAsync<WallhavenApiResponse>(changer.Config.Wallhaven);
        if (response is null)
        {
            _log.LogError("Failed to fetch wallpapers (response is null). Stopping changer.");
            changer.RequestStop();
        }

        return response;
    }

    private async Task DownloadAndSetWallpaper(WallpaperChanger changer, string folder,
        List<WallhavenWallpaperInfo> wallpapers)
    {
        var randomWp = wallpapers[Random.Shared.Next(wallpapers.Count)];
        var wpName = GetWallpaperNameFromUrl(randomWp.Path!);
        var wpPath = await new WallhavenRequest().DownloadWallpaperAsync(folder, wpName, randomWp.Path!);
        
        if (wpPath is null)
        {
            _log.LogError("Failed to download wallpaper. Stopping changer.");
            changer.RequestStop();
            return;
        }

        _log.LogDebug("Download: Setting wallpaper.");
        changer.WpEnvironment.SetWallpaperFromPath(wpPath);
    }
}
