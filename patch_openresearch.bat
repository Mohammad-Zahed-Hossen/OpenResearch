@echo off
cd /d "%~dp0"
echo ========================================================
echo  OpenResearch UI Enhancements Patcher
echo ========================================================
powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0patch_openresearch.ps1"
echo.
pause
