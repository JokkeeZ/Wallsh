namespace Wallsh.Models;

public interface IWallpaperChanger
{
    Task OnChange(WallpaperManager manager);

    void Reset(WallpaperManager manager) { }

    bool ShouldFetchNewWallpapers(WallpaperManager manager) => true;

    string GetWallpaperNameFromUrl(string url) => string.Empty;
}
