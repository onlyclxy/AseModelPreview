@echo off
echo 正在卸载 AssimpThumbnailProvider 缩略图处理器...

REM 检查管理员权限
net session >nul 2>&1
if %errorlevel% neq 0 (
    echo 请以管理员身份运行此脚本！
    pause
    exit /b 1
)

cd /d "%~dp0"

REM 强制结束所有 COM Surrogate 进程（dllhost.exe）
echo 正在结束 COM Surrogate 进程...
taskkill /f /im dllhost.exe >nul 2>&1
if %errorlevel% equ 0 (
    echo COM Surrogate 进程已结束。
) else (
    echo 未能结束 COM Surrogate 进程，可能没有正在运行的实例。
)

REM 注销 DLL
echo 注销 AssimpThumbnailProvider.dll...
regasm.exe /codebase AssimpThumbnailProvider.dll /u
regasm.exe /unregister AssimpThumbnailProvider.dll

REM 删除文件扩展名处理
echo 删除文件扩展名处理...
reg delete "HKCR\.ase\shellex\{E357FCCD-A995-4576-B01F-234630154E96}" /f
reg delete "HKCR\.fbx\shellex\{E357FCCD-A995-4576-B01F-234630154E96}" /f
reg delete "HKCR\.obj\shellex\{E357FCCD-A995-4576-B01F-234630154E96}" /f
reg delete "HKCR\.gltf\shellex\{E357FCCD-A995-4576-B01F-234630154E96}" /f
reg delete "HKCR\.glb\shellex\{E357FCCD-A995-4576-B01F-234630154E96}" /f
reg delete "HKCR\.dml\shellex\{E357FCCD-A995-4576-B01F-234630154E96}" /f
reg delete "HKCR\.chr\shellex\{E357FCCD-A995-4576-B01F-234630154E96}" /f

REM 清理缩略图缓存
echo 清理缩略图缓存...
taskkill /f /im explorer.exe
timeout /t 0
del /f /s /q "%LocalAppData%\Microsoft\Windows\Explorer\thumbcache_*.db"
del /f /s /q "%LocalAppData%\Microsoft\Windows\Explorer\iconcache_*.db"

REM 启动资源管理器
start explorer.exe

echo 卸载完成！
pause
