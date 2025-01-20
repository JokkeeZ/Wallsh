using Wallsh.Models.Wallhaven;

namespace Wallsh.Models;

public class AppJsonConfiguration
{
    public WallpaperService Service { get; set; } = WallpaperService.None;
    public int Hours { get; set; }
    public int Minutes { get; set; } = 10;
    public int Seconds { get; set; }
    public string WallpapersDirectory { get; set; } = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
    public string? WallpaperAdjustment { get; set; }
    public WallhavenConfiguration Wallhaven { get; init; } = new();
}
