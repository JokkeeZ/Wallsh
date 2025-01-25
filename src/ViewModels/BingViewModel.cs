using System.Collections.ObjectModel;
using Avalonia.Platform;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using Wallsh.Models;
using Wallsh.Services.Bing;

namespace Wallsh.ViewModels;

public partial class BingViewModel : ViewModelBase, IWpChangerConfigValidator
{
    [ObservableProperty]
    private ObservableCollection<string> _availableResolutions;

    [ObservableProperty]
    private int _numberOfWallpapers;

    [ObservableProperty]
    private ScreenOrientation _orientation;

    [ObservableProperty]
    private string _resolution;

    public BingViewModel(AppConfiguration cfg)
    {
        Messenger.RegisterAll(this);

        Resolution = cfg.Bing.Resolution;
        Orientation = cfg.Bing.Orientation;
        NumberOfWallpapers = cfg.Bing.NumberOfWallpapers;

        AvailableResolutions = new(BingConfiguration.Resolutions[Orientation]);
    }

    public (bool Success, string? Message) ValidateConfiguration() => (true, null);

    partial void OnOrientationChanged(ScreenOrientation value)
    {
        AvailableResolutions = new(BingConfiguration.Resolutions[value]);

        if (string.IsNullOrWhiteSpace(Resolution) || !AvailableResolutions.Contains(Resolution))
            Resolution = AvailableResolutions[0];
    }
}
