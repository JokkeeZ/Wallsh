using Wallsh.Models;

namespace Wallsh.Changers;

public class LocalWallpaperChanger : IWallpaperChanger
{
    public async Task OnChange(WallpaperChanger changer)
    {
        var randomWallpaperPath = changer.GetRandomWallpaperFromDisk(changer.Config.WallpapersFolder);
        changer.WpEnvironment.SetWallpaperFromPath(randomWallpaperPath);

        await Task.CompletedTask;
    }

    public void Reset(WallpaperChanger changer) => Console.WriteLine("Local reset");
}
