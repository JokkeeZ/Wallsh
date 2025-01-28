using System.Text.Json;

namespace Wallsh.Models.Config;

public static class JsonFile
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    public static T ReadAndDeserialize<T>(string fileName) where T : IJsonFile, new()
    {
        var filePath = Path.Combine(GetFolderPath(), fileName);
        T config = new() { FilePath = filePath };

        try
        {
            var json = File.ReadAllText(filePath);
            config = JsonSerializer.Deserialize<T>(json) ?? new();
            config.FilePath = filePath;

            return config;
        }
        catch
        {
            return config;
        }
    }

    public static bool SerializeAndWrite<T>(T file) where T : IJsonFile, new()
    {
        try
        {
            // Create ~/.config/wallsh folder if not exist
            Directory.CreateDirectory(GetFolderPath());

            var text = JsonSerializer.Serialize(file, JsonOptions);
            File.WriteAllText(file.FilePath!, text);

            return true;
        }
        catch
        {
            return false;
        }
    }

    private static string GetFolderPath()
    {
        var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        var folderPath = Path.Combine(home, ".config", "wallsh");

        return folderPath;
    }
}
