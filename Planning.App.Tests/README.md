# Planning.App.Tests

End-to-end tests for the Planning.App dockable Workspace, driven through a real headless
Chromium browser with [Microsoft.Playwright](https://playwright.dev/dotnet/).

These tests are **local-only** — they launch the app and a browser, so they are *not* part of
CI (which runs Model / Interfaces / Xpln tests on Linux without browsers).

## One-time setup: install the browser

After the project has been built once, install the Chromium binary Playwright drives:

```powershell
pwsh bin/Debug/net10.0/playwright.ps1 install chromium
```

(If `pwsh` is unavailable, use the cross-platform script the same way, or
`dotnet tool install --global Microsoft.Playwright.CLI` then `playwright install chromium`.)

## Running

```powershell
dotnet test Planning.App.Tests/Planning.App.Tests.csproj
```

By default the fixture launches the app with `dotnet run --launch-profile http` (port 5097)
and stops it afterwards. To run the tests against an already-running server (e.g. one started
by the run-skill), set:

```powershell
$env:PLANNING_APP_BASEURL = "http://localhost:5097"
```

## What is covered

- App loads with no uncaught JS errors; top bar + Workspace tab render.
- Empty Workspace shows the "Drag a view here" placeholder.
- A persisted layout in `localStorage` is restored into two panes.
- Dragging a view tab onto a pane's edge creates a split.

Screenshots are written to `bin/Debug/net10.0/screenshots/`.
