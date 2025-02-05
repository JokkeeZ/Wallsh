using CommunityToolkit.Mvvm.DependencyInjection;
using SkiaSharp;
using Wallsh.Models.Config;
using Wallsh.Models.Environments;
using Wallsh.Models.History;

namespace Wallsh.Models;

public abstract class FetchWallpaperChanger<TResponse, TImage>(IWpEnvironment env) : GenericWallpaperChanger(env)
{
    protected TResponse? LatestResponse { get; set; }

    protected virtual bool ShouldFetchNewWallpapers(WallpaperManager manager) => LatestResponse is null;
    protected virtual string GetWallpaperNameFromUrl(string url) => url.Split('/').Last();

    protected abstract Task DownloadAndSetWallpaper(WallpaperManager manager, string folder, TImage img);
}

public abstract class GenericWallpaperChanger(IWpEnvironment env) : IWallpaperChanger
{
    protected IWpEnvironment WpEnvironment { get; } = env;
    public abstract Task OnChange(WallpaperManager manager);

    protected bool SetRandomWallpaperFromFolder(WallpaperManager manager, string folder, out string? filePath)
    {
        var wpPath = manager.GetRandomWallpaperFromFolder(folder);
        if (wpPath is null)
        {
            filePath = null;
            return false;
        }

        WpEnvironment.SetWallpaperFromPath(wpPath);

        filePath = wpPath;
        return true;
    }

    protected static void SaveWallpaperToHistory(string filePath,
        string? resolution = null, string? copyright = null, string? url = null, bool isLocal = false)
    {
        var history = Ioc.Default.GetRequiredService<WallpaperHistory>();

        if (string.IsNullOrWhiteSpace(resolution))
        {
            var imgInfo = SKBitmap.DecodeBounds(filePath);
            resolution = imgInfo.Width + "x" + imgInfo.Height;
        }

        history.AddWallpaper(new()
        {
            Resolution = resolution,
            Path = filePath,
            Copyright = copyright,
            Url = url,
            IsLocal = isLocal,
            Timestamp = DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss")
        });

        JsonFile.SerializeAndWrite(history);
    }
}
