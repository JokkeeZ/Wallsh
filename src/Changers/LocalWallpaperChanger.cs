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
        var folders = manager.Config.IncludeFolders.Append(manager.Config.WallpapersFolder).ToList();
        var randomFolder = folders[Random.Shared.Next(folders.Count)];

        if (!SetRandomWallpaperFromFolder(manager, randomFolder, out var wpPath))
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

    public void ManuallySetWallpaperFromPath(string path)
    {
        WpEnvironment.SetWallpaperFromPath(path);

        SaveWallpaperToHistory(path!, isLocal: true);
        WeakReferenceMessenger.Default.Send(new WallpaperUpdatedMessage());

        _log.LogDebug("Wallpaper set manually from the disk.");
    }
}
