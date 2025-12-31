# EMU7800
> Mike Murphy (mike@emu7800.net) - 12/31/2025

EMU7800 offers classic video game nostalgia on demand! It emulates the Atari 7800 ProSystem video game console (c. 1987) that was also backward compatible with the older and more popular Atari 2600 Video Computer System.
EMU7800 was originally completed in 2003 as a .NET programming exercise.
It continues to be maintained to the present day as a non-commercial endeavor.
Feel free to email feedback.
Enjoy!


#### WHAT'S NEW

Finished eliminating dependencies on Windows to facilitate ports to other platforms.
 
  - Added Linux ARM, Linux ARM64 targets using [SDL3](https://www.libsdl.org/)
  - Included a WinSDL3 target for convenient dev/testing [SDL3](https://www.libsdl.org/) builds on Windows
  - Added experimental OSX X64, OSX ARM64 targets using [SDL3](https://www.libsdl.org/)

## Building from Source

The [.NET 10.0 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/10.0) and [Git](https://git-scm.com/install/windows) are required to build EMU7800 from source.

Obtain the source by cloning the GitHub repository:
```
git clone https://github.com/emu7800/emu7800.github.io.git
```

Run the following from a command-prompt at the root of the source folder:
```
dotnet msbuild /t:Clean
dotnet msbuild
```
The Default target will build and drop everything under a newly created ```artifacts\``` sub-folder.
All deployments are .NET self-contained deployments.

If this is run on Windows, platform-optimized native executables (i.e., an AOT build) will be produced for Windows targets.
Non-Windows targets will be built as ReadyToRun, single-file managed executables.

If this is run on the other platforms (e.g., Linux ARM, Linux ARM64), platform-optimized native executables will be produced
only for that specific platform.

OSX 64 and OSX ARM64 targets are experimental and have not been tested. Use the ```/t:DefaultExp``` to build those targets.

To turn off AOT builds, specify ```/p:PublishAot=false``` on the msbuild command-line.

To build the Win32 Installer, the [Inno Setup Compiler 6.5.4](https://www.innosetup.com/) is required.

Run the following from a command-prompt at the root of the source folder on a Windows system:

```
dotnet msbuild /t:BuildWin32Installer
```

This will drop an installer executable ```EMU7800Setup-x64-x.y.z.exe``` into the ```artifacts\``` folder.

## Running on Linux

The SDL3 dependencies likely will not be pre-installed on your Linux system, so run the following to install them:

```
sudo apt install libsdl3-0
sudo apt install libsdl3-image0
sudo apt install libsdl3-ttf0
```

Deploy EMU7800 by unzipping the built .zip archive from the ```artifacts\``` folder to the designated deployment location on your Linux system:

```
unzip <path-to-emu7800-zip-file> -d <path-to-deployment-folder>
```