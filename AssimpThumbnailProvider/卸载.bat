@echo off
echo ����ж�� AssimpThumbnailProvider ����ͼ������...

REM ������ԱȨ��
net session >nul 2>&1
if %errorlevel% neq 0 (
    echo ���Թ���Ա������д˽ű���
    pause
    exit /b 1
)

cd /d "%~dp0"

REM ǿ�ƽ������� COM Surrogate ���̣�dllhost.exe��
echo ���ڽ��� COM Surrogate ����...
taskkill /f /im dllhost.exe >nul 2>&1
if %errorlevel% equ 0 (
    echo COM Surrogate �����ѽ�����
) else (
    echo δ�ܽ��� COM Surrogate ���̣�����û���������е�ʵ����
)

REM ע�� DLL
echo ע�� AssimpThumbnailProvider.dll...
regasm.exe /codebase AssimpThumbnailProvider.dll /u
regasm.exe /unregister AssimpThumbnailProvider.dll

REM ɾ���ļ���չ������
echo ɾ���ļ���չ������...
reg delete "HKCR\.ase\shellex\{E357FCCD-A995-4576-B01F-234630154E96}" /f
reg delete "HKCR\.fbx\shellex\{E357FCCD-A995-4576-B01F-234630154E96}" /f
reg delete "HKCR\.obj\shellex\{E357FCCD-A995-4576-B01F-234630154E96}" /f
reg delete "HKCR\.gltf\shellex\{E357FCCD-A995-4576-B01F-234630154E96}" /f
reg delete "HKCR\.glb\shellex\{E357FCCD-A995-4576-B01F-234630154E96}" /f
reg delete "HKCR\.dml\shellex\{E357FCCD-A995-4576-B01F-234630154E96}" /f
reg delete "HKCR\.chr\shellex\{E357FCCD-A995-4576-B01F-234630154E96}" /f

REM ��������ͼ����
echo ��������ͼ����...
taskkill /f /im explorer.exe
timeout /t 0
del /f /s /q "%LocalAppData%\Microsoft\Windows\Explorer\thumbcache_*.db"
del /f /s /q "%LocalAppData%\Microsoft\Windows\Explorer\iconcache_*.db"

REM ������Դ������
start explorer.exe

echo ж����ɣ�
pause
