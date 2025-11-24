using EMU7800.Services.Dto;
using EMU7800.Shell;
using System.Runtime.InteropServices;

namespace EMU7800.Win32.Interop;

public sealed partial class CommandLineWin32Driver : ICommandLineDriver
{
    public static CommandLineWin32Driver Factory() => new();
    CommandLineWin32Driver() {}

    #region ICommandLineDriver Members

    public void AttachConsole(bool allocNewConsole)
    {
        if (allocNewConsole)
        {
            AllocConsole();
        }
        else
        {
            AttachConsole(-1);
        }
    }

    public void Start(bool startMaximized)
    {
        using var app = new Win32App();
        app.Run(startMaximized);
    }

    public void StartGameProgram(GameProgramInfoViewItem gpivi)
    {
        using var app = new Win32App(gpivi);
        app.Run();
    }

    #endregion

    #region Helpers

    [LibraryImport("Kernel32.dll")]
    internal static partial void AllocConsole();

    [LibraryImport("Kernel32.dll")]
    internal static partial void AttachConsole(int dwProcessId);

    #endregion
}
