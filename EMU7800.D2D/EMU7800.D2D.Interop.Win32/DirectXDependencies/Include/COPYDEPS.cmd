@echo off

rem Copies dependencies from the installed DirectX SDK

if "%DXSDK_DIR%"=="" (
	echo DXSDK_DIR not defined, DirectX SDK not installed
	pause
	exit /b 1
)
set dxsdkincludedir=%DXSDK_DIR%Include

xcopy /y "%dxsdkincludedir%\XAudio2.h"
xcopy /y "%dxsdkincludedir%\audiodefs.h"
xcopy /y "%dxsdkincludedir%\comdecl.h"
xcopy /y "%dxsdkincludedir%\xma2defs.h"
pause
