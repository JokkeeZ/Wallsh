using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;

namespace Wallsh.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private void LocalRotationIsCheckedChanged(object? sender, RoutedEventArgs e)
    {
        var checkBox = (CheckBox)sender!;

        if (checkBox.IsChecked.HasValue && checkBox.IsChecked.Value)
            WallhavenRotationCheckBox.IsChecked = false;
    }

    private void WallhavenRotationIsCheckedChanged(object? sender, RoutedEventArgs e)
    {
        var checkBox = (CheckBox)sender!;

        if (checkBox.IsChecked.HasValue && checkBox.IsChecked.Value)
            LocalRotationCheckBox.IsChecked = false;
    }

    private void WallhavenApiKeyTextChanged(object? sender, TextChangedEventArgs e)
    {
        var textBox = (TextBox)sender!;

        PurityNsfw.IsEnabled = !string.IsNullOrEmpty(textBox.Text);

        if (!PurityNsfw.IsEnabled)
            PurityNsfw.IsChecked = false;
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
                FolderPathTextBox.Text = folders[0].TryGetLocalPath();
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
