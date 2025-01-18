using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Wallsh.ViewModels;
using Wallsh.Views;

namespace Wallsh;

public class App : Application
{
    public static readonly TrayCommandHandler CommandHandler = new();
    
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // Avoid duplicate validations from both Avalonia and the CommunityToolkit. 
            // More info: https://docs.avaloniaui.net/docs/guides/development-guides/data-validation#manage-validationplugins
            DisableAvaloniaDataAnnotationValidation();
            desktop.MainWindow = new MainWindow
            {
                DataContext = new MainWindowViewModel(),
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
        {
            BindingPlugins.DataValidators.Remove(plugin);
        }
    }
}