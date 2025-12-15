using EMU7800.Services.Dto;
using EMU7800.Shell;

namespace EMU7800.SDL3.Interop;

public sealed class CommandLineSDL3Driver : ICommandLineDriver
{
    #region ICommandLineDriver Members

    public void AttachConsole(bool _) {}

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

    public CommandLineSDL3Driver() {}

    #endregion
}
