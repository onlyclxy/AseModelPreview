@echo off
echo 编译DML编辑器...
echo.

REM 尝试使用不同的MSBuild路径
set MSBUILD_PATH=""

REM Visual Studio 2022
if exist "C:\Program Files\Microsoft Visual Studio\2022\Enterprise\MSBuild\Current\Bin\MSBuild.exe" (
    set MSBUILD_PATH="C:\Program Files\Microsoft Visual Studio\2022\Enterprise\MSBuild\Current\Bin\MSBuild.exe"
    goto :build
)

REM Visual Studio 2022 Community
if exist "C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe" (
    set MSBUILD_PATH="C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe"
    goto :build
)

REM Visual Studio 2019
if exist "C:\Program Files (x86)\Microsoft Visual Studio\2019\Enterprise\MSBuild\Current\Bin\MSBuild.exe" (
    set MSBUILD_PATH="C:\Program Files (x86)\Microsoft Visual Studio\2019\Enterprise\MSBuild\Current\Bin\MSBuild.exe"
    goto :build
)

REM Visual Studio 2019 Community
if exist "C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\MSBuild.exe" (
    set MSBUILD_PATH="C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\MSBuild.exe"
    goto :build
)

REM Build Tools
if exist "C:\Program Files (x86)\Microsoft Visual Studio\2019\BuildTools\MSBuild\Current\Bin\MSBuild.exe" (
    set MSBUILD_PATH="C:\Program Files (x86)\Microsoft Visual Studio\2019\BuildTools\MSBuild\Current\Bin\MSBuild.exe"
    goto :build
)

echo 错误: 找不到MSBuild.exe
echo 请确保已安装Visual Studio或Build Tools
pause
exit /b 1

:build
echo 使用MSBuild路径: %MSBUILD_PATH%
echo.

REM 编译项目
%MSBUILD_PATH% DMLView\DMLView.csproj /p:Configuration=Release /p:Platform="Any CPU"

if %ERRORLEVEL% EQU 0 (
    echo.
    echo 编译成功！
    echo 可执行文件位置: DMLView\bin\Release\DMLView.exe
    echo.
    
    REM 询问是否运行程序
    set /p choice="是否要运行DML编辑器? (y/n): "
    if /i "%choice%"=="y" (
        echo 启动DML编辑器...
        start DMLView\bin\Release\DMLView.exe
    )
) else (
    echo.
    echo 编译失败！
    echo 请检查错误信息并修复。
)

echo.
pause 