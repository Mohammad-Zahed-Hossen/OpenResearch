@echo off
echo Stopping OpenResearch processes...
taskkill /F /IM OpenResearchLauncher.exe >nul 2>&1
taskkill /F /IM orx.exe >nul 2>&1
echo [OK] OpenResearch stopped.
timeout /t 2 >nul
