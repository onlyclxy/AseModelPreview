@echo off

echo ���ڽ���ļ�δ�������״̬��Zone.Identifier��...
powershell -command "Get-ChildItem -Recurse -Path '%~dp0' | Unblock-File -ErrorAction SilentlyContinue"

echo ����ע�� AssimpThumbnailProvider ����ͼ�������...

REM ������ԱȨ��
net session >nul 2>&1
if %errorLevel% neq 0 (
    echo ���Թ���Ա������д˽ű���
    pause
    exit /b 1
)

cd /d "%~dp0"

REM ע�� DLL
echo ע�� AssimpThumbnailProvider.dll...
regasm.exe  /codebase   /tlb   AssimpThumbnailProvider.dll
if %errorLevel% neq 0 (
    echo ע��ʧ�ܣ�
    pause
    exit /b 1
)

REM ע���ļ���չ������ - ʹ������·����GUID��ʽ
echo ע���ļ���չ������...
reg add "HKLM\SOFTWARE\Classes\.fbx\ShellEx\{e357fccd-a995-4576-b01f-234630154e96}" /ve /d "{5F1B8D18-9E3A-4F2F-8AFE-4FCFE7D7A686}" /f
reg add "HKLM\SOFTWARE\Classes\.ase\ShellEx\{e357fccd-a995-4576-b01f-234630154e96}" /ve /d "{5F1B8D18-9E3A-4F2F-8AFE-4FCFE7D7A686}" /f
reg add "HKLM\SOFTWARE\Classes\.obj\ShellEx\{e357fccd-a995-4576-b01f-234630154e96}" /ve /d "{5F1B8D18-9E3A-4F2F-8AFE-4FCFE7D7A686}" /f
reg add "HKLM\SOFTWARE\Classes\.gltf\ShellEx\{e357fccd-a995-4576-b01f-234630154e96}" /ve /d "{5F1B8D18-9E3A-4F2F-8AFE-4FCFE7D7A686}" /f
reg add "HKLM\SOFTWARE\Classes\.glb\ShellEx\{e357fccd-a995-4576-b01f-234630154e96}" /ve /d "{5F1B8D18-9E3A-4F2F-8AFE-4FCFE7D7A686}" /f

REM ע�ᵱǰ�û����ļ�����
echo ע�ᵱǰ�û����ļ�����...
reg add "HKCU\SOFTWARE\Classes\.fbx\ShellEx\{e357fccd-a995-4576-b01f-234630154e96}" /ve /d "{5F1B8D18-9E3A-4F2F-8AFE-4FCFE7D7A686}" /f
reg add "HKCU\SOFTWARE\Classes\.ase\ShellEx\{e357fccd-a995-4576-b01f-234630154e96}" /ve /d "{5F1B8D18-9E3A-4F2F-8AFE-4FCFE7D7A686}" /f
reg add "HKCU\SOFTWARE\Classes\.obj\ShellEx\{e357fccd-a995-4576-b01f-234630154e96}" /ve /d "{5F1B8D18-9E3A-4F2F-8AFE-4FCFE7D7A686}" /f
reg add "HKCU\SOFTWARE\Classes\.gltf\ShellEx\{e357fccd-a995-4576-b01f-234630154e96}" /ve /d "{5F1B8D18-9E3A-4F2F-8AFE-4FCFE7D7A686}" /f
reg add "HKCU\SOFTWARE\Classes\.glb\ShellEx\{e357fccd-a995-4576-b01f-234630154e96}" /ve /d "{5F1B8D18-9E3A-4F2F-8AFE-4FCFE7D7A686}" /f

REM ȡ����������ע�������ͼ�������
echo �Ƴ����ܵĳ�ͻ�������...
reg delete "HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\PreviewHandlers\{5F1B8D18-9E3A-4F2F-8AFE-4FCFE7D7A686}" /f >nul 2>&1

REM ��ȫ������������ͼ��ͼ�껺��
echo �����������ͼ����...
taskkill /f /im explorer.exe >nul 2>&1

REM ɾ����������ͼ��ͼ�껺��
echo ����ˢ������ͼ����...
del /f /s /q "%LOCALAPPDATA%\Microsoft\Windows\Explorer\thumbcache_*.db" >nul 2>&1
del /f /s /q "%LOCALAPPDATA%\Microsoft\Windows\Explorer\iconcache_*.db" >nul 2>&1
del /f /s /q "%LOCALAPPDATA%\Microsoft\Windows\Explorer\expcache_*.db" >nul 2>&1

REM ��������ͼ�����С
echo ��������ͼ����...
reg delete "HKCU\Software\Microsoft\Windows\CurrentVersion\Explorer\Thumbcache" /f >nul 2>&1
reg add "HKCU\Software\Microsoft\Windows\CurrentVersion\Explorer\Thumbcache" /f >nul 2>&1

REM ���� Shell ͼ�껺��
echo ���� Shell ͼ�껺��...
attrib -h -s -r "%USERPROFILE%\AppData\Local\IconCache.db" >nul 2>&1
del /f "%USERPROFILE%\AppData\Local\IconCache.db" >nul 2>&1

REM Ӧ���������
echo ���� Shell ����...
ie4uinit.exe -show

REM ������Դ������
echo ������Դ������...
start explorer.exe

echo ��װ��ɣ�
echo ע�⣺������Ҫע�������µ�¼���������������ʹ���и�����Ч��
pause