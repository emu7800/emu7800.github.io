@echo off

pushd %~dp0

robocopy DirectX\Unmanaged\bin\Debug\x86   bin\Debug\x86   EMU7800.DirectX.dll
robocopy DirectX\Unmanaged\bin\Debug\x64   bin\Debug\x64   EMU7800.DirectX.dll
robocopy DirectX\Unmanaged\bin\Release\x86 bin\Release\x86 EMU7800.DirectX.dll
robocopy DirectX\Unmanaged\bin\Release\x64 bin\Release\x64 EMU7800.DirectX.dll

pause