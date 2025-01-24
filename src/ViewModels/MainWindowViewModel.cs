using System.Reflection;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Wallsh.Messages;
using Wallsh.Models;

namespace Wallsh.ViewModels;

[NotifyPropertyChangedRecipients]
public partial class MainWindowViewModel : ViewModelBase,
    IRecipient<WallpaperChangerUpdatedMessage>,
    IRecipient<TimerUpdatedMessage>
{
    private readonly AppConfiguration _cfg;
    private readonly WallpaperChanger _wallpaperChanger;

    [ObservableProperty]
    private string _appTitle = "Wallsh";

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
        }
        else
        {
            _cfg = Ioc.Default.GetRequiredService<AppConfiguration>();
            LocalViewModel = Ioc.Default.GetRequiredService<LocalViewModel>();
            WallhavenViewModel = Ioc.Default.GetRequiredService<WallhavenViewModel>();
            BingViewModel = Ioc.Default.GetRequiredService<BingViewModel>();
        }

        _wallpaperChanger = new(_cfg);

        _changerType = _cfg.ChangerType;
        _hours = _cfg.Interval.Hour;
        _minutes = _cfg.Interval.Minute;
        _seconds = _cfg.Interval.Second;
        _wallpapersFolder = _cfg.WallpapersFolder;
        _wallpaperAdjustment = _cfg.WallpaperAdjustment;

        Adjustments = _wallpaperChanger.WpEnvironment.WallpaperAdjustments;

        if (string.IsNullOrWhiteSpace(_cfg.WallpaperAdjustment))
            WallpaperAdjustment = _wallpaperChanger.WpEnvironment.GetWallpaperAdjustment();

        // We don't start the changer if changer
        // is None OR we are in design mode.
        if (_wallpaperChanger.CanStart)
        {
            _wallpaperChanger.Start();
            UpdateAppTitle(Interval);
        }
    }

    public void Receive(TimerUpdatedMessage message) => UpdateAppTitle(message.Time);

    public void Receive(WallpaperChangerUpdatedMessage message) => _changerType = message.ChangerType;

    [RelayCommand]
    private async Task SaveConfiguration()
    {
        if (!await IsValidConfiguration())
            return;

        _wallpaperChanger.Stop();

        _cfg.ChangerType = _changerType;
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

        _wallpaperChanger.Config = _cfg;
        _wallpaperChanger.SetInterval(_cfg.Interval);

        _wallpaperChanger.WpEnvironment.SetWallpaperAdjustment(_cfg.WallpaperAdjustment);

        if (_cfg.ToFile())
        {
            await CreateNotification("Settings saved!", NotificationType.Success);

            // We don't start the changer if changer
            // is None OR we are in design mode.
            if (_wallpaperChanger.CanStart)
            {
                _wallpaperChanger.Start();
                UpdateAppTitle(Interval);
            }
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

        if (_changerType == WallpaperChangerType.None)
            return true;

        var (success, message) = _changerType switch
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
        Messenger.Send(new IntervalUpdatedMessage(Interval));
        Messenger.Send(new WallpaperFolderUpdatedMessage(WallpapersFolder));
    }

    private async Task CreateNotification(string message, NotificationType type)
    {
        NotificationType = type;
        NotificationText = message;
        IsNotificationVisible = true;

        await Task.Delay(TimeSpan.FromSeconds(5));
        IsNotificationVisible = false;
    }

    private void UpdateAppTitle(TimeOnly time)
    {
        var nextChangeTime = DateTime.Now
            .AddHours(time.Hour)
            .AddMinutes(time.Minute)
            .AddSeconds(time.Second);

        var version = Assembly.GetExecutingAssembly().GetName().Version?.ToString(3);
        AppTitle = $"Wallsh {version} | WP Change: {nextChangeTime.ToLongTimeString()}";
    }
}
