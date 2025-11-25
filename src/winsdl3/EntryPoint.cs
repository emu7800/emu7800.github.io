using EMU7800.Shell;

CommandLine.DriverFactory = EMU7800.SDL3.Interop.CommandLineWinSDL3Driver.Factory;
CommandLine.Run(args);
