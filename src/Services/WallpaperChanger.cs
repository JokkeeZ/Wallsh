using System;
using System.Collections.Generic;
using System.Timers;
using CommunityToolkit.Mvvm.ComponentModel;
using Wallsh.Models;
using Wallsh.Services.Wallhaven;

namespace Wallsh.Services;

public class WallpaperChanger : ObservableObject
{
    private readonly Timer _timer;

    private readonly Dictionary<WallpaperService, IWallpaperChangerService> _services = new()
    {
        [WallpaperService.Local] = new LocalWallpaperService(),
        [WallpaperService.Wallhaven] = new WallhavenWallpaperService()
    };
    
    public AppJsonConfiguration Config { get; }
    
    public WallpaperChanger()
    {
        Config = AppConfiguration.FromFile();
        
        _timer = new(new TimeSpan(0, Config.Hours, Config.Minutes, Config.Seconds));
        _timer.Elapsed += OnTimerElapsed;
        _timer.AutoReset = true;
    }

    private void OnTimerElapsed(object? sender, ElapsedEventArgs e)
    {
        try
        {
            if (!_services.TryGetValue(Config.Service, out var service))
            {
                Console.WriteLine($"[WallpaperChangerService] No service for type {Config.Service}");
                return;
            }
            
            service.OnChange(this, Config);
            Console.WriteLine($"[WallpaperChangerService]: Timer - {e.SignalTime}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[WallpaperChangerService]: Exception: {ex}");
        }
    }

    public void SetService(WallpaperService service)
    {
        Config.Service = service;
        Console.WriteLine($"[WallpaperChangerService]: Service set to: {service}");
    }
    
    public void Toggle(bool state)
    {
        _timer.Enabled = state;
        Console.WriteLine($"[WallpaperChangerService][{Config.Service}]: Enabled = {state}");
    }

    public void UpdateInterval(int hours, int minutes, int seconds)
    {
        Toggle(false);
        _timer.Interval = new TimeSpan(0, hours, minutes,seconds).TotalMilliseconds;
        Console.WriteLine($"[WallpaperChangerService][{Config.Service}]: Interval - {hours}h  {minutes}m {seconds}s");
    }

    public static string GetWallpaperAdjustment()
    {
        // TODO: Other platforms
        return GnomeWallpaperHandler.IsGnome() 
            ? GnomeWallpaperHandler.GetCurrentAdjustment() 
            : string.Empty;
    }
}
