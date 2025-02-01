<h1>
    <p align="center">
        <img width="128" alt="Logo" src="https://github.com/JokkeeZ/Wallsh/blob/master/src/Assets/Wallsh.svg"/>
        <br/>Wallsh
    </p>
</h1>

Wallsh is a cross-platform C#/.NET desktop application for slideshow wallpapers. Optionally downloads and sets wallpapers from different sources, such as wallhaven.cc and Bing wallpaper of the day. See currently supported sources [below](#supported-sources).

> [!CAUTION]
> Currently, only GNOME and Windows 8 and later are supported
>
> **Tested with Ubuntu 24.04 (GNOME Shell 46) and Windows 11**

# Supported sources
- [wallhaven.cc](https://wallhaven.cc)
- [Bing Wallpaper](https://www.bing.com)

# Build and Run Wallsh
> [!IMPORTANT]
> You need to have [.NET 9.0](https://dotnet.microsoft.com/en-us/download) installed on your system before building Wallsh.

```sh
git clone https://github.com/JokkeeZ/Wallsh.git
cd Wallsh/src
dotnet run
```

# Dependencies
- [Avalonia](https://github.com/AvaloniaUI/Avalonia)
- [Material.Avalonia](https://github.com/AvaloniaCommunity/Material.Avalonia)
- [CommunityToolkit.Mvvm](https://github.com/CommunityToolkit/dotnet)
- [Microsoft.Extensions.DependencyInjection](https://github.com/dotnet/runtime)
- [Microsoft.Extensions.Logging](https://github.com/dotnet/runtime)
- [Microsoft.Extensions.Logging.Console](https://github.com/dotnet/runtime)
- [Microsoft.Extensions.Logging.Debug](https://github.com/dotnet/runtime)

# License
Wallsh is licensed under the [MIT License](https://github.com/JokkeeZ/Wallsh/blob/master/LICENSE).
