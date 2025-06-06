@echo off
echo 清理旧的注册项并重新注册 ASE 定位器（二级菜单版本）...
echo 注意：需要管理员权限
echo.

echo === 清理所有旧的注册项 ===
echo 删除旧的单级菜单...
reg delete "HKEY_CLASSES_ROOT\dml.1\shell\LocateASE" /f >nul 2>&1
reg delete "HKEY_CLASSES_ROOT\3dsmat\shell\LocateASE" /f >nul 2>&1
reg delete "HKEY_CLASSES_ROOT\dmlfile\shell\LocateASE" /f >nul 2>&1
reg delete "HKEY_CLASSES_ROOT\matfile\shell\LocateASE" /f >nul 2>&1
reg delete "HKEY_CLASSES_ROOT\dmlfile" /f >nul 2>&1
reg delete "HKEY_CLASSES_ROOT\matfile" /f >nul 2>&1

echo 删除旧的二级菜单（如果存在）...
reg delete "HKEY_CLASSES_ROOT\dml.1\shell\ASETools" /f >nul 2>&1
reg delete "HKEY_CLASSES_ROOT\3dsmat\shell\ASETools" /f >nul 2>&1
reg delete "HKEY_CLASSES_ROOT\SystemFileAssociations\.dml\shell\ase工具" /f >nul 2>&1
reg delete "HKEY_CLASSES_ROOT\SystemFileAssociations\.mat\shell\ase工具" /f >nul 2>&1

echo ✓ 旧版本菜单项清理完成

echo.
echo === 注册新的二级菜单（使用 shell 嵌套方法）===

set "PROGRAM_PATH=%~dp0ASELocator.exe"
echo 程序路径: %PROGRAM_PATH%

echo.
echo 为 .dml 文件添加 ASE 工具二级菜单...

REM 为 .dml 文件创建主菜单
reg add "HKEY_CLASSES_ROOT\SystemFileAssociations\.dml\shell\ase工具" /ve /d "ASE 工具" /f >nul 2>&1

REM 创建第一个子菜单：定位 ASE 文件
reg add "HKEY_CLASSES_ROOT\SystemFileAssociations\.dml\shell\ase工具\shell\定位ase文件" /ve /d "定位 ASE 文件" /f >nul 2>&1
reg add "HKEY_CLASSES_ROOT\SystemFileAssociations\.dml\shell\ase工具\shell\定位ase文件\command" /ve /d "\"%PROGRAM_PATH%\" \"locate\" \"%%1\"" /f >nul 2>&1

REM 创建第二个子菜单：打开 ASE 文件
reg add "HKEY_CLASSES_ROOT\SystemFileAssociations\.dml\shell\ase工具\shell\打开ase文件" /ve /d "打开 ASE 文件" /f >nul 2>&1
reg add "HKEY_CLASSES_ROOT\SystemFileAssociations\.dml\shell\ase工具\shell\打开ase文件\command" /ve /d "\"%PROGRAM_PATH%\" \"open\" \"%%1\"" /f >nul 2>&1

reg query "HKEY_CLASSES_ROOT\SystemFileAssociations\.dml\shell\ase工具" >nul 2>&1
if %errorlevel% == 0 (
    echo ✓ DML 文件 ASE 工具菜单注册成功
) else (
    echo × DML 文件 ASE 工具菜单注册失败
)

echo.
echo 为 .mat 文件添加 ASE 工具二级菜单...

REM 为 .mat 文件创建主菜单
reg add "HKEY_CLASSES_ROOT\SystemFileAssociations\.mat\shell\ase工具" /ve /d "ASE 工具" /f >nul 2>&1

REM 创建第一个子菜单：定位 ASE 文件
reg add "HKEY_CLASSES_ROOT\SystemFileAssociations\.mat\shell\ase工具\shell\定位ase文件" /ve /d "定位 ASE 文件" /f >nul 2>&1
reg add "HKEY_CLASSES_ROOT\SystemFileAssociations\.mat\shell\ase工具\shell\定位ase文件\command" /ve /d "\"%PROGRAM_PATH%\" \"locate\" \"%%1\"" /f >nul 2>&1

REM 创建第二个子菜单：打开 ASE 文件
reg add "HKEY_CLASSES_ROOT\SystemFileAssociations\.mat\shell\ase工具\shell\打开ase文件" /ve /d "打开 ASE 文件" /f >nul 2>&1
reg add "HKEY_CLASSES_ROOT\SystemFileAssociations\.mat\shell\ase工具\shell\打开ase文件\command" /ve /d "\"%PROGRAM_PATH%\" \"open\" \"%%1\"" /f >nul 2>&1

reg query "HKEY_CLASSES_ROOT\SystemFileAssociations\.mat\shell\ase工具" >nul 2>&1
if %errorlevel% == 0 (
    echo ✓ MAT 文件 ASE 工具菜单注册成功
) else (
    echo × MAT 文件 ASE 工具菜单注册失败
)

echo.
echo === 刷新系统文件关联 ===
echo 重启 Windows 资源管理器...
taskkill /f /im explorer.exe >nul 2>&1
timeout /t 2 /nobreak >nul
start explorer.exe

echo.
echo 注册完成！
echo.
echo 现在请测试二级菜单：
echo 1. 找到一个 .dml 或 .mat 文件
echo 2. 右键点击该文件
echo 3. 查看是否有 "ASE 工具" 主菜单项
echo 4. 悬停在 "ASE 工具" 上查看子菜单：
echo    - "定位 ASE 文件" - 只在资源管理器中定位
echo    - "打开 ASE 文件" - 用 ModelPreview.exe 打开
echo.
pause 