using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Wallsh.Messages;
using Wallsh.Models;
using Wallsh.Models.Environments;

namespace Wallsh.ViewModels;

[NotifyPropertyChangedRecipients]
public partial class MainWindowViewModel : ViewModelBase,
    IRecipient<TimerUpdatedMessage>,
    IRecipient<StopRequestedMessage>
{
    private readonly AppConfiguration _cfg;
    private readonly WallpaperManager _wallpaperManager;
    private readonly IWpEnvironment _wpEnvironment;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanStart))]
    private WallpaperChangerType _changerType;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Interval))]
    private int _hours;

    [ObservableProperty]
    private bool _isNotificationVisible;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Interval))]
    private int _minutes;

    [ObservableProperty]
    private string? _notificationText;

    [ObservableProperty]
    private NotificationType _notificationType;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Interval))]
    private int _seconds;

    [ObservableProperty]
    private string? _wallpaperAdjustment;

    [ObservableProperty]
    private string _wallpapersFolder;

    public LocalViewModel LocalViewModel { get; }
    public WallhavenViewModel WallhavenViewModel { get; }
    public BingViewModel BingViewModel { get; }

    public TimeOnly Interval => new(Hours, Minutes, Seconds);

    private bool CanStart => ChangerType != WallpaperChangerType.None && !Design.IsDesignMode;

    public string[] Adjustments { get; }

    public MainWindowViewModel()
    {
        Messenger.RegisterAll(this);

        // Avalonia designer doesn't like DI
        if (Design.IsDesignMode)
        {
            _cfg = new();
            LocalViewModel = new(_cfg);
            WallhavenViewModel = new(_cfg);
            BingViewModel = new(_cfg);
            _wpEnvironment = null!;
            _wallpaperManager = null!;
            WallpapersFolder = _cfg.WallpapersFolder;
            Adjustments = [];
            return;
        }

        _cfg = Ioc.Default.GetRequiredService<AppConfiguration>();
        _wpEnvironment = Ioc.Default.GetRequiredService<IWpEnvironment>();
        LocalViewModel = Ioc.Default.GetRequiredService<LocalViewModel>();
        WallhavenViewModel = Ioc.Default.GetRequiredService<WallhavenViewModel>();
        BingViewModel = Ioc.Default.GetRequiredService<BingViewModel>();
        _wallpaperManager = new(_cfg, _wpEnvironment);

        Adjustments = _wpEnvironment.WallpaperAdjustments;
        ChangerType = _cfg.ChangerType;
        Hours = _cfg.Interval.Hour;
        Minutes = _cfg.Interval.Minute;
        Seconds = _cfg.Interval.Second;
        WallpapersFolder = _cfg.WallpapersFolder;
        WallpaperAdjustment = _cfg.WallpaperAdjustment ?? _wpEnvironment.GetWallpaperAdjustment();

        if (CanStart)
            _wallpaperManager.Start();
    }

    public void Receive(StopRequestedMessage message) => ChangerType = WallpaperChangerType.None;

    public void Receive(TimerUpdatedMessage message) { }

    [RelayCommand]
    private async Task SaveConfiguration()
    {
        if (!await IsValidConfiguration())
            return;

        _wallpaperManager.Stop();

        _cfg.ChangerType = ChangerType;
        _cfg.Interval = Interval;
        _cfg.WallpapersFolder = WallpapersFolder;
        _cfg.WallpaperAdjustment = WallpaperAdjustment;

        // Wallhaven config
        _cfg.Wallhaven.ApiKey = WallhavenViewModel.ApiKey;
        _cfg.Wallhaven.General = WallhavenViewModel.CategoryGeneral;
        _cfg.Wallhaven.Anime = WallhavenViewModel.CategoryAnime;
        _cfg.Wallhaven.People = WallhavenViewModel.CategoryPeople;
        _cfg.Wallhaven.Ratio = WallhavenViewModel.Ratio;
        _cfg.Wallhaven.Sorting = WallhavenViewModel.Sorting;
        _cfg.Wallhaven.Resolution = WallhavenViewModel.Resolution;
        _cfg.Wallhaven.PuritySfw = WallhavenViewModel.PuritySfw;
        _cfg.Wallhaven.PuritySketchy = WallhavenViewModel.PuritySketchy;
        _cfg.Wallhaven.PurityNsfw = WallhavenViewModel.PurityNsfw;

        // Bing config
        _cfg.Bing.Resolution = BingViewModel.Resolution;
        _cfg.Bing.NumberOfWallpapers = BingViewModel.NumberOfWallpapers;
        _cfg.Bing.Orientation = BingViewModel.Orientation;

        _wallpaperManager.Config = _cfg;
        _wallpaperManager.SetInterval(_cfg.Interval);

        _wpEnvironment.SetWallpaperAdjustment(_cfg.WallpaperAdjustment);

        if (_cfg.ToFile())
        {
            await CreateNotification("Settings saved!", NotificationType.Success);

            if (CanStart)
                _wallpaperManager.Start();
        }
        else
            await CreateNotification("Failed to save settings!", NotificationType.Error);
    }

    private async Task<bool> IsValidConfiguration()
    {
        if (Interval == TimeOnly.MinValue)
        {
            await CreateNotification("Interval cannot be 00:00:00.", NotificationType.Warning);
            return false;
        }

        if (string.IsNullOrWhiteSpace(WallpaperAdjustment))
        {
            await CreateNotification("Wallpaper adjustment cannot be empty.", NotificationType.Error);
            return false;
        }

        if (string.IsNullOrWhiteSpace(WallpapersFolder))
        {
            await CreateNotification("Wallpaper folder cannot be empty.", NotificationType.Error);
            return false;
        }

        if (!Directory.Exists(WallpapersFolder))
        {
            await CreateNotification("Wallpaper folder does not exist.", NotificationType.Error);
            return false;
        }

        if (ChangerType == WallpaperChangerType.None)
            return true;

        var (success, message) = ChangerType switch
        {
            WallpaperChangerType.Local => LocalViewModel.ValidateConfiguration(),
            WallpaperChangerType.Wallhaven => WallhavenViewModel.ValidateConfiguration(),
            WallpaperChangerType.Bing => BingViewModel.ValidateConfiguration(),
            _ => (true, null)
        };

        if (message is not null)
            await CreateNotification(message, NotificationType.Error);

        return success;
    }

    protected override void Broadcast<T>(T oldValue, T newValue, string? propertyName)
    {
        switch (propertyName)
        {
            case nameof(Interval):
                Messenger.Send(new IntervalUpdatedMessage(Interval));
            break;
            case nameof(WallpapersFolder):
                Messenger.Send(new WallpaperFolderUpdatedMessage(WallpapersFolder));
            break;
        }
    }

    private async Task CreateNotification(string message, NotificationType type)
    {
        NotificationType = type;
        NotificationText = message;
        IsNotificationVisible = true;

        await Task.Delay(TimeSpan.FromSeconds(5));
        IsNotificationVisible = false;
    }
}
