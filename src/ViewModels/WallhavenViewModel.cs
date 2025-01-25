using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using Wallsh.Messages;
using Wallsh.Models;
using Wallsh.Services.Wallhaven;

namespace Wallsh.ViewModels;

public partial class WallhavenViewModel : ViewModelBase,
    IWpChangerConfigValidator,
    IRecipient<IntervalUpdatedMessage>
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanEnableNsfw))]
    private string? _apiKey;

    [ObservableProperty]
    private ObservableCollection<string> _availableResolutions;

    [ObservableProperty]
    private bool _categoryAnime;

    [ObservableProperty]
    private bool _categoryGeneral;

    [ObservableProperty]
    private bool _categoryPeople;

    private TimeOnly _interval;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanEnableNsfw))]
    private bool _purityNsfw;

    [ObservableProperty]
    private bool _puritySfw;

    [ObservableProperty]
    private bool _puritySketchy;

    [ObservableProperty]
    private WallhavenRatio _ratio;

    [ObservableProperty]
    private string _resolution;

    [ObservableProperty]
    private WallhavenSorting _sorting;

    public bool CanEnableNsfw => !string.IsNullOrWhiteSpace(ApiKey) && ApiKey?.Length == 32;

    public WallhavenViewModel(AppConfiguration cfg)
    {
        Messenger.RegisterAll(this);

        _interval = cfg.Interval;

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

        AvailableResolutions = new(WallhavenConfiguration.Resolutions[Ratio]);
    }

    public void Receive(IntervalUpdatedMessage message) => _interval = message.Interval;

    public (bool Success, string? Message) ValidateConfiguration()
    {
        // Wallhaven.cc allows 45 requests per minute, so request
        // for every 2 seconds is easily on the safe side.
        if (_interval.ToTimeSpan().TotalSeconds < 2)
            return (false, "Wallhaven interval needs to be at least 2 seconds.");

        if (!string.IsNullOrWhiteSpace(Resolution))
            return (true, null);

        return (false, "Wallpaper resolution cannot be empty.");
    }

    partial void OnApiKeyChanged(string? value)
    {
        // wallhaven.cc api key seems to always be 32 chars.
        if (string.IsNullOrWhiteSpace(value) || value.Length != 32)
            PurityNsfw = false;
    }

    partial void OnRatioChanged(WallhavenRatio value)
    {
        AvailableResolutions = new(WallhavenConfiguration.Resolutions[value]);

        if (string.IsNullOrWhiteSpace(Resolution) || !AvailableResolutions.Contains(Resolution))
            Resolution = AvailableResolutions[0];
    }

    partial void OnAvailableResolutionsChanged(ObservableCollection<string> value)
    {
        if (string.IsNullOrWhiteSpace(Resolution))
            Resolution = value[0];
    }
}
