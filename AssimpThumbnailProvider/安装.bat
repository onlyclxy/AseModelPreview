@echo off

echo 正在解除文件未知来源状态（Zone.Identifier）...
powershell -command "Get-ChildItem -Recurse -Path '%~dp0' | Unblock-File -ErrorAction SilentlyContinue"

echo 正在注册 AssimpThumbnailProvider 缩略图处理器...

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

REM 注册文件扩展名处理 - 使用完整路径和GUID格式
echo 注册文件扩展名处理...
reg add "HKLM\SOFTWARE\Classes\.fbx\ShellEx\{e357fccd-a995-4576-b01f-234630154e96}" /ve /d "{5F1B8D18-9E3A-4F2F-8AFE-4FCFE7D7A686}" /f
reg add "HKLM\SOFTWARE\Classes\.ase\ShellEx\{e357fccd-a995-4576-b01f-234630154e96}" /ve /d "{5F1B8D18-9E3A-4F2F-8AFE-4FCFE7D7A686}" /f
reg add "HKLM\SOFTWARE\Classes\.obj\ShellEx\{e357fccd-a995-4576-b01f-234630154e96}" /ve /d "{5F1B8D18-9E3A-4F2F-8AFE-4FCFE7D7A686}" /f
reg add "HKLM\SOFTWARE\Classes\.gltf\ShellEx\{e357fccd-a995-4576-b01f-234630154e96}" /ve /d "{5F1B8D18-9E3A-4F2F-8AFE-4FCFE7D7A686}" /f
reg add "HKLM\SOFTWARE\Classes\.glb\ShellEx\{e357fccd-a995-4576-b01f-234630154e96}" /ve /d "{5F1B8D18-9E3A-4F2F-8AFE-4FCFE7D7A686}" /f
reg add "HKLM\SOFTWARE\Classes\.dml\ShellEx\{e357fccd-a995-4576-b01f-234630154e96}" /ve /d "{5F1B8D18-9E3A-4F2F-8AFE-4FCFE7D7A686}" /f
reg add "HKLM\SOFTWARE\Classes\.chr\ShellEx\{e357fccd-a995-4576-b01f-234630154e96}" /ve /d "{5F1B8D18-9E3A-4F2F-8AFE-4FCFE7D7A686}" /f

REM 注册当前用户的文件处理
echo 注册当前用户的文件处理...
reg add "HKCU\SOFTWARE\Classes\.fbx\ShellEx\{e357fccd-a995-4576-b01f-234630154e96}" /ve /d "{5F1B8D18-9E3A-4F2F-8AFE-4FCFE7D7A686}" /f
reg add "HKCU\SOFTWARE\Classes\.ase\ShellEx\{e357fccd-a995-4576-b01f-234630154e96}" /ve /d "{5F1B8D18-9E3A-4F2F-8AFE-4FCFE7D7A686}" /f
reg add "HKCU\SOFTWARE\Classes\.obj\ShellEx\{e357fccd-a995-4576-b01f-234630154e96}" /ve /d "{5F1B8D18-9E3A-4F2F-8AFE-4FCFE7D7A686}" /f
reg add "HKCU\SOFTWARE\Classes\.gltf\ShellEx\{e357fccd-a995-4576-b01f-234630154e96}" /ve /d "{5F1B8D18-9E3A-4F2F-8AFE-4FCFE7D7A686}" /f
reg add "HKCU\SOFTWARE\Classes\.glb\ShellEx\{e357fccd-a995-4576-b01f-234630154e96}" /ve /d "{5F1B8D18-9E3A-4F2F-8AFE-4FCFE7D7A686}" /f
reg add "HKCU\SOFTWARE\Classes\.dml\ShellEx\{e357fccd-a995-4576-b01f-234630154e96}" /ve /d "{5F1B8D18-9E3A-4F2F-8AFE-4FCFE7D7A686}" /f
reg add "HKCU\SOFTWARE\Classes\.chr\ShellEx\{e357fccd-a995-4576-b01f-234630154e96}" /ve /d "{5F1B8D18-9E3A-4F2F-8AFE-4FCFE7D7A686}" /f

REM 删除所有用户的缩略图缓存
echo 删除所有用户的缩略图缓存...
reg delete "HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\PreviewHandlers\{5F1B8D18-9E3A-4F2F-8AFE-4FCFE7D7A686}" /f >nul 2>&1

REM 强制关闭所有用户的缩略图预览
echo 强制关闭所有用户的缩略图预览...
taskkill /f /im explorer.exe >nul 2>&1

REM 删除所有用户的缩略图缓存
echo 删除所有用户的缩略图缓存...
del /f /s /q "%LOCALAPPDATA%\Microsoft\Windows\Explorer\thumbcache_*.db" >nul 2>&1
del /f /s /q "%LOCALAPPDATA%\Microsoft\Windows\Explorer\iconcache_*.db" >nul 2>&1
del /f /s /q "%LOCALAPPDATA%\Microsoft\Windows\Explorer\expcache_*.db" >nul 2>&1

REM 重置所有用户的缩略图缓存
echo 重置所有用户的缩略图缓存...
reg delete "HKCU\Software\Microsoft\Windows\CurrentVersion\Explorer\Thumbcache" /f >nul 2>&1
reg add "HKCU\Software\Microsoft\Windows\CurrentVersion\Explorer\Thumbcache" /f >nul 2>&1

REM 重置 Shell 缩略图缓存
echo 重置 Shell 缩略图缓存...
attrib -h -s -r "%USERPROFILE%\AppData\Local\IconCache.db" >nul 2>&1
del /f "%USERPROFILE%\AppData\Local\IconCache.db" >nul 2>&1

REM 应用所有用户的更改
echo 应用所有用户的更改...
ie4uinit.exe -show

REM 打开资源管理器
echo 打开资源管理器...
start explorer.exe

echo 安装完成！
echo 注意：如果需要使用新的登录，请重新运行此脚本，以确保所有更改生效。
pause