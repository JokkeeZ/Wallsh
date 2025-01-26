using Wallsh.Models;

namespace Wallsh.Changers;

public class NoneWallpaperChanger : IWallpaperChanger
{
    public Task OnChange(WallpaperManager manager) => throw new NotImplementedException();
}
