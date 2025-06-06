@echo off
echo ===================================
echo ASE 文件定位器 - 完整功能测试（二级菜单版本）
echo ===================================
echo.

echo 1. 检查程序文件...
if exist "ASELocator.exe" (
    echo ✓ ASELocator.exe 存在
) else (
    echo × ASELocator.exe 不存在
    goto :End
)

if exist "ModelPreview.exe" (
    echo ✓ ModelPreview.exe 存在
) else (
    echo × ModelPreview.exe 不存在 - 这会导致警告，但程序仍能定位文件
)

echo.
echo 2. 检查测试文件...
if exist "test.dml" (
    echo ✓ test.dml 存在
) else (
    echo × test.dml 不存在
    goto :End
)

if exist "test.ase" (
    echo ✓ test.ase 存在
) else (
    echo × test.ase 不存在 - 程序会显示未找到警告
)

echo.
echo 3. 检查注册表状态...
reg query "HKEY_CLASSES_ROOT\dml.1\shell\ASETools" >nul 2>&1
if %errorlevel% == 0 (
    echo ✓ DML ASE 工具菜单已注册
) else (
    echo × DML ASE 工具菜单未注册
    echo   请运行 CleanAndRegister.bat 进行注册
)

reg query "HKEY_CLASSES_ROOT\3dsmat\shell\ASETools" >nul 2>&1
if %errorlevel% == 0 (
    echo ✓ MAT ASE 工具菜单已注册
) else (
    echo × MAT ASE 工具菜单未注册
    echo   请运行 CleanAndRegister.bat 进行注册
)

echo.
echo 4. 测试程序功能...
echo 测试 "定位" 模式：
echo 正在调用 ASELocator.exe 定位 test.dml...
"ASELocator.exe" "locate" "test.dml"

echo.
echo 测试 "打开" 模式：
echo 正在调用 ASELocator.exe 打开 test.dml...
"ASELocator.exe" "open" "test.dml"

echo.
echo 5. 手动测试步骤...
echo 现在请手动测试二级菜单：
echo 1. 在文件资源管理器中找到 test.dml 文件
echo 2. 右键点击 test.dml
echo 3. 查看是否有 "ASE 工具" 主菜单项
echo 4. 悬停在 "ASE 工具" 上查看子菜单：
echo    - "定位 ASE 文件" - 只在资源管理器中定位
echo    - "打开 ASE 文件" - 用 ModelPreview.exe 打开
echo 5. 分别测试两个子菜单项的功能
echo.

:End
echo 测试完成！
pause 