using System.Collections.ObjectModel;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Wallsh.Models;
using Wallsh.Models.Config;
using Wallsh.Services.System;

namespace Wallsh.ViewModels;

public class ListBoxFolderItem
{
    public required string Path { get; set; }
    public required int Index { get; set; }
}

public partial class LocalViewModel : ViewModelBase, IWpChangerConfigValidator
{
    
    public ObservableCollection<ListBoxFolderItem> AdditionalWallpaperFolders { get; set; }

    public LocalViewModel(AppConfiguration cfg)
    {
        AdditionalWallpaperFolders = [];

        for (var i = 0; i < cfg.IncludeFolders.Count; i++)
        {
            var item = cfg.IncludeFolders[i];

            AdditionalWallpaperFolders.Add(new()
            {
                Path = item,
                Index = i
            });
        }
    }

    public (bool Success, string? Message) ValidateConfiguration()
    {
        if (AdditionalWallpaperFolders.Count == 0)
            return (true, null);

        foreach (var folder in AdditionalWallpaperFolders)
        {
            if (string.IsNullOrWhiteSpace(folder.Path))
                return (false, "Folder path cannot be empty.");

            if (!Directory.Exists(folder.Path))
                return (false, $"Folder: {folder.Path} does not exist.");
        }

        return (true, null);
    }

    [RelayCommand]
    private async Task BrowseWallpaperFolder()
    {
        var folderService = Ioc.Default.GetRequiredService<OpenFolderService>();
        var folder = await folderService.OpenFolderAsync();

        var path = folder?.TryGetLocalPath();
        if (path != null)
        {
            AdditionalWallpaperFolders.Add(new()
            {
                Path = path,
                Index = AdditionalWallpaperFolders.Count
            });
        }
    }

    [RelayCommand]
    private void RemoveFolder(ListBoxFolderItem item) => AdditionalWallpaperFolders.RemoveAt(item.Index);
}
