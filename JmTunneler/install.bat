@echo off

bcdedit >>nul
if %errorlevel% == 1 goto noadmin

pushd "%~dp0"
set Directory=%cd%
set ApplicationName=JmTunneler
echo %Directory%
echo %ApplicationName%

sc stop %ApplicationName%
timeout /t 1
sc delete %ApplicationName%
timeout /t 1
sc create %ApplicationName% binPath="%Directory%\JmTunneler.exe" type=own start=auto
timeout /t 1
sc start JmTunneler

pause
exit
:noadmin
cls
echo �� ������ ������ �ƴմϴ�!
echo �� ������ ���콺 Ŭ���ؼ� ������ �������� �������ּ���.
pause
exit