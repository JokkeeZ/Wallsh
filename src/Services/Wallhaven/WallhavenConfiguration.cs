using System.Text.Json.Serialization;

namespace Wallsh.Services.Wallhaven;

public class WallhavenConfiguration
{
    [JsonIgnore]
    public static readonly Dictionary<WallhavenRatio, List<string>> Resolutions = new()
    {
        [WallhavenRatio.Ratio16X10] = ["1280x800", "1600x1000", "1920x1200", "2560x1600", "3840x2400"],
        [WallhavenRatio.Ratio16X9] = ["1280x720", "1600x900", "1920x1080", "2560x1440", "3840x2160"],
        [WallhavenRatio.Ratio4X3] = ["1280x960", "1600x1200", "1920x1440", "2560x1920", "3840x2880"],
        [WallhavenRatio.Ratio5X4] = ["1280x1024", "1600x1280", "1920x1536", "2560x2048", "3840x3072"],
        [WallhavenRatio.RatioUltrawide] = ["2560x1080", "3440x1440", "3840x1600"]
    };

    public string? ApiKey { get; set; }
    public bool General { get; set; } = true;
    public bool Anime { get; set; } = true;
    public bool People { get; set; }
    public WallhavenRatio Ratio { get; set; } = WallhavenRatio.Ratio16X9;
    public WallhavenSorting Sorting { get; set; } = WallhavenSorting.Random;
    public string Resolution { get; set; } = "1920x1080";
    public bool PuritySfw { get; set; } = true;
    public bool PuritySketchy { get; set; }
    public bool PurityNsfw { get; set; }

    public int Page { get; set; } = 1;

    public string PurityToString() => string.Join(string.Empty,
        PuritySfw ? '1' : '0',
        PuritySketchy ? '1' : '0',
        PurityNsfw ? '1' : '0'
    );

    public string CategoriesToString() => string.Join(string.Empty,
        General ? '1' : '0',
        Anime ? '1' : '0',
        People ? '1' : '0'
    );

    public string RatioToString() => Ratio switch
    {
        WallhavenRatio.RatioUltrawide => "Ultrawide",
        WallhavenRatio.Ratio16X9 => "16x9",
        WallhavenRatio.Ratio16X10 => "16x10",
        WallhavenRatio.Ratio5X4 => "5x4",
        WallhavenRatio.Ratio4X3 => "4x3",
        _ => throw new("Unknown ratio")
    };

    public string SortingToString() => Sorting switch
    {
        WallhavenSorting.Top => "toplist",
        WallhavenSorting.Views => "views",
        WallhavenSorting.Random => "random",
        WallhavenSorting.Date => "date_added",
        _ => throw new("Unknown sorting")
    };
}
