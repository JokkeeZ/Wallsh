using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using Wallsh.Messages;
using Wallsh.Models;

namespace Wallsh.ViewModels;

public partial class LocalViewModel : ViewModelBase, IWpHandlerConfigValidator,
    IRecipient<WallpaperFolderChangedMessage>
{
    [ObservableProperty]
    private bool _isActiveHandler;

    private string? _wallpapersFolder;

    public LocalViewModel(AppJsonConfiguration cfg)
    {
        Messenger.RegisterAll(this);

        _wallpapersFolder = cfg.WallpapersFolder;
        IsActiveHandler = cfg.Handler == WallpaperHandler.Local;
    }

    public void Receive(WallpaperFolderChangedMessage message) => _wallpapersFolder = message.WallpapersFolder;

    public (bool Success, string Message) ValidateConfiguration()
    {
        if (string.IsNullOrWhiteSpace(_wallpapersFolder))
            return (false, "Wallpapers folder cannot be empty.");

        // Folder must contain wallpapers inorder to
        // use local handler.
        if (Directory.GetFiles(_wallpapersFolder).Length == 0)
            return (false, "Wallpapers folder does not contain any files.");

        return (true, string.Empty);
    }

    partial void OnIsActiveHandlerChanged(bool value) =>
        Messenger.Send(new WallpaperHandlerChanged(value
            ? WallpaperHandler.Local
            : WallpaperHandler.None));
}
