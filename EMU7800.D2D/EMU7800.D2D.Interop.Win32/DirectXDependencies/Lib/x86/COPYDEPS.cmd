@echo off

rem Copies dependencies from the installed DirectX SDK

if "%DXSDK_DIR%"=="" (
	echo DXSDK_DIR not defined, DirectX SDK not installed
	pause
	exit /b 1
)
set dxsdklibx86dir=%DXSDK_DIR%Lib\x86

xcopy /y "%dxsdklibx64dir%\dxguid.lib"
xcopy /y "%dxsdklibx64dir%\X3DAudio.lib"
pause
