using System.Runtime.InteropServices;

namespace EMU7800.SDL3.Interop;

public sealed partial class CommandLineWinSDL3Driver : CommandLineSDL3Driver
{
    #region ICommandLineDriver Members

    public override void AttachConsole(bool allocNewConsole)
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