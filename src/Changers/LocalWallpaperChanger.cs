using Microsoft.Extensions.Logging;
using Wallsh.Models;

namespace Wallsh.Changers;

public class LocalWallpaperChanger : IWallpaperChanger
{
    private readonly ILogger<LocalWallpaperChanger> _log = App.CreateLogger<LocalWallpaperChanger>();

    public async Task OnChange(WallpaperChanger changer)
    {
        var wpPath = changer.GetRandomWallpaperFromDisk(changer.Config.WallpapersFolder);

        if (wpPath is null)
        {
            changer.RequestStop();
            _log.LogError("Could not set random wallpaper from disk. (wp is null)");
            return;
        }

        _log.LogDebug("Setting a random wallpaper from disk.");
        changer.WpEnvironment.SetWallpaperFromPath(wpPath);

        await Task.CompletedTask;
    }
}
