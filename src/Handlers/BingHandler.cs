using Wallsh.Models;
using Wallsh.Services;

namespace Wallsh.Handlers;

public class BingHandler : IWallpaperHandler
{
    public void OnChange(WallpaperChanger changer) => Console.WriteLine("Bing On Change");
}
