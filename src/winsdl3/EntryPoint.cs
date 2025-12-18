using EMU7800.Shell;

var logger = new EMU7800.SDL3.Interop.SDL3Logger { Level = 9 };

var commandLine = new CommandLine(logger);
commandLine.Run(new EMU7800.SDL3.Interop.WindowWinSDL3Driver(args, logger), args);
