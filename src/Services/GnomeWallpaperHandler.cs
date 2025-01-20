using GLib;

namespace Wallsh.Services;

public static class GnomeWallpaperHandler
{
    private const string SchemaId = "org.gnome.desktop.background";
    private static readonly string[] Extensions = ["*.jpg", "*.png"];

    public static void SetLocalRandomWallpaper(string searchDirectory)
    {
        var currentWallpaper = GetCurrentWallpaperPath().Split("file://")[1];

        var directory = new DirectoryInfo(searchDirectory);
        var wallpapers = Extensions.SelectMany(directory.EnumerateFiles)
            .Where(wp => wp.FullName != currentWallpaper)
            .ToArray();

        if (wallpapers.Length == 0)
        {
            Console.WriteLine("No wallpapers found");
            return;
        }

        var nextWallpaper = wallpapers[Random.Shared.Next(wallpapers.Length)];
        Console.WriteLine($"{Path.GetFileName(currentWallpaper)} -> {nextWallpaper.Name}");
        SetWallpaper(nextWallpaper.FullName);
    }

    private static string GetCurrentWallpaperPath()
    {
        using var settings = new Settings(SchemaId);
        return settings.GetString("picture-uri");
    }

    public static string GetCurrentAdjustment()
    {
        using var settings = new Settings(SchemaId);
        return settings.GetString("picture-options");
    }

    public static void SetAdjustment(string? adjustment)
    {
        if (adjustment == null)
            return;

        using var settings = new Settings(SchemaId);
        settings.SetString("picture-options", adjustment);
    }

    public static bool IsGnome()
    {
        var de = Environment.GetEnvironmentVariable("XDG_CURRENT_DESKTOP");
        return de is not null && de.Contains("GNOME");
    }

    public static void SetWallpaper(string path)
    {
        using var settings = new Settings(SchemaId);
        settings.SetString("picture-uri", $"file://{path}");
        settings.SetString("picture-uri-dark", $"file://{path}");
    }
}
