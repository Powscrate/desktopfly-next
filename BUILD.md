# Build and packaging

## Prerequisites

- Windows 10/11 x64 for the native overlay and runtime.
- .NET 8 SDK (8.0.4xx or later servicing release).
- No external NuGet packages are used. The repository does not require a package lock file; restore is SDK-only.

## Build and test

From the repository root in PowerShell:

```powershell
dotnet --info
dotnet restore .\DesktopFlyNext.sln
dotnet build .\DesktopFlyNext.sln -c Release --no-restore
dotnet run --project .\tests\DesktopFly.Simulation.Tests\DesktopFly.Simulation.Tests.csproj -c Release --no-build
```

The solution currently has no `packages.lock.json` because it has no external NuGet dependencies. `DesktopFly.Simulation.Tests` is a self-contained assertion executable, not a Microsoft.Test SDK test project.

## Run

```powershell
dotnet run --project .\src\DesktopFly.App\DesktopFly.App.csproj -c Release
```

Quit from the notification-area icon. Configuration and logs are created beneath `%LOCALAPPDATA%\DesktopFlyNext`.

## Self-contained Windows publish

Run `scripts/publish-windows.ps1` from a PowerShell session with the .NET 8 SDK. Output is placed under ignored `artifacts\win-x64\` and is deliberately not checked into Git. The script publishes a self-contained single-file executable; verify the result on Windows before distribution. This repository currently has no installer/signing setup.

```powershell
.\scripts\publish-windows.ps1
```

A Linux environment may be able to cross-compile the framework-dependent simulation project, but cannot execute or validate the WinForms overlay. A Windows SDK/desktop targeting pack is required for a trustworthy Windows publish. Never treat a successful cross-compile alone as verification of overlay behavior.
