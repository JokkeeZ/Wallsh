using Wallsh.Models;

namespace Wallsh.Handlers;

public class LocalHandler : IWpService
{
    public async Task OnChange(WallpaperChanger changer)
    {
        var randomWallpaperPath = changer.GetRandomWallpaperFromDisk(changer.Config.WallpapersFolder);
        changer.WpEnvironment.SetWallpaperFromPath(randomWallpaperPath);

        await Task.CompletedTask;
    }

    public void Reset(WallpaperChanger changer) => Console.WriteLine("Local reset");
}
