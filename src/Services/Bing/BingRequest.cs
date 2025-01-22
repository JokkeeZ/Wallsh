using Wallsh.Models;

namespace Wallsh.Services.Bing;

public class BingRequest : ApiRequest<BingConfiguration>
{
    protected override UriBuilder BuildRequestUri(BingConfiguration cfg)
    {
        var qParams = new Dictionary<string, string>
        {
            ["format"] = "js",
            ["n"] = cfg.NumberOfWallpapers.ToString()
        };
        
        var uri = new UriBuilder
        {
            Scheme = "http",
            Host = "bing.com",
            Path = "HPImageArchive.aspx",
            Query = string.Join("&", qParams.Select(kv => $"{kv.Key}={kv.Value}"))
        };

        return uri;
    }
}
