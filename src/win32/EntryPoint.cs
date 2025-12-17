using EMU7800.Shell;

var logger = new EMU7800.Win32.Interop.ConsoleLogger { Level = 9 };

var commandLine = new CommandLine(logger);
commandLine.Run(new EMU7800.Win32.Interop.CommandLineWin32Driver(args, logger), args);
