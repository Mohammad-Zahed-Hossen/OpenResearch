@echo off
echo Compiling OpenResearchLauncher.exe...
C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe /nologo /target:winexe /win32icon:app.ico /r:System.dll,System.Drawing.dll,System.Windows.Forms.dll /out:OpenResearchLauncher.exe OpenResearchLauncher.cs
if %ERRORLEVEL% EQU 0 (
    echo [OK] OpenResearchLauncher.exe compiled successfully!
) else (
    echo [ERROR] Compilation failed.
)
pause
