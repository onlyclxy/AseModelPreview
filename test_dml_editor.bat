@echo off
echo ===============================================
echo DML编辑器测试脚本
echo ===============================================
echo.

REM 检查可执行文件是否存在
if not exist "DMLView\bin\Debug\DMLView.exe" (
    echo 错误: 找不到DMLView.exe
    echo 请先运行 compile_solution.bat 编译项目
    pause
    exit /b 1
)

echo ✅ 找到可执行文件: DMLView\bin\Debug\DMLView.exe
echo.

REM 检查示例文件是否存在
if exist "DMLView\sample.dml" (
    echo ✅ 找到示例文件: DMLView\sample.dml
) else (
    echo ⚠️  未找到示例文件，将创建一个测试文件
    echo // DML测试文件 > "DMLView\test.dml"
    echo [SHADER] >> "DMLView\test.dml"
    echo test_shader >> "DMLView\test.dml"
    echo [GEOMETRY MESH] >> "DMLView\test.dml"
    echo ..\resources\test.ase >> "DMLView\test.dml"
    echo # 这是注释 >> "DMLView\test.dml"
    echo ✅ 已创建测试文件: DMLView\test.dml
)
echo.

echo 🚀 启动DML编辑器...
echo.
echo 测试项目:
echo 1. 程序是否正常启动
echo 2. 界面是否正确显示
echo 3. 菜单功能是否可用
echo 4. 语法高亮是否工作
echo 5. 行号显示是否正常
echo.

REM 启动程序
start "" "DMLView\bin\Debug\DMLView.exe"

echo 程序已启动。请手动测试以下功能:
echo.
echo ✓ 文件 → 打开 → 选择sample.dml或test.dml
echo ✓ 查看语法高亮效果 (段标题蓝色，路径绿色，注释灰色)
echo ✓ 视图 → 语法高亮 (切换测试)
echo ✓ 视图 → 行号 (切换测试) 
echo ✓ 编辑一些内容测试实时高亮
echo ✓ 文件 → 保存测试
echo.
echo 如果程序正常工作，测试成功！
echo 如果出现错误，请查看 故障排除指南.md
echo.
pause 