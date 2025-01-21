using Wallsh.Models;
using Wallsh.Services;

namespace Wallsh.Handlers;

public class LocalHandler : IWallpaperHandler
{
    public void OnChange(WallpaperChanger changer, AppJsonConfiguration cfg)
    {
        var currentWallpaper = changer.WpEnvironment.GetCurrentWallpaperPath();

        var directory = new DirectoryInfo(changer.Config.WallpapersFolder);
        var wallpapers = changer.WpEnvironment.SupportedFileExtensions
            .SelectMany(directory.EnumerateFiles)
            .Where(wp => wp.FullName != currentWallpaper)
            .ToArray();

        if (wallpapers.Length == 0)
        {
            Console.WriteLine("No wallpapers found");
            return;
        }

        var nextWallpaper = wallpapers[Random.Shared.Next(wallpapers.Length)];
        changer.WpEnvironment.SetWallpaperFromPath(nextWallpaper.FullName);
    }
}
