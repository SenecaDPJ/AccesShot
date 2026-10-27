param(
    [string]$BaseZip = (Join-Path $PSScriptRoot '..\..\release\AccessShot-2.12-apps-Windows-portable-Windows-x64.zip'),
    [Parameter(Mandatory = $true)][string]$OutputDirectory
)

$ErrorActionPreference = 'Stop'
$repo = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
$base = (Resolve-Path -LiteralPath $BaseZip).Path
$expected = 'c8f8658f8ed9c4f3a2ef8eec69fea86eab0f354b9ca4b0c82f1ca87bb4c1f6c6'
if ((Get-FileHash -LiteralPath $base -Algorithm SHA256).Hash.ToLowerInvariant() -ne $expected) {
    throw 'El ZIP base de la 2.12 no coincide con el aprobado.'
}
if (Test-Path -LiteralPath $OutputDirectory) { throw 'La carpeta de salida ya existe; elige una nueva.' }
$out = [IO.Path]::GetFullPath($OutputDirectory)
New-Item -ItemType Directory -Force -Path $out | Out-Null
$oldEnv = @{}
foreach ($name in @('DOTNET_CLI_HOME','NUGET_PACKAGES','NUGET_HTTP_CACHE_PATH','NUGET_PLUGINS_CACHE_PATH','DOTNET_CLI_TELEMETRY_OPTOUT','DOTNET_ROLL_FORWARD')) {
    $oldEnv[$name] = [Environment]::GetEnvironmentVariable($name, 'Process')
}
try {
    $env:DOTNET_CLI_HOME = Join-Path $out '.dotnet'
    $env:NUGET_PACKAGES = Join-Path $out '.nuget-packages'
    $env:NUGET_HTTP_CACHE_PATH = Join-Path $out '.nuget-http-cache'
    $env:NUGET_PLUGINS_CACHE_PATH = Join-Path $out '.nuget-plugins'
    $env:DOTNET_CLI_TELEMETRY_OPTOUT = '1'
    $source = 'https://api.nuget.org/v3/index.json'
    foreach ($project in @('src\Styles\AccessShotStyles.csproj','src\WindowsApps\AccessShotWindowsApps.csproj','src\Core\AccessShotCore.csproj')) {
        & dotnet build (Join-Path $repo $project) -c Release --source $source -v:q -p:NuGetAudit=false -p:EnableNETAnalyzers=false -clp:ErrorsOnly
        if ($LASTEXITCODE -ne 0) { throw "No se pudo compilar $project" }
    }
    & dotnet restore (Join-Path $repo 'tests\CoreStart\CoreStart.csproj') --source $source -v:q -p:NuGetAudit=false
    if ($LASTEXITCODE -ne 0) { throw 'No se pudieron restaurar las pruebas.' }
    $env:DOTNET_ROLL_FORWARD = 'Major'
    & dotnet run --project (Join-Path $repo 'tests\CoreStart\CoreStart.csproj') -c Release --no-restore -v:q -p:EnableNETAnalyzers=false
    if ($LASTEXITCODE -ne 0) { throw 'Falló la prueba de arranque.' }
    $stage = Join-Path $out 'runtime-stage'
    & dotnet publish (Join-Path $repo 'src\Core\AccessShotCore.csproj') -c Release -r win-x64 --self-contained true -o $stage --source $source -v:q -p:NuGetAudit=false -p:EnableNETAnalyzers=false -clp:ErrorsOnly
    if ($LASTEXITCODE -ne 0) { throw 'No se pudo publicar el núcleo portátil.' }

    $package = Join-Path $out 'AccesShot-2.13-portable-Windows-x64'
    $app = Join-Path $package 'Aplicacion'
    New-Item -ItemType Directory -Force -Path $app | Out-Null
    Get-ChildItem -LiteralPath $stage -Recurse -File | Where-Object Extension -ne '.pdb' | ForEach-Object {
        $relative = $_.FullName.Substring($stage.Length).TrimStart('\')
        $destination = Join-Path $app $relative
        New-Item -ItemType Directory -Force -Path (Split-Path $destination -Parent) | Out-Null
        Copy-Item -LiteralPath $_.FullName -Destination $destination
    }

    Add-Type -AssemblyName System.IO.Compression.FileSystem
    $zip = [IO.Compression.ZipFile]::OpenRead($base)
    try {
        foreach ($name in @('AccessShot.exe','Aplicacion/AccessShot.exe','Aplicacion/AccessShot.dll','Aplicacion/AccessShot.deps.json','Aplicacion/AccessShot.runtimeconfig.json','Aplicacion/assets/AccessShot.ico')) {
            $entry = $zip.GetEntry($name)
            if ($null -eq $entry) { throw "Falta $name en el ZIP base." }
            $destination = Join-Path $package ($name.Replace('/','\'))
            New-Item -ItemType Directory -Force -Path (Split-Path $destination -Parent) | Out-Null
            $inputStream = $entry.Open()
            try {
                $outputStream = [IO.File]::Create($destination)
                try { $inputStream.CopyTo($outputStream) } finally { $outputStream.Dispose() }
            } finally { $inputStream.Dispose() }
        }
    } finally { $zip.Dispose() }

    $launcher = Join-Path $package 'AccessShot.exe'
    $bytes = [IO.File]::ReadAllBytes($launcher)
    $before = [Text.Encoding]::Unicode.GetBytes('AccessShot 2.12')
    $after = [Text.Encoding]::Unicode.GetBytes('AccessShot 2.13')
    $hits = @()
    for ($i=0; $i -le $bytes.Length-$before.Length; $i++) {
        $match = $true
        for ($j=0; $j -lt $before.Length; $j++) { if ($bytes[$i+$j] -ne $before[$j]) { $match = $false; break } }
        if ($match) { $hits += $i }
    }
    if ($hits.Count -ne 1) { throw 'No se pudo actualizar la etiqueta de versión del lanzador de manera unívoca.' }
    [Array]::Copy($after,0,$bytes,$hits[0],$after.Length)
    [IO.File]::WriteAllBytes($launcher,$bytes)
    Copy-Item -LiteralPath (Join-Path $repo 'docs\LEEME_2.13.txt') -Destination (Join-Path $app 'LEEME.txt')
    Copy-Item -LiteralPath (Join-Path $repo 'LICENSE') -Destination (Join-Path $package 'LICENSE')
    Copy-Item -LiteralPath (Join-Path $repo 'THIRD_PARTY_NOTICES.md') -Destination (Join-Path $package 'THIRD_PARTY_NOTICES.md')
    Copy-Item -LiteralPath (Join-Path $repo 'licenses') -Destination (Join-Path $package 'licenses') -Recurse
    if (Get-ChildItem -LiteralPath $package -Recurse -File -Filter 'entries.json') { throw 'El paquete contiene datos de usuario.' }
    $archive = Join-Path $out 'AccessShot-2.13-portable-Windows-x64.zip'
    [IO.Compression.ZipFile]::CreateFromDirectory($package,$archive,[IO.Compression.CompressionLevel]::Optimal,$false)
    $hash = (Get-FileHash -LiteralPath $archive -Algorithm SHA256).Hash.ToLowerInvariant()
    [IO.File]::WriteAllText((Join-Path $out 'SHA256SUMS_v2.13.txt'), "$hash  $(Split-Path $archive -Leaf)`n")
    Write-Output "ZIP: $archive"
    Write-Output "SHA256: $hash"
} finally {
    foreach ($name in $oldEnv.Keys) { [Environment]::SetEnvironmentVariable($name, $oldEnv[$name], 'Process') }
}
