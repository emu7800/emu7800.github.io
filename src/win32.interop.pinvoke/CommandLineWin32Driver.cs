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
        WindowWin32Driver.StartWindowAndProcessEvents(startMaximized);
    }

    public void StartGameProgram(GameProgramInfoViewItem gpivi, bool startMaximized)
    {
        Window.ReplaceStartPage(gpivi);
        WindowWin32Driver.StartWindowAndProcessEvents(startMaximized);
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
