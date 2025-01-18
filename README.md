# Wallsh
Wallsh is a cross-platform C# application for slideshow wallpapers. Optionally downloads and sets wallpapers from different services, such as wallhaven.cc. See currently supported services [below](#supported-services).

> [!WARNING]
> Currently supports only GNOME Desktop environment. Tested in Ubuntu 24.04.

# Supported services
- [wallhaven.cc](https://wallhaven.cc)

# Build and Run Wallsh
You need to have [.NET 9.0](https://dotnet.microsoft.com/en-us/download) installed on your system before building Wallsh.
```sh
git clone https://github.com/JokkeeZ/Wallsh.git
cd Wallsh/src
dotnet run
```

# Dependencies
- [Avalonia](https://github.com/AvaloniaUI/Avalonia)
- [Community Toolkit](https://github.com/CommunityToolkit/dotnet)
- [GtkSharp](https://github.com/GtkSharp/GtkSharp)

# License
Wallsh is licensed under the [MIT License](https://github.com/JokkeeZ/Wallsh/blob/main/LICENSE), the same as **Avalonia** and **Community Toolkit**.

This application also uses **GtkSharp** and it's related components, which are licensed under the [LGPL v2.0 License](https://github.com/GtkSharp/GtkSharp/blob/develop/LICENSE).
