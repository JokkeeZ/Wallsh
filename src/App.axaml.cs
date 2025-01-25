using System.Diagnostics;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Wallsh.Changers;
using Wallsh.Models;
using Wallsh.Models.Environments;
using Wallsh.Models.Environments.Linux;
using Wallsh.Models.Environments.Windows;
using Wallsh.ViewModels;
using Wallsh.Views;

namespace Wallsh;

public class App : Application
{
    public static ICommand OpenWallpapersFolderFromTray =>
        new RelayCommand(() =>
        {
            var cfg = AppConfiguration.FromFile();

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

    public static ILogger<T> CreateLogger<T>() => Ioc.Default.GetRequiredService<ILoggerFactory>().CreateLogger<T>();

    private static IWpEnvironment GetWpEnvironment()
    {
        if (OperatingSystem.IsLinux() && GnomeWpEnvironment.IsGnome())
            return new GnomeWpEnvironment();

        if (OperatingSystem.IsWindows())
            return new WindowsWpEnvironment();

        throw new NotImplementedException("This environment is not supported.");
    }

    public override void OnFrameworkInitializationCompleted()
    {
        Ioc.Default.ConfigureServices(new ServiceCollection()
            .AddSingleton(LoggerFactory.Create(
                builder => builder
                    .AddConsole()
                    .AddDebug()
                    .SetMinimumLevel(LogLevel.Debug)
            ))
            .AddSingleton(AppConfiguration.FromFile())
            .AddSingleton(GetWpEnvironment())
            .AddKeyedSingleton<IWallpaperChanger, LocalWallpaperChanger>(WallpaperChangerType.Local)
            .AddKeyedSingleton<IWallpaperChanger, WallhavenWallpaperChanger>(WallpaperChangerType.Wallhaven)
            .AddKeyedSingleton<IWallpaperChanger, BingWallpaperChanger>(WallpaperChangerType.Bing)
            .AddTransient<MainWindowViewModel>()
            .AddTransient<LocalViewModel>()
            .AddTransient<WallhavenViewModel>()
            .AddTransient<BingViewModel>()
            .BuildServiceProvider()
        );

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // Avoid duplicate validations from both Avalonia and the CommunityToolkit. 
            // More info: https://docs.avaloniaui.net/docs/guides/development-guides/data-validation#manage-validationplugins
            DisableAvaloniaDataAnnotationValidation();
            desktop.MainWindow = new MainWindow
            {
                DataContext = Ioc.Default.GetRequiredService<MainWindowViewModel>()
            };

            desktop.MainWindow.Closing += Closing;
        }

        base.OnFrameworkInitializationCompleted();
    }

    private void Closing(object? sender, WindowClosingEventArgs e)
    {
        if (e.CloseReason != WindowCloseReason.WindowClosing)
            return;

        if (ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop)
            return;

        e.Cancel = true;
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
