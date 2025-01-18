using Wallsh.Models;

namespace Wallsh.Services;

public class LocalWallpaperService : IWallpaperChangerService
{
    public void OnChange(WallpaperChanger changer, AppJsonConfiguration cfg)
    {
        if (GnomeWallpaperHandler.IsGnome())
        {
            GnomeWallpaperHandler.SetLocalRandomWallpaper(cfg.WallpapersDirectory);
        }
    }
}