using Wallsh.Services;

namespace Wallsh.Models;

public interface IWallpaperChangerService
{
    void OnChange(WallpaperChanger changer, AppJsonConfiguration cfg);
}
