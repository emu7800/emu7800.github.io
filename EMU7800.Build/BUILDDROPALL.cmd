@echo off

rem prepares DROP folder, but does not build source distribution

pushd %~dp0

set path=%programfiles(x86)%\MSBuild\12.0\Bin;%windir%\Microsoft.NET\Framework\v4.0.30319

msbuild _build.proj /t:DropAll /m

pause

