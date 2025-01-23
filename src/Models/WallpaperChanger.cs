using System.Timers;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using Wallsh.Changers;
using Wallsh.Messages;
using Wallsh.Models.Environments;
using Wallsh.Models.Environments.Linux;
using Wallsh.Models.Environments.Windows;
using Timer = System.Timers.Timer;

namespace Wallsh.Models;

public class WallpaperChanger : ObservableRecipient, IDisposable
{
    private readonly Dictionary<WallpaperChangerType, IWallpaperChanger> _services = new()
    {
        [WallpaperChangerType.Local] = new LocalWallpaperChanger(),
        [WallpaperChangerType.Wallhaven] = new WallhavenWallpaperChanger(),
        [WallpaperChangerType.Bing] = new BingWallpaperChanger()
    };

    private readonly Timer _timer;

    public IWpEnvironment WpEnvironment { get; } = null!;

    public AppConfiguration Config { get; set; }

    public WallpaperChanger(AppConfiguration cfg)
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
            WpEnvironment = new WindowsWpEnvironment();
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
            if (!_services.TryGetValue(Config.ChangerType, out var service))
            {
                Console.WriteLine($"[WallpaperChanger]: No service for type {Config.ChangerType}");
                return;
            }

            Messenger.Send(new TimerUpdatedMessage(Config.Interval));
            Task.Run(async () => await service.OnChange(this));
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[WallpaperChanger]: Exception: {ex}");
        }
    }

    public void Stop()
    {
        _timer.Stop();
        Console.WriteLine($"[WallpaperChanger][{Config.ChangerType}]: Stopped.");
    }

    public void Start()
    {
        if (Config.ChangerType == WallpaperChangerType.None)
        {
            Console.WriteLine($"[WallpaperChanger][{Config.ChangerType}]: No handler configured.");
            return;
        }

        if (_services.TryGetValue(Config.ChangerType, out var service))
        {
            Console.WriteLine($"[WallpaperChanger]: Resetting {Config.ChangerType} before starting.");
            service.Reset(this);

            if (Config.ChangerType != WallpaperChangerType.Local)
                Directory.CreateDirectory(GetChangerDownloadFolderPath());
        }

        _timer.Start();
        Console.WriteLine($"[WallpaperChanger][{Config.ChangerType}]: Started.");
    }

    public void SetInterval(TimeOnly time)
    {
        _timer.Interval = time.ToTimeSpan().TotalMilliseconds;
        Console.WriteLine($"[WallpaperChanger][{Config.ChangerType}]: Interval - {time:HH:mm:ss}");
    }

    public string? GetRandomWallpaperFromDisk(string folder)
    {
        var currentWallpaper = WpEnvironment.GetCurrentWallpaperPath();

        var directory = new DirectoryInfo(folder);
        var wallpapers = WpEnvironment.SupportedFileExtensions
            .SelectMany(directory.EnumerateFiles)
            .ToArray();

        switch (wallpapers.Length)
        {
            // Folder does not have any wallpapers.
            case 0:
                return null;
            // Folder only has 1 wallpaper in it.
            case 1:
                return wallpapers[0].FullName;
        }

        wallpapers = [.. wallpapers.Where(wp => wp.FullName != currentWallpaper)];

        var randomFromDisk = wallpapers[Random.Shared.Next(wallpapers.Length)];
        return randomFromDisk.FullName;
    }

    public string GetChangerDownloadFolderPath() =>
        Path.Combine(Config.WallpapersFolder, Config.ChangerType.ToString());
}
