@echo off
echo ����ж�� AssimpThumbnailProvider ����ͼ�������...

REM ȷ���Թ���ԱȨ������
>nul 2>&1 "%SYSTEMROOT%\system32\cacls.exe" "%SYSTEMROOT%\system32\config\system"
if '%errorlevel%' NEQ '0' (
    echo �������ԱȨ��...
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

REM ж������ͼ�������
echo ж������ͼ�������...
srm uninstall "%~dp0bin\Debug\AssimpThumbnailProvider.dll" -codebase

REM ע�� DLL
echo ע�� AssimpThumbnailProvider.dll...
%SystemRoot%\Microsoft.NET\Framework64\v4.0.30319\RegAsm.exe /unregister "%~dp0bin\Debug\AssimpThumbnailProvider.dll"

echo �������ͼ����...
taskkill /IM explorer.exe /F
timeout /T 1
DEL /A /F /Q "%LOCALAPPDATA%\Microsoft\Windows\Explorer\thumbcache_*.db"
DEL /A /F /Q "%LOCALAPPDATA%\Microsoft\Windows\Explorer\iconcache_*.db"
start explorer.exe

echo ж����ɣ�
pause 