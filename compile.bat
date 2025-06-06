@echo off
echo 正在编译DML编辑器...
echo.

cd /d "%~dp0"

echo 当前目录: %cd%
echo.

REM 设置MSBuild路径
set "MSBUILD="
for %%i in (
    "C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe"
    "C:\Program Files\Microsoft Visual Studio\2022\Enterprise\MSBuild\Current\Bin\MSBuild.exe"
    "C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\MSBuild.exe"
    "C:\Program Files (x86)\Microsoft Visual Studio\2019\Enterprise\MSBuild\Current\Bin\MSBuild.exe"
) do (
    if exist %%i (
        set "MSBUILD=%%i"
        goto :found
    )
)

echo 错误: 找不到MSBuild.exe
echo 请确保已安装Visual Studio
pause
exit /b 1

:found
echo 找到MSBuild: %MSBUILD%
echo.

REM 首先尝试还原NuGet包
echo 还原NuGet包...
%MSBUILD% DMLView\DMLView.csproj /t:Restore

REM 然后编译项目
echo 编译项目...
%MSBUILD% DMLView\DMLView.csproj /p:Configuration=Debug /p:Platform="Any CPU" /v:minimal

if %ERRORLEVEL% EQU 0 (
    echo.
    echo 编译成功！
    echo 可执行文件位置: DMLView\bin\Debug\DMLView.exe
    echo.
    if exist "DMLView\bin\Debug\DMLView.exe" (
        echo 启动DML编辑器...
        start "" "DMLView\bin\Debug\DMLView.exe"
    ) else (
        echo 警告: 找不到编译生成的可执行文件
    )
) else (
    echo.
    echo 编译失败！
    echo 错误代码: %ERRORLEVEL%
)

echo.
pause 