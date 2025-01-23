namespace Wallsh.Models;

public interface IWallpaperChanger
{
    Task OnChange(WallpaperChanger changer);

    void Reset(WallpaperChanger changer);
}
