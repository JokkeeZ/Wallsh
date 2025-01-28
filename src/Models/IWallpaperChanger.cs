namespace Wallsh.Models;

public interface IWallpaperChanger
{
    Task OnChange(WallpaperManager manager);

    void Reset(WallpaperManager manager) { }
}
