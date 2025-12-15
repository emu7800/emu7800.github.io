using EMU7800.Services.Dto;
using EMU7800.Shell;
using System.Runtime.InteropServices;

namespace EMU7800.SDL3.Interop;

public sealed partial class CommandLineWinSDL3Driver : ICommandLineDriver
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
        WindowSDL3Driver.StartWindowAndProcessEvents(startMaximized);
    }

    public void StartGameProgram(GameProgramInfoViewItem gpivi, bool startMaximized)
    {
        Window.ReplaceStartPage(gpivi);
        WindowSDL3Driver.StartWindowAndProcessEvents(startMaximized);
    }

    #endregion

    #region Constructors

    public CommandLineWinSDL3Driver() {}

    #endregion

    #region Helpers

    [LibraryImport("Kernel32.dll")]
    internal static partial void AllocConsole();

    [LibraryImport("Kernel32.dll")]
    internal static partial void AttachConsole(int dwProcessId);

    #endregion
}