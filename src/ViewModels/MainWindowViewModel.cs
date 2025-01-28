using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.Logging;
using Wallsh.Messages;
using Wallsh.Models;
using Wallsh.Models.Config;
using Wallsh.Models.Environments;
using Wallsh.Models.History;
using Wallsh.Services.System;

namespace Wallsh.ViewModels;

[NotifyPropertyChangedRecipients]
public partial class MainWindowViewModel : ViewModelBase,
    IRecipient<StopRequestedMessage>,
    IRecipient<WallpaperUpdatedMessage>
{
    private readonly AppConfiguration _cfg;
    private readonly WallpaperHistory _history;
    private readonly ILogger<MainWindowViewModel> _log = App.CreateLogger<MainWindowViewModel>();
    private readonly WallpaperManager _wallpaperManager;
    private readonly IWpEnvironment _wpEnvironment;

    [ObservableProperty]
    private WallpaperChangerType _changerType;

    [ObservableProperty]
    private bool _historyEnabled;

    [ObservableProperty]
    private int _historyMaxItems;

    [ObservableProperty]
    private TimeSpan _interval;

    [ObservableProperty]
    private bool _isNotificationVisible;

    [ObservableProperty]
    private string? _notificationText;

    [ObservableProperty]
    private NotificationType _notificationType;

    [ObservableProperty]
    private string? _wallpaperAdjustment;

    [ObservableProperty]
    private string _wallpapersFolder;

    public LocalViewModel LocalViewModel { get; }
    public WallhavenViewModel WallhavenViewModel { get; }
    public BingViewModel BingViewModel { get; }
    public string[] Adjustments { get; }

    public MainWindowViewModel()
    {
        Messenger.RegisterAll(this);

        // Avalonia designer doesn't like DI
        if (Design.IsDesignMode)
        {
            _cfg = new();
            _history = new();
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
        _history = Ioc.Default.GetRequiredService<WallpaperHistory>();

        _wpEnvironment = Ioc.Default.GetRequiredService<IWpEnvironment>();
        LocalViewModel = Ioc.Default.GetRequiredService<LocalViewModel>();
        WallhavenViewModel = Ioc.Default.GetRequiredService<WallhavenViewModel>();
        BingViewModel = Ioc.Default.GetRequiredService<BingViewModel>();
        _wallpaperManager = new(_wpEnvironment);

        Adjustments = _wpEnvironment.WallpaperAdjustments;
        ChangerType = _cfg.ChangerType;
        Interval = _cfg.Interval;
        WallpapersFolder = _cfg.WallpapersFolder;
        WallpaperAdjustment = _cfg.WallpaperAdjustment ?? _wpEnvironment.GetWallpaperAdjustment();

        HistoryEnabled = _history.Enabled;
        HistoryMaxItems = _history.MaxItems;

        if (Design.IsDesignMode)
            return;

        if (ChangerType != WallpaperChangerType.None)
            _wallpaperManager.Start();
    }

    public void Receive(StopRequestedMessage message) => ChangerType = WallpaperChangerType.None;

    public void Receive(WallpaperUpdatedMessage message)
    {
        var history = Ioc.Default.GetRequiredService<WallpaperHistory>();
        var latest = history.Wallpapers.FirstOrDefault();

        if (latest is null)
            _log.LogWarning("Latest wallpaper is null.");
    }

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

        // History
        _history.Enabled = HistoryEnabled;
        _history.MaxItems = HistoryMaxItems;

        if (_history.MaxItems == 0)
            _history.Wallpapers.Clear();

        _wpEnvironment.SetWallpaperAdjustment(_cfg.WallpaperAdjustment);

        if (!JsonFile.SerializeAndWrite(_cfg))
        {
            await CreateNotification("Failed to save settings!", NotificationType.Error);
            return;
        }

        if (!JsonFile.SerializeAndWrite(_history))
        {
            await CreateNotification("Failed to save history settings!", NotificationType.Error);
            return;
        }

        if (ChangerType != WallpaperChangerType.None)
        {
            _wallpaperManager.SetInterval(_cfg.Interval);
            _wallpaperManager.Start();
        }

        await CreateNotification("Settings saved!", NotificationType.Success);
    }

    private async Task<bool> IsValidConfiguration()
    {
        if (Interval == TimeSpan.Zero)
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

    [RelayCommand]
    private async Task BrowseWallpapersFolder()
    {
        var folderService = Ioc.Default.GetRequiredService<OpenFolderService>();
        var folder = await folderService.OpenFolderAsync();

        if (folder is not null)
            WallpapersFolder = folder.TryGetLocalPath() ?? string.Empty;
    }

    private async Task CreateNotification(string message, NotificationType type)
    {
        NotificationType = type;
        NotificationText = message;
        IsNotificationVisible = true;

        await Task.Delay(TimeSpan.FromSeconds(2));
        IsNotificationVisible = false;
    }
}
