@echo off
echo 正在卸载 AssimpThumbnailProvider 缩略图处理程序...

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

REM 注销 DLL
echo 注销 AssimpThumbnailProvider.dll...
"C:\Windows\Microsoft.NET\Framework64\v4.0.30319\regasm.exe" /unregister "AssimpThumbnailProvider\bin\Debug\AssimpThumbnailProvider.dll"

REM 删除COM注册表项
echo 删除COM注册表项...
reg delete "HKCR\CLSID\{B4FB3A0A-3B0D-4B58-B89C-3E4E83E76A08}" /f
reg delete "HKCR\AssimpThumbnailProvider.Thumbnail" /f

REM 删除文件扩展名关联
echo 删除文件扩展名关联...
reg delete "HKCR\.ase\shellex\{E357FCCD-A995-4576-B01F-234630154E96}" /f
reg delete "HKCR\.fbx\shellex\{E357FCCD-A995-4576-B01F-234630154E96}" /f
reg delete "HKCR\.obj\shellex\{E357FCCD-A995-4576-B01F-234630154E96}" /f
reg delete "HKCR\.gltf\shellex\{E357FCCD-A995-4576-B01F-234630154E96}" /f
reg delete "HKCR\.glb\shellex\{E357FCCD-A995-4576-B01F-234630154E96}" /f

REM 清理缩略图缓存
echo 清除缩略图缓存...
del /f /s /q "%LocalAppData%\Microsoft\Windows\Explorer\thumbcache_*.db"
del /f /s /q "%LocalAppData%\Microsoft\Windows\Explorer\iconcache_*.db"

REM 删除COM组件缓存
echo 删除COM组件缓存...
reg delete "HKCU\Software\Classes\CLSID\{B4FB3A0A-3B0D-4B58-B89C-3E4E83E76A08}" /f
reg delete "HKLM\Software\Classes\CLSID\{B4FB3A0A-3B0D-4B58-B89C-3E4E83E76A08}" /f
reg delete "HKCU\Software\Classes\Wow6432Node\CLSID\{B4FB3A0A-3B0D-4B58-B89C-3E4E83E76A08}" /f
reg delete "HKLM\Software\Classes\Wow6432Node\CLSID\{B4FB3A0A-3B0D-4B58-B89C-3E4E83E76A08}" /f

REM 清理 GAC
echo 清理 GAC...
"C:\Windows\Microsoft.NET\Framework64\v4.0.30319\gacutil.exe" /u AssimpThumbnailProvider

REM 重启资源管理器
echo 重启资源管理器...
timeout /t 2
start explorer.exe

echo 卸载完成！
echo 如果仍然看到缩略图，请重启计算机。
pause 