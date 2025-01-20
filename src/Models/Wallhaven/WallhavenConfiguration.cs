namespace Wallsh.Models.Wallhaven;

public class WallhavenConfiguration
{
    [NonSerialized]
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

    public string PurityToString()
    {
        var str = new[] { '0', '0', '0' };

        if (PuritySfw)
            str[0] = '1';
        if (PuritySketchy)
            str[1] = '1';
        if (PurityNsfw)
            str[2] = '1';

        return new(str);
    }

    public string CategoriesToString()
    {
        var str = new[] { '0', '0', '0' };

        if (General)
            str[0] = '1';
        if (Anime)
            str[1] = '1';
        if (People)
            str[2] = '1';

        return new(str);
    }

    public string RatioToString() =>
        Ratio switch
        {
            WallhavenRatio.RatioUltrawide => "Ultrawide",
            WallhavenRatio.Ratio16X9 => "16x9",
            WallhavenRatio.Ratio16X10 => "16x10",
            WallhavenRatio.Ratio5X4 => "5x4",
            WallhavenRatio.Ratio4X3 => "4x3",
            _ => throw new("Unknown ratio")
        };

    public string SortingToString() =>
        Sorting switch
        {
            WallhavenSorting.Top => "toplist",
            WallhavenSorting.Views => "views",
            WallhavenSorting.Random => "random",
            WallhavenSorting.Date => "date_added",
            _ => throw new("Unknown sorting")
        };
}
