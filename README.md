# EMU7800
> Mike Murphy (mike@emu7800.net) - 12/31/2025

EMU7800 was originally completed in 2003 as a .NET programming exercise.
It continues to be maintained to the present day as a non-commercial endeavor.
Feel free to email feedback.
Enjoy!

[Open in VSCode](https://vscode.dev/emu7800/emu7800.github.io)

## Building from Source

The following tools are needed for a default build:

- [.NET 10.0](https://dotnet.microsoft.com/en-us/download/dotnet/10.0)

Run the following from a command-prompt at the root of the source directory (where this README is found):
```
dotnet msbuild Build.proj /t:Clean
dotnet msbuild Build.proj
```
This will build and drop everything under a newly created subdirectory ```artifacts\```.

To build the Win32 Installer, the following additional tool is needed:

- [Inno Setup Compiler 6.5.4](https://www.innosetup.com/)

Run the following from a command-prompt at the root of the source directory on a Windows system:

```
dotnet msbuild Build.proj /t:BuildWin32Installer
```

### Building Platform-Optimized Native Executables

To build platform-optimized native executables, simply add ```/p:PublishAot=true``` to the msbuild command line.
Cross-compilation is not supported by the dotnet tooling, so this must be run on the target platform.

Examples:

Build a windows installer containing the native executable (run on Windows):

```
dotnet msbuild Build.proj /t:BuildWin32Installer /p:PublishAot=true
```

For 32-bit Linux ARM executables, run the following on a Raspberry Pi 32-bit OS
(e.g., Debian Trixie with Desktop, as it will include the needed tooling):

```
dotnet msbuild Build.proj /t:LinuxArm /p:PublishAot=true
```

For 64-bit Linux ARM executables, run the following on a Raspberry Pi 64-bit OS
(e.g., Debian Trixie with Desktop, as it will include the needed tooling):
```
dotnet msbuild Build.proj /t:LinuxArm64 /p:PublishAot=true
```

The SDL3 dependencies likely will not be pre-installed on your Linux system, so run the following to install them:

```
sudo apt install libsdl3-0
sudo apt install libsdl3-image0
sudo apt install libsdl3-ttf0
```
