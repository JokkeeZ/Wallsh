using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;

namespace Wallsh.Views;

public partial class MainWindow : Window
{
    private readonly string[] _resolutionUltrawide = ["2560x1080", "3440x1440", "3840x1600"];
    private readonly string[] _resolution16X9 = ["1280x720", "1600x900", "1920x1080", "2560x1440", "3840x2160"];
    private readonly string[] _resolution16X10 = ["1280x800", "1600x1000", "1920x1200", "2560x1600", "3840x2400"];
    private readonly string[] _resolution4X3 = ["1280x960", "1600x1200", "1920x1440", "2560x1920", "3840x2880"];
    private readonly string[] _resolution5X4 = ["1280x1024", "1600x1280", "1920x1536", "2560x2048", "3840x3072"];
    private readonly string[] _adjustments = ["none", "scaled", "zoom", "wallpaper"];
    
    public MainWindow()
    {
        InitializeComponent();
        WallpaperAdjustments.ItemsSource = _adjustments;
    }
    
    private void LocalRotationIsCheckedChanged(object? sender, RoutedEventArgs e)
    {
        var checkBox = (CheckBox)sender!;

        if (checkBox.IsChecked.HasValue && checkBox.IsChecked.Value)
        {
            WallhavenRotationCheckBox.IsChecked = false;
        }
    }

    private void WallhavenRotationIsCheckedChanged(object? sender, RoutedEventArgs e)
    {
        var checkBox = (CheckBox)sender!;
            
        if (checkBox.IsChecked.HasValue && checkBox.IsChecked.Value)
        {
            LocalRotationCheckBox.IsChecked = false;
        }
    }

    private void RatioIsCheckedChanged(object? sender, RoutedEventArgs routedEventArgs)
    {
        var radioButton = (RadioButton)sender!;
        if (!radioButton.IsChecked.HasValue || !radioButton.IsChecked.Value) return;
        
        var resolutions = radioButton.Content switch
        {
            "Ultrawide" => _resolutionUltrawide,
            "16:9" => _resolution16X9,
            "16:10" => _resolution16X10,
            "4:3" => _resolution4X3,
            "5:4" => _resolution5X4,
            _ => _resolution16X9
        };

        WallhavenResolutions.ItemsSource = resolutions;
        WallhavenResolutions.SelectedItem ??= WallhavenResolutions.Items[0];
    }

    private void WallhavenApiKeyTextChanged(object? sender, TextChangedEventArgs e)
    {
        var textBox = (TextBox)sender!;

        PurityNsfw.IsEnabled = !string.IsNullOrEmpty(textBox.Text);

        if (!PurityNsfw.IsEnabled)
        {
            PurityNsfw.IsChecked = false;
        }
    }
    
    private async void BrowseFolderClick(object? sender, RoutedEventArgs e)
    {
        try
        {
            if (!StorageProvider.CanPickFolder)
            {
                Console.WriteLine("Can't pick a folder.");
                return;
            }
        
            var folders = await StorageProvider.OpenFolderPickerAsync(new()
            {
                AllowMultiple = false,
                Title = "Select Wallpaper Folder",
                SuggestedStartLocation = await StorageProvider.TryGetFolderFromPathAsync(
                    new(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)))
            });
        
            if (folders is { Count: > 0 })
            {
                FolderPathTextBox.Text = folders[0].TryGetLocalPath();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }

    private async void InfoTextChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        await Task.Delay(5000);
        InfoTextBlock.Text = string.Empty;
    }
}
