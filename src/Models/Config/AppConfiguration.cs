using System.Text.Json.Serialization;
using Wallsh.Services.Bing;
using Wallsh.Services.Wallhaven;

namespace Wallsh.Models.Config;

public class AppConfiguration : IJsonFile
{
    public WallpaperChangerType ChangerType { get; set; } = WallpaperChangerType.None;
    public TimeSpan Interval { get; set; } = new(0, 0, 10, 0);
    public string WallpapersFolder { get; set; } = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
    public string? WallpaperAdjustment { get; set; }
    public WallhavenConfiguration Wallhaven { get; init; } = new();
    public BingConfiguration Bing { get; init; } = new();

    public List<string> IncludeFolders { get; set; } = [];

    [JsonIgnore]
    public string? FilePath { get; set; }
}
