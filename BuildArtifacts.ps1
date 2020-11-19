Push-Location $PSScriptRoot

# clear target
Remove-Item -Recurse -Force .\artifacts

# copy source artifacts
robocopy.exe .\src\                      .\artifacts\srcdist\src\ *.cs *.csproj *.cpp *.h *.manifest *.sln  *.filters *.rc *.vcxproj *.ico *.txt *.png *.json /S /XD obj bin /NFL
robocopy.exe .\lib\DirectXDependencies\  .\artifacts\srcdist\lib\DirectXDependencies\ /S /NFL
robocopy.exe .\lib\vcredistDependencies\ .\artifacts\srcdist\lib\vcredistDependencies\ /S /NFL
robocopy.exe .\docs\                     .\artifacts\srcdist\docs\ /S /NFL
Compress-Archive .\artifacts\srcdist\ .\artifacts\EMU7800.src.zip -CompressionLevel Optimal -Force

# copy executable artifacts
robocopy.exe     .\src\shell\bin\Release\net5.0\win-x64\ .\artifacts\bindist\ *.dll *.exe /S /NFL
New-Item -Name   .\artifacts\bindist\ROMS\ -ItemType directory
Compress-Archive .\lib\roms\Bios78      .\artifacts\bindist\ROMS\Bios78.zip      -CompressionLevel Optimal -Force
Compress-Archive .\lib\roms\HomeBrews26 .\artifacts\bindist\ROMS\HomeBrews26.zip -CompressionLevel Optimal -Force
Compress-Archive .\lib\roms\HomeBrews78 .\artifacts\bindist\ROMS\HomeBrews78.zip -CompressionLevel Optimal -Force
robocopy.exe     .\lib\vcredistDependencies\x64\ .\artifacts\bindist\ msvcp120.dll

Pop-Location