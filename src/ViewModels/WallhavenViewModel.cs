using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using Wallsh.Messages;
using Wallsh.Models;
using Wallsh.Models.Wallhaven;

namespace Wallsh.ViewModels;

public partial class WallhavenViewModel : ViewModelBase, IWpServiceConfigValidator, IRecipient<IntervalChanged>
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanEnableNsfw))]
    private string? _apiKey;

    [ObservableProperty]
    private bool _categoryAnime;

    [ObservableProperty]
    private bool _categoryGeneral;

    [ObservableProperty]
    private bool _categoryPeople;

    private TimeOnly _interval;

    [ObservableProperty]
    private bool _isActiveHandler;

    [ObservableProperty]
    private bool _purityNsfw;

    [ObservableProperty]
    private bool _puritySfw;

    [ObservableProperty]
    private bool _puritySketchy;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(AvailableResolutions))]
    [NotifyPropertyChangedFor(nameof(Resolution))]
    private WallhavenRatio _ratio;

    [ObservableProperty]
    private string? _resolution;

    [ObservableProperty]
    private WallhavenSorting _sorting;

    public WallhavenViewModel(AppJsonConfiguration cfg)
    {
        Messenger.RegisterAll(this);

        _interval = cfg.Interval;
        IsActiveHandler = cfg.Handler == WallpaperHandler.Wallhaven;

        Sorting = cfg.Wallhaven.Sorting;
        Resolution = cfg.Wallhaven.Resolution;
        Ratio = cfg.Wallhaven.Ratio;
        ApiKey = cfg.Wallhaven.ApiKey;
        CategoryAnime = cfg.Wallhaven.Anime;
        CategoryGeneral = cfg.Wallhaven.General;
        CategoryPeople = cfg.Wallhaven.People;
        PurityNsfw = cfg.Wallhaven.PurityNsfw;
        PuritySfw = cfg.Wallhaven.PuritySfw;
        PuritySketchy = cfg.Wallhaven.PuritySketchy;
    }

    public List<string> AvailableResolutions => WallhavenConfiguration.Resolutions[Ratio];

    public bool CanEnableNsfw => !string.IsNullOrWhiteSpace(ApiKey);

    public void Receive(IntervalChanged message) => _interval = message.Interval;

    public bool ValidateConfiguration()
    {
        // Wallhaven.cc allows 45 requests per minute, so request
        // for every 2 seconds is easily on the safe side.
        if (_interval.ToTimeSpan().TotalSeconds < 2)
        {
            Console.WriteLine("Interval needs to be at least 2 seconds.");
            return false;
        }

        if (!string.IsNullOrEmpty(Resolution))
            return true;

        Console.WriteLine("Wallpaper resolution cannot be empty.");
        return false;
    }

    partial void OnApiKeyChanged(string? value)
    {
        if (string.IsNullOrEmpty(value))
            PurityNsfw = false;
    }

    partial void OnIsActiveHandlerChanged(bool value) =>
        Messenger.Send(new WallpaperHandlerChanged(value
            ? WallpaperHandler.Wallhaven
            : WallpaperHandler.None));
}
