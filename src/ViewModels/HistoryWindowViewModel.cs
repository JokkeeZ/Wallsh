using System.Collections.ObjectModel;
using System.Diagnostics;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.DependencyInjection;
using Wallsh.Changers;
using Wallsh.Messages;
using Wallsh.Models;
using Wallsh.Models.Environments;
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
    private static void SetWallpaper(string path)
    {
        var env = Ioc.Default.GetRequiredService<IWpEnvironment>();
        if (env.GetCurrentWallpaperPath() == path)
            return;

        var sp = Ioc.Default.GetRequiredService<IServiceProvider>();
        var changer =
            sp.GetRequiredKeyedService<IWallpaperChanger>(WallpaperChangerType.Local) as LocalWallpaperChanger;

        changer?.ManuallySetWallpaperFromPath(path);
    }

    [RelayCommand]
    private static void ViewWallpaper(string path) => Process.Start(new ProcessStartInfo
    {
        FileName = path,
        UseShellExecute = true
    });

    [RelayCommand]
    private static void OpenWallpaperUrl(string url) => Process.Start(new ProcessStartInfo
    {
        FileName = url,
        UseShellExecute = true
    });
}
