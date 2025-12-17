using EMU7800.Services.Dto;
using EMU7800.Shell;
using System.Runtime.InteropServices;

namespace EMU7800.Win32.Interop;

public sealed partial class CommandLineWin32Driver : ICommandLineDriver
{

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
        var logger = new ConsoleLogger { Level = 1 };
        var window = new Window(logger);
        var windowDriver = new WindowWin32Driver(window, logger);
        windowDriver.ProcessEvents(startMaximized);
    }

    public void StartGameProgram(GameProgramInfoViewItem gpivi, bool startMaximized)
    {
        var logger = new ConsoleLogger { Level = 1 };
        var window = new Window(gpivi, logger);
        var windowDriver = new WindowWin32Driver(window, logger);
        windowDriver.ProcessEvents(startMaximized);
    }

    #endregion

    #region Constructors

    public CommandLineWin32Driver() {}

    #endregion

    #region Helpers

    [LibraryImport("Kernel32.dll")]
    internal static partial void AllocConsole();

    [LibraryImport("Kernel32.dll")]
    internal static partial void AttachConsole(int dwProcessId);

    #endregion
}
