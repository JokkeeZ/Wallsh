namespace Wallsh.Models;

public interface IWpService
{
    Task OnChange(WallpaperChanger changer);

    void Reset(WallpaperChanger changer);
}
