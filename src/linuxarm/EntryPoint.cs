using EMU7800.Shell;

CommandLine.DriverFactory = EMU7800.LinuxArm.Interop.CommandLineLinuxArmDriver.Factory;
CommandLine.Run(args);