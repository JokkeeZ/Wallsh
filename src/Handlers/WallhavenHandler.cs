using Wallsh.Models;
using Wallsh.Services;
using Wallsh.Services.Wallhaven;

namespace Wallsh.Handlers;

public class WallhavenHandler : IWallpaperHandler
{
    public void OnChange(WallpaperChanger changer, AppJsonConfiguration cfg)
    {
        var task = Task.Run(async () =>
            await WallhavenRequest.RequestWallpapersAsync(cfg.Wallhaven));

        var wallpapers = task.GetAwaiter().GetResult();
        if (wallpapers is null)
        {
            changer.Stop();
            return;
        }

        var randomWallpaper = wallpapers.Data[Random.Shared.Next(wallpapers.Data.Count)];

        var requestTask = Task.Run(async () =>
            await WallhavenRequest.DownloadWallPaperAsync(cfg, randomWallpaper));

        var wallpaperPath = requestTask.GetAwaiter().GetResult();
        if (wallpaperPath is null)
        {
            changer.Stop();
            return;
        }

        changer.WpEnvironment.SetWallpaperFromPath(wallpaperPath);
    }
}
