using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using Wallsh.Messages;
using Wallsh.Models;

namespace Wallsh.ViewModels;

public partial class LocalViewModel : ViewModelBase, IWpChangerConfigValidator,
    IRecipient<WallpaperFolderUpdatedMessage>
{
    [ObservableProperty]
    private bool _isActiveHandler;

    private string? _wallpapersFolder;

    public LocalViewModel(AppConfiguration cfg)
    {
        Messenger.RegisterAll(this);

        _wallpapersFolder = cfg.WallpapersFolder;
        IsActiveHandler = cfg.ChangerType == WallpaperChangerType.Local;
    }

    public void Receive(WallpaperFolderUpdatedMessage message) => _wallpapersFolder = message.WallpapersFolder;

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
        Messenger.Send(new WallpaperChangerUpdatedMessage(value
            ? WallpaperChangerType.Local
            : WallpaperChangerType.None));
}
