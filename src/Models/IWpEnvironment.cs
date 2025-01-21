using System.Runtime.InteropServices;

namespace Wallsh.Models;

public interface IWpEnvironment
{
    OSPlatform OsPlatform { get; }
    string[] SupportedFileExtensions { get; }
    string GetWallpaperAdjustment();
    void SetWallpaperAdjustment(string? adjustment);
    string GetCurrentWallpaperPath();
    void SetWallpaperFromPath(string path);
}
