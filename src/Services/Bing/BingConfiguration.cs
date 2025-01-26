using System.Text.Json.Serialization;
using Avalonia.Platform;

namespace Wallsh.Services.Bing;

public class BingConfiguration
{
    [JsonIgnore]
    public static readonly Dictionary<ScreenOrientation, List<string>> Resolutions = new()
    {
        [ScreenOrientation.Landscape] =
        [
            "UHD", "1920x1200", "1920x1080", "1366x768", "1280x768", "1024x768", "800x600", "800x480", "640x480",
            "400x240", "320x240"
        ],
        [ScreenOrientation.Portrait] = ["768x1280", "720x1280", "480x800", "240x320"]
    };

    public string Resolution { get; set; } = "1920x1080";

    public ScreenOrientation Orientation { get; set; } = ScreenOrientation.Landscape;

    public int NumberOfWallpapers { get; set; } = 4;

    public DateTime LastFetchTime { get; set; } = DateTime.MinValue;
}
