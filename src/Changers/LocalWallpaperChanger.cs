using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.Logging;
using Wallsh.Messages;
using Wallsh.Models;
using Wallsh.Models.Environments;

namespace Wallsh.Changers;

public class LocalWallpaperChanger(IWpEnvironment env) : GenericWallpaperChanger(env), IWallpaperChanger
{
    private readonly ILogger<LocalWallpaperChanger> _log = App.CreateLogger<LocalWallpaperChanger>();

    public override async Task OnChange(WallpaperManager manager)
    {
        if (!SetRandomWallpaperFromFolder(manager, manager.Config.WallpapersFolder, out var wpPath))
        {
            _log.LogError("Could not set random wallpaper from disk. Requesting stop.");
            manager.RequestStop();
            return;
        }

        SaveWallpaperToHistory(wpPath!, isLocal: true);
        WeakReferenceMessenger.Default.Send(new WallpaperUpdatedMessage());

        _log.LogDebug("Set random wallpaper from disk completed.");
        await Task.CompletedTask;
    }
}
