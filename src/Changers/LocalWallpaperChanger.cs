using Microsoft.Extensions.Logging;
using Wallsh.Models;
using Wallsh.Models.Environments;

namespace Wallsh.Changers;

public class LocalWallpaperChanger(IWpEnvironment env) : IWallpaperChanger
{
    private readonly ILogger<LocalWallpaperChanger> _log = App.CreateLogger<LocalWallpaperChanger>();

    public async Task OnChange(WallpaperManager manager)
    {
        var wpPath = manager.GetRandomWallpaperFromDisk(manager.Config.WallpapersFolder);

        if (wpPath is null)
        {
            _log.LogError("Could not set random wallpaper from disk (wpPath is null). Requesting stop.");
            manager.RequestStop();
            return;
        }

        _log.LogDebug("Setting a random wallpaper from disk.");
        env.SetWallpaperFromPath(wpPath);

        await Task.CompletedTask;
    }
}
