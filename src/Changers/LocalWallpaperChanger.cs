using Wallsh.Models;

namespace Wallsh.Changers;

public class LocalWallpaperChanger : IWallpaperChanger
{
    public async Task OnChange(WallpaperChanger changer)
    {
        var wp = changer.GetRandomWallpaperFromDisk(changer.Config.WallpapersFolder);

        if (wp is null)
        {
            changer.Stop();
            Console.WriteLine("[LocalChanger]: Could not set random wallpaper from the disk. (wp is null)");
            return;
        }

        changer.WpEnvironment.SetWallpaperFromPath(wp);
        await Task.CompletedTask;
    }

    public void Reset(WallpaperChanger changer)
    {
    }
}
