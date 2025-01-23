using System.Timers;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.Logging;
using Wallsh.Changers;
using Wallsh.Messages;
using Wallsh.Models.Environments;
using Wallsh.Models.Environments.Linux;
using Wallsh.Models.Environments.Windows;
using Timer = System.Timers.Timer;

namespace Wallsh.Models;

public class WallpaperChanger : ObservableRecipient, IDisposable
{
    private readonly ILogger<WallpaperChanger> _log = App.CreateLogger<WallpaperChanger>();

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
        if (!_services.TryGetValue(Config.ChangerType, out var service))
        {
            _log.LogError("No service found for type: {Changer}", Config.ChangerType);
            return;
        }

        Messenger.Send(new TimerUpdatedMessage(Config.Interval));
        Task.Run(async () => await service.OnChange(this));
    }

    public void Stop()
    {
        _timer.Stop();
        _log.LogDebug("{Changer} stopped.", Config.ChangerType);
    }

    public void Start()
    {
        if (Config.ChangerType == WallpaperChangerType.None)
        {
            _log.LogInformation("Not starting with changer: {Changer}", Config.ChangerType);
            return;
        }

        if (_services.TryGetValue(Config.ChangerType, out var service))
        {
            _log.LogDebug("Resetting {Changer} before starting.", Config.ChangerType);
            service.Reset(this);

            var folder = GetChangerDownloadFolderPath();
            if (Config.ChangerType != WallpaperChangerType.Local && !Directory.Exists(folder))
            {
                var cwd = Directory.CreateDirectory(folder);
                _log.LogDebug("Attempted to make folder for: {Changer} -> '{Path}'", Config.ChangerType, cwd.FullName);
            }
        }

        _timer.Start();
        _log.LogDebug("{Changer} started.", Config.ChangerType);
    }

    public void SetInterval(TimeOnly time)
    {
        _timer.Interval = time.ToTimeSpan().TotalMilliseconds;
        _log.LogDebug("{Changer} interval set to: {Interval:HH:mm:ss}", Config.ChangerType, time);
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
