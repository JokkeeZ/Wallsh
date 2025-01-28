using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using CommunityToolkit.Mvvm.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Wallsh.Changers;
using Wallsh.Models;
using Wallsh.Models.Config;
using Wallsh.Models.Environments;
using Wallsh.Models.Environments.Linux;
using Wallsh.Models.Environments.Windows;
using Wallsh.Models.History;
using Wallsh.Services.System;
using Wallsh.ViewModels;
using Wallsh.Views;

namespace Wallsh;

public class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
        DataContext = new ApplicationViewModel();
    }

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
            .AddSingleton(JsonFile.ReadAndDeserialize<AppConfiguration>("config.json"))
            .AddSingleton(JsonFile.ReadAndDeserialize<WallpaperHistory>("history.json"))
            .AddSingleton(GetWpEnvironment())
            .AddSingleton<OpenFolderService>()
            .AddKeyedSingleton<IWallpaperChanger, NoneWallpaperChanger>(WallpaperChangerType.None)
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

    private static void Closing(object? sender, WindowClosingEventArgs e)
    {
        if (e.CloseReason != WindowCloseReason.WindowClosing)
            return;

        e.Cancel = true;

        var window = sender as MainWindow;
        window?.Hide();
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
