using System.Text.Json;
using Wallsh.Services.Bing;
using Wallsh.Services.Wallhaven;

namespace Wallsh.Models;

public class AppJsonConfiguration
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    public WallpaperHandler Handler { get; set; } = WallpaperHandler.None;

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

    public static AppJsonConfiguration FromFile()
    {
        if (!File.Exists(GetConfigurationPath()))
        {
            Console.WriteLine("[AppConfiguration] Could not locate existing `config.json`.");
            return new();
        }

        try
        {
            var json = File.ReadAllText(GetConfigurationPath());
            return JsonSerializer.Deserialize<AppJsonConfiguration>(json) ?? new();
        }
        catch
        {
            Console.WriteLine("[AppConfiguration] Failed to deserialize.");
            return new();
        }
    }

    public static bool ToFile(AppJsonConfiguration jsonConfiguration)
    {
        try
        {
            var text = JsonSerializer.Serialize(jsonConfiguration, JsonOptions);
            File.WriteAllText(GetConfigurationPath(), text);

            return true;
        }
        catch
        {
            return false;
        }
    }
}
