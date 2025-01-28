using System.Timers;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Wallsh.Messages;
using Wallsh.Models.Config;
using Wallsh.Models.Environments;
using Timer = System.Timers.Timer;

namespace Wallsh.Models;

public class WallpaperManager : IDisposable
{
    private readonly ILogger<WallpaperManager> _log = App.CreateLogger<WallpaperManager>();
    private readonly Timer _timer;

    private readonly IWpEnvironment _wpEnvironment;
    private IWallpaperChanger _changer;

    public AppConfiguration Config { get; set; }

    public WallpaperManager(IWpEnvironment env)
    {
        _wpEnvironment = env;

        Config = Ioc.Default.GetRequiredService<AppConfiguration>();
        var sp = Ioc.Default.GetRequiredService<IServiceProvider>();
        _changer = sp.GetRequiredKeyedService<IWallpaperChanger>(Config.ChangerType);

        _timer = new(Config.Interval);
        _timer.Elapsed += OnTimerElapsed;
    }

    public void Dispose()
    {
        _timer.Dispose();
        GC.SuppressFinalize(this);
    }

    private void OnTimerElapsed(object? sender, ElapsedEventArgs e)
    {
        Task.Run(async () => await _changer.OnChange(this));
        _log.LogDebug("OnTimerElapsed completed.");
    }

    public void Stop()
    {
        _timer.Stop();
        _log.LogDebug("Timer stopped.");
    }

    public void RequestStop()
    {
        _timer.Stop();
        _log.LogDebug("{Changer} requested to stop.. Timer stopped.", Config.ChangerType);
        WeakReferenceMessenger.Default.Send(new StopRequestedMessage());
    }

    public void Start()
    {
        var sp = Ioc.Default.GetRequiredService<IServiceProvider>();
        _changer = sp.GetRequiredKeyedService<IWallpaperChanger>(Config.ChangerType);

        _changer.Reset(this);

        var folder = GetChangerDownloadFolderPath();
        if (Config.ChangerType != WallpaperChangerType.Local && !Directory.Exists(folder))
        {
            var cwd = Directory.CreateDirectory(folder);
            _log.LogDebug("Attempted to make folder for: {Changer} -> '{Path}'", Config.ChangerType, cwd.FullName);
        }

        _timer.Start();
        _log.LogDebug("Timer started.");
    }

    public void SetInterval(TimeSpan time)
    {
        _timer.Interval = time.TotalMilliseconds;
        _log.LogDebug("Timer interval set to: {Hour:00}:{Minute:00}:{Second:00}", 
            time.Hours, time.Minutes, time.Seconds);
    }

    public string? GetRandomWallpaperFromFolder(string folder)
    {
        var currentWallpaper = _wpEnvironment.GetCurrentWallpaperPath();

        var directory = new DirectoryInfo(folder);
        var wallpapers = _wpEnvironment.SupportedFileExtensions
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

    public bool FileExistsInChangerDownloadFolder(string filename) =>
        File.Exists(Path.Combine(GetChangerDownloadFolderPath(), filename));

    public string GetChangerDownloadFolderPath() =>
        Path.Combine(Config.WallpapersFolder, Config.ChangerType.ToString());
}
