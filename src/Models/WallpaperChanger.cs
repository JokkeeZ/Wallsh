using System.Timers;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using Wallsh.Handlers;
using Wallsh.Messages;
using Wallsh.Models.Environments;
using Timer = System.Timers.Timer;

namespace Wallsh.Models;

public class WallpaperChanger : ObservableRecipient, IDisposable
{
    private readonly Dictionary<WallpaperHandler, IWpService> _services = new()
    {
        [WallpaperHandler.Local] = new LocalHandler(),
        [WallpaperHandler.Wallhaven] = new WallhavenHandler(),
        [WallpaperHandler.Bing] = new BingHandler()
    };

    private readonly Timer _timer;

    public IWpEnvironment WpEnvironment { get; } = null!;

    public AppJsonConfiguration Config { get; set; }

    public WallpaperChanger(AppJsonConfiguration cfg)
    {
        Config = cfg;

        _timer = new(cfg.Interval.ToTimeSpan());
        _timer.Elapsed += OnTimerElapsed;

        if (OperatingSystem.IsLinux())
        {
            if (GnomeWpEnvironment.IsGnome())
                WpEnvironment = new GnomeWpEnvironment();
            else
                throw new NotImplementedException("This environment is not supported.");
        }
        // TODO: Add Windows support.
        else if (OperatingSystem.IsWindows())
            throw new NotImplementedException("Windows is not supported.");
    }

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

            Messenger.Send(new TimerUpdatedMessage(Config.Interval));
            service.OnChange(this);
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

        if (_services.TryGetValue(Config.Handler, out var service))
        {
            Console.WriteLine($"[WallpaperChanger] Resetting {Config.Handler} before starting.");
            service.Reset(this);
        }

        _timer.Start();
        Console.WriteLine($"[WallpaperChanger][{Config.Handler}]: Started.");
    }

    public void SetInterval(TimeOnly time)
    {
        _timer.Interval = time.ToTimeSpan().TotalMilliseconds;
        Console.WriteLine($"[WallpaperChanger][{Config.Handler}]: Interval - {time:HH:mm:ss}");
    }

    public string GetRandomWallpaperFromDisk(string folder)
    {
        var currentWallpaper = WpEnvironment.GetCurrentWallpaperPath();

        var directory = new DirectoryInfo(folder);
        var wallpapers = WpEnvironment.SupportedFileExtensions
            .SelectMany(directory.EnumerateFiles)
            .Where(wp => wp.FullName != currentWallpaper)
            .ToArray();

        var randomFromDisk = wallpapers[Random.Shared.Next(wallpapers.Length)];
        return randomFromDisk.FullName;
    }
}
