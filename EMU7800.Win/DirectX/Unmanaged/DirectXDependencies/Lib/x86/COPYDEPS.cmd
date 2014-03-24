@echo off

rem Copies dependencies from the installed DirectX SDK

if "%DXSDK_DIR%"=="" (
	echo DXSDK_DIR not defined, DirectX SDK not installed
	pause
	exit /b 1
)
set dxsdklibx86dir=%DXSDK_DIR%Lib\x86

xcopy /y "%dxsdklibx86dir%\d3d9.lib"    .
xcopy /y "%dxsdklibx86dir%\dinput8.lib" .
xcopy /y "%dxsdklibx86dir%\dxguid.lib"  .
xcopy /y "%dxsdklibx86dir%\dxerr.lib"   .
xcopy /y "%dxsdklibx86dir%\dxerr9.lib"  .
pause
