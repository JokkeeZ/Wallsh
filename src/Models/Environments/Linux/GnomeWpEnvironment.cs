using System.Runtime.Versioning;

namespace Wallsh.Models.Environments.Linux;

[SupportedOSPlatform("linux")]
public class GnomeWpEnvironment : IWpEnvironment
{
    private const string SchemaId = "org.gnome.desktop.background";
    private const string PictureOptions = "picture-options";
    private const string PictureUri = "picture-uri";
    private const string PictureUriDark = "picture-uri-dark";
    private const string EnvXdgCurrentDesktop = "XDG_CURRENT_DESKTOP";
    private const string Gnome = "GNOME";

    public string[] SupportedFileExtensions => ["*.jpg", "*.png"];
    public string[] WallpaperAdjustments => ["none", "wallpaper", "centered", "scaled", "stretched", "zoom", "spanned"];

    public string GetWallpaperAdjustment()
    {
        using var gSettings = new GSettings(SchemaId);
        var adjustment = gSettings.GetString(PictureOptions);

        if (adjustment is not null)
            return adjustment;

        return string.Empty;
    }

    public void SetWallpaperAdjustment(string? adjustment)
    {
        if (string.IsNullOrWhiteSpace(adjustment))
            return;

        using var gSettings = new GSettings(SchemaId);
        gSettings.SetString(PictureOptions, adjustment);
    }

    public string GetCurrentWallpaperPath()
    {
        using var gSettings = new GSettings(SchemaId);
        var pictureUri = gSettings.GetString(PictureUri);

        if (!string.IsNullOrWhiteSpace(pictureUri))
            return pictureUri.Split("file://").Last();

        return string.Empty;
    }

    public void SetWallpaperFromPath(string path)
    {
        using var gSettings = new GSettings(SchemaId);
        gSettings.SetString(PictureUri, $"file://{path}");
        gSettings.SetString(PictureUriDark, $"file://{path}");
    }

    public static bool IsGnome()
    {
        var de = Environment.GetEnvironmentVariable(EnvXdgCurrentDesktop);
        return de is not null && de.Contains(Gnome);
    }
}
