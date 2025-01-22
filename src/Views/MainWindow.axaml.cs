using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;

namespace Wallsh.Views;

public partial class MainWindow : Window
{
    public MainWindow() => InitializeComponent();

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
                Title = "Select wallpaper folder",
                SuggestedStartLocation = await StorageProvider.TryGetFolderFromPathAsync(
                    new(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures)))
            });

            if (folders is { Count: > 0 })
                FolderPathTextBox.Text = folders[0].TryGetLocalPath();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }
}
