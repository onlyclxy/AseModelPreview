@echo off
echo 正在卸载 ASE 文件定位器的右键菜单...
echo.
echo 注意: 此操作需要管理员权限

REM 删除 .dml 文件类型的新二级菜单
reg delete "HKEY_CLASSES_ROOT\dml.1\shell\ASETools" /f >nul 2>&1
if %errorlevel% == 0 (
    echo ✓ 已删除 DML 文件的 ASE 工具菜单
) else (
    echo × 删除 DML 文件 ASE 工具菜单失败（可能不存在或权限不足）
)

REM 删除 .mat 文件类型的新二级菜单
reg delete "HKEY_CLASSES_ROOT\3dsmat\shell\ASETools" /f >nul 2>&1
if %errorlevel% == 0 (
    echo ✓ 已删除 MAT 文件的 ASE 工具菜单
) else (
    echo × 删除 MAT 文件 ASE 工具菜单失败（可能不存在或权限不足）
)

REM 删除旧的单级菜单（如果存在）
echo.
echo 清理旧版本菜单项...
reg delete "HKEY_CLASSES_ROOT\dml.1\shell\LocateASE" /f >nul 2>&1
reg delete "HKEY_CLASSES_ROOT\3dsmat\shell\LocateASE" /f >nul 2>&1
reg delete "HKEY_CLASSES_ROOT\dmlfile\shell\LocateASE" /f >nul 2>&1
reg delete "HKEY_CLASSES_ROOT\matfile\shell\LocateASE" /f >nul 2>&1

echo ✓ 旧版本菜单项清理完成

echo.
echo 卸载完成！
echo.
echo 按任意键退出...
pause >nul 