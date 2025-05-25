@echo off

echo 正在解除文件未解除保护状态（Zone.Identifier）...
powershell -command "Get-ChildItem -Recurse -Path '%~dp0' | Unblock-File -ErrorAction SilentlyContinue"

echo 正在注册 AssimpThumbnailProvider 缩略图处理程序...

REM 检查管理员权限
net session >nul 2>&1
if %errorLevel% neq 0 (
    echo 请以管理员身份运行此脚本！
    pause
    exit /b 1
)

cd /d "%~dp0"

REM 注册 DLL
echo 注册 AssimpThumbnailProvider.dll...
regasm.exe  /codebase   /tlb   AssimpThumbnailProvider.dll
if %errorLevel% neq 0 (
    echo 注册失败！
    pause
    exit /b 1
)

REM 注册文件扩展名关联 - 使用完整路径和GUID格式
echo 注册文件扩展名关联...
reg add "HKLM\SOFTWARE\Classes\.fbx\ShellEx\{e357fccd-a995-4576-b01f-234630154e96}" /ve /d "{5F1B8D18-9E3A-4F2F-8AFE-4FCFE7D7A686}" /f
reg add "HKLM\SOFTWARE\Classes\.ase\ShellEx\{e357fccd-a995-4576-b01f-234630154e96}" /ve /d "{5F1B8D18-9E3A-4F2F-8AFE-4FCFE7D7A686}" /f
reg add "HKLM\SOFTWARE\Classes\.obj\ShellEx\{e357fccd-a995-4576-b01f-234630154e96}" /ve /d "{5F1B8D18-9E3A-4F2F-8AFE-4FCFE7D7A686}" /f
reg add "HKLM\SOFTWARE\Classes\.gltf\ShellEx\{e357fccd-a995-4576-b01f-234630154e96}" /ve /d "{5F1B8D18-9E3A-4F2F-8AFE-4FCFE7D7A686}" /f
reg add "HKLM\SOFTWARE\Classes\.glb\ShellEx\{e357fccd-a995-4576-b01f-234630154e96}" /ve /d "{5F1B8D18-9E3A-4F2F-8AFE-4FCFE7D7A686}" /f

REM 注册当前用户的文件关联
echo 注册当前用户的文件关联...
reg add "HKCU\SOFTWARE\Classes\.fbx\ShellEx\{e357fccd-a995-4576-b01f-234630154e96}" /ve /d "{5F1B8D18-9E3A-4F2F-8AFE-4FCFE7D7A686}" /f
reg add "HKCU\SOFTWARE\Classes\.ase\ShellEx\{e357fccd-a995-4576-b01f-234630154e96}" /ve /d "{5F1B8D18-9E3A-4F2F-8AFE-4FCFE7D7A686}" /f
reg add "HKCU\SOFTWARE\Classes\.obj\ShellEx\{e357fccd-a995-4576-b01f-234630154e96}" /ve /d "{5F1B8D18-9E3A-4F2F-8AFE-4FCFE7D7A686}" /f
reg add "HKCU\SOFTWARE\Classes\.gltf\ShellEx\{e357fccd-a995-4576-b01f-234630154e96}" /ve /d "{5F1B8D18-9E3A-4F2F-8AFE-4FCFE7D7A686}" /f
reg add "HKCU\SOFTWARE\Classes\.glb\ShellEx\{e357fccd-a995-4576-b01f-234630154e96}" /ve /d "{5F1B8D18-9E3A-4F2F-8AFE-4FCFE7D7A686}" /f

REM 取消其他可能注册的缩略图处理程序
echo 移除可能的冲突处理程序...
reg delete "HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\PreviewHandlers\{5F1B8D18-9E3A-4F2F-8AFE-4FCFE7D7A686}" /f >nul 2>&1

REM 完全清理所有缩略图和图标缓存
echo 彻底清除缩略图缓存...
taskkill /f /im explorer.exe >nul 2>&1

REM 删除所有缩略图和图标缓存
echo 正在刷新缩略图缓存...
del /f /s /q "%LOCALAPPDATA%\Microsoft\Windows\Explorer\thumbcache_*.db" >nul 2>&1
del /f /s /q "%LOCALAPPDATA%\Microsoft\Windows\Explorer\iconcache_*.db" >nul 2>&1
del /f /s /q "%LOCALAPPDATA%\Microsoft\Windows\Explorer\expcache_*.db" >nul 2>&1

REM 重置缩略图缓存大小
echo 重置缩略图设置...
reg delete "HKCU\Software\Microsoft\Windows\CurrentVersion\Explorer\Thumbcache" /f >nul 2>&1
reg add "HKCU\Software\Microsoft\Windows\CurrentVersion\Explorer\Thumbcache" /f >nul 2>&1

REM 更新 Shell 图标缓存
echo 重置 Shell 图标缓存...
attrib -h -s -r "%USERPROFILE%\AppData\Local\IconCache.db" >nul 2>&1
del /f "%USERPROFILE%\AppData\Local\IconCache.db" >nul 2>&1

REM 应用桌面更改
echo 更新 Shell 设置...
ie4uinit.exe -show

REM 重启资源管理器
echo 重启资源管理器...
start explorer.exe

echo 安装完成！
echo 注意：可能需要注销并重新登录，或重启计算机以使所有更改生效。
pause