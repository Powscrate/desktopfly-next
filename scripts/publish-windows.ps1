$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$project = Join-Path $root 'src/DesktopFly.App/DesktopFly.App.csproj'
$output = Join-Path $root 'artifacts/win-x64'
if (-not (Get-Command dotnet -ErrorAction SilentlyContinue)) { throw '.NET 8 SDK was not found. Install the SDK and retry.' }
$version = (& dotnet --version).Trim()
if (-not $version.StartsWith('8.')) { throw "Expected .NET 8 SDK, found $version" }
& dotnet publish $project -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -p:EnableCompressionInSingleFile=true -o $output
if ($LASTEXITCODE -ne 0) { throw "dotnet publish failed ($LASTEXITCODE)" }
Write-Host "Publish output written to $output. Run and smoke-test on Windows before release."
