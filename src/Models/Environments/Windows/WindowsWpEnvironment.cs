using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using Microsoft.Win32;

namespace Wallsh.Models.Environments.Windows;

[SuppressMessage("Globalization", "CA2101:Specify marshaling for P/Invoke string arguments")]
[SupportedOSPlatform("windows")]
public class WindowsWpEnvironment : IWpEnvironment
{
    private const string RegCpDesktop = @"Control Panel\Desktop";
    private const string RegWallpapers = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\Wallpapers";
    private const string RegKeyWallpaperStyle = "WallpaperStyle";
    private const string RegKeyTileWallpaper = "TileWallpaper";

    // ReSharper disable once InconsistentNaming
    private const int SPI_SETDESKWALLPAPER = 0x0014;

    // ReSharper disable once InconsistentNaming
    private const int SPIF_SENDWININICHANGE = 0x002;

    // ReSharper disable once InconsistentNaming
    private const int SPIF_UPDATEINIFILE = 0x001;
    public string[] SupportedFileExtensions => ["*.png", "*.jpg", "*.bmp"];

    public string[] WallpaperAdjustments => ["center", "fill", "fit", "stretch", "tile", "span"];

    public string GetWallpaperAdjustment()
    {
        var key = Registry.CurrentUser.OpenSubKey(RegCpDesktop, true);

        if (key is not null)
        {
            var adjustment = (string)key.GetValue(RegKeyWallpaperStyle, "0");
            var isTiled = (string)key.GetValue(RegKeyTileWallpaper, "0");

            return adjustment switch
            {
                "10" => "fill",
                "6" => "fit",
                "2" => "stretch",
                "0" => isTiled == "1" ? "tile" : "center",
                "22" => "span",
                _ => throw new("Unknown wallpaper adjustment")
            };
        }

        return string.Empty;
    }

    public void SetWallpaperAdjustment(string? adjustment)
    {
        var (style, tiled) = adjustment switch
        {
            "fill" => (10, false),
            "fit" => (6, false),
            "stretch" => (2, false),
            "tile" => (0, true),
            "center" => (0, false),
            "span" => (22, false),
            _ => throw new("Unknown wallpaper adjustment.")
        };

        var key = Registry.CurrentUser.OpenSubKey(RegCpDesktop, true);

        if (key is not null)
        {
            key.SetValue(RegKeyWallpaperStyle, style.ToString());
            key.SetValue(RegKeyTileWallpaper, tiled ? "1" : "0");
        }
    }

    public string GetCurrentWallpaperPath()
    {
        using var key = Registry.CurrentUser.OpenSubKey(RegWallpapers, true);
        var historyPath = key?.GetValue("BackgroundHistoryPath0");

        if (historyPath is not null)
            return (string)historyPath;

        return string.Empty;
    }

    public void SetWallpaperFromPath(string path) =>
        _ = SystemParametersInfo(SPI_SETDESKWALLPAPER, 0, path, SPIF_UPDATEINIFILE | SPIF_SENDWININICHANGE);

    [DllImport("user32.dll", CharSet = CharSet.Ansi)]
    private static extern int SystemParametersInfo(int uAction, int uParam, string lpvParam, int fuWinIni);
}
