using GLib;

namespace Wallsh.Models.Environments;

public class GnomeWpEnvironment : IWpEnvironment
{
    private const string SchemaId = "org.gnome.desktop.background";
    public string[] SupportedFileExtensions => ["*.jpg", "*.png"];

    public string GetWallpaperAdjustment()
    {
        using var settings = new Settings(SchemaId);
        return settings.GetString("picture-options");
    }

    public void SetWallpaperAdjustment(string? adjustment)
    {
        if (adjustment is null)
            return;

        using var settings = new Settings(SchemaId);
        settings.SetString("picture-options", adjustment);
    }

    public string GetCurrentWallpaperPath()
    {
        using var settings = new Settings(SchemaId);
        var pictureUri = settings.GetString("picture-uri");

        if (!string.IsNullOrEmpty(pictureUri))
            return pictureUri.Split("file://").Last();

        return string.Empty;
    }

    public void SetWallpaperFromPath(string path)
    {
        using var settings = new Settings(SchemaId);
        settings.SetString("picture-uri", $"file://{path}");
        settings.SetString("picture-uri-dark", $"file://{path}");
    }

    public static bool IsGnome()
    {
        var de = Environment.GetEnvironmentVariable("XDG_CURRENT_DESKTOP");
        return de is not null && de.Contains("GNOME");
    }
}
