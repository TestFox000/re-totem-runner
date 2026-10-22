param(
    [switch]$ListOnly,
    [string]$OriginalApkPath
)
$ErrorActionPreference = 'Stop'
$workspacePath = [System.IO.Path]::GetFullPath((Split-Path -Parent $PSScriptRoot))
if ([string]::IsNullOrWhiteSpace($OriginalApkPath)) {
    $OriginalApkPath = Join-Path (Split-Path -Parent $workspacePath) 'com.chillingo.totemrunner.android.rowgplay_1.0.1.apk'
    if (!(Test-Path -LiteralPath $OriginalApkPath -PathType Leaf)) {
        $OriginalApkPath = Join-Path $workspacePath 'Original\com.chillingo.totemrunner.android.rowgplay_1.0.1.apk'
    }
}
$entries = @(
    @{ Source = (Join-Path $workspacePath 'TotemRunner\Assets'); Target = 'TotemRunner\Assets'; Required = $true },
    @{ Source = (Join-Path $workspacePath 'TotemRunner\Packages'); Target = 'TotemRunner\Packages'; Required = $true },
    @{ Source = (Join-Path $workspacePath 'TotemRunner\ProjectSettings'); Target = 'TotemRunner\ProjectSettings'; Required = $true },
    @{ Source = (Join-Path $workspacePath 'TotemRunner\README.txt'); Target = 'TotemRunner\README.txt'; Required = $true },
    @{ Source = (Join-Path $workspacePath 'TotemRunner\DEEPSEEK_HANDOFF.md'); Target = 'TotemRunner\DEEPSEEK_HANDOFF.md'; Required = $true },
    @{ Source = (Join-Path $workspacePath 'TotemRunner\DEEPSEEK_HANDOFF_PROMPT.txt'); Target = 'TotemRunner\DEEPSEEK_HANDOFF_PROMPT.txt'; Required = $true },
    @{ Source = (Join-Path $workspacePath 'TotemRunner\DSH'); Target = 'TotemRunner\DSH'; Required = $true },
    @{ Source = (Join-Path $workspacePath 'Tools\Build-TotemRunner.ps1'); Target = 'Tools\Build-TotemRunner.ps1'; Required = $true },
    @{ Source = (Join-Path $workspacePath 'Tools\Run-TotemRunner.ps1'); Target = 'Tools\Run-TotemRunner.ps1'; Required = $true },
    @{ Source = $PSCommandPath; Target = 'Tools\Export-TotemRunnerHandoff.ps1'; Required = $true },
    @{ Source = (Join-Path $workspacePath 'Analysis\TotemRunner'); Target = 'Analysis\TotemRunner'; Required = $true },
    @{ Source = (Join-Path $workspacePath 'TotemRunner\Builds\TotemRunner-ARM64.apk'); Target = 'TotemRunner\Builds\TotemRunner-ARM64.apk'; Required = $true },
    @{ Source = $OriginalApkPath; Target = 'Original\com.chillingo.totemrunner.android.rowgplay_1.0.1.apk'; Required = $true }
)
# Include source recovery helpers, but not downloaded tools, SDKs or caches.
foreach ($helper in (Get-ChildItem -LiteralPath $PSScriptRoot -File | Where-Object { $_.Extension -in @('.py', '.ps1') })) {
    $helperTarget = 'Tools\' + $helper.Name
    if ($entries.Target -notcontains $helperTarget) {
        $entries += @{ Source = $helper.FullName; Target = $helperTarget; Required = $true }
    }
}
foreach ($entry in $entries) {
    $entry.Exists = Test-Path -LiteralPath $entry.Source
    if ($entry.Required -and !$entry.Exists) { throw "Required input missing: $($entry.Source)" }
}
if ($ListOnly) {
    $entries | ForEach-Object {
        [pscustomobject]@{ Source = $_.Source; Target = $_.Target; Exists = $_.Exists; Required = $_.Required }
    }
    return
}
# Run only after all agents, Unity and builds have stopped writing these inputs.
# A package is a transport snapshot; it does not certify gameplay.
$runToken = [DateTimeOffset]::UtcNow.ToString('yyyyMMdd-HHmmss') + '-' + [guid]::NewGuid().ToString('N').Substring(0, 8)
$handoffsPath = Join-Path $workspacePath 'Handoffs'
$outputPath = Join-Path $handoffsPath ("TotemRunner-DeepSeek-" + $runToken)
$zipPath = $outputPath + '.zip'
if ((Test-Path -LiteralPath $outputPath) -or (Test-Path -LiteralPath $zipPath)) { throw 'Export destination already exists.' }
New-Item -ItemType Directory -Path $outputPath -Force | Out-Null
foreach ($entry in $entries) {
    if (!$entry.Exists) { Write-Warning "Optional input missing: $($entry.Source)"; continue }
    $targetPath = Join-Path $outputPath $entry.Target
    $parentPath = Split-Path -Parent $targetPath
    New-Item -ItemType Directory -Path $parentPath -Force | Out-Null
    Copy-Item -LiteralPath $entry.Source -Destination $targetPath -Recurse
}
$manifest = @(
    Get-ChildItem -LiteralPath $outputPath -File -Recurse -Force | ForEach-Object {
        [pscustomobject]@{
            Path = $_.FullName.Substring($outputPath.Length + 1).Replace('\', '/')
            Bytes = $_.Length
            SHA256 = (Get-FileHash -LiteralPath $_.FullName -Algorithm SHA256).Hash.ToLowerInvariant()
        }
    }
)
$metadata = @{
    CreatedUtc = [DateTimeOffset]::UtcNow.ToString('o')
    Workspace = $workspacePath
    Purpose = 'Transport snapshot; no gameplay validation implied'
    MissingOptionalInputs = @($entries | Where-Object { !$_.Exists } | ForEach-Object { $_.Target })
    Files = $manifest
}
$metadata | ConvertTo-Json -Depth 6 | Set-Content -LiteralPath (Join-Path $outputPath 'MANIFEST.json') -Encoding utf8
Compress-Archive -Path (Join-Path $outputPath '*') -DestinationPath $zipPath -CompressionLevel Optimal
$zipHash = (Get-FileHash -LiteralPath $zipPath -Algorithm SHA256).Hash.ToLowerInvariant()
("$zipHash  " + [System.IO.Path]::GetFileName($zipPath)) | Set-Content -LiteralPath ($zipPath + '.sha256') -Encoding ascii
[pscustomobject]@{ Directory = $outputPath; Zip = $zipPath; SHA256 = $zipHash; Files = $manifest.Count }

