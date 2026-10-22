param(
    [string]$UnityPath = 'D:\SoftWare\Unity 2021.3.45f2c1\Editor\Unity.exe'
)
$ErrorActionPreference = 'Stop'
$workspacePath = Split-Path -Parent $PSScriptRoot
$projectPath = Join-Path $workspacePath 'TotemRunner'
$logPath = Join-Path $workspacePath 'Analysis\TotemRunner\unity-android-build.log'
if (!(Test-Path -LiteralPath $UnityPath -PathType Leaf)) { throw "Unity editor not found: $UnityPath" }
if (!(Test-Path -LiteralPath (Join-Path $projectPath 'Assets\Editor\TotemRunnerBuild.cs'))) { throw 'Restored project not found.' }
$unityArguments = @('-batchmode', '-nographics', '-quit', '-projectPath', ('"' + $projectPath + '"'), '-buildTarget', 'Android', '-executeMethod', 'TotemRunnerBuild.BuildAndroid', '-logFile', ('"' + $logPath + '"'))
$editorProcess = Start-Process -FilePath $UnityPath -ArgumentList $unityArguments -WorkingDirectory $projectPath -WindowStyle Hidden -PassThru
$editorProcess.WaitForExit()
if ($editorProcess.ExitCode -ne 0) { throw "Unity exited with code $($editorProcess.ExitCode). Inspect $logPath" }
if (!(Select-String -LiteralPath $logPath -Pattern 'TOTEM_BUILD_SUCCEEDED:' -Quiet)) { throw "Build success was not confirmed. Inspect $logPath" }
Write-Output (Join-Path $projectPath 'Builds\TotemRunner-ARM64.apk')
