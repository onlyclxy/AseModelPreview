@echo off
echo 手动注册 ASE 定位器到右键菜单...
echo 注意：需要管理员权限
echo.

set "PROGRAM_PATH=%~dp0ASELocator.exe"
echo 程序路径: %PROGRAM_PATH%
echo.

echo 正在为 dml.1 程序类型添加右键菜单...
reg add "HKEY_CLASSES_ROOT\dml.1\shell\LocateASE" /ve /d "定位 ASE 文件" /f
reg add "HKEY_CLASSES_ROOT\dml.1\shell\LocateASE\command" /ve /d "\"%PROGRAM_PATH%\" \"%%1\"" /f

if %errorlevel% == 0 (
    echo ✓ DML 文件右键菜单注册成功
) else (
    echo × DML 文件右键菜单注册失败
)

echo.
echo 正在为 3dsmat 程序类型添加右键菜单...
reg add "HKEY_CLASSES_ROOT\3dsmat\shell\LocateASE" /ve /d "定位 ASE 文件" /f
reg add "HKEY_CLASSES_ROOT\3dsmat\shell\LocateASE\command" /ve /d "\"%PROGRAM_PATH%\" \"%%1\"" /f

if %errorlevel% == 0 (
    echo ✓ MAT 文件右键菜单注册成功
) else (
    echo × MAT 文件右键菜单注册失败
)

echo.
echo 正在刷新文件关联...
taskkill /f /im explorer.exe >nul 2>&1
start explorer.exe

echo.
echo 注册完成！
echo 现在应该可以在 .dml 和 .mat 文件的右键菜单中看到 "定位 ASE 文件" 选项了。
echo.
pause 