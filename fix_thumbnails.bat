@echo off
echo 修复缩略图显示问题...

REM 检查管理员权限
net session >nul 2>&1
if %errorLevel% neq 0 (
    echo 请以管理员身份运行此脚本！
    pause
    exit /b 1
)

cd /d "%~dp0"

echo 重新注册文件关联及缩略图处理程序...

REM 确保 COM 组件已正确注册
echo 重新注册 AssimpThumbnailProvider.dll...
"%SystemRoot%\Microsoft.NET\Framework\v4.0.30319\RegAsm.exe" /unregister "AssimpThumbnailProvider\bin\Debug\AssimpThumbnailProvider.dll"
"%SystemRoot%\Microsoft.NET\Framework\v4.0.30319\RegAsm.exe" /codebase /tlb "AssimpThumbnailProvider\bin\Debug\AssimpThumbnailProvider.dll"

REM 重新注册缩略图处理程序 - 直接写入系统级注册表
echo 重新注册系统级文件关联...
reg add "HKLM\SOFTWARE\Classes\.fbx\ShellEx\{e357fccd-a995-4576-b01f-234630154e96}" /ve /d "{5F1B8D18-9E3A-4F2F-8AFE-4FCFE7D7A686}" /f
reg add "HKLM\SOFTWARE\Classes\.ase\ShellEx\{e357fccd-a995-4576-b01f-234630154e96}" /ve /d "{5F1B8D18-9E3A-4F2F-8AFE-4FCFE7D7A686}" /f
reg add "HKLM\SOFTWARE\Classes\.obj\ShellEx\{e357fccd-a995-4576-b01f-234630154e96}" /ve /d "{5F1B8D18-9E3A-4F2F-8AFE-4FCFE7D7A686}" /f
reg add "HKLM\SOFTWARE\Classes\.gltf\ShellEx\{e357fccd-a995-4576-b01f-234630154e96}" /ve /d "{5F1B8D18-9E3A-4F2F-8AFE-4FCFE7D7A686}" /f
reg add "HKLM\SOFTWARE\Classes\.glb\ShellEx\{e357fccd-a995-4576-b01f-234630154e96}" /ve /d "{5F1B8D18-9E3A-4F2F-8AFE-4FCFE7D7A686}" /f

REM 也尝试使用 HKCR 路径 (HKEY_CLASSES_ROOT)
echo 尝试使用 HKCR 路径注册...
reg add "HKCR\.fbx\ShellEx\{e357fccd-a995-4576-b01f-234630154e96}" /ve /d "{5F1B8D18-9E3A-4F2F-8AFE-4FCFE7D7A686}" /f
reg add "HKCR\.ase\ShellEx\{e357fccd-a995-4576-b01f-234630154e96}" /ve /d "{5F1B8D18-9E3A-4F2F-8AFE-4FCFE7D7A686}" /f
reg add "HKCR\.obj\ShellEx\{e357fccd-a995-4576-b01f-234630154e96}" /ve /d "{5F1B8D18-9E3A-4F2F-8AFE-4FCFE7D7A686}" /f
reg add "HKCR\.gltf\ShellEx\{e357fccd-a995-4576-b01f-234630154e96}" /ve /d "{5F1B8D18-9E3A-4F2F-8AFE-4FCFE7D7A686}" /f
reg add "HKCR\.glb\ShellEx\{e357fccd-a995-4576-b01f-234630154e96}" /ve /d "{5F1B8D18-9E3A-4F2F-8AFE-4FCFE7D7A686}" /f

REM 将 DLL 的位置添加到 PATH 环境变量以确保可访问性
echo 添加 DLL 路径到系统 PATH...
set DLL_PATH=%~dp0AssimpThumbnailProvider\bin\Debug
setx PATH "%PATH%;%DLL_PATH%" /m

REM 彻底清理缩略图缓存
echo 关闭资源管理器...
taskkill /f /im explorer.exe

echo 删除缩略图缓存...
cd /d %LOCALAPPDATA%\Microsoft\Windows\Explorer
del /f /s /q thumbcache_*.db
del /f /s /q iconcache_*.db
del /f /s /q expcache_*.db

echo 删除 IconCache 数据库...
cd /d %LOCALAPPDATA%
if exist IconCache.db attrib -h IconCache.db
if exist IconCache.db del /f IconCache.db

echo 重置 ShellIconCache...
reg delete "HKCU\Software\Microsoft\Windows\CurrentVersion\Explorer\IconCache" /f
reg delete "HKCU\Software\Microsoft\Windows\CurrentVersion\Explorer\Thumbcache" /f
reg add "HKCU\Software\Microsoft\Windows\CurrentVersion\Explorer\IconCache" /f
reg add "HKCU\Software\Microsoft\Windows\CurrentVersion\Explorer\Thumbcache" /f

echo 重置 Shell 预览设置...
reg delete "HKCU\SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\FileExts\.fbx" /v "UserChoice" /f
reg delete "HKCU\SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\FileExts\.ase" /v "UserChoice" /f
reg delete "HKCU\SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\FileExts\.obj" /v "UserChoice" /f

echo 重置文件夹查看选项...
reg delete "HKCU\Software\Microsoft\Windows\CurrentVersion\Explorer\Streams\Defaults" /f
reg delete "HKCU\Software\Microsoft\Windows\CurrentVersion\Explorer\FolderTypes" /f /va /s

echo 执行系统文件缓存更新...
ie4uinit.exe -ClearIconCache
ie4uinit.exe -show

echo 重新启动资源管理器...
start explorer.exe

echo 修复完成！请尝试查看文件。如果还未显示缩略图，请尝试注销并重新登录，或重启计算机。
pause 