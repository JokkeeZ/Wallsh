namespace Wallsh.Models.Wallhaven;

public class WallhavenConfiguration
{
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
