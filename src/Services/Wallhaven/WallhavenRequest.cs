using Wallsh.Models;

namespace Wallsh.Services.Wallhaven;

public class WallhavenRequest : ApiRequest<WallhavenConfiguration>
{
    protected WallhavenApiResponse? LatestResponse;

    protected override UriBuilder BuildRequestUri(WallhavenConfiguration cfg)
    {
        var qParams = new Dictionary<string, string>
        {
            ["categories"] = cfg.CategoriesToString(),
            ["purity"] = cfg.PurityToString(),
            ["sorting"] = cfg.SortingToString(),
            ["atleast"] = cfg.Resolution,
            ["page"] = cfg.Page.ToString(),
            ["ratios"] = cfg.RatioToString()
        };

        if (!string.IsNullOrWhiteSpace(cfg.ApiKey))
            qParams.Add("apikey", cfg.ApiKey);

        var uri = new UriBuilder
        {
            Scheme = "http",
            Host = "wallhaven.cc",
            Path = "api/v1/search",
            Query = string.Join("&", qParams.Select(kv => $"{kv.Key}={kv.Value}"))
        };

        return uri;
    }

    protected bool ShouldFetchNewWallpapers() => LatestResponse is null;
}
