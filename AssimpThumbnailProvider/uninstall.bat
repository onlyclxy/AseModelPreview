@echo off
echo 正在卸载 AssimpThumbnailProvider 缩略图处理程序...

REM 确保以管理员权限运行
>nul 2>&1 "%SYSTEMROOT%\system32\cacls.exe" "%SYSTEMROOT%\system32\config\system"
if '%errorlevel%' NEQ '0' (
    echo 请求管理员权限...
    goto UACPrompt
) else ( goto gotAdmin )

:UACPrompt
    echo Set UAC = CreateObject^("Shell.Application"^) > "%temp%\getadmin.vbs"
    echo UAC.ShellExecute "%~s0", "", "", "runas", 1 >> "%temp%\getadmin.vbs"
    "%temp%\getadmin.vbs"
    exit /B

:gotAdmin
    if exist "%temp%\getadmin.vbs" ( del "%temp%\getadmin.vbs" )
    pushd "%CD%"
    CD /D "%~dp0"

REM 卸载缩略图处理程序
echo 卸载缩略图处理程序...
srm uninstall "%~dp0bin\Debug\AssimpThumbnailProvider.dll" -codebase

REM 注销 DLL
echo 注销 AssimpThumbnailProvider.dll...
%SystemRoot%\Microsoft.NET\Framework64\v4.0.30319\RegAsm.exe /unregister "%~dp0bin\Debug\AssimpThumbnailProvider.dll"

echo 清除缩略图缓存...
taskkill /IM explorer.exe /F
timeout /T 1
DEL /A /F /Q "%LOCALAPPDATA%\Microsoft\Windows\Explorer\thumbcache_*.db"
DEL /A /F /Q "%LOCALAPPDATA%\Microsoft\Windows\Explorer\iconcache_*.db"
start explorer.exe

echo 卸载完成！
pause 