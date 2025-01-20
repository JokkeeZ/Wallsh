using System.Diagnostics;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using CommunityToolkit.Mvvm.Input;
using Wallsh.Models;
using Wallsh.ViewModels;
using Wallsh.Views;

namespace Wallsh;

public class App : Application
{
    public static ICommand OpenWallpapersFolderFromTray =>
        new RelayCommand(() =>
        {
            var cfg = AppJsonConfiguration.FromFile();

            Process.Start(new ProcessStartInfo
            {
                FileName = cfg.WallpapersFolder,
                UseShellExecute = true
            });
        });

    public static ICommand OpenWebsiteFromTray =>
        new RelayCommand(() =>
            Process.Start(new ProcessStartInfo
            {
                FileName = "https://github.com/JokkeeZ/Wallsh",
                UseShellExecute = true
            }));

    public static ICommand ShowAppFromTray =>
        new RelayCommand(() =>
        {
            if (Current?.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime
                {
                    MainWindow: not null
                } desktop)
                return;

            if (!desktop.MainWindow.IsVisible)
                desktop.MainWindow.Show();
            else
                desktop.MainWindow.Activate();
        });

    public static ICommand ExitAppFromTray =>
        new RelayCommand(() =>
        {
            if (Current?.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime
                {
                    MainWindow: not null
                } desktop)
                return;

            desktop.Shutdown();
        });

    public override void Initialize() => AvaloniaXamlLoader.Load(this);

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // Avoid duplicate validations from both Avalonia and the CommunityToolkit. 
            // More info: https://docs.avaloniaui.net/docs/guides/development-guides/data-validation#manage-validationplugins
            DisableAvaloniaDataAnnotationValidation();
            desktop.MainWindow = new MainWindow
            {
                DataContext = new MainWindowViewModel()
            };

            desktop.MainWindow.Closing += Closing;
        }

        base.OnFrameworkInitializationCompleted();
    }

    private void Closing(object? sender, WindowClosingEventArgs e)
    {
        e.Cancel = true;

        if (e.CloseReason != WindowCloseReason.WindowClosing)
            return;

        if (ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop)
            return;

        desktop.MainWindow?.Hide();
    }

    private static void DisableAvaloniaDataAnnotationValidation()
    {
        // Get an array of plugins to remove
        var dataValidationPluginsToRemove =
            BindingPlugins.DataValidators.OfType<DataAnnotationsValidationPlugin>().ToArray();

        // remove each entry found
        foreach (var plugin in dataValidationPluginsToRemove)
            BindingPlugins.DataValidators.Remove(plugin);
    }
}
