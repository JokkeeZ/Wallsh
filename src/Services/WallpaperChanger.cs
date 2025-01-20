using System.Timers;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using Wallsh.Messages;
using Wallsh.Models;
using Wallsh.Services.Wallhaven;
using Timer = System.Timers.Timer;

namespace Wallsh.Services;

public class WallpaperChanger : ObservableRecipient, IDisposable
{
    private readonly Dictionary<WallpaperHandler, IWallpaperHandler> _services = new()
    {
        [WallpaperHandler.Local] = new LocalHandler(),
        [WallpaperHandler.Wallhaven] = new WallhavenHandler()
    };

    private readonly Timer _timer;

    public WallpaperChanger(AppJsonConfiguration cfg)
    {
        Config = cfg;

        _timer = new(cfg.Interval.ToTimeSpan());
        _timer.Elapsed += OnTimerElapsed;
        _timer.AutoReset = true;
    }

    public AppJsonConfiguration Config { get; set; }

    public void Dispose()
    {
        _timer.Dispose();
        GC.SuppressFinalize(this);
    }

    private void OnTimerElapsed(object? sender, ElapsedEventArgs e)
    {
        try
        {
            if (!_services.TryGetValue(Config.Handler, out var service))
            {
                Console.WriteLine($"[WallpaperChanger] No service for type {Config.Handler}");
                return;
            }

            var now = DateTime.Now
                .AddHours(Config.Interval.Hour)
                .AddMinutes(Config.Interval.Minute)
                .AddSeconds(Config.Interval.Second);

            Messenger.Send(new TimerUpdatedMessage(new(now.Hour, now.Minute, now.Second)));
            service.OnChange(this, Config);
            Console.WriteLine($"[WallpaperChanger]: Timer - {e.SignalTime}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[WallpaperChanger]: Exception: {ex}");
        }
    }

    public void Stop()
    {
        _timer.Stop();
        Console.WriteLine($"[WallpaperChanger][{Config.Handler}]: Stopped.");
    }

    public void Start()
    {
        if (Config.Handler == WallpaperHandler.None)
        {
            Console.WriteLine($"[WallpaperChanger][{Config.Handler}]: No handler configured.");
            return;
        }

        _timer.Start();
        Console.WriteLine($"[WallpaperChanger][{Config.Handler}]: Started.");
    }

    public void SetInterval(TimeOnly time)
    {
        _timer.Interval = time.ToTimeSpan().TotalMilliseconds;
        Console.WriteLine(
            $"[WallpaperChanger][{Config.Handler}]: Interval - {time.Hour}h  {time.Minute}m {time.Second}s");
    }

    public static string GetWallpaperAdjustment()
    {
        if (OperatingSystem.IsLinux())
        {
            if (GnomeWallpaperHandler.IsGnome())
                return GnomeWallpaperHandler.GetCurrentAdjustment();
        }
        else if (OperatingSystem.IsWindows())
        {
            // TODO: Windows support
        }

        throw new NotImplementedException("Your operating system is not supported.");
    }

    public void SetWallpaperAdjustment()
    {
        if (OperatingSystem.IsLinux())
        {
            if (GnomeWallpaperHandler.IsGnome())
                GnomeWallpaperHandler.SetAdjustment(Config.WallpaperAdjustment);
        }
        else if (OperatingSystem.IsWindows())
        {
            // TODO: Windows support
            throw new NotImplementedException("Your operating system is not supported.");
        }
    }
}
