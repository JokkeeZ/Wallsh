namespace Wallsh.Models;

public interface IWallpaperChanger
{
    Task OnChange(WallpaperChanger changer);

    void Reset(WallpaperChanger changer) { }

    bool ShouldFetchNewWallpapers(WallpaperChanger changer) => true;

    string GetWallpaperNameFromUrl(string url) => string.Empty;
}
