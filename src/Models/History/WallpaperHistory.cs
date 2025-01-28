using System.Text.Json.Serialization;
using Wallsh.Models.Config;

namespace Wallsh.Models.History;

public class WallpaperInfo
{
    public string? Path { get; init; }
    public string? Copyright { get; init; }
    public string? Url { get; init; }
    public string? Resolution { get; init; }
    public bool IsLocal { get; init; }
}

public class WallpaperHistory : IJsonFile
{
    public bool Enabled { get; set; } = true;

    public int MaxItems { get; set; } = 5;

    // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Global
    public Queue<WallpaperInfo> Wallpapers { get; init; } = [];

    [JsonIgnore]
    public string? FilePath { get; set; }

    public void AddWallpaper(WallpaperInfo wpInfo)
    {
        if (Wallpapers.Count >= MaxItems)
            Wallpapers.Dequeue();

        Wallpapers.Enqueue(wpInfo);
    }
}
