@echo off
echo ����ע�� AssimpThumbnailProvider ����ͼ�������...

REM ������ԱȨ��
net session >nul 2>&1
if %errorLevel% neq 0 (
    echo ���Թ���Ա������д˽ű���
    pause
    exit /b 1
)

cd /d "%~dp0"

REM ��������ʹ��DLL�Ľ���
echo ������ؽ���...
taskkill /f /im dllhost.exe
taskkill /f /im explorer.exe

REM ע�� DLL
echo ע�� AssimpThumbnailProvider.dll...
"C:\Windows\Microsoft.NET\Framework64\v4.0.30319\regasm.exe" /codebase /tlb "AssimpThumbnailProvider\bin\Debug\AssimpThumbnailProvider.dll"
if %errorLevel% neq 0 (
    echo ע��ʧ�ܣ�
    pause
    exit /b 1
)

REM ���COMע�����
echo ���COMע�����...
reg add "HKCR\CLSID\{B4FB3A0A-3B0D-4B58-B89C-3E4E83E76A08}" /ve /d "Assimp 3D Thumbnail Provider" /f
reg add "HKCR\AssimpThumbnailProvider.Thumbnail" /ve /d "Assimp 3D Thumbnail Provider" /f
reg add "HKCR\AssimpThumbnailProvider.Thumbnail\CLSID" /ve /d "{B4FB3A0A-3B0D-4B58-B89C-3E4E83E76A08}" /f

REM ע���ļ���չ������
echo ע���ļ���չ������...
reg add "HKCR\.ase\shellex\{E357FCCD-A995-4576-B01F-234630154E96}" /ve /d "{B4FB3A0A-3B0D-4B58-B89C-3E4E83E76A08}" /f
reg add "HKCR\.fbx\shellex\{E357FCCD-A995-4576-B01F-234630154E96}" /ve /d "{B4FB3A0A-3B0D-4B58-B89C-3E4E83E76A08}" /f
reg add "HKCR\.obj\shellex\{E357FCCD-A995-4576-B01F-234630154E96}" /ve /d "{B4FB3A0A-3B0D-4B58-B89C-3E4E83E76A08}" /f
reg add "HKCR\.gltf\shellex\{E357FCCD-A995-4576-B01F-234630154E96}" /ve /d "{B4FB3A0A-3B0D-4B58-B89C-3E4E83E76A08}" /f
reg add "HKCR\.glb\shellex\{E357FCCD-A995-4576-B01F-234630154E96}" /ve /d "{B4FB3A0A-3B0D-4B58-B89C-3E4E83E76A08}" /f

REM ���COM�������
echo ���COM�������...
reg add "HKCU\Software\Classes\CLSID\{B4FB3A0A-3B0D-4B58-B89C-3E4E83E76A08}" /ve /d "Assimp 3D Thumbnail Provider" /f
reg add "HKLM\Software\Classes\CLSID\{B4FB3A0A-3B0D-4B58-B89C-3E4E83E76A08}" /ve /d "Assimp 3D Thumbnail Provider" /f
reg add "HKCU\Software\Classes\Wow6432Node\CLSID\{B4FB3A0A-3B0D-4B58-B89C-3E4E83E76A08}" /ve /d "Assimp 3D Thumbnail Provider" /f
reg add "HKLM\Software\Classes\Wow6432Node\CLSID\{B4FB3A0A-3B0D-4B58-B89C-3E4E83E76A08}" /ve /d "Assimp 3D Thumbnail Provider" /f

REM ��װ�� GAC
echo ��װ�� GAC...
"C:\Windows\Microsoft.NET\Framework64\v4.0.30319\gacutil.exe" /i "AssimpThumbnailProvider\bin\Debug\AssimpThumbnailProvider.dll"

REM ��������ͼ����
echo �������ͼ����...
del /f /s /q "%LocalAppData%\Microsoft\Windows\Explorer\thumbcache_*.db"
del /f /s /q "%LocalAppData%\Microsoft\Windows\Explorer\iconcache_*.db"

REM ������Դ������
echo ������Դ������...
timeout /t 2
start explorer.exe

echo ��װ��ɣ�
echo ���û�п�������ͼ���������������
pause 