using Wallsh.Services;

namespace Wallsh.Models;

public interface IWallpaperHandler
{
    void OnChange(WallpaperChanger changer, AppJsonConfiguration cfg);
}
