@echo off

rem prepares DROP folder and source distribution

pushd %~dp0

set path=%programfiles(x86)%\MSBuild\12.0\Bin;%windir%\Microsoft.NET\Framework\v4.0.30319

msbuild _build.proj /t:All /m

pause

