using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Wallsh.Messages;
using Wallsh.Models;
using Wallsh.Services;

namespace Wallsh.ViewModels;

public partial class MainWindowViewModel : ViewModelBase,
    IRecipient<WallpaperHandlerChanged>,
    IRecipient<TimerUpdatedMessage>
{
    private readonly AppJsonConfiguration _cfg;
    private readonly WallpaperChanger _wallpaperChanger;

    [ObservableProperty]
    private string _appTitle = "Wallsh";

    private WallpaperHandler _handler;

    [ObservableProperty]
    [NotifyPropertyChangedRecipients]
    [NotifyPropertyChangedFor(nameof(Interval))]
    private int _hours;

    [ObservableProperty]
    [NotifyPropertyChangedRecipients]
    [NotifyPropertyChangedFor(nameof(Interval))]
    private int _minutes;

    [ObservableProperty]
    [NotifyPropertyChangedRecipients]
    [NotifyPropertyChangedFor(nameof(Interval))]
    private int _seconds;

    [ObservableProperty]
    private string? _wallpaperAdjustment;

    [ObservableProperty]
    [NotifyPropertyChangedRecipients]
    private string _wallpapersFolder;

    public MainWindowViewModel()
    {
        Messenger.RegisterAll(this);

        _cfg = AppJsonConfiguration.FromFile();

        LocalViewModel = new(_cfg);
        WallhavenViewModel = new(_cfg);

        _handler = _cfg.Handler;
        _hours = _cfg.Interval.Hour;
        _minutes = _cfg.Interval.Minute;
        _seconds = _cfg.Interval.Second;
        _wallpapersFolder = _cfg.WallpapersFolder;
        _wallpaperAdjustment = _cfg.WallpaperAdjustment;

        _wallpaperChanger = new(_cfg);

        if (_handler != WallpaperHandler.None)
        {
            _wallpaperChanger.Start();
            UpdateAppTitle(Interval);
        }
    }

    public LocalViewModel LocalViewModel { get; }
    public WallhavenViewModel WallhavenViewModel { get; }

    public TimeOnly Interval => new(Hours, Minutes, Seconds);

    public static List<string> Adjustments => ["none", "scaled", "zoom", "wallpaper"];

    public void Receive(TimerUpdatedMessage message) => UpdateAppTitle(message.time);

    public void Receive(WallpaperHandlerChanged message) => _handler = message.Handler;

    [RelayCommand(CanExecute = nameof(IsValidConfiguration))]
    private void SaveConfiguration()
    {
        _wallpaperChanger.Stop();

        _cfg.Handler = _handler;
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

        _wallpaperChanger.Config = _cfg;
        _wallpaperChanger.SetInterval(_cfg.Interval);

        _wallpaperChanger.SetWallpaperAdjustment();

        if (AppJsonConfiguration.ToFile(_cfg))
        {
            Console.WriteLine("Settings saved!");

            if (_handler != WallpaperHandler.None)
            {
                UpdateAppTitle(Interval);
                _wallpaperChanger.Start();
            }
        }
        else
            Console.WriteLine("Failed to save settings!");
    }

    private bool IsValidConfiguration()
    {
        if (Interval == TimeOnly.MinValue)
        {
            Console.WriteLine("Interval cannot be zero.");
            return false;
        }

        if (string.IsNullOrEmpty(WallpaperAdjustment))
        {
            Console.WriteLine("Wallpaper adjustment cannot be empty.");
            return false;
        }

        if (string.IsNullOrEmpty(WallpapersFolder))
        {
            Console.WriteLine("Wallpaper folder cannot be empty.");
            return false;
        }

        if (!Directory.Exists(WallpapersFolder))
        {
            Console.WriteLine("Wallpaper folder does not exist.");
            return false;
        }

        switch (_handler)
        {
            case WallpaperHandler.Local when !LocalViewModel.ValidateConfiguration():
            case WallpaperHandler.Wallhaven when !WallhavenViewModel.ValidateConfiguration():
                return false;
            case WallpaperHandler.None:
            default:
                return true;
        }
    }

    protected override void Broadcast<T>(T oldValue, T newValue, string? propertyName)
    {
        Messenger.Send(new IntervalChanged(Interval));
        Messenger.Send(new WallpaperFolderChangedMessage(WallpapersFolder));
    }

    private void UpdateAppTitle(TimeOnly time)
    {
        var nextChangeTime = DateTime.Now
            .AddHours(time.Hour)
            .AddMinutes(time.Minute)
            .AddSeconds(time.Second);

        AppTitle = $"Wallsh -- Next update ~{nextChangeTime.ToLongTimeString()}";
    }
}
