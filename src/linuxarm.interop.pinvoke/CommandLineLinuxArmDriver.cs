using EMU7800.Services.Dto;
using EMU7800.Shell;
using System;

namespace EMU7800.LinuxArm.Interop;

public sealed partial class CommandLineLinuxArmDriver : ICommandLineDriver
{
    public static CommandLineLinuxArmDriver Factory() => new();
    CommandLineLinuxArmDriver() {}

    #region ICommandLineDriver Members

    public void AttachConsole(bool _) {}

    public void Start(bool startMaximized)
    {
        //using var app = new Win32App();
        //app.Run(startMaximized);
        Console.WriteLine($"[Start(startMaximized={startMaximized})");
    }

    public void StartGameProgram(GameProgramInfoViewItem gpivi)
    {
        //using var app = new Win32App(gpivi);
        //app.Run();
        Console.WriteLine($"[StartGameProgram({gpivi.Title})");
    }

    #endregion
}
