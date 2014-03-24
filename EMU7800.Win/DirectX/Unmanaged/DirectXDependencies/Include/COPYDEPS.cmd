@echo off

rem Copies dependencies from the installed DirectX SDK

if "%DXSDK_DIR%"=="" (
	echo DXSDK_DIR not defined, DirectX SDK not installed
	pause
	exit /b 1
)
set dxsdkincludedir=%DXSDK_DIR%Include

xcopy /y "%dxsdkincludedir%\d3d9.h"  .
xcopy /y "%dxsdkincludedir%\DxErr.h" .

pause
