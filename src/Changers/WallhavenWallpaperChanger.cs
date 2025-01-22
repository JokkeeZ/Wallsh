using Wallsh.Models;
using Wallsh.Services.Wallhaven;

namespace Wallsh.Changers;

public class WallhavenWallpaperChanger : IWallpaperChanger
{
    private WallhavenApiResponse? _latestResponse;

    public async Task OnChange(WallpaperChanger changer)
    {
        var wallhavenRequest = new WallhavenRequest();

        // Request wallpapers if there is no stored wallpaper urls
        if (_latestResponse is null)
        {
            var response =
                await wallhavenRequest.RequestWallpapersAsync<WallhavenApiResponse>(changer.Config.Wallhaven);

            if (response is null)
            {
                changer.Stop();
                Console.WriteLine("[WallhavenHandler]: Could not be request wallpapers. (response is null)");
                return;
            }

            _latestResponse = response;
        }

        var folder = changer.GetChangerDownloadFolderPath();
        
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
        var wpName = randomWallpaper.Path!.Split('/').Last();

        var wallpaperPath = await wallhavenRequest.DownloadWallpaperAsync(folder, wpName, randomWallpaper.Path);
        if (wallpaperPath is null)
        {
            changer.Stop();
            Console.WriteLine("[WallhavenHandler]: Wallpaper could not be downloaded. (wallpaperPath is null)");
            return;
        }

        Console.WriteLine("[WallhavenHandler][DOWNLOAD]: Setting wallpaper.");
        changer.WpEnvironment.SetWallpaperFromPath(wallpaperPath);
    }

    public void Reset(WallpaperChanger changer)
    {
        changer.Config.Wallhaven.Page = 1;
        _latestResponse = null;
    }
}
