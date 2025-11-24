using EMU7800.Shell;

CommandLine.DriverFactory = EMU7800.Win32.Interop.CommandLineWin32Driver.Factory;
CommandLine.Run(args);
