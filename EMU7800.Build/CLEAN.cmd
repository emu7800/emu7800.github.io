@echo off

pushd %~dp0

set path=%programfiles(x86)%\MSBuild\12.0\Bin;%windir%\Microsoft.NET\Framework\v4.0.30319

msbuild _build.proj /t:Clean

echo Removing bin folders...
rd /s/q ..\EMU7800.D2D\EMU7800.DumpBin\bin
rd /s/q ..\EMU7800.D2D\EMU7800.Launcher\bin
rd /s/q ..\EMU7800.D2D\EMU7800.D2D.Shell\Win32\bin

pause

