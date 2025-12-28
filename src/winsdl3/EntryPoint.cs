using EMU7800.Shell;

EMU7800.SDL3.Interop.SDL3.RegisterDllImportResolver();

var logger = new EMU7800.Win32.Interop.ConsoleLogger(CommandLine.GetOpenConsoleOption(args))
{
    Level = CommandLine.GetLoggingVerbosityOption(args)
};

var commandLine = new CommandLine(logger);
commandLine.Run(new EMU7800.SDL3.Interop.WindowSDL3Driver(logger), args);
