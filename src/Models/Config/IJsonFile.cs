using System.Text.Json.Serialization;

namespace Wallsh.Models.Config;

public interface IJsonFile
{
    [JsonIgnore]
    string? FilePath { get; set; }
}
