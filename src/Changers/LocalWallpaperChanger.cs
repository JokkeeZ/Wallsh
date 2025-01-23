using Microsoft.Extensions.Logging;
using Wallsh.Models;

namespace Wallsh.Changers;

public class LocalWallpaperChanger : IWallpaperChanger
{
    private readonly ILogger<LocalWallpaperChanger> _log = App.CreateLogger<LocalWallpaperChanger>();

    public async Task OnChange(WallpaperChanger changer)
    {
        var wp = changer.GetRandomWallpaperFromDisk(changer.Config.WallpapersFolder);

        if (wp is null)
        {
            changer.Stop();
            _log.LogError("Could not set random wallpaper from disk. (wp is null)");
            return;
        }

        changer.WpEnvironment.SetWallpaperFromPath(wp);
        await Task.CompletedTask;
    }

    public void Reset(WallpaperChanger changer)
    {
    }
}
