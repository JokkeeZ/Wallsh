using System.Text.Json.Serialization;
using Wallsh.Models;

namespace Wallsh.Services.Wallhaven;

// ReSharper disable once ClassNeverInstantiated.Global
public class WallhavenApiResponse : IApiResponse
{
    [JsonPropertyName("data")]
    // ReSharper disable once CollectionNeverUpdated.Global
    public List<WallhavenImage> Data { get; set; } = [];

    [JsonPropertyName("meta")]
    public Meta? Meta { get; set; }
}

// ReSharper disable once ClassNeverInstantiated.Global
public class WallhavenImage
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("url")]
    public string? Url { get; set; }

    [JsonPropertyName("short_url")]
    public string? ShortUrl { get; set; }

    [JsonPropertyName("views")]
    public int Views { get; set; }

    [JsonPropertyName("favorites")]
    public int Favorites { get; set; }

    [JsonPropertyName("source")]
    public string? Source { get; set; }

    [JsonPropertyName("purity")]
    public string? Purity { get; set; }

    [JsonPropertyName("category")]
    public string? Category { get; set; }

    [JsonPropertyName("dimension_x")]
    public int DimensionX { get; set; }

    [JsonPropertyName("dimension_y")]
    public int DimensionY { get; set; }

    [JsonPropertyName("resolution")]
    public string? Resolution { get; set; }

    [JsonPropertyName("ratio")]
    public string? Ratio { get; set; }

    [JsonPropertyName("file_size")]
    public int FileSize { get; set; }

    [JsonPropertyName("file_type")]
    public string? FileType { get; set; }

    [JsonPropertyName("created_at")]
    public string? CreatedAt { get; set; }

    [JsonPropertyName("colors")]
    public List<string> Colors { get; set; } = [];

    [JsonPropertyName("path")]
    // ReSharper disable once UnusedAutoPropertyAccessor.Global
    public string? Path { get; set; }

    [JsonPropertyName("thumbs")]
    public Thumbs? Thumbs { get; set; }
}

// ReSharper disable once ClassNeverInstantiated.Global
public class Thumbs
{
    [JsonPropertyName("large")]
    public string? Large { get; set; }

    [JsonPropertyName("original")]
    public string? Original { get; set; }

    [JsonPropertyName("small")]
    public string? Small { get; set; }
}

// ReSharper disable once ClassNeverInstantiated.Global
public class Meta
{
    [JsonPropertyName("current_page")]
    public int CurrentPage { get; set; }

    [JsonPropertyName("last_page")]
    public int LastPage { get; set; }

    [JsonPropertyName("per_page")]
    public int PerPage { get; set; }

    [JsonPropertyName("total")]
    public int Total { get; set; }

    [JsonPropertyName("query")]
    public object? Query { get; set; }

    [JsonPropertyName("seed")]
    public object? Seed { get; set; }
}
