# EMU7800 Source Code README
> Mike Murphy (mike@emu7800.net) - 12/1/2020 

## Tooling Required

To build everything, the following tools are needed:

- Visual Studio 2019 16.9.0 Preview 1.0 (https://visualstudio.microsoft.com/vs/community/)

- .NET 5.0 SDK x64 Windows (https://dotnet.microsoft.com/download)

- Powershell 7.1.0 (``dotnet tool install powershell -g``)

- Inno Setup Compiler 6.1.2 (https://www.innosetup.com/)

- MSI Wrapper 9.0.42.0 (https://www.exemsi.com/)


There isn't a command-line tool that can build `.vcxproj` (that I know of.)


## Build Steps

1. Build ``EMU7800.Win32.Interop.dll``

    Visual Studio 2019 will be necessary to build this C++ component. There is a dedicated solution file that can be opened directly:

    `.\src\win32\interop\EMU7800.Win32.Interop.sln`
    
    Alternatively, the solution file located here can build everything:

    `.\src\EMU7800.Win32.sln`

    However, this will not build the deployment packages.

2. Run ``pwsh.exe .\BuildArtifacts.ps1``

    This will build everying other than the C++ component and drop the artifacts to a new `.\artifacts` folder.
