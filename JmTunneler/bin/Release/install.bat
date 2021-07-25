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
echo ★ 관리자 권한이 아닙니다!
echo ★ 오른쪽 마우스 클릭해서 관리자 권한으로 실행해주세요.
pause
exit