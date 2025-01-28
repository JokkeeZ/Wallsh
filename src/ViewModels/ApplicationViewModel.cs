using System.Diagnostics;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using Wallsh.Models.Config;

namespace Wallsh.ViewModels;

public partial class ApplicationViewModel : ObservableObject
{
    [RelayCommand]
    private void Exit()
    {
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            desktop.Shutdown();
    }

    [RelayCommand]
    private void OpenWallpapersFolderFromTray()
    {
        var cfg = Ioc.Default.GetRequiredService<AppConfiguration>();

        Process.Start(new ProcessStartInfo
        {
            FileName = cfg.WallpapersFolder,
            UseShellExecute = true
        });
    }

    [RelayCommand]
    private void OpenWebsiteFromTray() =>
        Process.Start(new ProcessStartInfo
        {
            FileName = "https://github.com/JokkeeZ/Wallsh",
            UseShellExecute = true
        });

    [RelayCommand]
    private void ShowAppFromTray()
    {
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            if (!desktop.MainWindow!.IsVisible)
                desktop.MainWindow.Show();
            else
                desktop.MainWindow.Activate();
        }
    }

    [RelayCommand]
    private void ExitAppFromTray()
    {
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            desktop.Shutdown();
    }
}
