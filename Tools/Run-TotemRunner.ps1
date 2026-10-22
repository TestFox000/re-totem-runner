param([string]$DeviceSerial = '1d16aa01')
$ErrorActionPreference = 'Stop'
$workspacePath = Split-Path -Parent $PSScriptRoot
$adbPath = 'C:\Users\TestFox\AppData\Local\Android\Sdk\platform-tools\adb.exe'
$apkPath = Join-Path $workspacePath 'TotemRunner\Builds\TotemRunner-ARM64.apk'
if (!(Test-Path -LiteralPath $apkPath -PathType Leaf)) { throw 'Build the restored APK first.' }
$deviceState = & $adbPath -s $DeviceSerial get-state
if ($LASTEXITCODE -ne 0 -or $deviceState.Trim() -ne 'device') { throw 'The selected Android device is not connected and authorized.' }
& $adbPath -s $DeviceSerial install -r $apkPath
if ($LASTEXITCODE -ne 0) { throw 'APK installation failed.' }
& $adbPath -s $DeviceSerial shell am start -W -n 'local.totemrunner.restored/com.unity3d.player.UnityPlayerActivity'
if ($LASTEXITCODE -ne 0) { throw 'Could not start Totem Runner Restored.' }
