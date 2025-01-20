using Wallsh.Models;

namespace Wallsh.Services.Wallhaven;

public class WallhavenHandler : IWallpaperHandler
{
    public void OnChange(WallpaperChanger changer, AppJsonConfiguration cfg)
    {
        var task = Task.Run(async () =>
            await WallhavenRequest.RequestWallpapersAsync(cfg.Wallhaven));

        var wallpapers = task.Result;
        if (wallpapers is null)
        {
            cfg.Handler = WallpaperHandler.None;
            changer.Stop();
            return;
        }

        var randomWallpaper = wallpapers.Data[Random.Shared.Next(wallpapers.Data.Count)];

        var requestTask = Task.Run(async () =>
            await WallhavenRequest.DownloadWallPaperAsync(cfg, randomWallpaper));

        var wallpaperPath = requestTask.Result;
        if (wallpaperPath is null)
        {
            cfg.Handler = WallpaperHandler.None;
            changer.Stop();
            return;
        }

        if (GnomeWallpaperHandler.IsGnome())
            GnomeWallpaperHandler.SetWallpaper(wallpaperPath);
    }
}
