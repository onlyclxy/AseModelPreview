@echo off
echo 正在注册 AssimpThumbnailProvider 缩略图处理程序...

REM 检查管理员权限
net session >nul 2>&1
if %errorLevel% neq 0 (
    echo 请以管理员身份运行此脚本！
    pause
    exit /b 1
)

cd /d "%~dp0"

REM 结束所有使用DLL的进程
echo 结束相关进程...
taskkill /f /im dllhost.exe
taskkill /f /im explorer.exe

REM 注册 DLL
echo 注册 AssimpThumbnailProvider.dll...
"C:\Windows\Microsoft.NET\Framework64\v4.0.30319\regasm.exe" /codebase /tlb "AssimpThumbnailProvider\bin\Debug\AssimpThumbnailProvider.dll"
if %errorLevel% neq 0 (
    echo 注册失败！
    pause
    exit /b 1
)

REM 添加COM注册表项
echo 添加COM注册表项...
reg add "HKCR\CLSID\{B4FB3A0A-3B0D-4B58-B89C-3E4E83E76A08}" /ve /d "Assimp 3D Thumbnail Provider" /f
reg add "HKCR\AssimpThumbnailProvider.Thumbnail" /ve /d "Assimp 3D Thumbnail Provider" /f
reg add "HKCR\AssimpThumbnailProvider.Thumbnail\CLSID" /ve /d "{B4FB3A0A-3B0D-4B58-B89C-3E4E83E76A08}" /f

REM 注册文件扩展名关联
echo 注册文件扩展名关联...
reg add "HKCR\.ase\shellex\{E357FCCD-A995-4576-B01F-234630154E96}" /ve /d "{B4FB3A0A-3B0D-4B58-B89C-3E4E83E76A08}" /f
reg add "HKCR\.fbx\shellex\{E357FCCD-A995-4576-B01F-234630154E96}" /ve /d "{B4FB3A0A-3B0D-4B58-B89C-3E4E83E76A08}" /f
reg add "HKCR\.obj\shellex\{E357FCCD-A995-4576-B01F-234630154E96}" /ve /d "{B4FB3A0A-3B0D-4B58-B89C-3E4E83E76A08}" /f
reg add "HKCR\.gltf\shellex\{E357FCCD-A995-4576-B01F-234630154E96}" /ve /d "{B4FB3A0A-3B0D-4B58-B89C-3E4E83E76A08}" /f
reg add "HKCR\.glb\shellex\{E357FCCD-A995-4576-B01F-234630154E96}" /ve /d "{B4FB3A0A-3B0D-4B58-B89C-3E4E83E76A08}" /f

REM 添加COM组件缓存
echo 添加COM组件缓存...
reg add "HKCU\Software\Classes\CLSID\{B4FB3A0A-3B0D-4B58-B89C-3E4E83E76A08}" /ve /d "Assimp 3D Thumbnail Provider" /f
reg add "HKLM\Software\Classes\CLSID\{B4FB3A0A-3B0D-4B58-B89C-3E4E83E76A08}" /ve /d "Assimp 3D Thumbnail Provider" /f
reg add "HKCU\Software\Classes\Wow6432Node\CLSID\{B4FB3A0A-3B0D-4B58-B89C-3E4E83E76A08}" /ve /d "Assimp 3D Thumbnail Provider" /f
reg add "HKLM\Software\Classes\Wow6432Node\CLSID\{B4FB3A0A-3B0D-4B58-B89C-3E4E83E76A08}" /ve /d "Assimp 3D Thumbnail Provider" /f

REM 安装到 GAC
echo 安装到 GAC...
"C:\Windows\Microsoft.NET\Framework64\v4.0.30319\gacutil.exe" /i "AssimpThumbnailProvider\bin\Debug\AssimpThumbnailProvider.dll"

REM 清理缩略图缓存
echo 清除缩略图缓存...
del /f /s /q "%LocalAppData%\Microsoft\Windows\Explorer\thumbcache_*.db"
del /f /s /q "%LocalAppData%\Microsoft\Windows\Explorer\iconcache_*.db"

REM 重启资源管理器
echo 重启资源管理器...
timeout /t 2
start explorer.exe

echo 安装完成！
echo 如果没有看到缩略图，请重启计算机。
pause 