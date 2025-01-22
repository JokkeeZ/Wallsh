$csproj = Get-Content -Path "./src/Wallsh.csproj" -Raw

# Project version
if ($csproj -match '<Version>(.*?)<\/Version>') {
    $version = $matches[1]
} else {
    Write-Output "Version tag not found in the .csproj"
    return
}

# .NET publish self-contained
dotnet publish "./src/Wallsh.csproj" `
    --nologo `
    --configuration Release `
    --self-contained true `
    --runtime win-x64 `
    --output "./staging/Wallsh"

# Make .zip
Compress-Archive -Path "./staging/*" -DestinationPath "./wallsh_${version}_win-x64.zip"

# Clean-up
rm "./staging/" -r -force
