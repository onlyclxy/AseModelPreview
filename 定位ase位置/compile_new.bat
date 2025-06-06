@echo off
echo 编译二级菜单版本的 ASE 定位器...
echo.

REM 尝试找到 C# 编译器
set "CSC_PATH="
if exist "C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe" (
    set "CSC_PATH=C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe"
)
if exist "C:\Windows\Microsoft.NET\Framework\v4.0.30319\csc.exe" (
    set "CSC_PATH=C:\Windows\Microsoft.NET\Framework\v4.0.30319\csc.exe"
)

if "%CSC_PATH%"=="" (
    echo 错误：找不到 C# 编译器
    echo 请确保安装了 .NET Framework
    pause
    exit /b 1
)

echo 使用编译器: %CSC_PATH%
echo.

REM 编译程序（修正参数顺序）
echo 正在编译...
"%CSC_PATH%" /out:ASELocator_v2.exe /reference:System.Windows.Forms.dll /reference:System.dll Program.cs

if exist "ASELocator_v2.exe" (
    echo ✓ 编译成功！
    echo.
    echo 备份旧版本...
    if exist "ASELocator.exe" (
        copy "ASELocator.exe" "ASELocator_old.exe" >nul
        echo ✓ 旧版本已备份为 ASELocator_old.exe
    )
    
    echo 更新程序...
    copy "ASELocator_v2.exe" "ASELocator.exe" >nul
    echo ✓ 新版本已更新为 ASELocator.exe
    
    echo.
    echo 🎉 编译和更新完成！
    echo 现在您可以测试二级菜单功能了。
) else (
    echo × 编译失败！
    echo 请检查代码是否有语法错误。
)

echo.
pause 