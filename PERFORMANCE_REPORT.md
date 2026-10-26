# Performance report

**Status: not measured.** This Linux container had no .NET SDK and cannot run the WinForms application. No frame-rate, CPU, memory, startup, idle, or neural-throughput figures are claimed.

The current neural demonstration has only two LIF units and four inner neural steps per body update, so it is a functional test circuit, not a representative large-network benchmark. The overlay repaints its full virtual-screen window on a 16 ms WinForms timer and draws simple GDI+ shapes. Performance and idle CPU behavior on an Intel HD Graphics 620 / i5-7200U must be measured on Windows before release.

Suggested reproducible measurement: record Windows version, CPU/GPU, display dimensions/DPI and monitor count; run 10 minutes idle then 10 minutes with synthetic approach stimuli; record process private bytes, working set, CPU average, frame/update interval percentiles, and observed dropped frames. Include build configuration and application version. Compare at 60/30 Hz and at 1366×768; do not extrapolate to connectomes.
