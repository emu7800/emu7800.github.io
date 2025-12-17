using EMU7800.Shell;

var logger = new EMU7800.SDL3.Interop.SDLConsoleLogger { Level = 9 };

var commandLine = new CommandLine(logger);
commandLine.Run(new EMU7800.SDL3.Interop.CommandLineSDL3Driver(logger), args);
