using System.Reflection;
using Avalonia.Controls.Notifications;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Wallsh.Messages;
using Wallsh.Models;

namespace Wallsh.ViewModels;

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
    [NotifyPropertyChangedRecipients]
    [NotifyPropertyChangedFor(nameof(Interval))]
    private int _hours;

    [ObservableProperty]
    private bool _isNotificationVisible;

    [ObservableProperty]
    [NotifyPropertyChangedRecipients]
    [NotifyPropertyChangedFor(nameof(Interval))]
    private int _minutes;

    [ObservableProperty]
    private string? _notificationText;

    [ObservableProperty]
    private NotificationType _notificationType;

    [ObservableProperty]
    [NotifyPropertyChangedRecipients]
    [NotifyPropertyChangedFor(nameof(Interval))]
    private int _seconds;

    [ObservableProperty]
    private string? _wallpaperAdjustment;

    [ObservableProperty]
    [NotifyPropertyChangedRecipients]
    private string _wallpapersFolder;

    public LocalViewModel LocalViewModel { get; }
    public WallhavenViewModel WallhavenViewModel { get; }
    public BingViewModel BingViewModel { get; }

    public TimeOnly Interval => new(Hours, Minutes, Seconds);

    public string[] Adjustments { get; }

    public MainWindowViewModel()
    {
        Messenger.RegisterAll(this);

        _cfg = AppConfiguration.FromFile();

        LocalViewModel = new(_cfg);
        WallhavenViewModel = new(_cfg);
        BingViewModel = new(_cfg);
        _wallpaperChanger = new(_cfg);

        _changerType = _cfg.ChangerType;
        _hours = _cfg.Interval.Hour;
        _minutes = _cfg.Interval.Minute;
        _seconds = _cfg.Interval.Second;
        _wallpapersFolder = _cfg.WallpapersFolder;
        _wallpaperAdjustment = _cfg.WallpaperAdjustment;

        Adjustments = _wallpaperChanger.WpEnvironment.WallpaperAdjustments;

        if (string.IsNullOrEmpty(_cfg.WallpaperAdjustment))
            WallpaperAdjustment = _wallpaperChanger.WpEnvironment.GetWallpaperAdjustment();

        if (_changerType != WallpaperChangerType.None)
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

        if (AppConfiguration.ToFile(_cfg))
        {
            await CreateNotification("Settings saved!", NotificationType.Success);

            if (_changerType != WallpaperChangerType.None)
            {
                UpdateAppTitle(Interval);
                _wallpaperChanger.Start();
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

        if (string.IsNullOrEmpty(WallpaperAdjustment))
        {
            await CreateNotification("Wallpaper adjustment cannot be empty.", NotificationType.Error);
            return false;
        }

        if (string.IsNullOrEmpty(WallpapersFolder))
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

        switch (_changerType)
        {
            case WallpaperChangerType.Local:
            {
                var (success, message) = LocalViewModel.ValidateConfiguration();
                if (!success)
                    await CreateNotification(message, NotificationType.Error);

                return success;
            }
            case WallpaperChangerType.Wallhaven:
            {
                var (success, message) = WallhavenViewModel.ValidateConfiguration();
                if (!success)
                    await CreateNotification(message, NotificationType.Error);

                return success;
            }
            case WallpaperChangerType.Bing:
            {
                var (success, message) = BingViewModel.ValidateConfiguration();
                if (!success)
                    await CreateNotification(message, NotificationType.Error);

                return success;
            }
            case WallpaperChangerType.None:
            default:
                return true;
        }
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

        var version = Assembly.GetExecutingAssembly().GetName().Version?.ToString(2);
        AppTitle = $"Wallsh {version} | WP Change: {nextChangeTime.ToLongTimeString()}";
    }
}
