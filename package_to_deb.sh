#!/bin/bash

# Project version
version=$(grep -oP '(?<=<Version>).*?(?=</Version>)' "./src/Wallsh.csproj")

# .NET publish self-contained
dotnet publish "./src/Wallsh.csproj" \
    --nologo \
    --configuration Release \
    --self-contained true \
    --runtime linux-x64 \
    --output "./staging/usr/lib/Wallsh"

# Debian control file
mkdir ./staging/DEBIAN
cat << EOF >> ./staging/DEBIAN/control
Package: Wallsh
Version: $version
Section: devel
Priority: optional
Architecture: amd64
Depends: libx11-6, libice6, libsm6, libfontconfig1, libc6, libgcc-s1, libicu74, libssl3, libstdc++6, zlib1g, libglib2.0-dev
Maintainer: Joel <https://github.com/JokkeeZ>
Homepage: https://github.com/JokkeeZ/Wallsh
Description: Wallpaper slideshow handler
Copyright: 2025 Joel <https://github.com/JokkeeZ>
EOF

# Start script
mkdir ./staging/usr/bin
cat << EOF >> ./staging/usr/bin/Wallsh
#!/bin/bash
exec /usr/lib/Wallsh/Wallsh "\$@"
EOF
chmod +x ./staging/usr/bin/Wallsh

# Set appropriate permissions
chmod -R a+rX ./staging/usr/lib/Wallsh/
chmod +x ./staging/usr/lib/Wallsh/Wallsh

# Desktop shortcut
mkdir ./staging/usr/share
mkdir ./staging/usr/share/applications
cat << EOF >> ./staging/usr/share/applications/Wallsh.desktop
[Desktop Entry]
Name=Wallsh
Comment=Wallpaper slideshow handler
Icon=Wallsh
Exec=Wallsh %F
StartupWMClass=Wallsh
Terminal=false
Type=Application
Categories=Development
GenericName=Wallsh
Keywords=wallpaper; background; Wallsh
EOF

# Desktop icon
mkdir ./staging/usr/share/pixmaps
cp ./src/Assets/Wallsh.png ./staging/usr/share/pixmaps/Wallsh.png

# Hicolor icons
mkdir ./staging/usr/share/icons
mkdir ./staging/usr/share/icons/hicolor
mkdir ./staging/usr/share/icons/hicolor/scalable
mkdir ./staging/usr/share/icons/hicolor/scalable/apps
cp ./src/Assets/Wallsh.svg ./staging/usr/share/icons/hicolor/scalable/apps/Wallsh.svg

# Make .deb file
dpkg-deb --root-owner-group \--build ./staging/ ./wallsh_${version}_amd64.deb

# Clean-up
rm -rf ./staging/
