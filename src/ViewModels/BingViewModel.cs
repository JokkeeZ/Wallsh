using System.Collections.ObjectModel;
using Avalonia.Platform;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using Wallsh.Messages;
using Wallsh.Models;
using Wallsh.Services.Bing;

namespace Wallsh.ViewModels;

public partial class BingViewModel : ViewModelBase, IWpChangerConfigValidator
{
    [ObservableProperty]
    private ObservableCollection<string> _availableResolutions;

    [ObservableProperty]
    private bool _isActiveHandler;

    [ObservableProperty]
    private int _numberOfWallpapers;

    [ObservableProperty]
    private ScreenOrientation _orientation;

    [ObservableProperty]
    private string _resolution;

    public BingViewModel(AppConfiguration cfg)
    {
        Messenger.RegisterAll(this);
        IsActiveHandler = cfg.ChangerType == WallpaperChangerType.Bing;

        Resolution = cfg.Bing.Resolution;
        Orientation = cfg.Bing.Orientation;
        NumberOfWallpapers = cfg.Bing.NumberOfWallpapers;

        AvailableResolutions = new(BingConfiguration.Resolutions[Orientation]);
    }

    public (bool Success, string Message) ValidateConfiguration() => (true, string.Empty);

    partial void OnOrientationChanged(ScreenOrientation value)
    {
        AvailableResolutions = new(BingConfiguration.Resolutions[value]);

        if (string.IsNullOrEmpty(Resolution) || !AvailableResolutions.Contains(Resolution))
            Resolution = AvailableResolutions[0];
    }

    partial void OnIsActiveHandlerChanged(bool value) =>
        Messenger.Send(new WallpaperChangerUpdatedMessage(value
            ? WallpaperChangerType.Bing
            : WallpaperChangerType.None));
}
