@echo off
rem dotnet tool install -g Microsoft.DotNet.Mage
rem https://docs.microsoft.com/en-us/visualstudio/deployment/building-clickonce-applications-from-the-command-line?view=vs-2019

dotnet-mage -new Application        ^
 -tofile EMU7800.manifest           ^
 -name EMU7800                      ^
 -processor MSIL                    ^
 -UseManifestForTrust true          ^
 -s http://emu7800.net              ^
 -v 5.0.0.0                         ^
 -fd ..\..\..\artifacts\EMU7800.bin ^
 -if ..\..\src\shell\EMU7800.ico

dotnet-mage -new Deployment         ^
 -tofile EMU7800.application        ^
 -name EMU7800                      ^
 -v 5.0.0.0                         ^
 -appc 5.0.0.0/EMU7800.manifest     ^
 -appm EMU7800.manifest             ^
 -ip true                           ^
 -pu http://emu7800.net/download/EMU7800.application ^
 -i true                            ^
 -pub "Mike Murphy"

