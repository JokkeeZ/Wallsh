using Wallsh.Models;

namespace Wallsh.Handlers;

public class BingHandler : IWpService
{
    public void OnChange(WallpaperChanger changer) => Console.WriteLine("Bing On Change");
    
    public void Reset(WallpaperChanger changer) => Console.Write("Bind reset");
}
