@echo off
setlocal enabledelayedexpansion
chcp 65001 >nul

cd /d "%~dp0"

REM MSBuild yolunu bul (Visual Studio Locator)
set "VSWHERE=%ProgramFiles(x86)%\Microsoft Visual Studio\Installer\vswhere.exe"
set "MSBUILD="
if exist "%VSWHERE%" (
    for /f "usebackq tokens=*" %%i in (`"%VSWHERE%" -latest -requires Microsoft.Component.MSBuild -find MSBuild\**\Bin\MSBuild.exe`) do (
        set "MSBUILD=%%i"
    )
)

:MENU
cls
echo ==============================================================
echo          CAPSLOCK NOTIFIER (.NET Fw 4.7) - Proje Yonetimi
echo ==============================================================
echo.
echo   [1] Baslat                  (Release exe - tray app)
echo   [2] Durdur                  (taskkill CapsLockChecker.exe)
echo   [3] Yeniden Baslat
echo   [4] Release Build
echo   [5] Debug Build
echo   [6] Durum Goster            (process listing)
echo   [7] Release Klasorunu Ac
echo.
echo   [i] Ilk Kurulum             (NuGet restore + Release build)
echo   [n] NuGet Restore
echo   [c] Runtime Kontrolu        (msbuild + .NET 4.7.2)
echo   [0] Cikis
echo.
echo ==============================================================
if defined MSBUILD (echo MSBuild: %MSBUILD%) else (echo MSBuild: [X] bulunamadi)
echo ==============================================================
echo.
set /p choice="Seciminiz: "

if "%choice%"=="1" goto START
if "%choice%"=="2" goto STOP
if "%choice%"=="3" goto RESTART
if "%choice%"=="4" goto BUILD_REL
if "%choice%"=="5" goto BUILD_DBG
if "%choice%"=="6" goto STATUS
if "%choice%"=="7" goto OPEN_BIN
if /i "%choice%"=="i" goto FIRST_SETUP
if /i "%choice%"=="n" goto NUGET
if /i "%choice%"=="c" goto CHECK_ENV
if "%choice%"=="0" goto EXIT
goto MENU

:FIRST_SETUP
if not defined MSBUILD ( echo [X] MSBuild yok. Visual Studio Community ya da Build Tools kurun. & pause & goto MENU )
"%MSBUILD%" CapsLockChecker.sln /t:Restore /v:minimal /nologo
"%MSBUILD%" CapsLockChecker.sln /p:Configuration=Release /v:minimal /nologo
pause
goto MENU

:NUGET
if not defined MSBUILD ( echo [X] MSBuild yok. & pause & goto MENU )
"%MSBUILD%" CapsLockChecker.sln /t:Restore /v:minimal /nologo
pause
goto MENU

:BUILD_REL
if not defined MSBUILD ( echo [X] MSBuild yok. & pause & goto MENU )
"%MSBUILD%" CapsLockChecker.sln /p:Configuration=Release /v:minimal /nologo
pause
goto MENU

:BUILD_DBG
if not defined MSBUILD ( echo [X] MSBuild yok. & pause & goto MENU )
"%MSBUILD%" CapsLockChecker.sln /p:Configuration=Debug /v:minimal /nologo
pause
goto MENU

:START
if not exist "CapsLockChecker\bin\Release\CapsLockChecker.exe" (
    echo [X] Build edilmemis. Once [4] Release Build calistirin.
    pause
    goto MENU
)
start "" "CapsLockChecker\bin\Release\CapsLockChecker.exe"
echo [OK] Tray ikonuna sag tiklayarak ayarlara erisebilirsiniz.
pause
goto MENU

:STOP
taskkill /IM CapsLockChecker.exe /F /T >nul 2>&1
echo [OK] CapsLockChecker.exe durduruldu.
pause
goto MENU

:RESTART
taskkill /IM CapsLockChecker.exe /F /T >nul 2>&1
timeout /t 1 >nul
if exist "CapsLockChecker\bin\Release\CapsLockChecker.exe" (
    start "" "CapsLockChecker\bin\Release\CapsLockChecker.exe"
    echo [OK] Yeniden baslatildi.
) else (
    echo [X] Build yok. Once Release Build calistirin.
)
pause
goto MENU

:STATUS
echo.
echo ==============================================================
echo                     PROCESS DURUMU
echo ==============================================================
tasklist /FI "IMAGENAME eq CapsLockChecker.exe" /NH
pause
goto MENU

:OPEN_BIN
if exist "CapsLockChecker\bin\Release" (
    explorer "CapsLockChecker\bin\Release"
) else (
    echo [X] Once Release Build calistirin.
    pause
)
goto MENU

:CHECK_ENV
echo.
if defined MSBUILD (echo [OK] MSBuild: %MSBUILD%) else (echo [X] MSBuild bulunamadi)
powershell -NoProfile -Command "$v = (Get-ItemProperty 'HKLM:\SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full' -Name Release -ErrorAction SilentlyContinue).Release; if ($v -ge 460798) { Write-Host ('[OK] .NET Fw 4.7.2+ kurulu (release=' + $v + ')') } else { Write-Host '[X] .NET Fw 4.7.2+ yok - https://dotnet.microsoft.com/download/dotnet-framework' }"
pause
goto MENU

:EXIT
echo.
echo Gule gule!
timeout /t 1 >nul
exit /b 0
