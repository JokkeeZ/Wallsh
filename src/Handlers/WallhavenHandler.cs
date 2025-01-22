using Wallsh.Models;
using Wallsh.Models.Wallhaven;
using Wallsh.Services;

namespace Wallsh.Handlers;

public class WallhavenHandler : IWallpaperHandler
{
    private WallhavenApiResponse? _latestResponse;

    public void OnChange(WallpaperChanger changer)
    {
        // Request wallpapers if there is no stored wallpaper urls
        if (_latestResponse is null)
        {
            var task = Task.Run(async () =>
                await WallhavenRequest.RequestWallpapersAsync(changer.Config.Wallhaven));

            var response = task.GetAwaiter().GetResult();
            if (response is null)
            {
                changer.Stop();
                Console.WriteLine("[WallhavenHandler]: Could not be request wallpapers. (response is null)");
                return;
            }

            _latestResponse = response;
        }

        var folder = Path.Combine(changer.Config.WallpapersFolder, "wallhaven");

        var notOnDisk = new List<WallhavenWallpaperInfo>();
        foreach (var wp in _latestResponse.Data)
        {
            var fileName = Path.Combine(folder, wp.Path!.Split('/').Last());

            if (!File.Exists(fileName))
                notOnDisk.Add(wp);
        }

        // All wallpapers from _latestResponse has been downloaded already,
        // so let's set random wallpaper from disk that is not current one.
        if (notOnDisk.Count == 0)
        {
            Console.WriteLine("[WallhavenHandler]: All queried wallpapers already exists on disk.");

            var wp = changer.GetRandomWallpaperFromDisk(folder);

            Console.WriteLine("[WallhavenHandler][DISK]: Setting wallpaper.");
            changer.WpEnvironment.SetWallpaperFromPath(wp);

            if (_latestResponse.Meta!.CurrentPage < _latestResponse.Meta.LastPage)
            {
                changer.Config.Wallhaven.Page++;
                _latestResponse = null;
            }

            return;
        }

        var randomWallpaper = notOnDisk[Random.Shared.Next(notOnDisk.Count)];

        var requestTask = Task.Run(async () =>
            await WallhavenRequest.DownloadWallPaperAsync(changer.Config.WallpapersFolder, randomWallpaper));

        var wallpaperPath = requestTask.GetAwaiter().GetResult();
        if (wallpaperPath is null)
        {
            changer.Stop();
            Console.WriteLine("[WallhavenHandler]: Wallpaper could not be downloaded. (wallpaperPath is null)");
            return;
        }

        Console.WriteLine(
            $"[WallhavenHandler][DOWNLOAD]: Setting wallpaper. ({_latestResponse.Data.Count - notOnDisk.Count}/{_latestResponse.Data.Count})");
        changer.WpEnvironment.SetWallpaperFromPath(wallpaperPath);
    }

    public void Reset(WallpaperChanger changer)
    {
        changer.Config.Wallhaven.Page = 1;
        _latestResponse = null;
    }
}
