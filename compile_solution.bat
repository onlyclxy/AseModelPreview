@echo off
echo 使用解决方案文件编译DML编辑器...
echo.

cd /d "%~dp0"

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
pause
exit /b 1

:found
echo 找到MSBuild: %MSBUILD%
echo.

REM 编译整个解决方案
echo 编译解决方案...
%MSBUILD% AseModelPreview.sln /p:Configuration=Debug /p:Platform="Any CPU" /v:minimal

if %ERRORLEVEL% EQU 0 (
    echo.
    echo 编译成功！
    echo.
    if exist "DMLView\bin\Debug\DMLView.exe" (
        echo 找到DML编辑器可执行文件，正在启动...
        start "" "DMLView\bin\Debug\DMLView.exe"
    ) else (
        echo 在DMLView\bin\Debug\目录下查找可执行文件...
        dir "DMLView\bin\Debug\" /b
    )
) else (
    echo.
    echo 编译失败！错误代码: %ERRORLEVEL%
)

echo.
pause 