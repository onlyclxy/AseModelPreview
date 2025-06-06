@echo off
echo 检查 DML 和 MAT 文件的注册表状态（二级菜单版本）...
echo.

echo === 检查 .dml 扩展名 ===
reg query "HKEY_CLASSES_ROOT\.dml" 2>nul
if %errorlevel% == 0 (
    echo 找到 .dml 注册表项
    reg query "HKEY_CLASSES_ROOT\.dml" /ve 2>nul
) else (
    echo 未找到 .dml 注册表项
)

echo.
echo === 检查 .mat 扩展名 ===
reg query "HKEY_CLASSES_ROOT\.mat" 2>nul
if %errorlevel% == 0 (
    echo 找到 .mat 注册表项
    reg query "HKEY_CLASSES_ROOT\.mat" /ve 2>nul
) else (
    echo 未找到 .mat 注册表项
)

echo.
echo === 检查 dml.1 程序类型的 ASE 工具菜单 ===
reg query "HKEY_CLASSES_ROOT\dml.1\shell\ASETools" 2>nul
if %errorlevel% == 0 (
    echo 找到 dml.1 ASE 工具主菜单
    reg query "HKEY_CLASSES_ROOT\dml.1\shell\ASETools" /ve 2>nul
    
    echo 检查子菜单：
    reg query "HKEY_CLASSES_ROOT\dml.1\shell\ASETools\shell\locate" /ve 2>nul
    reg query "HKEY_CLASSES_ROOT\dml.1\shell\ASETools\shell\locate\command" /ve 2>nul
    
    reg query "HKEY_CLASSES_ROOT\dml.1\shell\ASETools\shell\open" /ve 2>nul
    reg query "HKEY_CLASSES_ROOT\dml.1\shell\ASETools\shell\open\command" /ve 2>nul
) else (
    echo 未找到 dml.1 ASE 工具菜单
)

echo.
echo === 检查 3dsmat 程序类型的 ASE 工具菜单 ===
reg query "HKEY_CLASSES_ROOT\3dsmat\shell\ASETools" 2>nul
if %errorlevel% == 0 (
    echo 找到 3dsmat ASE 工具主菜单
    reg query "HKEY_CLASSES_ROOT\3dsmat\shell\ASETools" /ve 2>nul
    
    echo 检查子菜单：
    reg query "HKEY_CLASSES_ROOT\3dsmat\shell\ASETools\shell\locate" /ve 2>nul
    reg query "HKEY_CLASSES_ROOT\3dsmat\shell\ASETools\shell\locate\command" /ve 2>nul
    
    reg query "HKEY_CLASSES_ROOT\3dsmat\shell\ASETools\shell\open" /ve 2>nul
    reg query "HKEY_CLASSES_ROOT\3dsmat\shell\ASETools\shell\open\command" /ve 2>nul
) else (
    echo 未找到 3dsmat ASE 工具菜单
)

echo.
echo === 检查旧的注册项（应该清理）===
echo 检查旧的单级菜单：
reg query "HKEY_CLASSES_ROOT\dml.1\shell\LocateASE" 2>nul && echo "警告：发现旧的 dml.1 LocateASE 注册项，建议清理"
reg query "HKEY_CLASSES_ROOT\3dsmat\shell\LocateASE" 2>nul && echo "警告：发现旧的 3dsmat LocateASE 注册项，建议清理"
reg query "HKEY_CLASSES_ROOT\dmlfile\shell\LocateASE" 2>nul && echo "警告：发现旧的 dmlfile 注册项，建议清理"
reg query "HKEY_CLASSES_ROOT\matfile\shell\LocateASE" 2>nul && echo "警告：发现旧的 matfile 注册项，建议清理"

echo.
echo 检查完成！
pause 