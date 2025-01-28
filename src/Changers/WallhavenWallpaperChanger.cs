using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.Logging;
using Wallsh.Messages;
using Wallsh.Models;
using Wallsh.Models.Environments;
using Wallsh.Services.Wallhaven;

namespace Wallsh.Changers;

public class WallhavenWallpaperChanger(IWpEnvironment env) :
    FetchWallpaperChanger<WallhavenApiResponse, WallhavenImage>(env),
    IWallpaperChanger
{
    private readonly ILogger<WallhavenWallpaperChanger> _log = App.CreateLogger<WallhavenWallpaperChanger>();

    public override async Task OnChange(WallpaperManager manager)
    {
        if (ShouldFetchNewWallpapers(manager))
        {
            LatestResponse =
                await new WallhavenRequest().RequestWallpapersAsync<WallhavenApiResponse>(manager.Config.Wallhaven);
        }

        if (LatestResponse is null)
        {
            _log.LogError("LatestResponse is null after attempting to fetch wallpapers. Requesting stop");
            manager.RequestStop();
            return;
        }

        var folder = manager.GetChangerDownloadFolderPath();
        var notOnDisk = LatestResponse.Data.Where(wp =>
                !manager.FileExistsInChangerDownloadFolder(GetWallpaperNameFromUrl(wp.Path!)))
            .ToList();

        if (notOnDisk.Count == 0)
        {
            if (!SetRandomWallpaperFromFolder(manager, folder, out var filePath))
            {
                _log.LogError("Could not set random wallpaper from disk. Requesting stop.");
                manager.RequestStop();
                return;
            }

            SaveWallpaperToHistory(filePath!, isLocal: true);
            WeakReferenceMessenger.Default.Send(new WallpaperUpdatedMessage());

            _log.LogDebug("Set random wallpaper from disk completed.");
            return;
        }

        var randomWp = notOnDisk[Random.Shared.Next(notOnDisk.Count)];
        await DownloadAndSetWallpaper(manager, folder, randomWp);
    }

    public void Reset(WallpaperManager manager)
    {
        manager.Config.Wallhaven.Page = 1;
        LatestResponse = null;
    }

    protected override async Task DownloadAndSetWallpaper(WallpaperManager manager, string folder,
        WallhavenImage img)
    {
        var wpName = GetWallpaperNameFromUrl(img.Path!);
        var wpPath = await new WallhavenRequest().DownloadWallpaperAsync(folder, wpName, img.Path!);

        if (wpPath is null)
        {
            _log.LogError("Failed to download wallpaper. Requesting stop");
            manager.RequestStop();
            return;
        }

        _log.LogDebug("Setting the downloaded wallpaper.");
        WpEnvironment.SetWallpaperFromPath(wpPath);

        SaveWallpaperToHistory(wpPath, img.Resolution, url: img.Url);
        WeakReferenceMessenger.Default.Send(new WallpaperUpdatedMessage());
    }
}
