using Wallsh.Models;

namespace Wallsh.Services;

public class LocalHandler : IWallpaperHandler
{
    public void OnChange(WallpaperChanger changer, AppJsonConfiguration cfg)
    {
        if (GnomeWallpaperHandler.IsGnome())
            GnomeWallpaperHandler.SetLocalRandomWallpaper(cfg.WallpapersFolder);
    }
}
