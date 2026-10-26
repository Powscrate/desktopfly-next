# Troubleshooting

- **No tray icon:** Confirm DesktopFly Next is still running in Task Manager and check `%LOCALAPPDATA%\DesktopFlyNext\logs\desktopfly.log`. Windows may place notification icons in the overflow area.
- **Fly not visible:** Use tray menu → Reset fly, verify that the overlay is not paused, and check that the Windows desktop is not in a protected full-screen/exclusive mode. The transparency-key and topmost behavior require Windows and are not testable on Linux.
- **Underlying apps do not receive clicks:** Click-through should be enabled by default. If interaction mode is on, turn it off in the tray menu. Exit remains available from the tray icon.
- **Fly is difficult to grab:** Enable “Interaction mode (click / drag)” first; the overlay intentionally does not intercept pointer events otherwise.
- **Language:** Tray menu → “Language / Langue” → “English” or “Français”. Choice persists under `%LOCALAPPDATA%\DesktopFlyNext\config.json`.
- **Configuration invalid or unsupported:** Back up then remove or repair that JSON file. Unknown language values fall back to English; unsupported schema versions are reported rather than silently migrated.
- **Publish fails:** Install the .NET 8 SDK and Windows desktop targeting pack, run the PowerShell script from a Windows PowerShell session, and inspect the command output. Verify output on a Windows 10/11 machine; publishing does not itself verify DPI or click-through behavior.
- **Close application:** Right-click the notification-area icon → Exit. No keyboard hook or hidden startup registration exists.
