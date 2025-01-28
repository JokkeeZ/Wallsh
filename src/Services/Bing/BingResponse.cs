using System.Text.Json.Serialization;
using Wallsh.Models;

namespace Wallsh.Services.Bing;

public class BingResponse : IApiResponse
{
    [JsonPropertyName("images")]
    public List<BingWallpaper> Images { get; set; } = [];
}

public class BingWallpaper
{
    [JsonPropertyName("startdate")]
    public string? Startdate { get; set; }

    [JsonPropertyName("fullstartdate")]
    public string? Fullstartdate { get; set; }

    [JsonPropertyName("enddate")]
    public string? Enddate { get; set; }

    [JsonPropertyName("url")]
    public string? Url { get; set; }

    [JsonPropertyName("urlbase")]
    public string? Urlbase { get; set; }

    [JsonPropertyName("copyright")]
    public string? Copyright { get; set; }

    [JsonPropertyName("copyrightlink")]
    public string? Copyrightlink { get; set; }

    [JsonPropertyName("title")]
    public string? Title { get; set; }

    [JsonPropertyName("quiz")]
    public string? Quiz { get; set; }

    [JsonPropertyName("wp")]
    public bool Wp { get; set; }

    [JsonPropertyName("hsh")]
    public string? Hsh { get; set; }

    [JsonPropertyName("drk")]
    public int Drk { get; set; }

    [JsonPropertyName("top")]
    public int Top { get; set; }

    [JsonPropertyName("bot")]
    public int Bot { get; set; }

    [JsonPropertyName("hs")]
    public List<object> Hs { get; set; } = [];
}
