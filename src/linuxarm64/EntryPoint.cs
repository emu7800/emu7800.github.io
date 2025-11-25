using EMU7800.Shell;

CommandLine.DriverFactory = EMU7800.SDL3.Interop.CommandLineSDL3Driver.Factory;
CommandLine.Run(args);