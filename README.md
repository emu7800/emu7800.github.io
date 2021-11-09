# EMU7800
> Mike Murphy (mike@emu7800.net) - 12/1/2021

EMU7800 was originally completed in 2003 as a .NET programming exercise,
being ported to the litany of subsequent now defunct .NET platforms (Windows Phone, Silverlight, UWP, etc.)

EMU7800 continues to be maintained to the present day as a non-commercial endeavor.
Feel free to email feedback.
Enjoy!

[![Open in Visual Studio Code](https://open.vscode.dev/badges/open-in-vscode.svg)](https://open.vscode.dev/emu7800/emu7800.github.io)

## Build Tooling

To build, the following tools are needed:

- Visual Studio 2022 (https://visualstudio.microsoft.com/vs/community/)

- .NET 6 SDK x64 Windows (https://dotnet.microsoft.com/download)

- Powershell 7.2.x

- Inno Setup Compiler 6.1.2 (https://www.innosetup.com/)

## Build Steps

1. Build ``EMU7800.Win32.Interop.dll``

    Visual Studio 2022 will be necessary to build this C++ component. There is a dedicated solution file that can be opened directly:

    `.\src\win32\interop\EMU7800.Win32.Interop.sln`
    
    Alternatively, the solution file located here can build everything:

    `.\src\EMU7800.Win32.sln`

    However, this will not build the deployment packages.

2. Run ``pwsh.exe .\BuildArtifacts.ps1``

    This will compile everything other than the C++ component, build the deployment packages, and drop the artifacts to a `.\artifacts` folder.
