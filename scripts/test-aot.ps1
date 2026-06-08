<#
.SYNOPSIS
  Publish Planning.App (AOT by default), serve the published static site, run the
  Playwright test suite against it, then stop the server.

.EXAMPLE
  pwsh scripts/test-aot.ps1               # AOT publish + tests
  pwsh scripts/test-aot.ps1 -Aot:$false   # fast (non-AOT) publish, to validate the flow
  pwsh scripts/test-aot.ps1 -Port 5099
#>
param(
    [int]$Port = 5098,
    [bool]$Aot = $true,
    [string]$Configuration = "Release"
)

$ErrorActionPreference = "Stop"
$root = Split-Path $PSScriptRoot -Parent
$appProject = Join-Path $root "Planning.App"
$serverProject = Join-Path $root "tools/AppServer"
$testProject = Join-Path $root "Planning.App.Tests/Planning.App.Tests.csproj"
$publishDir = Join-Path $root "Planning.App/bin/$Configuration/net10.0/publish"
$wwwroot = Join-Path $publishDir "wwwroot"
$baseUrl = "http://localhost:$Port"

$aotFlag = if ($Aot) { "true" } else { "false" }
Write-Host "==> Publishing Planning.App ($Configuration, AOT=$aotFlag). AOT can take several minutes." -ForegroundColor Cyan
dotnet publish $appProject -c $Configuration "-p:RunAOTCompilation=$aotFlag" -o $publishDir

if (-not (Test-Path (Join-Path $wwwroot "index.html"))) {
    throw "Publish did not produce $wwwroot/index.html"
}

Write-Host "==> Starting static server at $baseUrl" -ForegroundColor Cyan
$server = Start-Process dotnet `
    -ArgumentList @("run", "--project", $serverProject, "-c", "Release", "--",
                    "--path", $wwwroot, "--urls", $baseUrl) `
    -PassThru -NoNewWindow

try {
    Write-Host "==> Waiting for server..." -ForegroundColor Cyan
    $ready = $false
    for ($i = 0; $i -lt 90; $i++) {
        try {
            if ((Invoke-WebRequest "$baseUrl/" -UseBasicParsing -TimeoutSec 3).StatusCode -eq 200) { $ready = $true; break }
        } catch { Start-Sleep -Milliseconds 800 }
    }
    if (-not $ready) { throw "Server did not become ready at $baseUrl" }

    Write-Host "==> Running app tests against the AOT build" -ForegroundColor Cyan
    $env:PLANNING_APP_BASEURL = $baseUrl
    dotnet test $testProject
    $testExit = $LASTEXITCODE
}
finally {
    Write-Host "==> Stopping static server" -ForegroundColor Cyan
    if ($server -and -not $server.HasExited) {
        # Kill the whole process tree (dotnet run spawns the server child).
        taskkill /PID $server.Id /T /F | Out-Null
    }
    Remove-Item Env:PLANNING_APP_BASEURL -ErrorAction SilentlyContinue
}

exit $testExit
