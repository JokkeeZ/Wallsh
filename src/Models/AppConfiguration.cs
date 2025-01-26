using System.Text.Json;
using System.Text.Json.Serialization;
using Wallsh.Services.Bing;
using Wallsh.Services.Wallhaven;

namespace Wallsh.Models;

public class AppConfiguration
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    [JsonIgnore]
    private string? ConfigurationFileLocation { get; set; }

    public WallpaperChangerType ChangerType { get; set; } = WallpaperChangerType.None;
    public TimeOnly Interval { get; set; } = new(0, 10, 0, 0);
    public string WallpapersFolder { get; set; } = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
    public string? WallpaperAdjustment { get; set; }
    public WallhavenConfiguration Wallhaven { get; init; } = new();
    public BingConfiguration Bing { get; init; } = new();

    public static AppConfiguration FromFile(string fileName)
    {
        try
        {
            var filePath = Path.Combine(GetFolderPath(), fileName);

            var json = File.ReadAllText(filePath);
            var config = JsonSerializer.Deserialize<AppConfiguration>(json) ?? new();
            config.ConfigurationFileLocation = filePath;

            return config;
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
            // Create ~/.config/wallsh folder if not exist
            Directory.CreateDirectory(GetFolderPath());

            var text = JsonSerializer.Serialize(this, JsonOptions);
            File.WriteAllText(ConfigurationFileLocation!, text);

            return true;
        }
        catch
        {
            return false;
        }
    }

    private static string GetFolderPath()
    {
        var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        var folderPath = Path.Combine(home, ".config", "wallsh");

        return folderPath;
    }
}
