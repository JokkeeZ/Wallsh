namespace Wallsh.Models;

public interface IWpEnvironment
{
    string[] SupportedFileExtensions { get; }
    string[] WallpaperAdjustments { get; }
    string GetWallpaperAdjustment();
    void SetWallpaperAdjustment(string? adjustment);
    string GetCurrentWallpaperPath();
    void SetWallpaperFromPath(string path);
}
