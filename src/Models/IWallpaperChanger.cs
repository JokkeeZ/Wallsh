namespace Wallsh.Models;

public interface IWallpaperChanger
{
    Task OnChange(WallpaperChanger changer);

    void Reset(WallpaperChanger changer);

    public void CreateFolderIfNotExists(WallpaperChanger changer)
    {
        var folder = Path.Combine(changer.Config.WallpapersFolder, changer.Config.ChangerType.ToString());
        Directory.CreateDirectory(folder);
    }
}
