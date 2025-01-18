using System;
using System.IO;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using Wallsh.Models;
using Wallsh.Models.Wallhaven;
using Wallsh.Services;

namespace Wallsh.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    [ObservableProperty] private string _infoText = string.Empty;
    [ObservableProperty] private IBrush _infoTextBrush = Brushes.Black;
    [ObservableProperty] private int _hours;
    [ObservableProperty] private int _minutes;
    [ObservableProperty] private int _seconds;
    [ObservableProperty] private string? _wallpapersDirectory;
    [ObservableProperty] private string? _wallpaperAdjustment;
    [ObservableProperty] private string? _resolution;
    [ObservableProperty] private string? _apiKey;
    [ObservableProperty] private bool _categoryGeneral;
    [ObservableProperty] private bool _categoryAnime;
    [ObservableProperty] private bool _categoryPeople;
    [ObservableProperty] private bool _puritySfw;
    [ObservableProperty] private bool _puritySketchy;
    [ObservableProperty] private bool _purityNsfw;

    [ObservableProperty] private WallhavenSorting _sorting;
    [ObservableProperty] private WallhavenRatio _ratio;
    [ObservableProperty] private WallpaperService _service;
    
    private readonly WallpaperChanger _wallpaperChanger;
    
    public MainWindowViewModel()
    {
        _wallpaperChanger = new();
        var cfg = _wallpaperChanger.Config;

        Service = _wallpaperChanger.Config.Service;
        
        WallpapersDirectory = cfg.WallpapersDirectory;
        Hours = cfg.Hours;
        Minutes = cfg.Minutes;
        Seconds = cfg.Seconds;
        WallpaperAdjustment = cfg.WallpaperAdjustment;
        Resolution = cfg.Wallhaven.Resolution;
        ApiKey = cfg.Wallhaven.ApiKey;
        CategoryGeneral = cfg.Wallhaven.General;
        CategoryAnime = cfg.Wallhaven.Anime;
        CategoryPeople = cfg.Wallhaven.People;
        Ratio = cfg.Wallhaven.Ratio;
        PuritySfw = cfg.Wallhaven.PuritySfw;
        PuritySketchy = cfg.Wallhaven.PuritySketchy;
        PurityNsfw = cfg.Wallhaven.PurityNsfw;
        Sorting = cfg.Wallhaven.Sorting;

        if (string.IsNullOrEmpty(WallpaperAdjustment))
        {
            WallpaperAdjustment = WallpaperChanger.GetWallpaperAdjustment();
        }
        
        _wallpaperChanger.Toggle(cfg.Service != WallpaperService.None);
    }

    public void SaveConfiguration()
    {
        if (!ValidateConfiguration())
        {
            return;
        }
        
        _wallpaperChanger.Toggle(false);
        _wallpaperChanger.SetService(Service);
        
        _wallpaperChanger.Config.WallpapersDirectory = WallpapersDirectory!;
        _wallpaperChanger.Config.Hours = Hours;
        _wallpaperChanger.Config.Minutes = Minutes;
        _wallpaperChanger.Config.Seconds = Seconds;
        _wallpaperChanger.Config.WallpaperAdjustment = WallpaperAdjustment;
        _wallpaperChanger.Config.Wallhaven.Resolution = Resolution!;
        _wallpaperChanger.Config.Wallhaven.ApiKey = ApiKey;
        _wallpaperChanger.Config.Wallhaven.General = CategoryGeneral;
        _wallpaperChanger.Config.Wallhaven.Anime = CategoryAnime;
        _wallpaperChanger.Config.Wallhaven.People = CategoryPeople;
        _wallpaperChanger.Config.Wallhaven.Ratio = Ratio;
        _wallpaperChanger.Config.Wallhaven.PuritySfw = PuritySfw;
        _wallpaperChanger.Config.Wallhaven.PuritySketchy = PuritySketchy;
        _wallpaperChanger.Config.Wallhaven.PurityNsfw = PurityNsfw;
        _wallpaperChanger.Config.Wallhaven.Sorting = Sorting;
        
        if (WallpaperAdjustment is not null)
        {
            _wallpaperChanger.Config.WallpaperAdjustment = WallpaperChanger.GetWallpaperAdjustment();
        }

        if (_wallpaperChanger.Config.Service != WallpaperService.None)
        {
            _wallpaperChanger.UpdateInterval(Hours, Minutes, Seconds);
        }
        
        if (AppConfiguration.ToFile(_wallpaperChanger.Config))
        {
            UpdateInfoText("Settings saved!", Brushes.Green);
            _wallpaperChanger.Toggle(_wallpaperChanger.Config.Service != WallpaperService.None);
        }
        else
        {
            UpdateInfoText("Failed to save settings!", Brushes.Red);
        }
    }

    private bool ValidateConfiguration()
    {
        var time = new TimeSpan(0, Hours, Minutes, Seconds);

        if (time == TimeSpan.Zero)
        {
            UpdateInfoText("Interval cannot be zero.", Brushes.Red);
            return false;
        }

        // NOTE:
        // Wallhaven.cc allows 45 requests per minute, so request
        // for every 2 seconds is easily on the safe side.
        if (time.TotalSeconds <= 2 && Service == WallpaperService.Wallhaven)
        {
            UpdateInfoText("Interval needs to be at least 2 seconds.", Brushes.Red);
            return false;
        }

        if (string.IsNullOrEmpty(WallpaperAdjustment))
        {
            UpdateInfoText("Wallpaper adjustment cannot be empty.", Brushes.Red);
            return false;
        }

        if (string.IsNullOrEmpty(WallpapersDirectory))
        {
            UpdateInfoText("Wallpaper folder cannot be empty.", Brushes.Red);
            return false;
        }

        if (!Directory.Exists(WallpapersDirectory))
        {
            UpdateInfoText("Wallpaper folder does not exist.", Brushes.Red);
            return false;
        }
        
        if (Service == WallpaperService.Local)
        {
            if (Directory.GetFiles(WallpapersDirectory).Length != 0) return true;
            UpdateInfoText("Wallpaper folder is empty.", Brushes.Red);
            return false;
        }

        if (string.IsNullOrEmpty(Resolution))
        {
            UpdateInfoText("Wallpaper resolution cannot be empty.", Brushes.Red);
            return false;
        }
        
        return true;
    }
    
    private void UpdateInfoText(string text, IImmutableSolidColorBrush brush)
    {
        InfoTextBrush = brush;
        InfoText = text;
    }
}
