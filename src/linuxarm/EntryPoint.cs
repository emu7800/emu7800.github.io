using EMU7800.Shell;

var logger = new EMU7800.SDL3.Interop.SDL3Logger { Level = CommandLine.GetLoggingVerbosityOption(args) };

var commandLine = new CommandLine(logger);
commandLine.Run(new EMU7800.SDL3.Interop.WindowSDL3Driver(logger), args);