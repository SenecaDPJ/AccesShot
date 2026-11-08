param(
    [Parameter(Mandatory = $true)][string]$BaseZip,
    [Parameter(Mandatory = $true)][string]$OutputDirectory,
    [string]$NuGetPackagesPath
)

$ErrorActionPreference = 'Stop'
$repo = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
$base = (Resolve-Path -LiteralPath $BaseZip).Path
$expectedBaseHash = 'b54f2bab06ed18d005d13bf4042e931624015e9d6cbb284907166293997d54a3'

if ((Get-FileHash $base -Algorithm SHA256).Hash.ToLowerInvariant() -ne $expectedBaseHash) {
    throw 'El ZIP base 2.16 no coincide con la release aprobada.'
}
if (Test-Path -LiteralPath $OutputDirectory) {
    throw 'Elige una carpeta de salida nueva.'
}

$out = [IO.Path]::GetFullPath($OutputDirectory)
New-Item -ItemType Directory -Force -Path $out | Out-Null
$env:DOTNET_CLI_HOME = Join-Path $out '.dotnet'
$env:DOTNET_CLI_TELEMETRY_OPTOUT = '1'
if ($NuGetPackagesPath) {
    $env:NUGET_PACKAGES = (Resolve-Path -LiteralPath $NuGetPackagesPath).Path
}

$source = 'https://api.nuget.org/v3/index.json'
$projects = @(
    'src\Styles\AccessShotStyles.csproj',
    'src\WindowsApps\AccessShotWindowsApps.csproj',
    'src\Core\AccessShotCore.csproj'
)
foreach ($project in $projects) {
    dotnet build (Join-Path $repo $project) -c Release --source $source -v:q -p:NuGetAudit=false -p:EnableNETAnalyzers=false -clp:ErrorsOnly
    if ($LASTEXITCODE) { throw "Falló $project" }
}

$stage = Join-Path $out 'runtime-stage'
dotnet publish (Join-Path $repo 'src\Core\AccessShotCore.csproj') -c Release -r win-x64 --self-contained true -o $stage --source $source -v:q -p:NuGetAudit=false -p:EnableNETAnalyzers=false -clp:ErrorsOnly
if ($LASTEXITCODE) { throw 'Falló publish win-x64' }

$package = Join-Path $out 'AccesShot-3.0-portable-Windows-x64'
$app = Join-Path $package 'Aplicacion'
New-Item -ItemType Directory -Force -Path $app | Out-Null

$initialLayout = Join-Path $repo 'assets\initial-layout.json'
if (!(Test-Path -LiteralPath $initialLayout)) { throw 'Falta la disposición inicial suministrada.' }
$initialTarget = Join-Path $app 'assets\initial-layout.json'
New-Item -ItemType Directory -Force -Path (Split-Path $initialTarget -Parent) | Out-Null
Copy-Item $initialLayout $initialTarget

Get-ChildItem $stage -Recurse -File | Where-Object Extension -ne '.pdb' | ForEach-Object {
    $relativePath = $_.FullName.Substring($stage.Length).TrimStart('\')
    $destination = Join-Path $app $relativePath
    New-Item -ItemType Directory -Force -Path (Split-Path $destination -Parent) | Out-Null
    Copy-Item $_.FullName $destination
}

Add-Type -AssemblyName System.IO.Compression.FileSystem
$archiveBase = [IO.Compression.ZipFile]::OpenRead($base)
try {
    foreach ($name in @('AccessShot.exe', 'Aplicacion/assets/AccessShot.ico')) {
        $entry = $archiveBase.GetEntry($name)
        if (!$entry) { throw "Falta $name" }
        $destination = Join-Path $package ($name.Replace('/', '\'))
        New-Item -ItemType Directory -Force -Path (Split-Path $destination -Parent) | Out-Null
        $input = $entry.Open()
        try {
            $output = [IO.File]::Create($destination)
            try { $input.CopyTo($output) } finally { $output.Dispose() }
        } finally { $input.Dispose() }
    }
} finally { $archiveBase.Dispose() }

$hostSource = Join-Path $stage 'AccessShotCore.exe'
$hostTarget = Join-Path $app 'AccessShot.exe'
if (!(Test-Path -LiteralPath $hostSource)) { throw 'Falta el ejecutable de la compilación 3.0.' }
Copy-Item $hostSource $hostTarget -Force
if ((Get-FileHash $hostSource -Algorithm SHA256).Hash -ne (Get-FileHash $hostTarget -Algorithm SHA256).Hash) {
    throw 'El ejecutable portable 3.0 no coincide con la compilación.'
}

$launcher = Join-Path $package 'AccessShot.exe'
$bytes = [IO.File]::ReadAllBytes($launcher)
$before = [Text.Encoding]::Unicode.GetBytes('AccessShot 2.16')
$after = [Text.Encoding]::Unicode.GetBytes('AccessShot 3.0')
$hits = @()
for ($i = 0; $i -le $bytes.Length - $before.Length; $i++) {
    $match = $true
    for ($j = 0; $j -lt $before.Length; $j++) {
        if ($bytes[$i + $j] -ne $before[$j]) { $match = $false; break }
    }
    if ($match) { $hits += $i }
}
if ($hits.Count -ne 1) { throw 'Etiqueta de versión del lanzador no unívoca.' }
[Array]::Copy($after, 0, $bytes, $hits[0], $after.Length)
if ($after.Length -lt $before.Length) { [Array]::Clear($bytes, $hits[0] + $after.Length, $before.Length - $after.Length) }
[IO.File]::WriteAllBytes($launcher, $bytes)

$wrapperBytes = [IO.File]::ReadAllBytes($launcher)
$wrapperPath = [Text.Encoding]::Unicode.GetBytes('Aplicacion\AccessShot.exe')
$pathHits = 0
for ($i = 0; $i -le $wrapperBytes.Length - $wrapperPath.Length; $i++) {
    $match = $true
    for ($j = 0; $j -lt $wrapperPath.Length; $j++) {
        if ($wrapperBytes[$i + $j] -ne $wrapperPath[$j]) { $match = $false; break }
    }
    if ($match) { $pathHits++ }
}
if ($pathHits -ne 1) { throw 'El lanzador principal no apunta al ejecutable Aplicacion\AccessShot.exe.' }

Copy-Item (Join-Path $repo 'docs\LEEME_3.0.txt') (Join-Path $app 'LEEME.txt')
Copy-Item (Join-Path $repo 'LICENSE'), (Join-Path $repo 'THIRD_PARTY_NOTICES.md') $package
Copy-Item (Join-Path $repo 'licenses') $package -Recurse
if (Get-ChildItem $package -Recurse -Filter entries.json) { throw 'No se admiten datos de usuario en el paquete.' }

$archive = Join-Path $out 'AccessShot-3.0-portable-Windows-x64.zip'
[IO.Compression.ZipFile]::CreateFromDirectory($package, $archive, [IO.Compression.CompressionLevel]::Optimal, $false)
$hash = (Get-FileHash $archive -Algorithm SHA256).Hash.ToLowerInvariant()
[IO.File]::WriteAllText((Join-Path $out 'SHA256SUMS_v3.0.txt'), "$hash  $(Split-Path $archive -Leaf)$([Environment]::NewLine)")
Write-Output "ZIP: $archive`nSHA256: $hash"
