[CmdletBinding()]
param()
$ErrorActionPreference = 'Stop'
$Root = Split-Path -Parent $PSScriptRoot
if ($env:OS -ne 'Windows_NT') { throw 'Run module builds and tests on Windows with the .NET 8 SDK.' }
if (!(Get-Command dotnet -ErrorAction SilentlyContinue)) { throw 'Install the .NET 8 SDK for development.' }
Push-Location $Root
try {
    foreach ($Project in @('src/Styles/AccessShotStyles.csproj', 'src/WindowsApps/AccessShotWindowsApps.csproj')) {
        & dotnet build $Project --configuration Release
        if ($LASTEXITCODE -ne 0) { throw "Build failed: $Project" }
    }
    foreach ($Project in @('tests/Styles/Tests.Styles.csproj', 'tests/WindowsApps/Tests.WindowsApps.csproj')) {
        & dotnet run --project $Project --configuration Release
        if ($LASTEXITCODE -ne 0) { throw "Tests failed: $Project" }
    }
    Write-Host 'Module builds/tests finished. The approved release ZIP was not modified.'
} finally { Pop-Location }
