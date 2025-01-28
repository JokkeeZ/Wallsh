using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;

namespace Wallsh.Services.System;

public class OpenFolderService
{
    public async Task<IStorageFolder?> OpenFolderAsync()
    {
        if (Application.Current?.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop)
            return null;

        var window = desktop.MainWindow!;
        if (!window.StorageProvider.CanPickFolder)
            return null;

        var folders = await window.StorageProvider.OpenFolderPickerAsync(new()
        {
            AllowMultiple = false,
            Title = "Select folder",
            SuggestedStartLocation = await window.StorageProvider.TryGetFolderFromPathAsync(
                new(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures)))
        });

        return folders is { Count: > 0 } ? folders[0] : null;
    }
}
