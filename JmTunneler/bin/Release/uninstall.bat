@echo off

bcdedit >>nul
if %errorlevel% == 1 goto noadmin

pushd "%~dp0"
set Directory=%cd%
set ApplicationName=JmTunneler
echo %Directory%
echo %ApplicationName%

sc stop %ApplicationName%
sc delete %ApplicationName%

pause
exit
:noadmin
cls
echo �� ������ ������ �ƴմϴ�!
echo �� ������ ���콺 Ŭ���ؼ� ������ �������� �������ּ���.
pause
exit