$WshShell = New-Object -ComObject WScript.Shell

$TargetExe = "D:\Project\AlphaXiV\openresearch-cli-x86_64-pc-windows-msvc\OpenResearchLauncher.exe"
$WorkingDir = "D:\Project\AlphaXiV\openresearch-cli-x86_64-pc-windows-msvc"
$IconPath = "D:\Project\AlphaXiV\openresearch-cli-x86_64-pc-windows-msvc\app.ico"

# 1. Desktop Shortcut
$DesktopPath = [System.Environment]::GetFolderPath([System.Environment+SpecialFolder]::Desktop)
$DesktopLnk = Join-Path $DesktopPath "OpenResearch.lnk"
$sc1 = $WshShell.CreateShortcut($DesktopLnk)
$sc1.TargetPath = $TargetExe
$sc1.WorkingDirectory = $WorkingDir
$sc1.Description = "Launch OpenResearch in seamless App Mode"
$sc1.IconLocation = "$IconPath, 0"
$sc1.Save()

# 2. Start Menu Shortcut (Enables Windows Start Search and 'Pin to Start')
$StartMenuPath = [System.Environment]::GetFolderPath([System.Environment+SpecialFolder]::Programs)
$StartMenuLnk = Join-Path $StartMenuPath "OpenResearch.lnk"
$sc2 = $WshShell.CreateShortcut($StartMenuLnk)
$sc2.TargetPath = $TargetExe
$sc2.WorkingDirectory = $WorkingDir
$sc2.Description = "Launch OpenResearch in seamless App Mode"
$sc2.IconLocation = "$IconPath, 0"
$sc2.Save()

Write-Output "[OK] Shortcuts updated with new research-centric icon:"
Write-Output "  - Desktop: $DesktopLnk"
Write-Output "  - Start Menu: $StartMenuLnk"
