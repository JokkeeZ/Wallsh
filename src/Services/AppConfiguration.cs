using System;
using System.IO;
using System.Text.Json;
using Wallsh.Models;

namespace Wallsh.Services;

public static class AppConfiguration
{
    private static string GetConfigurationPath()
    {
        var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        var wallshConfig = Path.Combine(home, ".config", "wallsh");

        // Create ~/.config/wallsh/ folder unless it already exists.
        Directory.CreateDirectory(wallshConfig);

        return Path.Combine(wallshConfig, "config.json");
    }
    
    public static AppJsonConfiguration FromFile()
    {
        if (!File.Exists(GetConfigurationPath()))
        {
            Console.WriteLine("[AppConfiguration] Could not locate existing `config.json`.");
            return new();
        }

        try
        {
            var json = File.ReadAllText(GetConfigurationPath());
            return JsonSerializer.Deserialize<AppJsonConfiguration>(json) ?? new();
        }
        catch
        {
            Console.WriteLine("[AppConfiguration] Failed to deserialize.");
            return new();
        }
    }

    public static bool ToFile(AppJsonConfiguration jsonConfiguration)
    {
        try
        {
            var text = JsonSerializer.Serialize(jsonConfiguration);
            File.WriteAllText(GetConfigurationPath(), text);

            return true;
        }
        catch
        {
            return false;
        }
    }
}
