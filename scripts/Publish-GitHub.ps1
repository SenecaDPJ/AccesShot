[CmdletBinding()]
param([switch]$Publish)
$ErrorActionPreference = 'Stop'
$Root = Split-Path -Parent $PSScriptRoot
$PackageRoot = Split-Path -Parent $Root
$ReleaseDir = Join-Path $PackageRoot 'release'
$Meta = Get-Content (Join-Path $Root 'docs/project.json') -Raw -Encoding UTF8 | ConvertFrom-Json
& (Join-Path $PSScriptRoot 'Verify-Release.ps1')
$Repository = 'SenecaDPJ/AccesShot'
if ($Meta.repository -ne $Repository -or $Meta.visibility -ne 'private' -or $Meta.tag -ne 'v2.12') {
    throw 'Repository, private visibility or release tag do not match the approved plan.'
}
$Assets = @(
    (Join-Path $ReleaseDir $Meta.release_asset),
    (Join-Path $ReleaseDir 'AccesShot-2.12-licencias.zip'),
    (Join-Path $ReleaseDir 'SHA256SUMS.txt')
)
$Notes = Join-Path $Root 'RELEASE_NOTES_v2.12.md'
Write-Host "PLAN: create $Repository as PRIVATE; push main; create stable v2.12 with 3 assets."
if (!$Publish) {
    Write-Host 'Plan only. Run again with -Publish when ready to perform the upload.'
    return
}
foreach ($Tool in @('git', 'gh')) {
    if (!(Get-Command $Tool -ErrorAction SilentlyContinue)) { throw "Missing command: $Tool" }
}
if (Test-Path -LiteralPath (Join-Path $Root '.git')) {
    throw 'A local repository already exists. Inspect any partial run before continuing manually.'
}
function Invoke-Checked {
    param([string]$Program, [string[]]$Arguments)
    & $Program @Arguments
    if ($LASTEXITCODE -ne 0) { throw "$Program failed (exit $LASTEXITCODE). Inspect the current state before retrying." }
}
function Get-GhApi {
    param([string]$Endpoint)
    # Endpoints below are fixed identifiers, never user-provided shell text.
    $Info = New-Object System.Diagnostics.ProcessStartInfo
    $Info.FileName = (Get-Command gh).Source
    $Info.Arguments = 'api ' + $Endpoint
    $Info.UseShellExecute = $false
    $Info.RedirectStandardOutput = $true
    $Info.RedirectStandardError = $true
    $Process = New-Object System.Diagnostics.Process
    $Process.StartInfo = $Info
    try {
        [void]$Process.Start()
        $Out = $Process.StandardOutput.ReadToEnd()
        $Err = $Process.StandardError.ReadToEnd()
        $Process.WaitForExit()
        return [pscustomobject]@{Code=$Process.ExitCode; Out=$Out; ErrorText=$Err}
    } finally { $Process.Dispose() }
}
$Profile = Get-GhApi -Endpoint 'user'
if ($Profile.Code -ne 0) { throw 'GitHub CLI authentication is not ready. Authenticate as SenecaDPJ.' }
if (($Profile.Out | ConvertFrom-Json).login -ne 'SenecaDPJ') { throw 'The authenticated GitHub account must be SenecaDPJ.' }
$Existing = Get-GhApi -Endpoint 'repos/SenecaDPJ/AccesShot'
if ($Existing.Code -eq 0) { throw 'The remote repository already exists. Inspect it rather than recreating or overwriting it.' }
if (($Existing.ErrorText + $Existing.Out) -notmatch '404|Not Found') { throw 'Cannot establish whether the repository exists. Stop without creating it.' }
# Validate the existing Git identity without setting a fabricated name or email.
Invoke-Checked -Program 'git' -Arguments @('-C', $Root, 'var', 'GIT_AUTHOR_IDENT')
Invoke-Checked -Program 'gh' -Arguments @('repo','create',$Repository,'--private','--description',$Meta.description)
$Created = Get-GhApi -Endpoint 'repos/SenecaDPJ/AccesShot'
if ($Created.Code -ne 0) { throw 'Cannot verify the newly created repository. Inspect GitHub before proceeding.' }
$Remote = $Created.Out | ConvertFrom-Json
if (!$Remote.private -or $Remote.full_name -ne $Repository) { throw 'Repository verification did not confirm the expected private target.' }
Invoke-Checked -Program 'git' -Arguments @('-C',$Root,'init','-b','main')
Invoke-Checked -Program 'git' -Arguments @('-C',$Root,'add','--','.')
Invoke-Checked -Program 'git' -Arguments @('-C',$Root,'commit','-m','Initial import: AccesShot 2.12 validated release')
Invoke-Checked -Program 'git' -Arguments @('-C',$Root,'remote','add','origin',$Remote.clone_url)
Invoke-Checked -Program 'git' -Arguments @('-C',$Root,'push','-u','origin','main')
Invoke-Checked -Program 'gh' -Arguments @('repo','edit',$Repository,'--add-topic',($Meta.topics -join ','))
Invoke-Checked -Program 'git' -Arguments @('-C',$Root,'tag','-a','v2.12','-m','AccesShot 2.12 - validated by David De Prado')
Invoke-Checked -Program 'git' -Arguments @('-C',$Root,'push','origin','v2.12')
$ReleaseArgs = @('release','create','v2.12') + $Assets + @('--repo',$Repository,'--verify-tag','--title',$Meta.release_title,'--notes-file',$Notes)
Invoke-Checked -Program 'gh' -Arguments $ReleaseArgs
$Final = Get-GhApi -Endpoint 'repos/SenecaDPJ/AccesShot'
if ($Final.Code -ne 0 -or !(($Final.Out | ConvertFrom-Json).private)) { throw 'Final private visibility could not be confirmed.' }
$Published = Get-GhApi -Endpoint 'repos/SenecaDPJ/AccesShot/releases/tags/v2.12'
if ($Published.Code -ne 0) { throw 'Final release verification failed.' }
$ReleaseObject = $Published.Out | ConvertFrom-Json
$ExpectedNames = @($Meta.release_asset,'AccesShot-2.12-licencias.zip','SHA256SUMS.txt')
foreach ($Name in $ExpectedNames) {
    if (!($ReleaseObject.assets | Where-Object { $_.name -eq $Name })) { throw "Release asset missing: $Name" }
}
Write-Host 'DONE: private repository and stable v2.12 with all expected assets verified.'
Write-Host $ReleaseObject.html_url
