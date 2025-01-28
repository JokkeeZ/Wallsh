using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.Logging;
using Wallsh.Messages;
using Wallsh.Models;
using Wallsh.Models.Environments;
using Wallsh.Services.Bing;

namespace Wallsh.Changers;

public sealed class BingWallpaperChanger(IWpEnvironment env) :
    FetchWallpaperChanger<BingResponse, BingWallpaper>(env),
    IWallpaperChanger
{
    private readonly ILogger<BingWallpaperChanger> _log = App.CreateLogger<BingWallpaperChanger>();

    public override async Task OnChange(WallpaperManager manager)
    {
        if (ShouldFetchNewWallpapers(manager))
        {
            LatestResponse = await new BingRequest().RequestWallpapersAsync<BingResponse>(manager.Config.Bing);
            manager.Config.Bing.LastFetchTime = DateTime.Now;
        }

        if (LatestResponse is null)
        {
            _log.LogError("LatestResponse is null after attempting to fetch wallpapers. Requesting stop.");
            manager.RequestStop();
            return;
        }

        var folder = manager.GetChangerDownloadFolderPath();
        var notOnDisk = LatestResponse.Images
            .Where(wp => wp.Urlbase != null)
            .Where(wp => !manager.FileExistsInChangerDownloadFolder(GetWallpaperNameFromUrl(wp.Urlbase!)))
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

        await DownloadAndSetWallpaper(manager, folder, notOnDisk.First());
    }

    public void Reset(WallpaperManager manager) => LatestResponse = null;

    protected override bool ShouldFetchNewWallpapers(WallpaperManager manager)
    {
        var timeDiff = DateTime.Now - manager.Config.Bing.LastFetchTime;
        return LatestResponse is null || timeDiff.TotalHours >= 12;
    }

    protected override string GetWallpaperNameFromUrl(string url)
    {
        var idPart = url.Split("?id=OHR.").LastOrDefault() ?? string.Empty;
        var wpName = idPart.Split('_').FirstOrDefault() ?? string.Empty;
        return $"{wpName}.jpg";
    }

    protected override async Task DownloadAndSetWallpaper(WallpaperManager manager, string folder,
        BingWallpaper img)
    {
        var wpName = GetWallpaperNameFromUrl(img.Urlbase!);
        var queryUri = $"https://www.bing.com{img.Urlbase}_{manager.Config.Bing.Resolution}.jpg";
        var wpPath = await new BingRequest().DownloadWallpaperAsync(folder, wpName, queryUri);

        if (wpPath is null)
        {
            _log.LogError("Failed to download wallpaper. Requesting stop.");
            manager.RequestStop();
            return;
        }

        _log.LogDebug("Setting the downloaded wallpaper.");
        WpEnvironment.SetWallpaperFromPath(wpPath);

        SaveWallpaperToHistory(wpPath, copyright: img.Copyright, url: img.Copyrightlink);
        WeakReferenceMessenger.Default.Send(new WallpaperUpdatedMessage());
    }
}
