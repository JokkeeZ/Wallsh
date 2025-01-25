using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using Wallsh.Messages;
using Wallsh.Models;

namespace Wallsh.ViewModels;

public partial class LocalViewModel : ViewModelBase, IWpChangerConfigValidator,
    IRecipient<WallpaperFolderUpdatedMessage>
{
    [ObservableProperty]
    private string? _wallpapersFolder;

    public LocalViewModel(AppConfiguration cfg)
    {
        Messenger.RegisterAll(this);

        WallpapersFolder = cfg.WallpapersFolder;
    }

    public void Receive(WallpaperFolderUpdatedMessage message) => WallpapersFolder = message.WallpapersFolder;

    public (bool Success, string? Message) ValidateConfiguration()
    {
        if (string.IsNullOrWhiteSpace(WallpapersFolder))
            return (false, "Wallpapers folder cannot be empty.");

        return (true, null);
    }
}
