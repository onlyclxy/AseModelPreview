@echo off
cd /d "%~dp0"
echo 正在编译 ASE 定位器...

REM 尝试使用MSBuild编译
where msbuild >nul 2>&1
if %errorlevel% == 0 (
    echo 使用 MSBuild 编译...
    msbuild "定位ase位置.csproj" /p:Configuration=Release /p:Platform="Any CPU" /verbosity:minimal
    if exist "bin\Release\定位ase位置.exe" (
        echo 编译成功！
        echo 可执行文件位于: bin\Release\定位ase位置.exe
        copy "bin\Release\定位ase位置.exe" "ASELocator.exe" >nul
        echo 已复制到当前目录: ASELocator.exe
    ) else (
        echo MSBuild 编译失败！
        goto :TryCSC
    )
) else (
    echo 未找到 MSBuild，尝试使用 CSC...
    goto :TryCSC
)
goto :End

:TryCSC
REM 尝试使用CSC编译
where csc >nul 2>&1
if %errorlevel% == 0 (
    echo 使用 CSC 编译...
    csc Program.cs /reference:System.Windows.Forms.dll /reference:System.dll /out:ASELocator.exe
    if exist ASELocator.exe (
        echo 编译成功！
        echo 可执行文件: ASELocator.exe
    ) else (
        echo CSC 编译失败！
    )
) else (
    echo 未找到 C# 编译器！
    echo 请确保已安装 .NET Framework SDK 或 Visual Studio。
)

:End
echo.
echo 按任意键退出...
pause >nul 