namespace Wallsh.Models;

public interface IWpService
{
    void OnChange(WallpaperChanger changer);

    void Reset(WallpaperChanger changer);
}
