using System.Text.Json;
using Wallsh.Services.Bing;
using Wallsh.Services.Wallhaven;

namespace Wallsh.Models;

public class AppConfiguration
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    public WallpaperChangerType ChangerType { get; set; } = WallpaperChangerType.None;
    public TimeOnly Interval { get; set; } = new(0, 10, 0, 0);
    public string WallpapersFolder { get; set; } = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
    public string? WallpaperAdjustment { get; set; } = string.Empty;
    public WallhavenConfiguration Wallhaven { get; init; } = new();
    public BingConfiguration Bing { get; init; } = new();

    private static string GetConfigurationPath()
    {
        var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        var wallshConfig = Path.Combine(home, ".config", "wallsh");

        // Create ~/.config/wallsh/ folder unless it already exists.
        Directory.CreateDirectory(wallshConfig);

        return Path.Combine(wallshConfig, "config.json");
    }

    public static AppConfiguration FromFile()
    {
        if (!File.Exists(GetConfigurationPath()))
            return new();

        try
        {
            var json = File.ReadAllText(GetConfigurationPath());
            return JsonSerializer.Deserialize<AppConfiguration>(json) ?? new();
        }
        catch
        {
            return new();
        }
    }

    public bool ToFile()
    {
        try
        {
            var text = JsonSerializer.Serialize(this, JsonOptions);
            File.WriteAllText(GetConfigurationPath(), text);

            return true;
        }
        catch
        {
            return false;
        }
    }
}
