using System.Diagnostics;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using CommunityToolkit.Mvvm.Input;
using Wallsh.Services;

namespace Wallsh;

public class TrayCommandHandler
{
    public ICommand ToggleApp { get; } = new RelayCommand(ShowAppFromTray);
    public ICommand ExitApp { get; } = new RelayCommand(ExitAppFromTray);
    public ICommand OpenWebsite { get; } = new RelayCommand(OpenWebsiteFromTray);
    public ICommand OpenWallpapersFolder { get; } = new RelayCommand(OpenWallpapersFolderFromTray);

    private static void OpenWallpapersFolderFromTray()
    {
        var cfg = AppConfiguration.FromFile();
        
        Process.Start(new ProcessStartInfo
        {
            FileName = cfg.WallpapersDirectory,
            UseShellExecute = true
        });
    }

    private static void OpenWebsiteFromTray()
    {
        Process.Start(new ProcessStartInfo
        {
            FileName = "https://github.com/JokkeeZ/Wallsh",
            UseShellExecute = true
        });
    }

    private static void ShowAppFromTray()
    {
        if (Application.Current?.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime
            { MainWindow: not null } desktop) return;
        
        if (!desktop.MainWindow.IsVisible)
        {
            desktop.MainWindow.Show();
        }
        else
        {
            desktop.MainWindow.Activate();
        }
    }
    
    private static void ExitAppFromTray()
    {
        if (Application.Current?.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime
                { MainWindow: not null } desktop) return;
        
        desktop.Shutdown();
    }
}
