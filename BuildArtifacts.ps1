Push-Location $PSScriptRoot

# clear target
Remove-Item -Recurse -Force .\artifacts

# copy source artifacts
robocopy.exe .\src\                                      .\artifacts\EMU7800.src\src\ *.cs *.csproj *.cpp *.h *.manifest *.sln  *.filters *.rc *.vcxproj *.ico *.txt *.png *.json /S /XD obj bin /NFL
robocopy.exe .\docs\                                     .\artifacts\EMU7800.src\docs\ /S /NFL
Compress-Archive .\artifacts\EMU7800.src\                .\artifacts\EMU7800.src.zip -CompressionLevel Optimal -Force

# copy executable artifacts
robocopy.exe     .\src\win32\bin\Release\net5.0\publish\ .\artifacts\EMU7800.bin\ /XD ref /XF *.runtimeconfig.dev.json *.exp *.ilk *.lib *.pdb /S /NFL
New-Item -Name                                           .\artifacts\EMU7800.bin\ROMS\ -ItemType directory
Compress-Archive .\lib\roms\Bios78\                      .\artifacts\EMU7800.bin\ROMS\Bios78.zip      -CompressionLevel Optimal -Force
Compress-Archive .\lib\roms\HomeBrews26\                 .\artifacts\EMU7800.bin\ROMS\HomeBrews26.zip -CompressionLevel Optimal -Force
Compress-Archive .\lib\roms\HomeBrews78\                 .\artifacts\EMU7800.bin\ROMS\HomeBrews78.zip -CompressionLevel Optimal -Force

Pop-Location