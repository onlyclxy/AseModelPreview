@echo off
echo ����ж�� AssimpThumbnailProvider ����ͼ�������...

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
"C:\Windows\Microsoft.NET\Framework64\v4.0.30319\regasm.exe" /unregister "AssimpThumbnailProvider\bin\Debug\AssimpThumbnailProvider.dll"

REM ɾ��COMע�����
echo ɾ��COMע�����...
reg delete "HKCR\CLSID\{B4FB3A0A-3B0D-4B58-B89C-3E4E83E76A08}" /f
reg delete "HKCR\AssimpThumbnailProvider.Thumbnail" /f

REM ɾ���ļ���չ������
echo ɾ���ļ���չ������...
reg delete "HKCR\.ase\shellex\{E357FCCD-A995-4576-B01F-234630154E96}" /f
reg delete "HKCR\.fbx\shellex\{E357FCCD-A995-4576-B01F-234630154E96}" /f
reg delete "HKCR\.obj\shellex\{E357FCCD-A995-4576-B01F-234630154E96}" /f
reg delete "HKCR\.gltf\shellex\{E357FCCD-A995-4576-B01F-234630154E96}" /f
reg delete "HKCR\.glb\shellex\{E357FCCD-A995-4576-B01F-234630154E96}" /f

REM ��������ͼ����
echo �������ͼ����...
del /f /s /q "%LocalAppData%\Microsoft\Windows\Explorer\thumbcache_*.db"
del /f /s /q "%LocalAppData%\Microsoft\Windows\Explorer\iconcache_*.db"

REM ɾ��COM�������
echo ɾ��COM�������...
reg delete "HKCU\Software\Classes\CLSID\{B4FB3A0A-3B0D-4B58-B89C-3E4E83E76A08}" /f
reg delete "HKLM\Software\Classes\CLSID\{B4FB3A0A-3B0D-4B58-B89C-3E4E83E76A08}" /f
reg delete "HKCU\Software\Classes\Wow6432Node\CLSID\{B4FB3A0A-3B0D-4B58-B89C-3E4E83E76A08}" /f
reg delete "HKLM\Software\Classes\Wow6432Node\CLSID\{B4FB3A0A-3B0D-4B58-B89C-3E4E83E76A08}" /f

REM ���� GAC
echo ���� GAC...
"C:\Windows\Microsoft.NET\Framework64\v4.0.30319\gacutil.exe" /u AssimpThumbnailProvider

REM ������Դ������
echo ������Դ������...
timeout /t 2
start explorer.exe

echo ж����ɣ�
echo �����Ȼ��������ͼ���������������
pause 