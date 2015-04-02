@echo off
setlocal

:: Check prerequisites
if not "%VisualStudioVersion%" == "14.0" (
    echo Error: build.cmd should be run from a Visual Studio 2015 Command Prompt.  
    echo        Please see EMU7800.Build\README.html for build instructions.
    exit /b 1
)

:: Check for a custom MSBuild path. If not defined, default to the one in your path.
if not defined MSBUILDCUSTOMPATH (
    set MSBUILDCUSTOMPATH=MSBuild.exe
)

:: Call MSBuild
echo ** "%MSBUILDCUSTOMPATH%" "%~dp0_build.proj" /maxcpucount /verbosity:minimal /nodeReuse:false /fileloggerparameters:Verbosity=diag;LogFile="%~dp0msbuild.log" %*
"%MSBUILDCUSTOMPATH%" "%~dp0_build.proj" /t:Clean /maxcpucount /verbosity:minimal /nodeReuse:false /fileloggerparameters:Verbosity=diag;LogFile="%~dp0msbuild.log" %*
set BUILDERRORLEVEL=%ERRORLEVEL%
echo.

echo Removing bin folders...
rd /s/q ..\EMU7800.D2D\EMU7800.DumpBin\bin
rd /s/q ..\EMU7800.D2D\EMU7800.Launcher\bin
rd /s/q ..\EMU7800.D2D\EMU7800.D2D.Shell\Win32\bin

echo ** MSBuild Path: %MSBUILDCUSTOMPATH%
echo ** Building all sources

:: Pull the build summary from the log file
findstr /ir /c:".*Warning(s)" /c:".*Error(s)" /c:"Time Elapsed.*" "%~dp0msbuild.log"
echo ** Build completed. Exit code: %BUILDERRORLEVEL%

exit /b %BUILDERRORLEVEL%
