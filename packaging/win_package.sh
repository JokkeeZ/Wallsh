#!/bin/bash

# Project version
version=$(grep -oP '(?<=<Version>).*?(?=</Version>)' "../src/Wallsh.csproj")

# .NET publish self-contained
dotnet publish "../src/Wallsh.csproj" \
    --nologo \
    --configuration Release \
    --self-contained true \
    --runtime win-x64 \
    --output "../staging/Wallsh"

# Make .zip
zip -rj ./wallsh_${version}_win-x64.zip ../staging/

# Clean-up
rm -rf ../staging/
