# EMU7800
> Mike Murphy (mike@emu7800.net) - 1/15/2025

EMU7800 was originally completed in 2003 as a .NET programming exercise.
It continues to be maintained to the present day as a non-commercial endeavor.
Feel free to email feedback.
Enjoy!

[Open in VSCode](https://vscode.dev/emu7800/emu7800.github.io)

## Building from Source

To build, the following tools are needed:

- [.NET 9.0](https://dotnet.microsoft.com/en-us/download/dotnet/9.0)

- [Inno Setup Compiler 6.2.2](https://www.innosetup.com/)

To execute the build, run the following from a command-prompt at the root of the source directory (where this README is found):
```
dotnet restore
dotnet msbuild Build.csproj /tl
```
This will build and drop everything under a newly created subdirectory ```artifacts\```.
