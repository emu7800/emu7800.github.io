# EMU7800
> Mike Murphy (mike@emu7800.net) - 12/31/2025

EMU7800 was originally completed in 2003 as a .NET programming exercise.
It continues to be maintained to the present day as a non-commercial endeavor.
Feel free to email feedback.
Enjoy!

[Open in VSCode](https://vscode.dev/emu7800/emu7800.github.io)

## Building from Source

The following tools are needed for a Default build:

- [.NET 10.0](https://dotnet.microsoft.com/en-us/download/dotnet/10.0)

Run the following from a command-prompt at the root of the source directory (where this README is found):
```
dotnet msbuild Build.proj /t:Clean
dotnet msbuild Build.proj
```
The Default target will build and drop everything under a newly created ```artifacts\``` sub-folder.
All targets are self-contained deployments.

If this is run on Windows, platform-optimized native executables (i.e., an AOT build) will be produced for Windows targets.
Non-Windows targets will be built as ReadyToRun, single-file managed executables.

If this is run on the other platforms (Linux Arm, Linux Arm64), platform-optimized native executables will be produced
only for that specific platform.

OSX 64 and OSX ARM64 targets are experimental and have not been tested. Use the ```/t:DefaultExp``` to build those targets.

To turn off AOT builds, specify ```/p:PublishAot=false``` on the msbuild command-line.

To build the Win32 Installer, the following additional tool is needed:

- [Inno Setup Compiler 6.5.4](https://www.innosetup.com/)

Run the following from a command-prompt at the root of the source directory on a Windows system:

```
dotnet msbuild Build.proj /t:BuildWin32Installer
```

This will drop an installer executable ```EMU7800-Setup-x.y.z.exe``` into the ```artifacts\``` folder.

## Running on Linux

The SDL3 dependencies likely will not be pre-installed on your Linux system, so run the following to install them:

```
sudo apt install libsdl3-0
sudo apt install libsdl3-image0
sudo apt install libsdl3-ttf0
```
