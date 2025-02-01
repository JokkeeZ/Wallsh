using System.Collections.ObjectModel;
using System.Diagnostics;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Wallsh.Messages;
using Wallsh.Models.History;

namespace Wallsh.ViewModels;

public partial class HistoryWindowViewModel : ViewModelBase,
    IRecipient<WallpaperUpdatedMessage>
{
    public ObservableCollection<WallpaperInfo> Wallpapers { get; set; }

    public HistoryWindowViewModel(WallpaperHistory history)
    {
        Messenger.RegisterAll(this);
        Wallpapers = new(history.Wallpapers.Reverse());
    }

    public void Receive(WallpaperUpdatedMessage message)
    {
        var history = Ioc.Default.GetRequiredService<WallpaperHistory>();
        Wallpapers.Clear();

        foreach (var wallpaper in history.Wallpapers.Reverse())
            Wallpapers.Add(wallpaper);
    }

    [RelayCommand]
    private static void ViewWallpaper(string? path)
    {
        if (path is null)
            return;

        Process.Start(new ProcessStartInfo
        {
            FileName = path,
            UseShellExecute = true
        });
    }
}
