using Wallsh.Models;

namespace Wallsh.Handlers;

public class BingHandler : IWpService
{
    public async Task OnChange(WallpaperChanger changer)
    {
        
        
        Console.WriteLine("[BingHandler] OnChange");
        await Task.CompletedTask;
    }

    public void Reset(WallpaperChanger changer)
    {
        Console.WriteLine("[BingHandler] Reset");
    }
}
