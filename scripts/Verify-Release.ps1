[CmdletBinding()]
param()
$ErrorActionPreference = 'Stop'
$Root = Split-Path -Parent $PSScriptRoot
$PackageRoot = Split-Path -Parent $Root
$ReleaseDir = Join-Path $PackageRoot 'release'
$Metadata = Get-Content (Join-Path $Root 'docs/project.json') -Raw -Encoding UTF8 | ConvertFrom-Json
if ($Metadata.repository -ne 'SenecaDPJ/AccesShot' -or $Metadata.visibility -ne 'private') {
    throw 'Unexpected repository or visibility in project.json.'
}
$App = Join-Path $ReleaseDir $Metadata.release_asset
if (!(Test-Path -LiteralPath $App -PathType Leaf)) { throw "Missing release asset: $App" }
if ((Get-Item -LiteralPath $App).Length -ne $Metadata.size_bytes) { throw 'Application ZIP size mismatch.' }
$Hash = (Get-FileHash -LiteralPath $App -Algorithm SHA256).Hash.ToLowerInvariant()
if ($Hash -ne $Metadata.sha256) { throw 'Application ZIP SHA256 mismatch. Do not publish.' }
foreach ($Line in Get-Content (Join-Path $ReleaseDir 'SHA256SUMS.txt') -Encoding UTF8) {
    if ($Line -notmatch '^([a-f0-9]{64})  ([A-Za-z0-9._-]+)$') { throw 'Invalid checksum line.' }
    $Expected = $Matches[1]
    $File = Join-Path $ReleaseDir $Matches[2]
    if (!(Test-Path -LiteralPath $File -PathType Leaf)) { throw "Missing asset: $File" }
    if ((Get-FileHash -LiteralPath $File -Algorithm SHA256).Hash.ToLowerInvariant() -ne $Expected) {
        throw "SHA256 mismatch: $File"
    }
}
Write-Host 'PASS: application ZIP and complementary license asset match their checksums.'
